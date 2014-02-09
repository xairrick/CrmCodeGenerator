

using System;

using System.Collections.Generic;
using System.Linq;

using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
//using VSLangProj80;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using CrmCodeGenerator.VSPackage.Model;
using CrmCodeGenerator.VSPackage.T4;
using CrmCodeGenerator.VSPackage.Dialogs;

namespace CrmCodeGenerator.VSPackage
{
    public static class vsContextGuids
    {
        public const string vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
        public const string vsContextGuidVCSEditor = "{694DD9B6-B865-4C5B-AD85-86356E9C88DC}";
        public const string vsContextGuidVBProject = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";
        public const string vsContextGuidVBEditor = "{E34ACDC0-BAAE-11D0-88BF-00A0C9110049}";
        public const string vsContextGuidVJSProject = "{E6FDF8B0-F3D1-11D4-8576-0002A516ECE8}";
        public const string vsContextGuidVJSEditor = "{E6FDF88A-F3D1-11D4-8576-0002A516ECE8}";
    }


    // http://blogs.msdn.com/b/vsx/archive/2013/11/27/building-a-vsix-deployable-single-file-generator.aspx
    [ComVisible(true)]
    [Guid(GuidList.guidCrmCodeGenerator_SimpleGenerator)]
    [ProvideObject(typeof(CrmCodeGenerator2011))]
    [CodeGeneratorRegistration(typeof(CrmCodeGenerator2011), "CrmCodeGenerator2011", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(CrmCodeGenerator2011), "CrmCodeGenerator2011", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true)]
    public class CrmCodeGenerator2011 : IVsSingleFileGenerator, IObjectWithSite, IDisposable
    {
        private object site = null;
        private CodeDomProvider codeDomProvider = null;
        private ServiceProvider serviceProvider = null;
        private Settings settings = Configuration.Instance.Settings;
        private String extension = null;
        private Context context = null;

        private CodeDomProvider CodeProvider
        {
            get
            {
                if (codeDomProvider == null)
                {
                    IVSMDCodeDomProvider provider = (IVSMDCodeDomProvider)SiteServiceProvider.GetService(typeof(IVSMDCodeDomProvider).GUID);
                    if (provider != null)
                        codeDomProvider = (CodeDomProvider)provider.CodeDomProvider;
                }
                return codeDomProvider;
            }
        }

        private ServiceProvider SiteServiceProvider
        {
            get
            {
                if (serviceProvider == null)
                {
                    IOleServiceProvider oleServiceProvider = site as IOleServiceProvider;
                    serviceProvider = new ServiceProvider(oleServiceProvider);
                }
                return serviceProvider;
            }
        }

        public void Dispose()
        {
            if (codeDomProvider != null)
            {
                codeDomProvider.Dispose();
                codeDomProvider = null;
            }
            if (serviceProvider != null)
            {
                serviceProvider.Dispose();
                serviceProvider = null;
            }
        }

        #region IVsSingleFileGenerator

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = "." + CodeProvider.FileExtension;
            if (extension != null)
                pbstrDefaultExtension = extension;
            return VSConstants.S_OK;
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
                throw new ArgumentException(bstrInputFileContents);

            ClearStatus();

            PromptToRefreshEntities();

            if (context == null)
            {
                UpdateStatus("In order to generate code from this template, you need to provide login credentials for your CRM system");
                UpdateStatus("The Discovery URL is the URL to your Discovery Service, you can find this URL in CRM -> Settings -> Customizations -> Developer Resources.  \n    eg " + @"https://dsc.yourdomain.com/XRMServices/2011/Discovery.svc");

                int exit = 0;
                try
                {
                    var m = new Login(settings);
                    var result = m.ShowDialog();
                    if (result == false)
                        exit = 1;
                }
                catch (UserException e)
                {
                    VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, e.Message, "Error", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    exit = 1;
                }
                catch (Exception e)
                {
                    var error = e.Message + "\n" + e.StackTrace;
                    System.Windows.MessageBox.Show(error, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    exit = 1;
                }

                if (exit > 0)
                {
                    pGenerateProgress.GeneratorError(1, (uint)1, "Code generation for CRM Template aborted", uint.MaxValue, uint.MaxValue);
                    // http://social.msdn.microsoft.com/Forums/vstudio/en-US/d8d72da3-ddb9-4811-b5da-2a167bbcffed/ivssinglefilegenerator-cancel-code-generation
                    // I don't think a login failure would be considered a invalid model.
                    // TODO read in original file and pass it back  (the extension need to be pulled from the template, so we need to process the template to see if the user specified a extension)
                    rgbOutputFileContents[0] = IntPtr.Zero;
                    pcbOutput = 0;
                    return exit;
                }

                UpdateStatus("Connecting... ");
                if (settings.CrmConnection == null)
                {
                    settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
                }

                UpdateStatus("Mapping entities, this might take a while depending on CRM server/connection speed... ");
                settings.Context = new Context { Namespace = wszDefaultNamespace };
                var mapper = new Mapper(settings);
                context = mapper.MapContext();
            }

            UpdateStatus("Generating code from template... ");
            pGenerateProgress.Progress(40, 100);
            string content = Processor.ProcessTemplateCore(wszInputFilePath, bstrInputFileContents, context, out extension);

            UpdateStatus("Writing code to disk... ");
            pGenerateProgress.Progress(50, 100);
            byte[] bytes = Encoding.UTF8.GetBytes(content);

            if (bytes == null)
            {
                rgbOutputFileContents[0] = IntPtr.Zero;
                pcbOutput = 0;
            }
            else
            {
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], bytes.Length);
                pcbOutput = (uint)bytes.Length;
            }

            UpdateStatus("Done!");

            return VSConstants.S_OK;
        }

        private void PromptToRefreshEntities()
        {
            if (context == null)
                return;

            var results = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, "Do you want to refresh the CRM Entities from the Server?", "Overwrite", OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            if (results == 6)
                context = null;
        }
        #endregion IVsSingleFileGenerator

        private void UpdateStatus(string message)
        {
            //Configuration.Instance.DTE.ExecuteCommand("View.Output");
            var dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
            var win = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            win.Visible = true;


            //System.Windows.Forms.Application.DoEvents();

            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "General", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);
            pane.Activate();
            pane.OutputString(message);
            pane.OutputString("\n");
            pane.FlushToTaskList();
            System.Windows.Forms.Application.DoEvents();
        }
        private void ClearStatus()
        {
            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "General", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);
            pane.Clear();
            pane.FlushToTaskList();
            System.Windows.Forms.Application.DoEvents();
        }
        #region IObjectWithSite

        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (site == null)
                Marshal.ThrowExceptionForHR(VSConstants.E_NOINTERFACE);

            // Query for the interface using the site object initially passed to the generator 
            IntPtr punk = Marshal.GetIUnknownForObject(site);
            int hr = Marshal.QueryInterface(punk, ref riid, out ppvSite);
            Marshal.Release(punk);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
        }

        public void SetSite(object pUnkSite)
        {
            // Save away the site object for later use 
            site = pUnkSite;

            // These are initialized on demand via our private CodeProvider and SiteServiceProvider properties 
            codeDomProvider = null;
            serviceProvider = null;
        }

        #endregion IObjectWithSite
    }
}
