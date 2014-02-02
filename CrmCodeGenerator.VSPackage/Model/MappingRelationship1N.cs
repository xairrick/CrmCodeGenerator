using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingRelationship1N
    {
        public CrmRelationshipAttribute Attribute { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public string ForeignKey
        {
            get;
            set;
        }

        public string PrivateName
        {
            get; set;
        }
        
        public string Type
        {
            get
            {
                return Naming.GetProperVariableName(Attribute.ToEntity);
            }
        }

        public static MappingRelationship1N Parse(OneToManyRelationshipMetadata rel, MappingField[] properties)
        {
            var propertyName =
                properties.First(p => p.Attribute.LogicalName.ToLower() == rel.ReferencedAttribute.ToLower()).DisplayName;

            return new MappingRelationship1N
            {
                Attribute = new CrmRelationshipAttribute
                {
                    FromEntity = rel.ReferencedEntity,
                    FromKey = rel.ReferencedAttribute,
                    ToEntity = rel.ReferencingEntity,
                    ToKey = rel.ReferencingAttribute,
                    IntersectingEntity = ""
                },
                ForeignKey = propertyName,
                DisplayName = Naming.GetPluralName(Naming.GetProperVariableName(rel.SchemaName)),
                PrivateName = Naming.GetEntityPropertyPrivateName(rel.SchemaName)
            };
        }
    }
}
