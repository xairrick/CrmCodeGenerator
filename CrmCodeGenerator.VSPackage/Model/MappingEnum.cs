using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingEnum
    {
        public string DisplayName { get; set; }
        public MapperEnumItem[] Items { get; set; }

        public static MappingEnum Parse(EnumAttributeMetadata picklist)
        {
            var enm = new MappingEnum
            {
                DisplayName = Naming.GetProperVariableName(Naming.GetProperVariableName(picklist.SchemaName)),
                Items =
                    picklist.OptionSet.Options.Select(
                        o => new MapperEnumItem
                        {
                            Attribute = new CrmPicklistAttribute
                            {
                                DisplayName = o.Label.UserLocalizedLabel.Label,
                                Value = o.Value ?? 1
                            },
                            Name = Naming.GetProperVariableName(o.Label.UserLocalizedLabel.Label)
                        }
                    ).ToArray()
            };

            Dictionary<string, int> duplicates = new Dictionary<string, int>();

            foreach (var i in enm.Items)
                if (duplicates.ContainsKey(i.Name))
                {
                    duplicates[i.Name] = duplicates[i.Name] + 1;
                    i.Name += "_" + duplicates[i.Name];
                }
                else
                    duplicates[i.Name] = 1;

            return enm;
        }
    }

    [Serializable]
    public class MapperEnumItem
    {
        public CrmPicklistAttribute Attribute { get; set; }
        public string Name { get; set; }
        public int Value
        {
            get
            {
                return Attribute.Value;
            }
        }
    }
}
