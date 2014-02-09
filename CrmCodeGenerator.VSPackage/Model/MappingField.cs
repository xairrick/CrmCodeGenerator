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
        public CrmPropertyAttribute Attribute { get; set; }

        public MappingEnum EnumData { get; set; }
        public AttributeTypeCode FieldType { get; set; }
        
        public string LookupSingleType { get; set; }

        public string PrivatePropertyName
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;set;
        }
        public string HybridName { get; set; }

        public static MappingField Parse(AttributeMetadata attribute)
        {
            var result = new MappingField();

            if (attribute is PicklistAttributeMetadata)
                result.EnumData =
                    MappingEnum.Parse(attribute as PicklistAttributeMetadata);

            if (attribute is LookupAttributeMetadata)
            {
                var lookup = attribute as LookupAttributeMetadata;

                if (lookup.Targets.Count() == 1)
                    result.LookupSingleType = lookup.Targets[0];
            }

            ParseMinMaxValues(attribute, result);
            
            if (attribute.AttributeType != null)
                result.FieldType = attribute.AttributeType.Value;

            result.IsPrimaryKey = attribute.IsPrimaryId == true;

            result.DisplayName = Naming.GetProperVariableName(attribute.SchemaName);
            result.PrivatePropertyName = Naming.GetEntityPropertyPrivateName(attribute.SchemaName);
            

            result.IsUpdatable = attribute.IsValidForUpdate == true;

            result.IsRequired = attribute.RequiredLevel != null && attribute.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired;

            result.Attribute =
                new CrmPropertyAttribute
                {
                    LogicalName = attribute.LogicalName,
                    IsLookup = attribute.AttributeType == AttributeTypeCode.Lookup || attribute.AttributeType == AttributeTypeCode.Customer
                };

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

        bool IsPrimaryKey { get; set; }

        public bool IsUpdatable { get; set; }

        public bool IsRequired { get; set; }

        public int? MaxLength { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }

        public string TargetTypeForCrmSvcUtil
        {
            get
            {
                if (IsPrimaryKey)
                    return "Nullable<Guid>";

                switch (FieldType)
                {
                    case AttributeTypeCode.Picklist:
                        return "OptionSetValue";

                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Integer:
                        return "int";

                    case AttributeTypeCode.Boolean:
                        return "bool";

                    case AttributeTypeCode.DateTime:
                        return "DateTime";

                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Money:
                        return "Money";

                    case AttributeTypeCode.Double:
                        return "double";

                    case AttributeTypeCode.Uniqueidentifier:
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                    case AttributeTypeCode.Customer:
                        return "EntityReference";

                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        return "OptionSetValue";

                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.Virtual:
                    case AttributeTypeCode.EntityName:
                    case AttributeTypeCode.String:
                        return "string";

                    default:
                        return "object";
                }
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

                if (methodName == "" || !this.IsUpdatable)
                    return "";

                if (FieldType == AttributeTypeCode.Picklist)
                    return string.Format("{0}(\"{1}\", (int?)value);", methodName, this.Attribute.LogicalName);

                if (FieldType == AttributeTypeCode.Lookup || FieldType == AttributeTypeCode.Customer)
                    if (string.IsNullOrEmpty(LookupSingleType))
                        return string.Format("{0}(\"{1}\", {2}Type, value);", methodName, Attribute.LogicalName,this.DisplayName);
                    else
                        return string.Format("{0}(\"{1}\", \"{2}\", value);", methodName, Attribute.LogicalName, this.LookupSingleType);

                return string.Format("{0}(\"{1}\", value);", methodName, this.Attribute.LogicalName);
            }
        }
    }
}
