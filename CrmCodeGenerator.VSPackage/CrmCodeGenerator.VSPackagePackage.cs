using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.VisualStudio.TextTemplating;
using CrmCodeGenerator.VSPackage.Helpers;
using CrmCodeGenerator.VSPackage.Dialogs;
using CrmCodeGenerator.VSPackage.Model;

namespace CrmCodeGenerator.VSPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    //this causes the class to load when VS starts [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCrmCodeGenerator_VSPackagePkgString)]
    [ProvideSolutionProps(_strSolutionPersistanceKey)]
    public sealed class CrmCodeGenerator_VSPackagePackage : Package, IVsPersistSolutionProps, IVsPersistSolutionOpts, IVsSolutionEvents3
    {


        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CrmCodeGenerator_VSPackagePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID templateCmd = new CommandID(GuidList.guidCrmCodeGenerator_VSPackageCmdSet, (int)PkgCmdIDList.cmdidAddTemplate);
                MenuCommand tempalteItem = new MenuCommand(AddTemplateCallback, templateCmd);
                mcs.AddCommand(tempalteItem);
            }
            Configuration.Instance.DTE = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;
            AdviseSolutionEvents();

        }

        protected override void Dispose(bool disposing)
        {
            UnadviseSolutionEvents();

            base.Dispose(disposing);
        }

        private IVsSolution solution = null;
        private uint _handleCookie;
        private void AdviseSolutionEvents()
        {
            UnadviseSolutionEvents();

            solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out _handleCookie);
            }
        }

        private void UnadviseSolutionEvents()
        {
            if (solution != null)
            {
                if (_handleCookie != uint.MaxValue)
                {
                    solution.UnadviseSolutionEvents(_handleCookie);
                    _handleCookie = uint.MaxValue;
                }

                solution = null;
            }
        }

        #endregion



        #region IVsPersistSolutionProps Implementation Code

        private Settings settings = Configuration.Instance.Settings;

        private const string _strSolutionPersistanceKey = "CrmCodeGeneration";
        private const string _strCrmUrl = "CrmUrl";
        private const string _strUseSSL = "UseSSL";
        private const string _strUseIFD = "UseIFD";
        private const string _strUseOnline = "UseOnline";
        private const string _strUseOffice365 = "UseOffice365";
        private const string _strServerPort = "ServerPort";
        private const string _strServerName = "ServerName";
        private const string _strHomeRealm = "HomeRealm";
        private const string _strUsername = "Username";
        private const string _strPassword = "Password";
        private const string _strDomain = "Domain";
        private const string _strOrganization = "Organization";
        private const string _strIncludeEntities = "IncludeEntities";
        private const string _strIncludeNonStandard = "IncludeNonStandard";

        #region Solution Properties
        public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
        {
            if (pHierarchy != null)   // if this contains something, then VS is asking for Solution Properties of a PROJECT,  
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasNoProps;
            else if (settings.Dirty)
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
            else
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps;
            return VSConstants.S_OK;
        }
        public int SaveSolutionProps([InAttribute] IVsHierarchy pHierarchy, [InAttribute] IVsSolutionPersistence pPersistence)
        {
            // This function gets called by the shell after determining the package has dirty props.
            // The package will pass in the key under which it wants to save its properties, 
            // and the IDE will call back on WriteSolutionProps

            // The properties will be saved in the Pre-Load section
            // When the solution will be reopened, the IDE will call our package to load them back before the projects in the solution are actually open
            // This could help if the source control package needs to persist information like projects translation tables, that should be read from the suo file
            // and should be available by the time projects are opened and the shell start calling IVsSccEnlistmentPathTranslation functions.
            pPersistence.SavePackageSolutionProps(1, null, this, _strSolutionPersistanceKey);
            settings.Dirty = false;

            return VSConstants.S_OK;
        }
        public int WriteSolutionProps([InAttribute] IVsHierarchy pHierarchy, [InAttribute] string pszKey, [InAttribute] IPropertyBag pPropBag)
        {
            pPropBag.WriteBool(_strUseSSL, settings.UseSSL);
            pPropBag.WriteBool(_strUseIFD, settings.UseIFD);
            pPropBag.WriteBool(_strUseOnline, settings.UseOnline);
            pPropBag.WriteBool(_strUseOffice365, settings.UseOffice365);
            pPropBag.WriteBool(_strUseOffice365, settings.UseOffice365);
            
            pPropBag.Write(_strServerName, settings.ServerName);
            pPropBag.Write(_strServerPort, settings.ServerPort);
            pPropBag.Write(_strHomeRealm, settings.HomeRealm);

            pPropBag.Write(_strCrmUrl, settings.CrmSdkUrl);
            pPropBag.Write(_strDomain, settings.Domain);
            //pPropBag.Write(_strUsername, settings.Username);
            //pPropBag.Write(_strPassword, settings.Password);
            
            pPropBag.Write(_strOrganization, settings.CrmOrg);
            pPropBag.Write(_strIncludeEntities, settings.EntitiesToIncludeString);
            pPropBag.WriteBool(_strIncludeNonStandard, settings.IncludeNonStandard);
            settings.Dirty = false;

            return VSConstants.S_OK;
        }
        public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
        {
            if (_strSolutionPersistanceKey.CompareTo(pszKey) == 0)
            {
                settings.UseSSL = pPropBag.Read(_strUseSSL, false);
                settings.UseIFD = pPropBag.Read(_strUseIFD, false);
                settings.UseOnline = pPropBag.Read(_strUseOnline, true);
                settings.UseOffice365 = pPropBag.Read(_strUseOffice365, true);

                settings.ServerName = pPropBag.Read(_strServerName, "crm.dynamics.com");
                settings.ServerPort = pPropBag.Read(_strServerPort, "");
                settings.HomeRealm = pPropBag.Read(_strHomeRealm, "");

                settings.CrmSdkUrl = pPropBag.Read(_strCrmUrl, @"https://dscdev.benco.com/XRMServices/2011/Discovery.svc");
                //settings.Username = pPropBag.Read(_strUsername, "");
                //settings.Password = pPropBag.Read(_strPassword, "");
                settings.Domain = pPropBag.Read(_strDomain, "");
                settings.CrmOrg = pPropBag.Read(_strOrganization, "DEV-CRM");
                settings.EntitiesToIncludeString = pPropBag.Read(_strIncludeEntities, "account, contact, systemuser");

                settings.IncludeNonStandard = pPropBag.Read(_strIncludeNonStandard, false);

                settings.Dirty = false;
            }
            return VSConstants.S_OK;
        }
        #endregion
        #region User Options
        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            pPersistence.LoadPackageUserOpts(this, _strSolutionPersistanceKey + _strUsername);
            pPersistence.LoadPackageUserOpts(this, _strSolutionPersistanceKey + _strPassword);
            return VSConstants.S_OK;
        }
        public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        {
            try
            {
                using (StreamEater wrapper = new StreamEater(pOptionsStream))
                {
                    string value;
                    using (var bReader = new System.IO.BinaryReader(wrapper))
                    {
                        value = bReader.ReadString();
                        using (var aes = new SimpleAES())
                        {
                            value = aes.Decrypt(value);
                        }
                    }

                    switch (pszKey)
                    {
                        case _strSolutionPersistanceKey + _strUsername:
                            settings.Username = value;
                            break;
                        case _strSolutionPersistanceKey + _strPassword:
                            settings.Password = value;
                            break;
                        default:
                            break;
                    }
                }
                return VSConstants.S_OK;
            }
            finally
            {
                Marshal.ReleaseComObject(pOptionsStream);
            }
        }

        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            pPersistence.SavePackageUserOpts(this, _strSolutionPersistanceKey + _strUsername);
            pPersistence.SavePackageUserOpts(this, _strSolutionPersistanceKey + _strPassword);
            return VSConstants.S_OK;
        }

        public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        {
            try
            {
                string value;
                switch (pszKey)
                {
                    case _strSolutionPersistanceKey + _strUsername:
                        value = settings.Username;
                        break;
                    case _strSolutionPersistanceKey + _strPassword:
                        value = settings.Password;
                        break;
                    default:
                        return VSConstants.S_OK;
                }

                using (var aes = new SimpleAES())
                {
                    value = aes.Encrypt(value);
                    using (StreamEater wrapper = new StreamEater(pOptionsStream))
                    {
                        using (var bw = new System.IO.BinaryWriter(wrapper))
                        {
                            bw.Write(value);
                        }
                    }
                }
                return VSConstants.S_OK;
            }
            finally
            {
                Marshal.ReleaseComObject(pOptionsStream);
            }
        }
        #endregion
        public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        {
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void AddTemplateCallback(object sender, EventArgs args)
        {
            try
            {
                AddTemplate();
            }
            catch (UserException e)
            {
                VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, e.Message, "Error", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            catch (Exception e)
            {
                var error = e.Message + "\n" + e.StackTrace;
                System.Windows.MessageBox.Show(error, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AddTemplate()
        {
            var dte2 = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;

            var project = dte2.GetSelectedProject();
            if (project == null || string.IsNullOrWhiteSpace(project.FullName))
            {
                throw new UserException("Please select a project first");
            }

            var m = new AddTemplate(dte2, project);
            m.Closed += (sender, e) =>
            {
                // logic here Will be called after the child window is closed
                if (((AddTemplate)sender).Canceled == true)
                    return;

                var templatePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(project.GetProjectDirectory(), m.Props.Template));  //GetFullpath removes un-needed relative paths  (ie if you are putting something in the solution directory)

                if (System.IO.File.Exists(templatePath))
                {
                    var results = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, "'" + templatePath + "' already exists, are you sure you want to overwrite?", "Overwrite", OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    if (results != 6)
                        return;

                    //if the window is open we have to close it before we overwrite it.
                    var pi = project.GetProjectItem(m.Props.Template);
                    if(pi != null && pi.Document != null)
                        pi.Document.Close(vsSaveChanges.vsSaveChangesNo);
                }

                var templateSamplesPath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates");
                var defaultTemplatePath = System.IO.Path.Combine(templateSamplesPath, m.DefaultTemplate.SelectedValue.ToString());
                if (!System.IO.File.Exists(defaultTemplatePath))
                {
                    throw new UserException("T4Path: " + defaultTemplatePath + " is missing or you can access it.");
                }

                var dir = System.IO.Path.GetDirectoryName(templatePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                Status.Update("Adding " + templatePath + " to project");
                // When you add a TT file to visual studio, it will try to automatically compile it, 
                // if there is error (and there will be error because we have custom generator) 
                // the error will persit until you close Visual Studio. The solution is to add 
                // a blank file, then overwrite it
                // http://stackoverflow.com/questions/17993874/add-template-file-without-custom-tool-to-project-programmatically
                var blankTemplatePath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\Blank.tt");
                System.IO.File.Copy(blankTemplatePath, templatePath, true);

                var p = project.ProjectItems.AddFromFile(templatePath);
                p.Properties.SetValue("CustomTool", "");

                System.IO.File.Copy(defaultTemplatePath, templatePath, true);
                p.Properties.SetValue("CustomTool", typeof(CrmCodeGenerator2011).Name);
            };
            m.ShowModal();
        }

        #region SolutionEvents
        public int OnAfterCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }
        public int OnAfterClosingChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) { return VSConstants.S_OK; }
        public int OnAfterMergeSolution(object pUnkReserved) { return VSConstants.S_OK; }
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) { return VSConstants.S_OK; }
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) { return VSConstants.S_OK; }
        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) { return VSConstants.S_OK; }
        public int OnBeforeCloseSolution(object pUnkReserved) { return VSConstants.S_OK; }
        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy) { return VSConstants.S_OK; }
        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) { return VSConstants.S_OK; }
        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) { return VSConstants.S_OK; }
        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) { return VSConstants.S_OK; }
        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) { return VSConstants.S_OK; }
        #endregion
    }
}
