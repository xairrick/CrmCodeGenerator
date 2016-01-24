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
        public static OrganizationDetail GetOrganizationDetails(Settings settings)
        {
            var orgs = GetOrganizations(settings);
            var details = orgs.Where(d => d.UrlName == settings.CrmOrg).FirstOrDefault();
            return details;
        }
        public static ObservableCollection<string> GetOrgList(Settings settings)
        {
            var orgs = GetOrganizations(settings);
            var newOrgs = new ObservableCollection<String>(orgs.Select(d => d.UrlName).ToList());
            return newOrgs;
        }
        public static OrganizationDetailCollection GetOrganizations(Settings settings)
        {
            try
            {
                var connection = Microsoft.Xrm.Client.CrmConnection.Parse(settings.GetDiscoveryCrmConnectionString());
                var service = new Microsoft.Xrm.Client.Services.DiscoveryService(connection);

                var request = new Microsoft.Xrm.Sdk.Discovery.RetrieveOrganizationsRequest();
                var response = (Microsoft.Xrm.Sdk.Discovery.RetrieveOrganizationsResponse)service.Execute(request);
                return response.Details;
            }
            catch (System.IO.FileNotFoundException e)
            {
                if (e.Message.Contains("Microsoft.IdentityModel"))
                {
                    throw new Exception("Unable to load Windows Identity Foundation 3.5.  This is a feature that can be enabled on windows 8+ or downloaded for earlier versions ->  https://www.microsoft.com/en-nz/download/details.aspx?id=17331 ", e);
                }
                else
                {
                    throw e;
                }
            }
        }
    }
}
