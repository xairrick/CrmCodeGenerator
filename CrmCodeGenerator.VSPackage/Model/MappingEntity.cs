using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string DisplayName
        {
            get;
            set;
        }
        public string HybridName { get; set; }
        public string StateName { get; set; }
        public MappingField PrimaryKey { get; set; }
        public string PrimaryKeyProperty
        {
            get;
            set;
        }

        public string Plural
        {
            get
            {
                return Naming.GetPluralName(DisplayName);
            }
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

            var fields = entityMetadata.Attributes
                .Where(a => !(a.LogicalName.EndsWith("_base") && a.AttributeType == AttributeTypeCode.Money) && a.AttributeType != AttributeTypeCode.EntityName && a.AttributeOf == null)
                .Select(a => MappingField.Parse(a, entity)).ToList();

            fields.ForEach(f =>
                    {
                        if (f.DisplayName == entity.DisplayName)
                            f.DisplayName += "1";
                        //f.HybridName = Naming.GetProperHybridFieldName(f.DisplayName, f.Attribute);
                    }
                );

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
                    HybridName = lookup.HybridName  + "Name",
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

            entity.Fields = fields.ToArray();
            entity.States = entityMetadata.Attributes.Where(a => a is StateAttributeMetadata).Select(a => MappingEnum.Parse(a as EnumAttributeMetadata)).FirstOrDefault();
            entity.Enums = entityMetadata.Attributes
                .Where(a => a is PicklistAttributeMetadata || a is StateAttributeMetadata || a is StatusAttributeMetadata)
                .Select(a => MappingEnum.Parse(a as EnumAttributeMetadata)).ToArray();
                //.Select(a => MapperEnum.Parse(a as PicklistAttributeMetadata)).ToArray();

            entity.PrimaryKey = entity.Fields.First(f => f.Attribute.LogicalName == entity.Attribute.PrimaryKey);
            entity.PrimaryKeyProperty = entity.PrimaryKey.DisplayName;

            entity.RelationshipsOneToMany = entityMetadata.OneToManyRelationships.Select(r =>
                MappingRelationship1N.Parse(r, entity.Fields)).ToArray();

            entity.RelationshipsManyToOne = entityMetadata.ManyToOneRelationships.Select(r =>
                MappingRelationshipN1.Parse(r, entity.Fields)).ToArray();

            entity.RelationshipsManyToMany = entityMetadata.ManyToManyRelationships.Select(r =>
                MappingRelationshipMN.Parse(r)).ToArray();

            return entity;
        }

        public MappingRelationshipN1[] RelationshipsManyToMany { get; set; }
    }
}
