﻿//HintName: Splat.DI.Extensions.Registrations.SourceGenerated.cs

// <auto-generated />
namespace Splat
{
    internal static partial class SplatRegistrations
    {
        static partial void SetupIOCInternal( Splat.IDependencyResolver resolver) 
        {
            Splat.Locator.CurrentMutable.Register(() => new global::Test.TestConcrete((global::Test.IService1)resolver.GetService(typeof(global::Test.IService1)), (global::Test.IService2)resolver.GetService(typeof(global::Test.IService2))), typeof(global::Test.ITest), "Test1");
        }
    }
}