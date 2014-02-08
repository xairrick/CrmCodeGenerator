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
    public sealed class CrmCodeGenerator_VSPackagePackage : Package, IVsPersistSolutionProps, IVsSolutionEvents3
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
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidCrmCodeGenerator_VSPackageCmdSet, (int)PkgCmdIDList.cmdidCrmCodeGenerator);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);


                CommandID templateCmd = new CommandID(GuidList.guidCrmCodeGenerator_VSPackageCmdSet, (int)PkgCmdIDList.cmdidAddTemplate);
                MenuCommand tempalteItem = new MenuCommand(AddTemplateCallback, templateCmd);
                mcs.AddCommand(tempalteItem);

            }
            DefaultSettings();
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

        public bool SolutionHasDirtyProps { get; set; }
        //public Microsoft.Xrm.Sdk.IOrganizationService CachedCrmConnection { get; set; }
        //public static CrmCodeGenerator.VSPackage.Model.Settings Settings { get; set; }
        private const string _strSolutionPersistanceKey = "CrmCodeGeneration";
        private const string _strSolutionBindingsProperty = "t4path";


        //TODO instead of magic strings, but an attribute on props or reflection on props
        private const string _strProjectName = "ProjectName";
        private const string _strCrmUrl = "CrmUrl";
        private const string _strDomain = "Domain";
        private const string _strT4Path = "T4Path";
        private const string _strTemplate = "Template";
        private const string _strOrganization = "Organization";
        private const string _strExcludeEntities = "ExcludeEntities";
        private const string _strIncludeEntities = "IncludeEntities";
        private const string _strOutputFile = "OutputFile";
        private const string _strUsername = "Username";
        private const string _strPassword = "Password";
        private const string _strNamespace = "Namespace";

        private void DefaultSettings()
        {
            //Settings = new CrmCodeGenerator.VSPackage.Model.Settings();
            //Settings.CrmSdkUrl = @"https://dscdev.benco.com/XRMServices/2011/Discovery.svc";
            //Settings.ProjectName = "";
            //Settings.Domain = "";
            //Settings.T4Path = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\CrmSvcUtil.tt");
            //Settings.Template = "";
            //Settings.CrmOrg = "DEV-CRM";
            ////Settings.EntitiesToExcludeString = "";
            //Settings.EntitiesToIncludeString = "account, contact, systemuser";
            //Settings.OutputPath = "";
            //Settings.Username = "";
            //Settings.Password = "";
            //Settings.Namespace = "";
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

            // Once we saved our props, the solution is not dirty anymore
            SolutionHasDirtyProps = false;

            return VSConstants.S_OK;
        }

        public int WriteSolutionProps([InAttribute] IVsHierarchy pHierarchy, [InAttribute] string pszKey, [InAttribute] IPropertyBag pPropBag)
        {
            //object obj = CrmUrl;
            //pPropBag.Write(_strCrmUrlKey, ref obj);
            //string strSolutionLocation = "\"This is the second key\"";
            //obj = strSolutionLocation;
            //pPropBag.Write(_strSolutionBindingsProperty, ref obj);
            pPropBag.Write(_strProjectName, Configuration.Instance.Settings.ProjectName.ToString());
            pPropBag.Write(_strCrmUrl, Configuration.Instance.Settings.CrmSdkUrl);
            pPropBag.Write(_strDomain, Configuration.Instance.Settings.Domain);
            pPropBag.Write(_strT4Path, Configuration.Instance.Settings.T4Path);
            pPropBag.Write(_strTemplate, Configuration.Instance.Settings.Template);
            pPropBag.Write(_strOrganization, Configuration.Instance.Settings.CrmOrg);
            //pPropBag.Write(_strExcludeEntities, Settings.EntitiesToExcludeString);
            pPropBag.Write(_strIncludeEntities, Configuration.Instance.Settings.EntitiesToIncludeString);
            pPropBag.Write(_strOutputFile, Configuration.Instance.Settings.OutputPath);
            // TODO move these two to the .sou file 
            pPropBag.Write(_strUsername, Configuration.Instance.Settings.Username);
            pPropBag.Write(_strPassword, Configuration.Instance.Settings.Password);
            pPropBag.Write(_strNamespace, Configuration.Instance.Settings.Namespace);

            //pPropBag.Write(_strSolutionBindingsProperty, "\"This is the second key\"");

            return VSConstants.S_OK;
        }

        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }

        public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        {
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }

        public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
        {
            if (pHierarchy != null)  //if not null, then it's asking to save properties for solution item eg project.  for now we are only save to the solution
            {
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasNoProps;
            }
            else if (SolutionHasDirtyProps)
            {
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
            }
            else
            {
                pqsspSave[0] = VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps;
            }
            return VSConstants.S_OK;
        }

        public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
        {
            if (_strSolutionPersistanceKey.CompareTo(pszKey) == 0)
            {
                // Now we can read all the data and store it in memory
                // The read data will be used when the solution has completed opening
                //object pVar;
                //pPropBag.Read(_strCrmUrlKey, out pVar, null, 0, null);
                //CrmUrl = pVar as string;
                Configuration.Instance.Settings.ProjectName = pPropBag.Read(_strProjectName, "");
                Configuration.Instance.Settings.CrmSdkUrl = pPropBag.Read(_strCrmUrl, @"https://dscdev.benco.com/XRMServices/2011/Discovery.svc");
                Configuration.Instance.Settings.Domain = pPropBag.Read(_strDomain, "");
                Configuration.Instance.Settings.T4Path = pPropBag.Read(_strT4Path, System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\CrmSvcUtil.tt"));  // TODO this doesn't need to be stored in the propbag
                Configuration.Instance.Settings.Template = pPropBag.Read(_strTemplate, "");
                Configuration.Instance.Settings.CrmOrg = pPropBag.Read(_strOrganization, "DEV-CRM");
                //Settings.EntitiesToExcludeString = pPropBag.Read(_strExcludeEntities, "");
                Configuration.Instance.Settings.EntitiesToIncludeString = pPropBag.Read(_strIncludeEntities, "account, contact, systemuser");
                Configuration.Instance.Settings.OutputPath = pPropBag.Read(_strOutputFile, "");
                Configuration.Instance.Settings.Username = pPropBag.Read(_strUsername, "");
                Configuration.Instance.Settings.Password = pPropBag.Read(_strPassword, "");
                Configuration.Instance.Settings.Namespace = pPropBag.Read(_strNamespace, "");   // This will get defaulted to the project if the user doesn't enter anything the first it's executed.
            }
            return VSConstants.S_OK;
        }

        public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        {
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }

        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }

        public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        {
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }


        #endregion



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

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs args)
        {
            //var dte = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;
            try
            {
                OpenWindow();
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
            Project project = dte2.GetProject(Configuration.Instance.Settings.ProjectName);

            if (project == null)
            {
                project = dte2.GetSelectedProject();
                if (project == null || string.IsNullOrWhiteSpace(project.FullName))
                {
                    throw new UserException("Please select a project first");
                }
                Configuration.Instance.Settings.ProjectName = project.UniqueName;
            }

            var m = new AddTemplate(dte2, project);
            m.ShowDialog();
            m.Close();
            m = null;
        }


        private void OpenWindow()
        {
            var dte2 = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;
            Project project = dte2.GetProject(Configuration.Instance.Settings.ProjectName);

            if (project == null)
            {
                project = dte2.GetSelectedProject();
                if (project == null || string.IsNullOrWhiteSpace(project.FullName))
                {
                    System.Windows.MessageBox.Show("Please select a project first", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                Configuration.Instance.Settings.ProjectName = project.UniqueName;
            }

            if (string.IsNullOrWhiteSpace(Configuration.Instance.Settings.OutputPath))
            {
                Configuration.Instance.Settings.OutputPath = System.IO.Path.Combine(project.GetProjectDirectory(), "CrmSchema.cs");
            }
            if (string.IsNullOrWhiteSpace(Configuration.Instance.Settings.Template))
            {
                Configuration.Instance.Settings.Template = System.IO.Path.Combine(project.GetProjectDirectory(), "CrmSchema.tt");
            }

            var m = new Main(dte2, project, Configuration.Instance.Settings);
            m.ShowDialog();
            if (Configuration.Instance.Settings.Dirty)   
            {
                SolutionHasDirtyProps = true;  // force save of custom setting in solution 
            }
            m.Close();
            m = null;
        }

        #region SolutionEvents
        public int OnAfterCloseSolution(object pUnkReserved)
        {
            DefaultSettings();
            return VSConstants.S_OK;
        }
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
