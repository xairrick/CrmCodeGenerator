using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using System.Configuration;
using Microsoft.Xrm.Sdk;
using System.IO;
using CrmCodeGenerator.VSPackage.Model;
using CrmCodeGenerator.VSPackage.T4;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmCodeGenerator.VSPackage
{
    public delegate void MapperHandler(object sender, MapperEventArgs e);

    public class Mapper
    {
        public Settings Settings { get; set; }

        public Mapper()
        {
        }

        public Mapper(Settings settings)
        {
            this.Settings = settings;
        }

        #region event handler
        public event MapperHandler Message;

        protected void OnMessage(string message, string extendedMessage = "")
        {
            if (this.Message != null)
            {
                Message(this, new MapperEventArgs { Message = message, MessageExtended = extendedMessage });
            }
        }
        #endregion
        
        public Context MapContext()
        {
            var context = new Context();
            context.Entities = GetEntities(GetConnection());
            SortEntities(context);
            return context;
        }



        internal MappingEntity[] GetEntities(EntityMetadata[] entities)
        {
            return entities.Select(e => MappingEntity.Parse(e)).ToArray();
        }

        internal Microsoft.Xrm.Sdk.IOrganizationService GetConnection()
        {
            OnMessage("Connecting to crm, please wait...");

            IOrganizationService sdk = null;

            if (this.Settings.CrmConnection == null)
            {
                sdk = QuickConnection.Connect(this.Settings.CrmSdkUrl,
                    this.Settings.Domain,
                    this.Settings.Username,
                    this.Settings.Password,
                    this.Settings.CrmOrg);
            }
            else
            {
                sdk = this.Settings.CrmConnection;
            }

            OnMessage("Connected to crm");

            return sdk;
        }


        public void SortEntities(Context context)
        {
            context.Entities = context.Entities.OrderBy(e => e.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.Enums = e.Enums.OrderBy(en => en.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.Fields = e.Fields.OrderBy(f => f.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.RelationshipsOneToMany =
                    e.RelationshipsOneToMany.Where(
                        r => context.Entities.Any(t => t.DisplayName == r.Type)).OrderBy(r => r.DisplayName).OrderBy(r => r.DisplayName).ToArray();

            foreach (var e in context.Entities)
                e.RelationshipsManyToOne =
                    e.RelationshipsManyToOne.Where(
                        r => context.Entities.Any(t => t.DisplayName == r.Type)).OrderBy(r => r.DisplayName).OrderBy(r => r.DisplayName).ToArray();
            return;
        }

        internal MappingEntity[] GetEntities(IOrganizationService sdk)
        {
            OnMessage("Gathering metadata, this may take a few minutes...");
            
            //TODO should change this to early binding RetrieveAllEntitiesRequest
            OrganizationRequest request = new OrganizationRequest("RetrieveAllEntities");
            request.Parameters["EntityFilters"] = EntityFilters.All;   
            request.Parameters["RetrieveAsIfPublished"] = true;

            //var entities = sdk.Execute(request).Results["EntityMetadata"] as EntityMetadata[];
            var results = sdk.Execute(request);
            var entities = results["EntityMetadata"] as EntityMetadata[];

            string[] forceIgnore = new string[] { "BulkOperation", "ContractTemplate", "DuplicateRule", "DuplicateRuleCondition", "FieldPermission", "FieldSecurityProfile", "Goal", "GoalRollupQuery", "ImportMap", "InternalAddress", "KbArticleTemplate", "MailMergeTemplate", "Metric", "ProcessSession", "Publisher", "Queue", "QueueItem", "Report", "ReportCategory", "Role", "RolePrivileges", "RollupField",
                "SavedQuery", "SharePointSite", "Solution", "UserQuery", "UserQueryVisualization", "Workflow"}.Select(w => w.ToLower()).ToArray();

            // Removed, as we are using combobox that will FORCE you to select which entities to pick
            //this.Settings.EntitiesToExclude = this.Settings.EntitiesToExclude.Concat(
            //        forceIgnore.Except(this.Settings.EntitiesToInclude)).ToArray();
            //this.Settings.EntitiesToInclude = this.Settings.EntitiesToInclude.Select(i => i.ToLower().Trim()).ToArray();
            //this.Settings.EntitiesToExclude = this.Settings.EntitiesToExclude.Select(e => e.ToLower().Trim()).ToArray();

            var testType = entities.Where(r => r.LogicalName != "subscriptionsyncinfo")
                .Where(r =>
                    {
                        bool include = false;

                        // this is helpful to generate code for just few entities, so we exlcude all and include just a few
                        //if (!string.IsNullOrWhiteSpace(this.Settings.EntitiesToIncludeString))
                        //{
                        include = this.Settings.EntitiesSelected.Contains(r.LogicalName);
                        //}
                        //else
                        //{
                        //    include =  && !this.Settings.EntitiesToExclude.Contains(r.LogicalName) // make sure it's not forcefully excluded
                        //        (r.IsCustomEntity == true || r.IsCustomizable.Value); // ignore the system entities, these are never used
                        //}

                        return include;

                    }).ToArray();

            OnMessage(string.Format("Found {0} entities", testType.Length));

            return GetEntities(testType);
        }
    }
}
