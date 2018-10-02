using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingField
    {
        public AttributeMetadata AttributeMetadata { get; set; }
        public CrmPropertyAttribute Attribute { get; set; }
        public MappingEntity Entity { get; set; }
        public string AttributeOf { get; set; }
        public MappingEnum EnumData { get; set; }
        public AttributeTypeCode FieldType { get; set; }
        public string FieldTypeString { get; set; }
        public bool IsValidForCreate { get; set; }
        public bool IsValidForRead { get; set; }
        public bool IsValidForUpdate { get; set; }
        public bool IsActivityParty { get; set; }
        public bool IsStateCode { get; set; }
        public bool IsDeprecated { get; set; }
        public bool IsOptionSet { get; private set; }
        public bool IsTwoOption { get; private set; }
        public string DeprecatedVersion { get; set; }
        public string LookupSingleType { get; set; }
        bool IsPrimaryKey { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public string PrivatePropertyName { get; set; }
        public string DisplayName { get; set; }
        public string HybridName { get; set; }
        public string LogicalName { get; set; }
        public string StateName { get; set; }
        public string TargetTypeForCrmSvcUtil { get; set; }
        public string Description { get; set; }
        public string DescriptionXmlSafe
        {
            get
            {
                return Naming.XmlEscape(Description);
            }
        }

        public string Label { get; set; }
        public MappingField()
        {
            IsValidForUpdate = false;
            IsValidForCreate = false;
            IsDeprecated = false;
            Description = "";
        }
        public static MappingField Parse(AttributeMetadata attribute, MappingEntity entity)
        {
            var result = new MappingField();
            result.Entity = entity;
            result.AttributeOf = attribute.AttributeOf;
            result.IsValidForCreate = (bool)attribute.IsValidForCreate;
            result.IsValidForRead = (bool)attribute.IsValidForRead;
            result.IsValidForUpdate = (bool)attribute.IsValidForUpdate;
            result.IsActivityParty = attribute.AttributeType == AttributeTypeCode.PartyList ? true : false;
            result.IsStateCode = attribute.AttributeType == AttributeTypeCode.State ? true : false;
            result.IsOptionSet = attribute.AttributeType == AttributeTypeCode.Picklist;
            result.IsTwoOption = attribute.AttributeType == AttributeTypeCode.Boolean;
            result.DeprecatedVersion = attribute.DeprecatedVersion;
            result.IsDeprecated = !string.IsNullOrWhiteSpace(attribute.DeprecatedVersion);

            if (attribute is PicklistAttributeMetadata)
                result.EnumData =
                    MappingEnum.Parse(attribute as PicklistAttributeMetadata);

            if (attribute is MultiSelectPicklistAttributeMetadata)
                result.EnumData =
                    MappingEnum.Parse(attribute as MultiSelectPicklistAttributeMetadata);

            if (attribute is LookupAttributeMetadata)
            {
                var lookup = attribute as LookupAttributeMetadata;

                if (lookup.Targets.Count() == 1)
                    result.LookupSingleType = lookup.Targets[0];
            }

            ParseMinMaxValues(attribute, result);

            if (attribute.AttributeType != null)
                result.FieldType = attribute.AttributeType.Value;
            if (attribute.AttributeTypeName != null)
            {
                result.AttributeTypeName = attribute.AttributeTypeName.Value;
            }

            result.IsPrimaryKey = attribute.IsPrimaryId == true;

            result.LogicalName = attribute.LogicalName;
            result.DisplayName = Naming.GetProperVariableName(attribute);
            result.PrivatePropertyName = Naming.GetEntityPropertyPrivateName(attribute.SchemaName);
            result.HybridName = Naming.GetProperHybridFieldName(result.DisplayName, result.Attribute);

            if (attribute.Description != null)
                if (attribute.Description.UserLocalizedLabel != null)
                    result.Description = attribute.Description.UserLocalizedLabel.Label;

            if (attribute.DisplayName != null)
                if (attribute.DisplayName.UserLocalizedLabel != null)
                    result.Label = attribute.DisplayName.UserLocalizedLabel.Label;

            result.IsRequired = attribute.RequiredLevel != null && attribute.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired;

            result.Attribute =
                new CrmPropertyAttribute
                {
                    LogicalName = attribute.LogicalName,
                    IsLookup = attribute.AttributeType == AttributeTypeCode.Lookup || attribute.AttributeType == AttributeTypeCode.Customer
                };
            result.TargetTypeForCrmSvcUtil = GetTargetType(result);
            result.FieldTypeString = result.TargetTypeForCrmSvcUtil;


            return result;
        }

        private static void ParseMinMaxValues(AttributeMetadata attribute, MappingField result)
        {
            if (attribute is StringAttributeMetadata) { result.MaxLength = (attribute as StringAttributeMetadata).MaxLength ?? -1; }
            if (attribute is MemoAttributeMetadata) { result.MaxLength = (attribute as MemoAttributeMetadata).MaxLength ?? -1; }

            if (attribute is IntegerAttributeMetadata)
            {
                var attr = attribute as IntegerAttributeMetadata;

                result.Min = attr.MinValue ?? -1;
                result.Max = attr.MaxValue ?? -1;
            }

            if (attribute is DecimalAttributeMetadata)
            {
                var attr = attribute as DecimalAttributeMetadata;

                result.Min = attr.MinValue ?? -1;
                result.Max = attr.MaxValue ?? -1;
            }

            if (attribute is MoneyAttributeMetadata)
            {
                var attr = attribute as MoneyAttributeMetadata;

                result.Min = attr.MinValue != null ? (decimal)attr.MinValue.Value : -1;
                result.Max = attr.MaxValue != null ? (decimal)attr.MaxValue.Value : -1;
            }

            if (attribute is DoubleAttributeMetadata)
            {
                var attr = attribute as DoubleAttributeMetadata;

                result.Min = attr.MinValue != null ? (decimal)attr.MinValue.Value : -1;
                result.Max = attr.MaxValue != null ? (decimal)attr.MaxValue.Value : -1;
            }
        }




        private static string GetTargetType(MappingField field)
        {
            if (field.IsPrimaryKey)
                return "System.Nullable<System.Guid>";

            switch (field.FieldType)
            {
                case AttributeTypeCode.Picklist:
                    return "Microsoft.Xrm.Sdk.OptionSetValue";
                case AttributeTypeCode.BigInt:
                    return "System.Nullable<long>";
                case AttributeTypeCode.Integer:
                    return "System.Nullable<int>";
                case AttributeTypeCode.Boolean:
                    return "System.Nullable<bool>";
                case AttributeTypeCode.DateTime:
                    return "System.Nullable<System.DateTime>";
                case AttributeTypeCode.Decimal:
                    return "System.Nullable<decimal>";
                case AttributeTypeCode.Money:
                    return "Microsoft.Xrm.Sdk.Money";
                case AttributeTypeCode.Double:
                    return "System.Nullable<double>";
                case AttributeTypeCode.Uniqueidentifier:
                    return "System.Nullable<System.Guid>";
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.Customer:
                    return "Microsoft.Xrm.Sdk.EntityReference";
                case AttributeTypeCode.State:
                    return "System.Nullable<" + field.Entity.StateName + ">";
                case AttributeTypeCode.Status:
                    return "Microsoft.Xrm.Sdk.OptionSetValue";
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.Virtual:
                case AttributeTypeCode.EntityName:
                case AttributeTypeCode.String:
                    if (field.AttributeTypeName == "MultiSelectPicklistType")
                    {
                        return "Microsoft.Xrm.Sdk.OptionSetValueCollection";
                    }
                    return "string";
                case AttributeTypeCode.PartyList:
                    return "System.Collections.Generic.IEnumerable<ActivityParty>";
                case AttributeTypeCode.ManagedProperty:
                    return "Microsoft.Xrm.Sdk.BooleanManagedProperty";

                default:
                    return "object";
            }
        }


        public string TargetType
        {
            get
            {
                if (IsPrimaryKey)
                    return "Guid";

                switch (FieldType)
                {
                    case AttributeTypeCode.Picklist:
                        return string.Format("Enums.{0}?", EnumData.DisplayName);

                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Integer:
                        return "int?";

                    case AttributeTypeCode.Boolean:
                        return "bool?";

                    case AttributeTypeCode.DateTime:
                        return "DateTime?";

                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Money:
                        return "decimal?";

                    case AttributeTypeCode.Double:
                        return "double?";

                    case AttributeTypeCode.Uniqueidentifier:
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                    case AttributeTypeCode.Customer:
                        return "Guid?";

                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        return "int";

                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.Virtual:
                    case AttributeTypeCode.EntityName:
                    case AttributeTypeCode.String:
                        if (AttributeTypeName == "MultiSelectPicklistType")
                        {
                            return "Microsoft.Xrm.Sdk.OptionSetValueCollection";
                        }
                        return "string";

                    default:
                        return "object";
                }
            }
        }

        public string GetMethod { get; set; }

        public string SetMethodCall
        {
            get
            {
                var methodName = "";

                switch (FieldType)
                {
                    case AttributeTypeCode.Picklist:
                        methodName = "SetPicklist"; break;
                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Integer:
                        methodName = "SetValue<int?>"; break;
                    case AttributeTypeCode.Boolean:
                        methodName = "SetValue<bool?>"; break;
                    case AttributeTypeCode.DateTime:
                        methodName = "SetValue<DateTime?>"; break;
                    case AttributeTypeCode.Decimal:
                        methodName = "SetValue<decimal?>"; break;
                    case AttributeTypeCode.Money:
                        methodName = "SetMoney"; break;
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.String:
                        methodName = "SetValue<string>"; break;
                    case AttributeTypeCode.Double:
                        methodName = "SetValue<double?>"; break;
                    case AttributeTypeCode.Uniqueidentifier:
                        methodName = "SetValue<Guid?>"; break;
                    case AttributeTypeCode.Lookup:
                        methodName = "SetLookup"; break;
                    //methodName = "SetLookup"; break;
                    case AttributeTypeCode.Virtual:
                        if (AttributeTypeName == "MultiSelectPicklistType")
                        {
                            return "SetValue<Microsoft.Xrm.Sdk.OptionSetValueCollection>";
                        }
                        methodName = "SetValue<string>"; break;
                    case AttributeTypeCode.Customer:
                        methodName = "SetCustomer"; break;
                    case AttributeTypeCode.Status:
                        methodName = ""; break;
                    case AttributeTypeCode.EntityName:
                        methodName = "SetEntityNameReference"; break;
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Owner:
                    default:
                        return "";
                }

                if (methodName == "" || !this.IsValidForUpdate)
                    return "";

                if (FieldType == AttributeTypeCode.Picklist)
                    return string.Format("{0}(\"{1}\", (int?)value);", methodName, this.Attribute.LogicalName);

                if (FieldType == AttributeTypeCode.Lookup || FieldType == AttributeTypeCode.Customer)
                    if (string.IsNullOrEmpty(LookupSingleType))
                        return string.Format("{0}(\"{1}\", {2}Type, value);", methodName, Attribute.LogicalName, this.DisplayName);
                    else
                        return string.Format("{0}(\"{1}\", \"{2}\", value);", methodName, Attribute.LogicalName, this.LookupSingleType);

                return string.Format("{0}(\"{1}\", value);", methodName, this.Attribute.LogicalName);
            }
        }

        public string AttributeTypeName { get; private set; }
    }
}
