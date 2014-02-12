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
        public List<ErrorMessage> ErrorMessages = new List<ErrorMessage>();
        public string FileExtension = ".txt";
        public Encoding OutputEncoding = Encoding.UTF8;

        public void ErrorCallback(bool warning, string message, int line, int column)
        { 
            ErrorMessages.Add(new ErrorMessage(){
                Message = message,
                Warning = warning,
                Line = (uint)line,
                Column = (uint)column
            }); 
        }

        public void SetFileExtension(string extension)
        { FileExtension = extension; }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        { OutputEncoding = encoding; }
    }
    class ErrorMessage
    {
        public string Message { get; set; }
        public uint Line { get; set;}
        public uint Column { get; set; }
        public bool Warning { get; set; }
    }
}
