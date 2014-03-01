using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingRelationshipMN
    {
        public CrmRelationshipAttribute Attribute { get; set; }

        public string DisplayName { get; set; }
        public string HybridName { get; set; }
        public string ForeignKey { get; set; }
        public string PrivateName { get; set; }
        public string Type {get; set; }
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

            result.DisplayName = Naming.GetProperVariableName(rel.SchemaName);
            if (result.DisplayName == ThisEntityLogicalName)
                result.DisplayName += "1";   // this is what CrmSvcUtil does
            result.HybridName = Naming.GetProperVariableName(rel.SchemaName) + "_NN";  
            result.PrivateName = "_nn" + Naming.GetEntityPropertyPrivateName(rel.SchemaName);
            result.ForeignKey =  Naming.GetProperVariableName(result.Attribute.ToKey);
            result.Type = Naming.GetProperVariableName(result.Attribute.ToEntity);

            return result;
        }
    }
}
