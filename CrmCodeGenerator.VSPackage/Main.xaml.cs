using CrmCodeGenerator.VSPackage;
using CrmCodeGenerator.VSPackage.Model;
using EnvDTE;
using CrmCodeGenerator.VSPackage.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrmCodeGenerator.VSPackage.T4;

namespace CrmCodeGenerator.VSPackage
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : System.Windows.Window
    {
        public EnvDTE80.DTE2 dte { get; set; }
        public EnvDTE.Project project { get; set; }
        private Settings settings;
        public Main(EnvDTE80.DTE2 dte, Project project, Settings settings)
        {
            InitializeComponent();
            this.dte = dte;
            this.project = project;
            this.settings = settings;
            this.DataContext = this.settings;
            settings.PropertyChanged += settings_PropertyChanged;
            settings.EntitiesSelected.CollectionChanged += EntitiesSelected_CollectionChanged;  //BUG This in not firing when the user changes something.

            var defaultTemplatePath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates");
            var dir = new DirectoryInfo(defaultTemplatePath);
            settings.TemplateList = new ObservableCollection<String>(dir.GetFiles().Select(x => x.Name).Where(x => !x.Equals("Blank.tt")).ToArray());

            settings.Folder = project.GetProjectDirectory();
        }

        public string OutputFullPath { get; set; }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            settings.Dirty = true;
            EnableContols(false);

            if (string.IsNullOrWhiteSpace(settings.Namespace))
            {
                settings.Namespace = project.GetDefaultNamespace();
            }
            settings.CrmConnection = null;
            settings.Template = MakeRelative(settings.Template, project.GetProjectDirectory());
            //settings.EntitiesToExclude = this.ExcludeEntities.Text.Split(',');
            //settings.EntitiesToInclude = this.IncludeEntities.Text.Split(',');
            settings.Context = new CrmCodeGenerator.VSPackage.Model.Context { Namespace = settings.Namespace };  // TODO review namespace, it might be better to use the settings???

            UpdateStatus("Saving Template...", true);
            string templatePath = AddTemplateToProject();

            UpdateStatus("Connecting...", true);
            if (settings.CrmConnection == null)
            {
                settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
            }

            var mapper = new CrmCodeGenerator.VSPackage.Mapper(settings);
            UpdateStatus("Mapping....", true);
            var context = mapper.MapContext();

            UpdateStatus("Generating....", true);
            var errorMessage = Processor.ProcessTemplate(templatePath, context);

            AddNewlyCreatedFileToProject();

            if (errorMessage != null)
            {
                throw new UserException(errorMessage);
            }
            UpdateStatus("Done! but you wont see this message, I'm closing...");
            this.Close();

        }

        
        static string MakeRelative(string fromAbsolutePath, string toDirectory)
        {
            if (!System.IO.Path.IsPathRooted(fromAbsolutePath))
                return fromAbsolutePath;  // we can't make a relative if it's not rooted(C:\)  so we'll assume we already have a relative path.

            if (!toDirectory[toDirectory.Length - 1].Equals("\\"))
                toDirectory += "\\";

            System.Uri from = new Uri(fromAbsolutePath);
            System.Uri to = new Uri(toDirectory);

            Uri relativeUri = to.MakeRelativeUri(from);
            return relativeUri.ToString();    // Warning the URI will return slashes /  not backslashes \  (but it does accept backslashes as input)
        }

        private void AddNewlyCreatedFileToProject()
        {
            // Add the newly created file to the project
            var output = MakeRelative(OutputFullPath, project.GetProjectDirectory());
            if (!project.HasProjectItem(output))
            {
                Console.Write("Adding " + output + " to project");
                project.ProjectItems.AddFromFile(OutputFullPath);
            }
        }
        private string AddTemplateToProject()
        {
            var templatePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(project.GetProjectDirectory(), settings.Template));  //GetFullpath removes un-needed relative paths  (ie if you are putting something in the solution directory)
            

            if (!System.IO.File.Exists(templatePath))
            {
                var defaultTemplatePath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\" + this.DefaultTemplate.SelectedValue);
                if (!System.IO.File.Exists(defaultTemplatePath))
                {
                    throw new UserException("T4Path: " + defaultTemplatePath + " is missing or you can access it.");
                }
                UpdateStatus("Copying Template to project....", true);
                var dir = System.IO.Path.GetDirectoryName(templatePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }


                // When you add a TT file to visual studio, it will try to automatically compile it, 
                // if there is error (and there will be error because we have custom generator) 
                // the error will persit until you close Visual Studio. The solution is to add 
                // a blank file, then overwrite it
                // http://stackoverflow.com/questions/17993874/add-template-file-without-custom-tool-to-project-programmatically
                var blankTemplatePath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\Blank.tt");
                System.IO.File.Copy(blankTemplatePath, templatePath);

                Console.Write("Adding " + templatePath + " to project");
                var p = project.ProjectItems.AddFromFile(templatePath);
                p.Properties.SetValue("CustomTool", "");

                System.IO.File.Copy(defaultTemplatePath, templatePath, true);
            }

            // If the project gets out of sync with what on disk, this will correct it.
            if (!project.HasProjectItem(templatePath))
            {
                var p = project.ProjectItems.AddFromFile(templatePath);
                p.Properties.SetValue("CustomTool", "");
            }

            var projectItem = project.GetProjectItem(settings.Template);
            if (projectItem.IsOpen)
                projectItem.Save();

            return templatePath;
        }


        private void EnableContols(bool enable = true)
        {
            this.Inputs.IsEnabled = enable;
            if (enable)
                this.Cursor = Cursors.Arrow;
            else
                this.Cursor = Cursors.Wait;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Logon_Click(object sender, RoutedEventArgs e)
        {
            var origCursor = this.Cursor;
            UpdateStatus("Getting Organizations...", true);

            var prevOrg = this.Organization.Text;

            var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
            var newOrgs = new ObservableCollection<String>(orgs);
            settings.OrgList = newOrgs;


            //this.Organization.Items.Clear();
            //foreach (var o in orgs)
            //{
            //    var index = this.Organization.Items.Add(o);
            //    if (o.Equals(prevOrg, StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        this.Organization.SelectedIndex = index;
            //    }
            //}

            //if (string.IsNullOrWhiteSpace(prevOrg) && this.Organization.Items.Count > 0)
            //{
            //    this.Organization.SelectedIndex = 0;
            //}
            this.Cursor = origCursor;
            UpdateStatus("");
        }

        private void EntitiesRefresh_Click(object sender, RoutedEventArgs events)
        {
            var origCursor = this.Cursor;
            UpdateStatus("Refreshing Entities...", true);

            var connection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);

            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Default,
                RetrieveAsIfPublished = true,
            };
            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)connection.Execute(request);

            var origSelection = settings.EntitiesToIncludeString;
            var newList = new ObservableCollection<string>();
            foreach (var entity in response.EntityMetadata.OrderBy(e => e.LogicalName))
            {
                newList.Add(entity.LogicalName);
            }

            settings.EntityList = newList;
            settings.EntitiesToIncludeString = origSelection;

            this.Cursor = origCursor;
            UpdateStatus("");
        }

        private void UpdateStatus(string message, bool working = false)
        {
            if(working)
                Dispatcher.BeginInvoke(new Action(() => {this.Cursor = Cursors.Wait;}));
                
            Dispatcher.BeginInvoke(new Action(() => { this.Status.Content = message; }));
            System.Windows.Forms.Application.DoEvents();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settings.OrgList.Add(settings.CrmOrg);
            this.Organization.SelectedIndex = 0;
            this.DefaultTemplate.SelectedIndex = 0;
        }

        private void DiscoryURL_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "This is the complete URL to your Discovery Service, you can find this URL in CRM -> Settings -> Customizations -> Developer Resources.  \r \n " + @"https://dsc.doman.com/XRMServices/2011/Discovery.svc";
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "Your username, if you are using CRM OnLine it will be your complete email address \n eg me@mycrm.onmicrosoft.com \n If you are on premise, then you only need your name.  WARNING: until I get this VSPAckage updated the username is saved as plain text in the .sln file";
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "WARNING: Until I get this VSPackage updated the password is saved as plain text in the .sln file";
        }

        private void OutputFile_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "The early bound code will be place in this file and added to the " + project.Name + " project";
        }

        private void IncludeEntities_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "To generate early bound code for only a few entities list the entity by schema name.  separate names with a comma \n \r eg account,contact,systemuser";
        }

        private void ExcludeEntities_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "To exclude specific entities from being generated in the early bound code list the names separate by commas \n \r This option can't be used if you have specified entities to include.";
        }

        private void Namespace_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "The generated code will be put inside this namespace to avoid naming conflicts, if you don't enter a namespace, then the default namespace from your project will be used \n \r eg " + project.GetDefaultNamespace();
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "This tool will automatically generate code that can be used to early bind entity in CRM 2011/2013.\n \r  Click on a field to get help.";
        }

        private void Domain_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "Only needed for OnPrem (non-IFD)";
        }

        private void Organization_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "The CRM organization that you want read entities from, you can type in a value or use the Get Organization to populate a list based on the connection info above.";
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Help.Text = "This file is installed with the VISX you can modify it but it possible that it will get overriden or moved. \n \r I'm still working on making the template part of the project so you always have the template";
        }




        private void EntitiesSelected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SettingChanged("EntitiesSelected");  
        }
        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SettingChanged(e.PropertyName);
        }


        private void SettingChanged(string propertyName)
        {
            UpdateStatus(propertyName);
            switch (propertyName)
            {
                case "CrmSdkUrl":
                case "Username":
                case "Password":
                case "Domain":
                    RefreshOrgInfo();
                    break;
            }
            switch (propertyName)
            {
                case "CrmSdkUrl":
                case "Username":
                case "Password":
                case "Domain":
                case "CrmOrg":
                    RefreshEntities();
                    break;
            }

            if (string.IsNullOrWhiteSpace(settings.CrmSdkUrl)
                || string.IsNullOrWhiteSpace(settings.Username)
                || string.IsNullOrWhiteSpace(settings.Password)
                || string.IsNullOrWhiteSpace(settings.CrmOrg)
                // || string.IsNullOrWhiteSpace(settings.EntitiesToIncludeString)   // BUG can't get the event to fire property change, if we include user could get stuck
                )  
            {
                Dispatcher.BeginInvoke(new Action(() => { this.GenerateSchemButton.IsEnabled = false; }));
            }
            else
            {

                Dispatcher.BeginInvoke(new Action(() => { this.GenerateSchemButton.IsEnabled = true; }));

            }
        }
        #region EntitiesWorker
        private BackgroundWorker entitiesWorker = null;
        private void RefreshEntities()
        {
            if (entitiesWorker == null)
            {
                entitiesWorker = new BackgroundWorker();
                entitiesWorker.DoWork += entitiesWorker_DoWork;
                entitiesWorker.RunWorkerCompleted += entitiesWorker_RunWorkerComplete;
            }
            if (!entitiesWorker.IsBusy
                && !string.IsNullOrWhiteSpace(settings.CrmSdkUrl)
                && !string.IsNullOrWhiteSpace(settings.Username)
                && !string.IsNullOrWhiteSpace(settings.Password)
                && !string.IsNullOrWhiteSpace(settings.CrmOrg))
            {
                UpdateStatus("Getting Entities...");
                entitiesWorker.RunWorkerAsync();
            }
        }
        private void entitiesWorker_DoWork(object sender, DoWorkEventArgs ea)
        {
            try
            {
                var connection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);

                RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Default,
                    RetrieveAsIfPublished = true,
                };
                RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)connection.Execute(request);

                var origSelection = settings.EntitiesToIncludeString;
                var newList = new ObservableCollection<string>();
                foreach (var entity in response.EntityMetadata.OrderBy(e => e.LogicalName))
                {
                    newList.Add(entity.LogicalName);
                }

                settings.EntityList = newList;
                settings.EntitiesToIncludeString = origSelection;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.GetType() + " " + ex.Message);
            }
        }
        private void entitiesWorker_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateStatus("Getting Entities...Done!");
        }
        #endregion
        #region OrgWorker
        private BackgroundWorker orgWorker = null;
        private void RefreshOrgInfo()
        {
            if (orgWorker == null)
            {
                orgWorker = new BackgroundWorker();
                orgWorker.DoWork += orgWorker_DoWork;
                orgWorker.RunWorkerCompleted += orgWorker_RunWorkerComplete;
            }
            if (!orgWorker.IsBusy
                && !string.IsNullOrWhiteSpace(settings.CrmSdkUrl)
                && !string.IsNullOrWhiteSpace(settings.Username)
                && !string.IsNullOrWhiteSpace(settings.Password))
            {
                UpdateStatus("Getting Organizations...");
                orgWorker.RunWorkerAsync();
            }
        }
        private void orgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
                var newOrgs = new ObservableCollection<String>(orgs);
                settings.OrgList = newOrgs;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.GetType() + " " + ex.Message);
            }
        }
        private void orgWorker_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateStatus("Getting Organizations...Done!");
        }
        #endregion
    }
}
