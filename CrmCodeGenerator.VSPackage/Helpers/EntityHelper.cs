using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public class EntityHelper
    {
        public static string[] NonStandard = new string[] {
                        "applicationfile"
                        , "attachment" // Not included with CrmSvcUtil 6.0.0001.0061
		                , "authorizationserver" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "businessprocessflowinstance" // Not included with CrmSvcUtil 2013  http://community.dynamics.com/crm/f/117/t/117642.aspx
                        , "businessunitmap" // Not included with CrmSvcUtil 2013
                        , "clientupdate"  // Not included with CrmSvcUtil 2013
                        , "commitment" // Not included with CrmSvcUtil 2013
                        //  , "competitoraddress" //isn't include in CrmSvcUtil but it shows in the default solution
                        , "complexcontrol" //Not Included with CrmSvcUtil 2013
                        , "dependencynode" //Not Included with CrmSvcUtil 2013
                        , "displaystringmap" // Not Included with CrmSvcUtil 2013
                        , "documentindex"  // Not Included with CrmSvcUtil 2013
                        , "emailhash"  // Not Included with CrmSvcUtil 2013
                        , "emailsearch" // Not Included with CrmSvcUtil 2013
                        , "filtertemplate" // Not Included with CrmSvcUtil 2013
                        , "imagedescriptor"  // Not included with CrmSvcUtil 2013
                        , "importdata" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "integrationstatus" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "interprocesslock" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "multientitysearchentities" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "multientitysearch" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "notification" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "organizationstatistic" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "owner"   // Not included with CrmSvcUtil 2013
                        , "partnerapplication" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "principalattributeaccessmap" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "principalobjectaccessreadsnapshot" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "principalobjectaccess" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "privilegeobjecttypecodes" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "postregarding"  // Not included with CrmSvcUtil 2013
                        , "postrole"  // Not included with CrmSvcUtil 2013
                        , "subscriptionclients" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "salesprocessinstance" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "recordcountsnapshot" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "replicationbacklog" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "resourcegroupexpansion" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "ribboncommand" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "ribboncontextgroup" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "ribbondiff" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "ribbonrule" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "ribbontabtocommandmap" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "roletemplate" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "statusmap" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "stringmap" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "sqlencryptionaudit"
                        , "subscriptionsyncinfo"
                        , "subscription" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "subscriptiontrackingdeletedobject"
                        , "systemapplicationmetadata" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "systemuserbusinessunitentitymap" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "systemuserprincipals" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "traceassociation" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "traceregarding" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "unresolvedaddress" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "userapplicationmetadata" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "userfiscalcalendar" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "webwizard" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "wizardaccessprivilege" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "wizardpage" // Not included with CrmSvcUtil 6.0.0001.0061
                        , "workflowwaitsubscription" // Not included with CrmSvcUtil 6.0.0001.0061
        };
    }
}
