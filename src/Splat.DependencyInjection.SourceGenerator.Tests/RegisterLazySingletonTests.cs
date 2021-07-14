﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

using VerifyXunit;

using Xunit;
using Xunit.Abstractions;

namespace Splat.DependencyInjection.SourceGenerator.Tests
{
    [UsesVerify]
    public class RegisterLazySingletonTests : TestBase
    {
        public RegisterLazySingletonTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, "RegisterLazySingleton")
        {
        }

        [Theory]
        [InlineData(LazyThreadSafetyMode.PublicationOnly, "")]
        [InlineData(LazyThreadSafetyMode.PublicationOnly, "Test1")]
        [InlineData(LazyThreadSafetyMode.PublicationOnly, "Test2")]
        [InlineData(LazyThreadSafetyMode.ExecutionAndPublication, "")]
        [InlineData(LazyThreadSafetyMode.ExecutionAndPublication, "Test1")]
        [InlineData(LazyThreadSafetyMode.ExecutionAndPublication, "Test2")]
        [InlineData(LazyThreadSafetyMode.None, "")]
        [InlineData(LazyThreadSafetyMode.None, "Test1")]
        [InlineData(LazyThreadSafetyMode.None, "Test2")]
        public Task ConstructionAndMultiplePropertyInjectionWithLazyMode(LazyThreadSafetyMode mode, string contract)
        {
            string arguments;
            if (string.IsNullOrWhiteSpace(contract))
            {
                arguments = $"LazyThreadSafetyMode.{mode}";
            }
            else
            {
                arguments = $"\"{contract}\", LazyThreadSafetyMode.{mode}";
            }

            var source = @$"
using System;
using System.Threading;
using Splat;

namespace Test
{{
    public static class DIRegister
    {{
        static DIRegister()
        {{
            SplatRegistrations.RegisterLazySingleton<ITest, TestConcrete>({arguments});
        }}
    }}

    public interface ITest {{ }}
    public class TestConcrete : ITest
    {{
        public TestConcrete(IService1 service1, IService2 service)
        {{
        }}

        [DependencyInjectionProperty]
        public IServiceProperty1 ServiceProperty1 {{ get; set; }}

        [DependencyInjectionProperty]
        public IServiceProperty2 ServiceProperty2 {{ get; set; }}

        [DependencyInjectionProperty]
        internal IServiceProperty3 ServiceProperty3 {{ get; set; }}
    }}

    public interface IService1 {{ }}
    public interface IService2 {{ }}
    public interface IServiceProperty1 {{ }}
    public interface IServiceProperty2 {{ }}
    public interface IServiceProperty3 {{ }}
}}";

            return TestPass(source, contract, mode);
        }
    }
}
