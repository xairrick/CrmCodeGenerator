using CrmCodeGenerator.VSPackage.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage
{
    public class Configuration
    {

        #region Singleton
        private static Configuration _instance;
        private static Object SyncLock = new System.Object();
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Configuration();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        
        public Configuration()
        {
            Settings = new CrmCodeGenerator.VSPackage.Model.Settings();
            Settings.CrmSdkUrl = @"https://dscdev.benco.com/XRMServices/2011/Discovery.svc";
            Settings.ProjectName = "";
            Settings.Domain = "";
            Settings.T4Path = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\CrmSvcUtil.tt");
            Settings.Template = "";
            Settings.CrmOrg = "DEV-CRM";
            Settings.EntitiesToIncludeString = "account, contact, systemuser";
            Settings.OutputPath = "";
            Settings.Username = "";
            Settings.Password = "";
            Settings.Namespace = "";
            Settings.Dirty = false;
        }
        public CrmCodeGenerator.VSPackage.Model.Settings Settings { get; set; }
    }
}
