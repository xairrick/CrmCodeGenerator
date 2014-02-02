using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    public class MappingRelationshipMN
    {
        public CrmRelationshipAttribute Attribute { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public string ForeignKey
        {
            get
            {
                return Naming.GetProperVariableName(Attribute.ToKey);
            }
        }

        public string PrivateName
        {
            get;
            set;
        }

        public string Type
        {
            get
            {
                return Naming.GetProperVariableName(Attribute.FromEntity);
            }
        }

        public static MappingRelationshipN1 Parse(ManyToManyRelationshipMetadata rel)
        {
            return new MappingRelationshipN1
            {
                Attribute = new CrmRelationshipAttribute
                {
                    //FromEntity = rel.ReferencedEntity,
                    //FromKey = rel.ReferencedAttribute,
                    //ToEntity = rel.ReferencingEntity,
                    //ToKey = rel.ReferencingAttribute,
                    IntersectingEntity = rel.IntersectEntityName
                },
                DisplayName = Naming.GetProperVariableName(rel.SchemaName),
                PrivateName = "_nn" + Naming.GetEntityPropertyPrivateName(rel.SchemaName)
            };
        }
    }
}
