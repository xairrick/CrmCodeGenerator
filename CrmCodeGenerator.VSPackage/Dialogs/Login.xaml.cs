using CrmCodeGenerator.VSPackage.Helpers;
using CrmCodeGenerator.VSPackage.Model;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CrmCodeGenerator.VSPackage.Dialogs
{


   

    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Context Context;
        Settings settings;
        private EntityMetadata[] _AllEntities;
        public Login(EnvDTE80.DTE2 dte, Settings settings)
        {
            InitializeComponent();

            var main = dte.GetMainWindow();
            Loaded += delegate
            {
                double newLeft = main.Left + (main.Width / 2) - (this.Width / 2);
                double newTop = main.Top + (main.Height / 2) - (this.Height / 2);
                this.Left = newLeft;
                this.Top = newTop;
                if (main.WindowState == System.Windows.WindowState.Maximized)
                {
                    var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)this.Left, (int)this.Top));

                    var left = screen.WorkingArea.Left;
                    var top = screen.WorkingArea.Top;
                    var width = screen.WorkingArea.Width;
                    var height = screen.WorkingArea.Height;

                    newLeft = left + (width / 2) - (this.Width / 2);
                    newTop = top + (height / 2) - (this.Height / 2);

                    UpdateStatus(left + " " + width + " " + this.Width + " = " + newLeft);
                    UpdateStatus(top + " " + height + " " + this.Height + " = " + newTop);

                    this.Left = newLeft;
                    this.Top = newTop;
                }
            };

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
            UpdateStatus("In order to generate code from this template, you need to provide login credentials for your CRM system");
            UpdateStatus("The Discovery URL is the URL to your Discovery Service, you can find this URL in CRM -> Settings -> Customizations -> Developer Resources.  \n    eg " + @"https://dsc.yourdomain.com/XRMServices/2011/Discovery.svc");
            if (settings.OrgList.Contains(settings.CrmOrg) == false)
            {
                settings.OrgList.Add(settings.CrmOrg);
            }
            this.Organization.SelectedItem = settings.CrmOrg;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


        private void RefreshOrgs(object sender, RoutedEventArgs e)
        {
            settings.Password = ((PasswordBox)((Button)sender).CommandParameter).Password;  // PasswordBox doesn't allow 2 way binding, so we have to manually read it
            var origCursor = this.Cursor;
            UpdateStatus("Refreshing Orgs", true);

            try
            {
                var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
                var newOrgs = new ObservableCollection<String>(orgs);
                settings.OrgList = newOrgs;
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error);
                UpdateStatus("Unable to refresh organizations, check connection information");
            }

            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = origCursor; }));
        }
        private void EntitiesRefresh_Click(object sender, RoutedEventArgs events)
        {
            settings.Password = ((PasswordBox)((Button)sender).CommandParameter).Password;  // PasswordBox doesn't allow 2 way binding, so we have to manually read it
            var origCursor = this.Cursor;
            UpdateStatus("Refreshing Entities...", true);

            RefreshEntityList();

            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = origCursor; }));
            UpdateStatus("");
        }
        
        private void RefreshEntityList()
        {
            if (_AllEntities == null)
            {
                Update_AllEntities();
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
                var connection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);

                RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Default,
                    RetrieveAsIfPublished = true,
                };
                RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)connection.Execute(request);
                _AllEntities = response.EntityMetadata;
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error);
                UpdateStatus("Unable to refresh entities, check connection information");
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
            var origCursor = this.Cursor;

            try
            {
                
                settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
                if (settings.CrmConnection == null)
                     throw new UserException("Unable to login to CRM, check to ensure you have the right organization");
                
                UpdateStatus("Mapping entities, this might take a while depending on CRM server/connection speed... ", true);
                var mapper = new Mapper(settings);
                Context = mapper.MapContext();

                settings.Dirty = true;  //  TODO Because the EntitiesSelected is a collection, the Settings class can't see when an item is added or removed.  when I have more time need to get the observable to bubble up.
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                var error = "[ERROR] " + ex.Message + (ex.InnerException != null ? "\n" + "[ERROR] " + ex.InnerException.Message : "");
                UpdateStatus(error);
                UpdateStatus(ex.StackTrace);
                UpdateStatus("Unable to map entities, see error above.");
            }

            this.Cursor = origCursor;
        }
        private void UpdateStatus(string message, bool working = false)
        {
            if (working)
                Dispatcher.BeginInvoke(new Action(() => { this.Cursor = Cursors.Wait; }));

            Dispatcher.BeginInvoke(new Action(() => { Status.Update(message); }));

            System.Windows.Forms.Application.DoEvents();
            //TODO  something with the message
        }
    }
}
