

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
            return VSConstants.S_OK;
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
                throw new ArgumentException(bstrInputFileContents);

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
                rgbOutputFileContents[0] = IntPtr.Zero;
                pcbOutput = 0;
                return exit;
            }


            if (Configuration.Instance.Settings.Dirty)
            {
                // TODO force save of custom setting in solution 
            }



            //connecting
            if (settings.CrmConnection == null)
            {
                settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
            }

            settings.Context = new CrmCodeGenerator.VSPackage.Model.Context { Namespace = settings.Namespace };  // TODO review namespace, it might be better to use the settings???
            var mapper = new CrmCodeGenerator.VSPackage.Mapper(settings);
            ///UpdateStatus("Mapping....", true);
            var context = mapper.MapContext();

            //UpdateStatus("Generating....", true);
            string content = Processor.ProcessTemplateCore(wszInputFilePath, context);   // TODO convert to just send the bstrInputFileContents

            
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

            System.Windows.MessageBox.Show("Done!!!!!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

            return VSConstants.S_OK;
        }

        #endregion IVsSingleFileGenerator

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



    //// Note: the class name is used as the name of the Custom Tool from the end-user's perspective.
    //[ComVisible(true)]
    //[Guid("BB69ADDB-6AB5-4E29-B263-F918D86D1CC0")]
    //[CodeGeneratorRegistration(typeof(ToUppercase), "Uppercasification!", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    //[CodeGeneratorRegistration(typeof(ToUppercase), "Uppercasification!", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true)]
    //[CodeGeneratorRegistration(typeof(ToUppercase), "Uppercasification!", vsContextGuids.vsContextGuidVJSProject, GeneratesDesignTimeSource = true)]
    ////[ProvideObject(typeof(ToUppercase))]
    //public class ToUppercase : CustomToolBase
    //{
    //    protected override string DefaultExtension()
    //    {
    //        return ".txt";
    //    }
    //    protected override byte[] Generate(string inputFilePath, string inputFileContents,
    //        string defaultNamespace, IVsGeneratorProgress progressCallback)
    //    {
    //        return Encoding.UTF8.GetBytes(inputFileContents.ToUpperInvariant());
    //    }
    //}


    //[ComVisible(true)]
    //[ClassInterface(ClassInterfaceType.None)]
    //[Guid("BB69ADDB-6AB5-4E29-B263-F918D86D1CC0")]
    //[CodeGeneratorRegistration(typeof(MySampleGenerator), "My Sample Generator", new Guid("BB69ADDB-6AB5-4E29-B263-F918D86D1CC0"))]

    //[ProvideObject(typeof(MySampleGenerator), RegisterUsing = RegistrationMethod.CodeBase)]
    //public class MySampleGenerator : IVsSingleFileGenerator
    //{
    //    public int DefaultExtension(out string pbstrDefaultExtension)
    //    {
    //        pbstrDefaultExtension = ".cs";
    //        return pbstrDefaultExtension.Length;
    //    }

    //    public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
    //    {
    //        int lineCount = bstrInputFileContents.Split('\n').Length;

    //        byte[] bytes = Encoding.UTF8.GetBytes(lineCount.ToString());
    //        int length = bytes.Length;

    //        rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
    //        Marshal.Copy(bytes, 0, rgbOutputFileContents[0], length);

    //        pcbOutput = (uint)length;
    //        return VSConstants.S_OK;

    //        throw new Exception("It worked????");
    //    }
    //}
}
