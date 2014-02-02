using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;

namespace CrmCodeGenerator.VSPackage.T4
{
    public class Compiler : MarshalByRefObject //,  ITextTemplatingEngineHost
    {
        private readonly AppDomain _appDomain;
        private string _fileExtensionValue = ".txt";
        private Encoding _fileEncodingValue = Encoding.UTF8;
        private CompilerErrorCollection _errorsValue;

        public Compiler()
        {
            _appDomain = AppDomain.CurrentDomain;
        }

        public Compiler(AppDomain appDomain)
        {
            this._appDomain = appDomain;
        }

        #region properties
        public string TemplateFileValue;

        public string TemplateFile
        {
            get { return TemplateFileValue; }
        }

        public string FileExtension
        {
            get { return _fileExtensionValue; }
        }

        public Encoding FileEncoding
        {
            get { return _fileEncodingValue; }
        }

        public CompilerErrorCollection Errors
        {
            get { return _errorsValue; }
        }

        public IList<string> StandardAssemblyReferences
        {
            get
            {
                return new string[] { typeof(System.Uri).Assembly.Location };
            }
        }

        public IList<string> StandardImports
        {
            get
            {
                return new string[] { "System" };
            }
        }

        public string T4_PreProcessedTemplate;
        #endregion

        public bool LoadIncludeText(string requestFileName, out string content, out string location)
        {
            content = "";
            location = "";

            if (File.Exists(requestFileName))
            {
                content = File.ReadAllText(requestFileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetHostOption(string optionName)
        {
            object returnObject;

            switch (optionName)
            {
                case "CacheAssemblies":
                    returnObject = true;
                    break;
                default:
                    returnObject = null;
                    break;
            }

            return returnObject;
        }

        public string ResolveAssemblyReference(string assemblyReference)
        {
            Console.WriteLine("HEY: " + assemblyReference);
                     System.Diagnostics.Debug.WriteLine("HEY: " + assemblyReference);
            

            if (File.Exists(assemblyReference))
            {
                return assemblyReference;
            }

            string candidate = Path.Combine(Path.GetDirectoryName(this.TemplateFile), assemblyReference);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            return "";
        }

        public Type ResolveDirectiveProcessor(string processorName)
        {
            if (string.Compare(processorName, "PropertyProcessor", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return typeof(CustomDirective);
            }

            throw new NotSupportedException(string.Format("Directive processor type of {0} is not type supported.", processorName));
        }

        public string ResolvePath(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("The file name cannot be null.");
            }

            if (File.Exists(fileName))
            {
                return fileName;
            }

            string candidate = Path.Combine(Path.GetDirectoryName(this.TemplateFile), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            return fileName;
        }

        public string ResolveParameterValue(string directiveId, string processorName, string parameterName)
        {
            if (directiveId == null)
            {
                throw new ArgumentNullException("The directiveId cannot be null.");
            }
            if (processorName == null)
            {
                throw new ArgumentNullException("The processorName cannot be null.");
            }
            if (parameterName == null)
            {
                throw new ArgumentNullException("The parameterName cannot be null.");
            }

            return "";
        }

        public void SetFileExtension(string extension)
        {
            _fileExtensionValue = extension;
        }

        public void SetOutputEncoding(System.Text.Encoding encoding, bool fromOutputDirective)
        {
            _fileEncodingValue = encoding;
        }

        public void LogErrors(CompilerErrorCollection errors)
        {
            _errorsValue = errors;
        }

        public AppDomain ProvideTemplatingAppDomain(string content)
        {
            T4_PreProcessedTemplate = content.ToString();

            return _appDomain;
        }
    }
}