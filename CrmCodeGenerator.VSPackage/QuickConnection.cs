using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using System.ServiceModel.Description;

namespace CrmCodeGenerator.VSPackage
{
    public class QuickConnection
    {
        public static IOrganizationService Connect(string url, string domain, string username, string password, string organization)
        {
            var credentials = GetCredentials(url, domain, username, password);
            ClientCredentials deviceCredentials = null;
            if (url.IndexOf("dynamics.com", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                deviceCredentials = Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
            }

            Uri orgUri = null;
            OrganizationServiceProxy sdk = null;

            using (DiscoveryServiceProxy disco = new DiscoveryServiceProxy(new Uri(url), null, credentials, deviceCredentials))
            {
                if (disco != null)
                {
                    OrganizationDetailCollection orgs = DiscoverOrganizations(disco);
                    if (orgs.Count > 0)
                    {
                        var found = orgs.ToList()
                            .Where(a => a.UniqueName.Equals(organization, StringComparison.InvariantCultureIgnoreCase))
                            .Take(1).SingleOrDefault();

                        if (found != null)
                        {
                            orgUri = new Uri(found.Endpoints[EndpointType.OrganizationService]);
                        }
                    }
                }
            }

            if (orgUri != null)
            {
                sdk = new OrganizationServiceProxy(orgUri, null, credentials, deviceCredentials);
            }
 
            return sdk;
        }

        public static List<string> GetOrganizations(string url, string domain, string username, string password)
        {
            var results = new List<string>() { };
            var credentials = GetCredentials(url, domain, username, password);
            ClientCredentials deviceCredentials = null;
            if (url.IndexOf("dynamics.com", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                deviceCredentials = Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
            }

            using (DiscoveryServiceProxy disco = new DiscoveryServiceProxy(new Uri(url), null, credentials, deviceCredentials))
            {
                if (disco != null)
                {
                    OrganizationDetailCollection orgs = DiscoverOrganizations(disco);
                    if (orgs.Count > 0)
                    {
                        results = orgs.Select(o => o.FriendlyName).ToList();
                    }
                }
            }

            return results;
        }



        private static OrganizationDetailCollection DiscoverOrganizations(DiscoveryServiceProxy service)
        {
            RetrieveOrganizationsRequest request = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse response = (RetrieveOrganizationsResponse)service.Execute(request);

            return response.Details;
        }

        private static ClientCredentials GetCredentials(string url, string domain, string username, string password)
        {
            ClientCredentials credentials = new ClientCredentials();

            var config = ServiceConfigurationFactory.CreateConfiguration<IDiscoveryService>(new Uri(url));

            if (config.AuthenticationType == AuthenticationProviderType.ActiveDirectory)
            {
                credentials.Windows.ClientCredential = new System.Net.NetworkCredential(username, password, domain);
            }
            else if (config.AuthenticationType == AuthenticationProviderType.Federation
                || config.AuthenticationType == AuthenticationProviderType.LiveId
                || config.AuthenticationType == AuthenticationProviderType.OnlineFederation)
            {
                credentials.UserName.UserName = username;
                credentials.UserName.Password = password;
            }
            else if (config.AuthenticationType == AuthenticationProviderType.None)
            {
            }

            return credentials;
        }
    }
}
