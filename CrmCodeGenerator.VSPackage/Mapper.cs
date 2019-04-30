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
            var connection = GetConnection();
            var context = new Context();
            context.Entities = GetEntities(connection);
            SortEntities(context);
            context.Enums = GetGlobalEnums(connection);
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

        internal MappingEntity[] GetEntities(IOrganizationService service)
        {
            var entities = GetEntitiesMetadata(service);

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

        internal MappingEnum[] GetGlobalEnums(IOrganizationService service)
        {
            var enums = RetrieveGlobalOptionSetsMetadataFromServer(service);

            OnMessage(string.Format("Found {0} option sets", enums.Count()));

            var mappedEnums = enums.Select(e => MappingEnum.Parse(e)).OrderBy(e => e.DisplayName).ToList();

            return mappedEnums.ToArray();
        }

        private EntityMetadata[] GetEntitiesMetadata(IOrganizationService service)
        {
            OnMessage("Gathering metadata, this may take a few minutes...");
            if (this.Settings.EntitiesSelected.Count > 20)
            {
                return RetrieveEntityMetadataFromServer(service);
            }

            var entitiesToRetreive = this.Settings.EntitiesSelected.Select(x => x).ToList();
            if (!entitiesToRetreive.Any(x => x.Equals("activityparty")))
            {
                entitiesToRetreive.Add("activityparty");
            }

            var results = new List<EntityMetadata>();
            foreach (var entity in entitiesToRetreive)
            {
                var req = new RetrieveEntityRequest();
                req.EntityFilters = EntityFilters.All;
                req.LogicalName = entity;
                req.RetrieveAsIfPublished = this.Settings.IncludeUnpublish;
                var res = (RetrieveEntityResponse)service.Execute(req);
                results.Add(res.EntityMetadata);
            }
            return results.ToArray();
        }

        private EntityMetadata[] RetrieveEntityMetadataFromServer(IOrganizationService service)
        {
            //TODO should change this to early binding RetrieveAllEntitiesRequest
            OrganizationRequest request = new OrganizationRequest("RetrieveAllEntities");
            request.Parameters["EntityFilters"] = EntityFilters.All;
            request.Parameters["RetrieveAsIfPublished"] = this.Settings.IncludeUnpublish;

            //var entities = sdk.Execute(request).Results["EntityMetadata"] as EntityMetadata[];
            var results = service.Execute(request);
            var entities = results["EntityMetadata"] as EntityMetadata[];
            return entities;
        }
        private OptionSetMetadata[] RetrieveGlobalOptionSetsMetadataFromServer(IOrganizationService service)
        {
            var results = new List<OptionSetMetadata>();
            // Use RetrieveAllOptionSetsRequest to retrieve all global option sets.
            // Create the request.
            RetrieveAllOptionSetsRequest retrieveAllOptionSetsRequest = new RetrieveAllOptionSetsRequest();

            // Execute the request
            RetrieveAllOptionSetsResponse retrieveAllOptionSetsResponse = (RetrieveAllOptionSetsResponse)service.Execute(retrieveAllOptionSetsRequest);

            //return retrieveAllOptionSetsResponse.OptionSetMetadata;
            // Now you can use RetrieveAllOptionSetsResponse.OptionSetMetadata property to 
            // work with all retrieved option sets.
            if (retrieveAllOptionSetsResponse.OptionSetMetadata.Count() > 0)
            {
                Console.WriteLine("All the global option sets retrieved as below:");
                foreach (OptionSetMetadataBase optionSetMetadata in retrieveAllOptionSetsResponse.OptionSetMetadata)
                {
                    if (!optionSetMetadata.IsManaged.Value && optionSetMetadata.IsGlobal.Value)
                    {
                        RetrieveOptionSetRequest retrieveOptionSetRequest = new RetrieveOptionSetRequest { Name = optionSetMetadata.Name };
                        // Execute the request.
                        RetrieveOptionSetResponse retrieveOptionSetResponse = (RetrieveOptionSetResponse)service.Execute(retrieveOptionSetRequest);
                        results.Add((OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata);
                    }
                }
            }

            return results.ToArray();
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
