using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using CrmCodeGenerator.VSPackage.Model;
using Microsoft.Xrm.Sdk.Discovery;
using System.Collections.ObjectModel;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public class ConnectionHelper
    {
        public static string GetURLName(Settings settings){
            var connection = CrmConnection.Parse(settings.GetDiscoveryCrmConnectionString());
            var service = new DiscoveryService(connection);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);
            var UrlName = response.Details.Where(d => d.UrlName == settings.CrmOrg).Select(d => d.UrlName).FirstOrDefault();
            return UrlName;
        }


        public static OrganizationDetail GetOrganizationDetails(Settings settings)
        {
            var connection = CrmConnection.Parse(settings.GetDiscoveryCrmConnectionString());
            var service = new DiscoveryService(connection);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);
            var details = response.Details.Where(d => d.UniqueName == settings.CrmOrg).FirstOrDefault();
            return details;
        }

        public static ObservableCollection<string> GetOrgList(Settings settings)
        {
            var connection = Microsoft.Xrm.Client.CrmConnection.Parse(settings.GetDiscoveryCrmConnectionString());
            var service = new Microsoft.Xrm.Client.Services.DiscoveryService(connection);

            var request = new Microsoft.Xrm.Sdk.Discovery.RetrieveOrganizationsRequest();
            var response = (Microsoft.Xrm.Sdk.Discovery.RetrieveOrganizationsResponse)service.Execute(request);
            var newOrgs = new ObservableCollection<String>(response.Details.Select(d => d.UrlName).ToList());
            return newOrgs;
        }
    }
}
