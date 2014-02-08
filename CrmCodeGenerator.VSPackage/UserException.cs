using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmCodeGenerator.VSPackage
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
