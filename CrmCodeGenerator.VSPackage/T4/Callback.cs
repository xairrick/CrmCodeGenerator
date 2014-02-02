using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CrmCodeGenerator.VSPackage.T4
{
    class Callback : ITextTemplatingCallback
    {
        public List<string> errorMessages = new List<string>();
        public string fileExtension = ".txt";
        public Encoding outputEncoding = Encoding.UTF8;

        public void ErrorCallback(bool warning, string message, int line, int column)
        { errorMessages.Add(message); }

        public void SetFileExtension(string extension)
        { fileExtension = extension; }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        { outputEncoding = encoding; }
    }
}
