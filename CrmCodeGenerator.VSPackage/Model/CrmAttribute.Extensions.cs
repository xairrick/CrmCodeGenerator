using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CrmCodeGenerator.VSPackage.Model
{
    public static class CrmAttributeExtensions
    {
        public static string MergeCode(this Attribute attribute)
        {
            var type = attribute.GetType();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var values =
                properties.ToDictionary(
                    p => p.Name,
                    p => p.GetValue(attribute, new object[0]));

            var typeName = type.Name;

            if (typeName.EndsWith("Attribute"))
                typeName = typeName.Substring(0, typeName.Length - 9);

            var valuesString = values.Where(v =>
                !(v.Value == null ||
                  object.Equals(v.Value, "") ||
                  v.Value.Equals(GetDefaultValue(v.Value.GetType())))).Select(v =>
                            string.Format("{0} = {1}", v.Key, FormatValue(v.Value))).ToArray();

            return
                string.Format("[{0}({1})]",
                    typeName,
                    string.Join(", ", valuesString
                        ));
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        private static string FormatValue(object value)
        {
            if (value.GetType() == typeof(bool))
                return (bool)value ? "true" : "false";

            if (value.GetType() == typeof(string))
                return string.Format("\"{0}\"", value);

            return value.ToString();
        }
    }
}
