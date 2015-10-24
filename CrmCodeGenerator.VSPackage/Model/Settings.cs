using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.Xrm.Sdk.Client;
using CrmCodeGenerator.VSPackage.Helpers;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmCodeGenerator.VSPackage.Model
{
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            EntityList = new ObservableCollection<string>();
            EntitiesSelected = new ObservableCollection<string>();

            Dirty = false;
        }

        #region boiler-plate INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            Dirty = true;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        private bool _UseSSL;
        private bool _UseIFD;
        private bool _UseOnline;
        private bool _UseOffice365;
        private string _OutputPath;
        private string _Namespace;
        private string _EntitiesToIncludeString;
        private string _CrmOrg;
        private string _Password;
        private string _Username;
        private string _Domain;
        private string _CrmSdkUrl;
        private string _Template;
        private string _T4Path;
        private bool _IncludeNonStandard;
        private bool _IncludeUnpublish;

        private string _ProjectName;
        public string ProjectName
        {
            get
            {
                return _ProjectName;
            }
            set
            {
                SetField(ref _ProjectName, value);
            }
        }
        public string T4Path
        {
            get
            {
                return _T4Path;
            }
            set
            {
                SetField(ref _T4Path, value);
            }
        }
        public string Template
        {
            get
            {
                return _Template;
            }
            set
            {
                SetField(ref _Template, value);
                NewTemplate = !System.IO.File.Exists(System.IO.Path.Combine(_Folder, _Template));
            }
        }
        private string _Folder = "";
        public string Folder
        {
            get
            {
                return _Folder;
            }
            set
            {
                SetField(ref _Folder, value);
            }
        }

        private bool _NewTemplate;
        public bool NewTemplate
        {
            get
            {
                return _NewTemplate;
            }
            set
            {
                SetField(ref _NewTemplate, value);
            }
        }
        public string OutputPath
        {
            get
            {
                return _OutputPath;
            }
            set
            {
                SetField(ref _OutputPath, value);
            }
        }

        public string CrmSdkUrl
        {
            get
            {
                return _CrmSdkUrl;
            }
            set
            {
                SetField(ref _CrmSdkUrl, value);
            }
        }
        public string Domain
        {
            get
            {
                return _Domain;
            }
            set
            {
                SetField(ref _Domain, value);
            }
        }
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                SetField(ref _Username, value);
            }
        }
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                SetField(ref _Password, value);
            }
        }
        public string CrmOrg
        {
            get
            {
                return _CrmOrg;
            }
            set
            {
                SetField(ref _CrmOrg, value);
            }
        }


        private ObservableCollection<String> _OnLineServers = new ObservableCollection<String>();
        public ObservableCollection<String> OnLineServers
        {
            get
            {
                return _OnLineServers;
            }
            set
            {
                SetField(ref _OnLineServers, value);
            }
        }
        //private string _OnlineServer;
        //public string OnlineServer
        //{
        //    get
        //    {
        //        return _OnlineServer;
        //    }
        //    set
        //    {
        //        SetField(ref _OnlineServer, value);
        //    }
        //}
        private string _ServerName = "";
        public string ServerName
        {
            get
            {
                return _ServerName;
            }
            set
            {
                SetField(ref _ServerName, value);
            }
        }
        private string _ServerPort = "";
        public string ServerPort
        {
            get
            {
                if (UseOnline || UseOffice365)
                {
                    return "";
                }
                return _ServerPort;
            }
            set
            {
                SetField(ref _ServerPort, value);
            }
        }
        private string _HomeRealm = "";
        public string HomeRealm
        {
            get
            {
                return _HomeRealm;
            }
            set
            {
                SetField(ref _HomeRealm, value);
            }
        }


        private ObservableCollection<String> _OrgList = new ObservableCollection<String>();
        public ObservableCollection<String> OrgList
        {
            get
            {
                return _OrgList;
            }
            set
            {
                SetField(ref _OrgList, value);
            }
        }


        private ObservableCollection<String> _TemplateList = new ObservableCollection<String>();
        public ObservableCollection<String> TemplateList
        {
            get
            {
                return _TemplateList;
            }
            set
            {
                SetField(ref _TemplateList, value);
            }
        }
        public IOrganizationService CrmConnection { get; set; }

        public string EntitiesToIncludeString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var value in _EntitiesSelected)
                {
                    if (sb.Length != 0)
                        sb.Append(',');
                    sb.Append(value);
                }
                return sb.ToString();
            }
            set
            {
                var newList = new ObservableCollection<string>();
                var split = value.Split(',').Select(p => p.Trim()).ToList();
                foreach (var s in split)
                {
                    newList.Add(s);
                    if (!_EntityList.Contains(s))
                        _EntityList.Add(s);
                }
                EntitiesSelected = newList;
                SetField(ref _EntitiesToIncludeString, value);
                OnPropertyChanged("EnableExclude");
            }
        }

        public ObservableCollection<string> _EntityList;
        public ObservableCollection<string> EntityList
        {
            get
            {
                return _EntityList;
            }
            set
            {
                SetField(ref _EntityList, value);
            }
        }

        public ObservableCollection<string> _EntitiesSelected;
        public ObservableCollection<string> EntitiesSelected
        {
            get
            {
                return _EntitiesSelected;
            }
            set
            {
                SetField(ref _EntitiesSelected, value);
            }
        }

        public string Namespace
        {
            get
            {
                return _Namespace;
            }
            set
            {
                SetField(ref _Namespace, value);
            }
        }

        public bool Dirty { get; set; }


        public bool IncludeNonStandard
        {
            get
            {
                return _IncludeNonStandard;
            }
            set
            {
                SetField(ref _IncludeNonStandard, value);
            }
        }
        public bool IncludeUnpublish
        {
            get
            {
                return _IncludeUnpublish;
            }
            set
            {
                SetField(ref _IncludeUnpublish, value);
            }
        }
        public bool UseSSL
        {
            get { return _UseSSL; }
            set
            {
                if (SetField(ref _UseSSL, value))
                {
                    ReEvalReadOnly();
                }
            }
        }
        public bool UseIFD
        {
            get { return _UseIFD; }
            set
            {
                if (SetField(ref _UseIFD, value))
                {
                    if (value)
                    {
                        UseOnline = false;
                        UseOffice365 = false;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }
        public bool UseOnline
        {
            get { return _UseOnline; }
            set
            {
                if (SetField(ref _UseOnline, value))
                {
                    if (value)
                    {
                        UseIFD = false;
                        UseOffice365 = true;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    else
                    {
                        UseOffice365 = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }
        public bool UseOffice365
        {
            get { return _UseOffice365; }
            set
            {
                if (SetField(ref _UseOffice365, value))
                {
                    if (value)
                    {
                        UseIFD = false;
                        UseOnline = true;
                        UseSSL = true;
                        UseWindowsAuth = false;
                    }
                    ReEvalReadOnly();
                }
            }
        }
        private bool _UseWindowsAuth;
        public bool UseWindowsAuth
        {
            get { return _UseWindowsAuth; }
            set
            {
                SetField(ref _UseWindowsAuth, value);
                ReEvalReadOnly();
            }
        }


        #region Read Only Properties
        private void ReEvalReadOnly()
        {
            OnPropertyChanged("NeedServer");
            OnPropertyChanged("NeedOnlineServer");
            OnPropertyChanged("NeedServerPort");
            OnPropertyChanged("NeedHomeRealm");
            OnPropertyChanged("NeedCredentials");
            OnPropertyChanged("CanUseWindowsAuth");
            OnPropertyChanged("CanUseSSL");
        }
        public bool NeedServer
        {
            get
            {
                return !(UseOnline || UseOffice365);
            }
        }
        public bool NeedOnlineServer
        {
            get
            {
                return (UseOnline || UseOffice365);
            }
        }
        public bool NeedServerPort
        {
            get
            {
                return !(UseOffice365 || UseOnline);
            }
        }
        public bool NeedHomeRealm
        {
            get
            {
                return !(UseIFD || UseOffice365 || UseOnline);
            }
        }
        public bool NeedCredentials
        {
            get
            {
                return !UseWindowsAuth;
            }
        }
        public bool CanUseWindowsAuth
        {
            get
            {
                return !(UseIFD || UseOnline || UseOffice365);
            }
        }
        public bool CanUseSSL
        {
            get
            {
                return !(UseOnline || UseOffice365 || UseIFD);
            }
        }
        #endregion


        #region Conntection Strings

        public AuthenticationProviderType AuthType
        {
            get {
                if (UseIFD)
                {
                    return AuthenticationProviderType.Federation;
                }
                else if (UseOffice365)
                {
                    return AuthenticationProviderType.OnlineFederation;
                }
                else if (UseOnline)
                {
                    return AuthenticationProviderType.LiveId;
                }
                
                return AuthenticationProviderType.ActiveDirectory;
            }
        }

        public string GetDiscoveryCrmConnectionString()
        {
            var connectionString = string.Format("Url={0}://{1}:{2};",
                UseSSL ? "https" : "http",
                UseIFD ? ServerName : UseOffice365 ? "disco." + ServerName : UseOnline ? "dev." + ServerName : ServerName,
                ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort));

            if (!UseWindowsAuth)
            {
                if (!UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        connectionString += string.Format("Domain={0};", Domain);
                    }
                }

                string sUsername = Username;
                if (UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        sUsername = string.Format("{0}\\{1}", Domain, Username);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", sUsername, Password);
            }

            if (UseOnline && !UseOffice365)
            {
                System.ServiceModel.Description.ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);
            }

            if (UseIFD && !string.IsNullOrEmpty(HomeRealm))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealm);
            }

            return connectionString;
        }


        public string GetOrganizationCrmConnectionString()
        {
            var currentServerName = string.Empty;

            var orgDetails  = ConnectionHelper.GetOrganizationDetails(this);
            if (UseOffice365 || UseOnline)
            {
                currentServerName = string.Format("{0}.{1}", orgDetails.UrlName, ServerName);
            }
            else if (UseIFD)
            {
                var serverNameParts = ServerName.Split('.');

                serverNameParts[0] = orgDetails.UrlName;


                currentServerName = string.Format("{0}:{1}",
                                                  string.Join(".", serverNameParts),
                                                  ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort));
            }
            else
            {
                currentServerName = string.Format("{0}:{1}/{2}",
                                                  ServerName,
                                                  ServerPort.Length == 0 ? (UseSSL ? 443 : 80) : int.Parse(ServerPort),
                                                  CrmOrg);
            }

            //var connectionString = string.Format("Url={0}://{1};",
            //                                     UseSSL ? "https" : "http",
            //                                     currentServerName);

            var connectionString = string.Format("Url={0};", orgDetails.Endpoints[EndpointType.OrganizationService].Replace("/XRMServices/2011/Organization.svc", ""));

            if (!UseWindowsAuth)
            {
                if (!UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        connectionString += string.Format("Domain={0};", Domain);
                    }
                }

                string username = Username;
                if (UseIFD)
                {
                    if (!string.IsNullOrEmpty(Domain))
                    {
                        username = string.Format("{0}\\{1}", Domain, Username);
                    }
                }

                connectionString += string.Format("Username={0};Password={1};", username, Password);
            }

            if (UseOnline)
            {
                System.ServiceModel.Description.ClientCredentials deviceCredentials;

                do
                {
                    deviceCredentials = DeviceIdManager.LoadDeviceCredentials() ??
                                        DeviceIdManager.RegisterDevice();
                } while (deviceCredentials.UserName.Password.Contains(";")
                         || deviceCredentials.UserName.Password.Contains("=")
                         || deviceCredentials.UserName.Password.Contains(" ")
                         || deviceCredentials.UserName.UserName.Contains(";")
                         || deviceCredentials.UserName.UserName.Contains("=")
                         || deviceCredentials.UserName.UserName.Contains(" "));

                connectionString += string.Format("DeviceID={0};DevicePassword={1};",
                                                  deviceCredentials.UserName.UserName,
                                                  deviceCredentials.UserName.Password);
            }

            if (UseIFD && !string.IsNullOrEmpty(HomeRealm))
            {
                connectionString += string.Format("HomeRealmUri={0};", HomeRealm);
            }

            //append timeout in seconds to connectionstring
            //connectionString += string.Format("Timeout={0};", Timeout.ToString(@"hh\:mm\:ss"));
            return connectionString;
        }
        #endregion

        public bool IsActive { get; set; }
    }
}
