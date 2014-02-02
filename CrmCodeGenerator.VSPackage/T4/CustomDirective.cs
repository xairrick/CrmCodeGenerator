using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TextTemplating;
using System.Runtime.Remoting.Messaging;

namespace CrmCodeGenerator.VSPackage.T4
{
    internal sealed class CustomDirective : DirectiveProcessor
    {
        private CodeDomProvider _provider;

        private readonly StringWriter _classCodeWriter = new StringWriter();
        private readonly StringWriter _initializationCodeWriter = new StringWriter();

        private static readonly CodeGeneratorOptions _options = new CodeGeneratorOptions
        {
            BlankLinesBetweenMembers = true,
            IndentString = "        ",
            VerbatimOrder = true,
            BracingStyle = "C"
        };

        public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
        {
            base.StartProcessingRun(languageProvider, templateContents, errors);

            _provider = languageProvider;
        }

        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            string name = arguments["name"];
            string fieldName = string.Format("_{0}", arguments["name"]);
            string type = arguments["type"];

            var field = new CodeMemberField(type, fieldName) { Attributes = MemberAttributes.Private };

            _provider.GenerateCodeFromMember(field, _classCodeWriter, _options);

            var property = new CodeMemberProperty
            {
                Name = name,
                Type = new CodeTypeReference(type),
                Attributes = MemberAttributes.Public,
                HasGet = true,
                HasSet = false
            };

            property.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), fieldName)));

            _provider.GenerateCodeFromMember(property, _classCodeWriter, _options);

            CodeAssignStatement assignment = new CodeAssignStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
                new CodeCastExpression(
                    new CodeTypeReference(type),
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(typeof(CallContext)),
                        "LogicalGetData",
                        new CodePrimitiveExpression(name))));

            _provider.GenerateCodeFromStatement(assignment, _initializationCodeWriter, _options);
        }

        public override string GetClassCodeForProcessingRun()
        {
            return _classCodeWriter.ToString();
        }

        public override string GetPostInitializationCodeForProcessingRun()
        {
            return _initializationCodeWriter.ToString();
        }

        public override void FinishProcessingRun()
        {
            return;
        }

        public override string[] GetImportsForProcessingRun()
        {
            return null;
        }

        public override string GetPreInitializationCodeForProcessingRun()
        {
            return null;
        }

        public override string[] GetReferencesForProcessingRun()
        {
            return null;
        }

        public override bool IsDirectiveSupported(string directiveName)
        {
            return true;
        }        
    }
}