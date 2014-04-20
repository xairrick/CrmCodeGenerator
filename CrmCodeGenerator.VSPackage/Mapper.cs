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
using CrmCodeGenerator.VSPackage.Helpers;

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
                e.RelationshipsOneToMany = e.RelationshipsOneToMany.OrderBy(r => r.LogicalName).ToArray();

            foreach (var e in context.Entities)
                e.RelationshipsManyToOne = e.RelationshipsManyToOne.OrderBy(r => r.LogicalName).ToArray();
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

            var selectedEntities = entities
                .Where(r => this.Settings.EntitiesSelected.Contains(r.LogicalName))
                    .Where(r =>
                    {
                        if (this.Settings.IncludeNonStandard)
                            return true;
                        else
                            return !EntityHelper.NonStandard.Contains(r.LogicalName);
                    })
                    .ToList();

            if (selectedEntities.Any(r => r.IsActivity == true || r.IsActivityParty == true))
            {
                if (!selectedEntities.Any(r => r.LogicalName.Equals("activityparty")))
                    selectedEntities.Add(entities.Where(r => r.LogicalName.Equals("activityparty")).Single());
            }

            OnMessage(string.Format("Found {0} entities", selectedEntities.Count));

            var mappedEntities = selectedEntities.Select(e => MappingEntity.Parse(e)).OrderBy(e => e.DisplayName).ToList();
            ExcludeRelationshipsNotIncluded(mappedEntities);
            foreach (var ent in mappedEntities)
            {
                foreach (var rel in ent.RelationshipsOneToMany)
                {
                    rel.ToEntity = mappedEntities.Where(e => e.LogicalName.Equals(rel.Attribute.ToEntity)).FirstOrDefault();
                }
                foreach (var rel in ent.RelationshipsManyToOne)
                {
                    rel.ToEntity = mappedEntities.Where(e => e.LogicalName.Equals(rel.Attribute.ToEntity)).FirstOrDefault();
                }
                foreach (var rel in ent.RelationshipsManyToMany)
                {
                    rel.ToEntity = mappedEntities.Where(e => e.LogicalName.Equals(rel.Attribute.ToEntity)).FirstOrDefault();
                }
            }

            return mappedEntities.ToArray();
        }
        private static void ExcludeRelationshipsNotIncluded(List<MappingEntity> mappedEntities)
        {
            foreach (var ent in mappedEntities)
            {
                ent.RelationshipsOneToMany = ent.RelationshipsOneToMany.ToList().Where(r => mappedEntities.Select(m => m.LogicalName).Contains(r.Type)).ToArray();
                ent.RelationshipsManyToOne = ent.RelationshipsManyToOne.ToList().Where(r => mappedEntities.Select(m => m.LogicalName).Contains(r.Type)).ToArray();
                ent.RelationshipsManyToMany = ent.RelationshipsManyToMany.ToList().Where(r => mappedEntities.Select(m => m.LogicalName).Contains(r.Type)).ToArray();
            }
        }
    }
}
