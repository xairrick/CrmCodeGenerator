using CrmCodeGenerator.VSPackage.Model;
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
            this.Close();
        }


        private void RefreshOrgs(object sender, RoutedEventArgs e)
        {
            var origCursor = this.Cursor;
            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = Cursors.Wait; }));

            var prevOrg = this.Organization.Text;
            var orgs = QuickConnection.GetOrganizations(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password);
            var newOrgs = new ObservableCollection<String>(orgs);
            settings.OrgList = newOrgs;


            this.Cursor = origCursor;
        }
        private void Logon_Click(object sender, RoutedEventArgs e)
        {
            var origCursor = this.Cursor;
            Dispatcher.BeginInvoke(new Action(() => { this.Cursor = Cursors.Wait; }));

            settings.CrmConnection = QuickConnection.Connect(settings.CrmSdkUrl, settings.Domain, settings.Username, settings.Password, settings.CrmOrg);
            // TODO what if there is a login error???

            this.Cursor = origCursor;
            this.Close();
        }

    }
}
