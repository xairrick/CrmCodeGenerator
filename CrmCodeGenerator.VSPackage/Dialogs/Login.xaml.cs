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
        Settings settings;
        public Login(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            this.DataContext = settings;
            
        }
        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settings.OrgList.Add(settings.CrmOrg);
            this.Organization.SelectedIndex = 0;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


        private void RefreshOrgs(object sender, RoutedEventArgs e)
        {
            var origCursor = this.Cursor;
            UpdateStatus("Refreshing Orgs", true);

            var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
            var newOrgs = new ObservableCollection<String>(orgs);
            settings.OrgList = newOrgs;

            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = origCursor; }));
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

            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = origCursor; }));
            UpdateStatus("");
        }


        private void Logon_Click(object sender, RoutedEventArgs e)
        {
            var origCursor = this.Cursor;
            UpdateStatus("Logging in...", true);

            settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
            if (settings.CrmConnection == null)
                throw new UserException("Unable to login to CRM, check to ensure you have the right organization");
            

            this.Cursor = origCursor;
            this.DialogResult = true;
            settings.Dirty = true;  //  TODO Because the EntitiesSelected is a collection, the Settings class can't see when an item is added or removed.  when I have more time need to get the observable to bubble up.
            this.Close();
        }
        private void UpdateStatus(string message, bool working = false)
        {
            if(working)
                Dispatcher.BeginInvoke(new Action(() => { this.Cursor = Cursors.Wait; }));

            System.Windows.Forms.Application.DoEvents();
            
            //TODO  something with the message
        }

    }
}
