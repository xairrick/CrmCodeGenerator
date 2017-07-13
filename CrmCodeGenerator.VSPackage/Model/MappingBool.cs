using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk.Metadata;
using CrmCodeGenerator.VSPackage.Helpers;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class MappingBoolDSDFDSFDSFSDF
    {
        public string DisplayName { get; set; }
        public MapperEnumItem[] Items { get; set; }

        public static MappingBool Parse(BooleanAttributeMetadata twoOption)
        {
            var enm = new MappingBool();
            enm.DisplayName = Naming.GetProperVariableName(Naming.GetProperVariableName(twoOption.SchemaName));
            enm.Items = new MapperEnumItem[2];
            enm.Items[0] = MapBoolOption(twoOption.OptionSet.TrueOption);
            enm.Items[1] = MapBoolOption(twoOption.OptionSet.FalseOption);
            RenameDuplicates(enm);

            return enm;
        }

        private static void RenameDuplicates(MappingBool enm)
        {
            Dictionary<string, int> duplicates = new Dictionary<string, int>();
            foreach (var i in enm.Items)
                if (duplicates.ContainsKey(i.Name))
                {
                    duplicates[i.Name] = duplicates[i.Name] + 1;
                    i.Name += "_" + duplicates[i.Name];
                }
                else
                    duplicates[i.Name] = 1;
        }

        private static MapperEnumItem MapBoolOption(OptionMetadata option)
        {
            var results = new MapperEnumItem()
            {
                Attribute = new CrmPicklistAttribute()
                {
                    DisplayName = option.Label.UserLocalizedLabel.Label,
                    Value = (int)option.Value
                },
                Name = Naming.GetProperVariableName(option.Label.UserLocalizedLabel.Label)
            };
            return results;
        }
    }
}
