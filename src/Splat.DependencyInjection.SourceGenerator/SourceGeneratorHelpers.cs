﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.RoslynHelpers;

using Splat.DependencyInjection.SourceGenerator.Metadata;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace Splat.DependencyInjection.SourceGenerator;

internal static class SourceGeneratorHelpers
{
    private const string RegisterMethodName = "Register";
    private const string LocatorName = "Splat.Locator.CurrentMutable";

    public static string Generate(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
    {
        var methods = MetadataExtractor.GetValidMethods(context, syntaxReceiver, compilation).ToList();

        methods = MetadataDependencyChecker.CheckMetadata(context, methods);

        var invocations = Generate(methods);

        var constructIoc = MethodDeclaration(
            [SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword],
            "void",
            Constants.IocMethod,
            [Parameter(Constants.ResolverType, Constants.ResolverParameterName)],
            1,
            Block(invocations.ToList(), 2));

        var registrationClass = ClassDeclaration(Constants.ClassName, [SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword], [constructIoc], 1);

        var namespaceDeclaration = NamespaceDeclaration(Constants.NamespaceName, [registrationClass], false);

        var compilationUnit = CompilationUnit(default, [namespaceDeclaration], []);

        return $@"
// <auto-generated />
{compilationUnit.ToFullString()}";
    }

    private static IEnumerable<StatementSyntax> Generate(IEnumerable<MethodMetadata> methodMetadataEnumerable)
    {
        foreach (var methodMetadata in methodMetadataEnumerable)
        {
            var typeConstructorArguments = methodMetadata.ConstructorDependencies
                .Select(parameter => parameter.Type)
                .Select(parameterType => parameterType.ToDisplayString(RoslynCommonHelpers.TypeFormat))
                .Select(parameterTypeName => Argument(GetSplatService(parameterTypeName)))
                .ToList();

            var contractParameter = methodMetadata.RegisterParameterValues.FirstOrDefault(x => x.ParameterName == "contract");

            string? contract = null;
            if (contractParameter is not null)
            {
                contract = contractParameter.ParameterValue;
            }

            var initializer = GetPropertyInitializer(methodMetadata.Properties);

            ExpressionSyntax call = initializer is null ?
                    ObjectCreationExpression(methodMetadata.ConcreteTypeName, typeConstructorArguments) :
                    ObjectCreationExpression(methodMetadata.ConcreteTypeName, typeConstructorArguments, initializer);

            switch (methodMetadata)
            {
                case RegisterLazySingletonMetadata lazyMetadata:
                    yield return GetLazyBlock(lazyMetadata, call, contract);
                    break;
                case RegisterMetadata registerMetadata:
                    yield return GenerateLocatorSetService(Argument(ParenthesizedLambdaExpression(call)), registerMetadata.InterfaceTypeName, contract);
                    break;
            }
        }
    }

    private static InitializerExpressionSyntax? GetPropertyInitializer(IEnumerable<PropertyDependencyMetadata> properties)
    {
        var propertySet = properties
            .Select(property =>
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    property.Name,
                    GetSplatService(property.TypeName)))
            .ToList();

        return propertySet.Count > 0 ? InitializerExpression(SyntaxKind.ObjectInitializerExpression, propertySet) : null;
    }

    private static CastExpressionSyntax GetSplatService(string parameterTypeName) =>
        CastExpression(
            parameterTypeName,
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ResolverParameterName, Constants.LocatorGetService),
                [
                    Argument($"typeof({parameterTypeName})"),
                ]));

    private static BlockSyntax GetLazyBlock(MethodMetadata methodMetadata, ExpressionSyntax call, string? contract)
    {
        var lazyType = $"global::System.Lazy<{methodMetadata.InterfaceType}>";

        const string lazyTypeValueProperty = "Value";
        const string lazyVariableName = "lazy";

        var lazyArguments = new List<ArgumentSyntax>
        {
            Argument(ParenthesizedLambdaExpression(call))
        };

        var lazyModeParameter = methodMetadata.RegisterParameterValues.FirstOrDefault(x => x.ParameterName == "mode");

        if (lazyModeParameter is not null)
        {
            var modeName = lazyModeParameter.ParameterValue;

            lazyArguments.Add(Argument(modeName));
        }

        return Block(
            [
                LocalDeclarationStatement(
                    VariableDeclaration(
                        lazyType,
                        [
                            VariableDeclarator(
                                lazyVariableName,
                                EqualsValueClause(
                                    ObjectCreationExpression(
                                        lazyType,
                                        lazyArguments)))
                        ])),
                GenerateLocatorSetService(
                    Argument(ParenthesizedLambdaExpression(IdentifierName(lazyVariableName))),
                    lazyType,
                    contract),
                GenerateLocatorSetService(
                    Argument(ParenthesizedLambdaExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, lazyVariableName, lazyTypeValueProperty))),
                    methodMetadata.InterfaceTypeName,
                    contract)
            ],
            3);
    }

    private static ExpressionStatementSyntax GenerateLocatorSetService(ArgumentSyntax argument, string interfaceType, string? contract)
    {
        var lambdaArguments = new List<ArgumentSyntax>
        {
            argument,
            Argument($"typeof({interfaceType})")
        };

        if (contract is not null)
        {
            lambdaArguments.Add(Argument(contract));
        }

        return ExpressionStatement(InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, LocatorName, RegisterMethodName),
            lambdaArguments));
    }
}
