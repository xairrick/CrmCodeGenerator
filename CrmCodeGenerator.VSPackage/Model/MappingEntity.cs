﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingEntity
    {
        public CrmEntityAttribute Attribute { get; set; }
        public bool IsIntersect { get; set; }
        public Nullable<int> TypeCode { get; set; }
        public MappingField[] Fields { get; set; }
        public MappingEnum States { get; set; }
        public MappingEnum[] Enums { get; set; }
        public MappingRelationship1N[] RelationshipsOneToMany { get; set; }
        public MappingRelationshipN1[] RelationshipsManyToOne { get; set; }

        public string LogicalName
        {
            get
            {
                return Attribute.LogicalName;
            }
        }

        public string DisplayName { get; set; }
        public string HybridName { get; set; }
        public string StateName { get; set; }
        public MappingField PrimaryKey { get; set; }
        public string PrimaryKeyProperty { get; set; }
        public string PrimaryNameAttribute { get; set; }
        public string Description { get; set; }
        public string DescriptionXmlSafe 
        {
            get
            {
                return Naming.XmlEscape(Description);
            }
        }

        public string Plural
        {
            get
            {
                return Naming.GetPluralName(DisplayName);
            }
        }
        public MappingEntity()
        {
            Description = "";
        }

        public static MappingEntity Parse(EntityMetadata entityMetadata)
        {
            var entity = new MappingEntity();
            
            entity.Attribute = new CrmEntityAttribute();
            entity.TypeCode = entityMetadata.ObjectTypeCode;
            entity.Attribute.LogicalName = entityMetadata.LogicalName;
            entity.IsIntersect = (bool)entityMetadata.IsIntersect;
            entity.Attribute.PrimaryKey = entityMetadata.PrimaryIdAttribute;

            // entity.DisplayName = Helper.GetProperVariableName(entityMetadata.SchemaName);
            entity.DisplayName = Naming.GetProperEntityName(entityMetadata.SchemaName);
            entity.HybridName = Naming.GetProperHybridName(entityMetadata.SchemaName, entityMetadata.LogicalName);
            entity.StateName = entity.HybridName + "State";

            if (entityMetadata.Description != null)
                if (entityMetadata.Description.UserLocalizedLabel != null)
                    entity.Description = entityMetadata.Description.UserLocalizedLabel.Label;
            //var d = entityMetadata.Attributes.Where(p=>p.LogicalName.Contains("sii_")).ToList();
            //var d2 = entityMetadata.Attributes.Where(p=>p.LogicalName.Contains("sii_")).Select(p=>p.LogicalName).ToList();
            var fields = entityMetadata.Attributes
                .Where(a => a.AttributeOf == null)
                .Select(a => MappingField.Parse(a, entity)).ToList();

            fields.ForEach(f =>
                    {
                        if (f.DisplayName == entity.DisplayName)
                            f.DisplayName += "1";
                        //f.HybridName = Naming.GetProperHybridFieldName(f.DisplayName, f.Attribute);
                    }
                );

            AddEnityImageCRM2013(fields);
            AddLookupFields(fields);

            entity.Fields = fields.ToArray();
            entity.States = entityMetadata.Attributes.Where(a => a is StateAttributeMetadata).Select(a => MappingEnum.Parse(a as EnumAttributeMetadata)).FirstOrDefault();

            entity.Enums = entityMetadata.Attributes
                .Where(a => a is PicklistAttributeMetadata || a is StateAttributeMetadata || a is StatusAttributeMetadata || a is BooleanAttributeMetadata || a is MultiSelectPicklistAttributeMetadata)
                .Select(a => MappingEnum.Parse(a)).ToArray();

            entity.PrimaryKey = entity.Fields.First(f => f.Attribute.LogicalName == entity.Attribute.PrimaryKey);
            entity.PrimaryKeyProperty = entity.PrimaryKey.DisplayName;
            entity.PrimaryNameAttribute = entityMetadata.PrimaryNameAttribute;

            entity.RelationshipsOneToMany = entityMetadata.OneToManyRelationships.Select(r =>
                MappingRelationship1N.Parse(r, entity.Fields)).ToArray();

            entity.RelationshipsOneToMany.ToList().ForEach(r => {
                var newName = r.DisplayName;

                if (newName  == entity.DisplayName || newName == entity.HybridName)
                    newName = r.DisplayName += "1";

                if(entity.Fields.Any(e => e.DisplayName == newName))
                {
                    newName = r.DisplayName += "2";
                }
            });


            entity.RelationshipsManyToOne = entityMetadata.ManyToOneRelationships.Select(r =>
                MappingRelationshipN1.Parse(r, entity.Fields)).ToArray();

            entity.RelationshipsManyToOne.ToList().ForEach(r => {
                var newName = r.DisplayName;

                if (newName == entity.DisplayName || newName == entity.HybridName)
                    newName = r.DisplayName += "1";

                if (entity.Fields.Any(e => e.DisplayName == newName))
                {
                    newName = r.DisplayName += "2";
                }
            });

            var RelationshipsManyToMany = entityMetadata.ManyToManyRelationships.Select(r => MappingRelationshipMN.Parse(r, entity.LogicalName)).ToList();
            var selfReferenced = RelationshipsManyToMany.Where(r => r.IsSelfReferenced).ToList();
            foreach (var referecned in selfReferenced)
            {
                var referencing = (MappingRelationshipMN)referecned.Clone();
                referencing.DisplayName = "Referencing" + Naming.GetProperVariableName(referecned.SchemaName);
                referencing.EntityRole = "Microsoft.Xrm.Sdk.EntityRole.Referencing";
                RelationshipsManyToMany.Add(referencing);
            }
            RelationshipsManyToMany.ForEach(r => {
                var newName = r.DisplayName;

                if (newName == entity.DisplayName || newName == entity.HybridName)
                    newName = r.DisplayName += "1";

                if (entity.Fields.Any(e => e.DisplayName == newName))
                {
                    newName = r.DisplayName += "2";
                }
            });
            entity.RelationshipsManyToMany = RelationshipsManyToMany.OrderBy(r => r.DisplayName).ToArray();

            return entity;
        }

        private static void AddLookupFields(List<MappingField> fields)
        {
            var fieldsIterator = fields.Where(e => e.Attribute.IsLookup).ToArray();
            foreach (var lookup in fieldsIterator)
            {
                var nameField = new MappingField
                {
                    Attribute = new CrmPropertyAttribute
                    {
                        IsLookup = false,
                        LogicalName = lookup.Attribute.LogicalName + "Name",
                        IsEntityReferenceHelper = true
                    },
                    DisplayName = lookup.DisplayName + "Name",
                    HybridName = lookup.HybridName + "Name",
                    FieldType = AttributeTypeCode.EntityName,
                    IsValidForUpdate = false,
                    GetMethod = "",
                    PrivatePropertyName = lookup.PrivatePropertyName + "Name"
                };

                if (fields.Count(f => f.DisplayName == nameField.DisplayName) == 0)
                    fields.Add(nameField);

                if (!string.IsNullOrEmpty(lookup.LookupSingleType))
                    continue;

                var typeField = new MappingField
                {
                    Attribute = new CrmPropertyAttribute
                    {
                        IsLookup = false,
                        LogicalName = lookup.Attribute.LogicalName + "Type",
                        IsEntityReferenceHelper = true
                    },
                    DisplayName = lookup.DisplayName + "Type",
                    HybridName = lookup.HybridName + "Type",
                    FieldType = AttributeTypeCode.EntityName,
                    IsValidForUpdate = false,
                    GetMethod = "",
                    PrivatePropertyName = lookup.PrivatePropertyName + "Type"
                };

                if (fields.Count(f => f.DisplayName == typeField.DisplayName) == 0)
                    fields.Add(typeField);
            }
        }
        private static void AddEnityImageCRM2013(List<MappingField> fields)
        {
            
            if (!fields.Any(f => f.DisplayName.Equals("EntityImageId")))
                return;

            var image = new MappingField {
                    Attribute = new CrmPropertyAttribute
                    {
                        IsLookup = false,
                        LogicalName = "entityimage",
                        IsEntityReferenceHelper = false
                    },
                    DisplayName = "EntityImage",
                    HybridName = "EntityImage",
                    TargetTypeForCrmSvcUtil = "byte[]",
                    IsValidForUpdate = true,
                    Description = "",  // TODO there is an Description for this entityimage, Need to figure out how to read it from the server
                    GetMethod = ""
            };
            SafeAddField(fields, image);

            var imageTimestamp = new MappingField
            {
                Attribute = new CrmPropertyAttribute
                {
                    IsLookup = false,
                    LogicalName = "entityimage_timestamp",
                    IsEntityReferenceHelper = false
                },
                DisplayName = "EntityImage_Timestamp",
                HybridName = "EntityImage_Timestamp",
                TargetTypeForCrmSvcUtil = "System.Nullable<long>",
                FieldType = AttributeTypeCode.BigInt,
                IsValidForUpdate = false,
                IsValidForCreate = false,
                Description = " ",  // CrmSvcUtil provides an empty description for this EntityImage_TimeStamp
                GetMethod = ""
            };
            SafeAddField(fields, imageTimestamp);

            var imageURL = new MappingField
            {
                Attribute = new CrmPropertyAttribute
                {
                    IsLookup = false,
                    LogicalName = "entityimage_url",
                    IsEntityReferenceHelper = false
                },
                DisplayName = "EntityImage_URL",
                HybridName = "EntityImage_URL",
                TargetTypeForCrmSvcUtil = "string",
                FieldType = AttributeTypeCode.String,
                IsValidForUpdate = false,
                IsValidForCreate = false,
                Description = " ",   // CrmSvcUtil provides an empty description for this EntityImage_URL
                GetMethod = ""
            };
            SafeAddField(fields, imageURL);
        }
        private static void SafeAddField(List<MappingField> fields, MappingField image)
        {
            if (!fields.Any(f => f.DisplayName == image.DisplayName))
                fields.Add(image);
        }
        public MappingRelationshipMN [] RelationshipsManyToMany { get; set; }
    }
}
