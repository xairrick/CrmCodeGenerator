using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingRelationshipMN : ICloneable
    {
        public CrmRelationshipAttribute Attribute { get; set; }

        public string DisplayName { get; set; }
        public string SchemaName { get; set; }
        public string HybridName { get; set; }
        public string ForeignKey { get; set; }
        public string PrivateName { get; set; }
        public string EntityRole { get; set; }
        public string Type {get; set; }
        public bool IsSelfReferenced { get; set; }
        public MappingEntity ToEntity { get; set; }

        public static MappingRelationshipMN Parse(ManyToManyRelationshipMetadata rel, string ThisEntityLogicalName)
        {
            var result = new MappingRelationshipMN();
            if (rel.Entity1LogicalName == ThisEntityLogicalName)
            {
                result.Attribute = new CrmRelationshipAttribute
                {
                    FromEntity = rel.Entity1LogicalName,
                    FromKey = rel.Entity1IntersectAttribute,
                    ToEntity = rel.Entity2LogicalName,
                    ToKey = rel.Entity2IntersectAttribute,
                    IntersectingEntity = rel.IntersectEntityName
                };
            }
            else
            {
                result.Attribute = new CrmRelationshipAttribute
                {
                    ToEntity = rel.Entity1LogicalName,
                    ToKey = rel.Entity1IntersectAttribute,
                    FromEntity = rel.Entity2LogicalName,
                    FromKey = rel.Entity2IntersectAttribute,
                    IntersectingEntity = rel.IntersectEntityName
                };
            }

            result.EntityRole = "null";
            result.SchemaName = Naming.GetProperVariableName(rel.SchemaName);
            result.DisplayName = Naming.GetProperVariableName(rel.SchemaName);
            if (rel.Entity1LogicalName == rel.Entity2LogicalName && rel.Entity1LogicalName == ThisEntityLogicalName)
            {
                result.DisplayName = "Referenced" + result.DisplayName;
                result.EntityRole = "Microsoft.Xrm.Sdk.EntityRole.Referenced";
                result.IsSelfReferenced = true;
            }
            if (result.DisplayName == ThisEntityLogicalName)
            {
                result.DisplayName += "1";   // this is what CrmSvcUtil does
            }

            result.HybridName = Naming.GetProperVariableName(rel.SchemaName) + "_NN";  
            result.PrivateName = "_nn" + Naming.GetEntityPropertyPrivateName(rel.SchemaName);
            result.ForeignKey =  Naming.GetProperVariableName(result.Attribute.ToKey);
            result.Type = Naming.GetProperVariableName(result.Attribute.ToEntity);

            return result;
        }

        public object Clone()
        {
            var newPerson = (MappingRelationshipMN)this.MemberwiseClone();
            newPerson.Attribute = (CrmRelationshipAttribute)this.Attribute.Clone();
            return newPerson;
        }
    }
}
