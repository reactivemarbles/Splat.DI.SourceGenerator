﻿//HintName: Splat.DI.Extensions.Registrations.SourceGenerated.cs

// <auto-generated />
namespace Splat
{
    internal static partial class SplatRegistrations
    {
        static partial void SetupIOCInternal( Splat.IDependencyResolver resolver) 
        {
            {
                global::System.Lazy<Test.TestConcrete> lazy = new global::System.Lazy<Test.TestConcrete>(() => new global::Test.TestConcrete((global::Test.IService1)resolver.GetService(typeof(global::Test.IService1)), (global::Test.IService2)resolver.GetService(typeof(global::Test.IService2))){ ServiceProperty=(global::Test.IServiceProperty)resolver.GetService(typeof(global::Test.IServiceProperty))} );
                Splat.Locator.CurrentMutable.Register(() => lazy, typeof(global::System.Lazy<Test.TestConcrete>), "Test2");
                Splat.Locator.CurrentMutable.Register(() => lazy.Value, typeof(global::Test.TestConcrete), "Test2");
            }
        }
    }
}