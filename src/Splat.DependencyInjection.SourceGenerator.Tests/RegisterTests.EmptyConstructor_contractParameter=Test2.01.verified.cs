﻿//HintName: Splat.DI.Extensions.Registrations.SourceGenerated.cs

// <auto-generated />
namespace Splat
{
    internal static partial class SplatRegistrations
    {
        static partial void SetupIOCInternal( Splat.IDependencyResolver resolver) 
        {
            Splat.Locator.CurrentMutable.Register(() => new global::Test.TestConcrete(), typeof(global::Test.ITest), "Test2");
        }
    }
}