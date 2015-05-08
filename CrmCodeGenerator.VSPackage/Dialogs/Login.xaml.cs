using CrmCodeGenerator.VSPackage.Helpers;
using CrmCodeGenerator.VSPackage.Model;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrmCodeGenerator.VSPackage.Dialogs
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        
        public Context Context;
        Settings settings;
        private EntityMetadata[] _AllEntities;
        private bool _StillOpen = true;
        public bool StillOpen
        {
            get
            {
                return _StillOpen;
            }
        }
        public Login(EnvDTE80.DTE2 dte, Settings settings)
        {
            WifDetector.CheckForWifInstall();
            InitializeComponent();

            var main = dte.GetMainWindow();
            this.Owner = main;
            //Loaded += delegate  { this.CenterWindow(main); };

            this.settings = settings;
            this.txtPassword.Password = settings.Password;  // PasswordBox doesn't allow 2 way binding
            this.DataContext = settings;
         
        }

        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HideMinimizeAndMaximizeButtons();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatus("In order to generate code from this template, you need to provide login credentials for your CRM system", null);
            UpdateStatus("The Discovery URL is the URL to your Discovery Service, you can find this URL in CRM -> Settings -> Customizations -> Developer Resources.  \n    eg " + @"https://dsc.yourdomain.com/XRMServices/2011/Discovery.svc", null);
            if (settings.OrgList.Contains(settings.CrmOrg) == false)
            {
                settings.OrgList.Add(settings.CrmOrg);
            }
            this.Organization.SelectedItem = settings.CrmOrg;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _StillOpen = false;
            this.Close();
        }


        private void RefreshOrgs(object sender, RoutedEventArgs e)
        {
            settings.Password = ((PasswordBox)((Button)sender).CommandParameter).Password;  // PasswordBox doesn't allow 2 way binding, so we have to manually read it
            UpdateStatus("Refreshing Orgs", true);
            try
            {
                //var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
                //var newOrgs = new ObservableCollection<String>(orgs);
                //settings.OrgList = newOrgs;

                var newOrgs = ConnectionHelper.GetOrgList(settings);
                settings.OrgList = newOrgs;
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error, false);
                UpdateStatus("Unable to refresh organizations, check connection information", false);
            }

            UpdateStatus("", false);
        }

        private void EntitiesRefresh_Click(object sender, RoutedEventArgs events)
        {
            settings.Password = ((PasswordBox)((Button)sender).CommandParameter).Password;  // PasswordBox doesn't allow 2 way binding, so we have to manually read it

            UpdateStatus("Refreshing Entities...", true);

            RefreshEntityList();

            UpdateStatus("", false);
        }
        
        private void RefreshEntityList()
        {
            Update_AllEntities();
            if (_AllEntities == null)
            {
                return;
            }

            var entities = _AllEntities.Where(e =>
                            {
                                if (settings.IncludeNonStandard)
                                    return true;
                                else
                                    return !EntityHelper.NonStandard.Contains(e.LogicalName);
                            });

            var origSelection = settings.EntitiesToIncludeString;
            var newList = new ObservableCollection<string>();
            foreach (var entity in entities.OrderBy(e => e.LogicalName))
            {
                newList.Add(entity.LogicalName);
            }

            settings.EntityList = newList;
            settings.EntitiesToIncludeString = origSelection;
        }
        private void Update_AllEntities()
        {
            try
            {
                var connString = Microsoft.Xrm.Client.CrmConnection.Parse(settings.GetOrganizationCrmConnectionString());
                var connection = new Microsoft.Xrm.Client.Services.OrganizationService(connString);

                RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Default,
                    RetrieveAsIfPublished = settings.IncludeUnpublish,
                };
                RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)connection.Execute(request);
                _AllEntities = response.EntityMetadata;
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error,false);
                UpdateStatus("Unable to refresh entities, check connection information", false);
            }

        }

        private void IncludeNonStandardEntities_Click(object sender, RoutedEventArgs e)
        {
            if(_AllEntities != null)
                RefreshEntityList();  // if we don't have the entire list of entities don't do anything (eg if they havn't entered a username & password)
        }

        private void Logon_Click(object sender, RoutedEventArgs e)
        {
            settings.Password = ((PasswordBox)((Button)sender).CommandParameter).Password;   // PasswordBox doesn't allow 2 way binding, so we have to manually read it
            UpdateStatus("Logging in to CRM...", true);
            try
            {

                var connection = Microsoft.Xrm.Client.CrmConnection.Parse(settings.GetOrganizationCrmConnectionString());
                settings.CrmConnection = new Microsoft.Xrm.Client.Services.OrganizationService(connection);
                // TODO remove the QuickConnection class -->  settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
                if (settings.CrmConnection == null)
                     throw new UserException("Unable to login to CRM, check to ensure you have the right organization");
                
                UpdateStatus("Mapping entities, this might take a while depending on CRM server/connection speed... ", true);
                var mapper = new Mapper(settings);
                Context = mapper.MapContext();

                settings.Dirty = true;  //  TODO Because the EntitiesSelected is a collection, the Settings class can't see when an item is added or removed.  when I have more time need to get the observable to bubble up.
                _StillOpen = false;
                this.Close();
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error, false);
                UpdateStatus(ex.StackTrace,false);
                UpdateStatus("Unable to map entities, see error above.",false);
            }
            UpdateStatus("", false);
        }
        private void UpdateStatus(string message, bool? working)
        {
            if (working == true)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Cursor = Cursors.Wait;
                    Inputs.IsEnabled = false;
                }));
            }
            if (working == false)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Cursor = null;
                    Inputs.IsEnabled = true;
                }));
            }

            if(!string.IsNullOrWhiteSpace(message))
            {
                Dispatcher.BeginInvoke(new Action(() => { Status.Update(message); }));
            }

            System.Windows.Forms.Application.DoEvents();  // Needed to allow the output window to update (also allows the cursor wait and form disable to show up)
        }
    }
}
