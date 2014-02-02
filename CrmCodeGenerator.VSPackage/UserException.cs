using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Labshosky.CrmCodeGenerator_VSPackage
{
    public class UserException : Exception
    {
        public UserException(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }
}
