// Guids.cs
// MUST match guids.h
using System;

namespace CrmCodeGenerator.VSPackage
{
    static class GuidList
    {
        public const string guidCrmCodeGenerator_VSPackagePkgString = "d57cc223-384b-41cc-b5d0-814a71ce8d61";
        public const string guidCrmCodeGenerator_VSPackageCmdSetString = "275915bd-3cf8-4fa5-b6fb-9c03e9699b82";
        public const string guidCrmCodeGenerator_SimpleGenerator = "BB69ADDB-6AB5-4E29-B263-F918D86D1CC0";

        public static readonly Guid guidCrmCodeGenerator_VSPackageCmdSet = new Guid(guidCrmCodeGenerator_VSPackageCmdSetString);
    };
}