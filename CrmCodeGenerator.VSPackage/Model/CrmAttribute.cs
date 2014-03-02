using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmCodeGenerator.VSPackage.Model
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CrmEntityAttribute : Attribute
    {
        public string LogicalName { get; set; }

        public string PrimaryKey { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CrmRelationshipAttribute : Attribute, ICloneable
    {
        public string FromEntity { get; set; }

        public string ToEntity { get; set; }

        public string FromKey { get; set; }

        public string ToKey { get; set; }

        public string IntersectingEntity { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CrmPropertyAttribute : Attribute
    {
        public string LogicalName { get; set; }
        public bool IsLookup { get; set; }
        public bool IsEntityReferenceHelper { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CrmPicklistAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public int Value { get; set; }

    }
}
