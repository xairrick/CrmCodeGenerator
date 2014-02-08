using CrmCodeGenerator.VSPackage.Model;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage.T4
{
    class Processor
    {

        public static string ProcessTemplateCore(string templatePath, Context context)
        {
            // Get the text template service:
            ITextTemplating t4 = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
            ITextTemplatingSessionHost sessionHost = t4 as ITextTemplatingSessionHost;

            // Create a Session in which to pass parameters:
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session["Context"] = context;

            string templateContent = System.IO.File.ReadAllText(templatePath);
            Callback cb = new Callback();

            // Process a text template:
            string result = t4.ProcessTemplate(templatePath, templateContent, cb);

            
            // TODO need to change the file output based on the extenstion defined in the TT template
            //string OutputFullPath;
            //if (!string.IsNullOrWhiteSpace(cb.fileExtension))
            //{
            //    // If there was an output directive in the TemplateFile, then cb.SetFileExtension() will have been called.
            //    OutputFullPath = System.IO.Path.ChangeExtension(templatePath, cb.fileExtension);
            //}
            //else
            //{
            //    OutputFullPath = System.IO.Path.ChangeExtension(templatePath, ".cs");
            //}


            // Append any error messages:
            if (cb.errorMessages.Count > 0)
            {
                result = cb.errorMessages.ToString();
            }
            return result;

        }

        public static string ProcessTemplate(string templatePath, Context context)
        {
            // Get the text template service:
            ITextTemplating t4 = Package.GetGlobalService(typeof(STextTemplating)) as ITextTemplating;
            ITextTemplatingSessionHost sessionHost = t4 as ITextTemplatingSessionHost;

            // Create a Session in which to pass parameters:
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session["Context"] = context;

            string templateContent = System.IO.File.ReadAllText(templatePath);
            Callback cb = new Callback();

            // Process a text template:
            string result = t4.ProcessTemplate(templatePath, templateContent, cb);
            string OutputFullPath;
            if (!string.IsNullOrWhiteSpace(cb.fileExtension))
            {
                // If there was an output directive in the TemplateFile, then cb.SetFileExtension() will have been called.
                OutputFullPath = System.IO.Path.ChangeExtension(templatePath, cb.fileExtension);
            }
            else
            {
                OutputFullPath = System.IO.Path.ChangeExtension(templatePath, ".cs");
            }


            // Write the processed output to file:
            // UpdateStatus("Writing......", true);
            System.IO.File.WriteAllText(OutputFullPath, result, cb.outputEncoding);

            // Append any error messages:
            if (cb.errorMessages.Count > 0)
            {
                System.IO.File.AppendAllLines(OutputFullPath, cb.errorMessages);
            }

            string errroMessage = null;
            if (cb.errorMessages.Count > 0)
            {
                errroMessage = "Unable to generate file see " + OutputFullPath + " for more details ";
            }
            return errroMessage;
        }
    }
}
