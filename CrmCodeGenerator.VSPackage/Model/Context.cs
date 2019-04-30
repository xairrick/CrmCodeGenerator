using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    public class Context
    {
        public string Namespace { get; set; }

        public MappingEntity[] Entities { get; set; }

        public MappingEnum[] Enums { get; set; }
    }
}
