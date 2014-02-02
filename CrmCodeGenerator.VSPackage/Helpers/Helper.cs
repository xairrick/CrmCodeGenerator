using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public class Naming
    {
        public static string Clean(string p)
        {
            string result = "";
            if (!string.IsNullOrEmpty(p))
            {
                p = p.Trim();
                p = Normalize(p);

                if (!string.IsNullOrEmpty(p))
                {
                    StringBuilder sb = new StringBuilder();
                    int start = 0;
                    if (!char.IsLetter(p[0]))
                    {
                        sb.Append("_");
                    }

                    for (int i = start; i < p.Length; i++)
                    {
                        if ((char.IsDigit(p[i]) || char.IsLetter(p[i])) && !string.IsNullOrEmpty(p[i].ToString()))
                        {
                            sb.Append(p[i]);
                        }
                    }

                    result = sb.ToString();
                }

                result = ReplaceKeywords(result);

                result = Regex.Replace(result, "[^A-Za-z0-9_]", "");
            }

            return result;
        }

        private static string Normalize(string regularString)
        {
            string normalizedString = regularString.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder(normalizedString);

            for (int i = 0; i < sb.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(sb[i]) == UnicodeCategory.NonSpacingMark)
                    sb.Remove(i, 1);
            }
            regularString = sb.ToString();

            return regularString.Replace("æ", "");
        }


        private static string ReplaceKeywords(string p)
        {
            if (p.Equals("public", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("private", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("event", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("single", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("new", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("partial", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("to", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("error", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("readonly", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("case", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("object", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("global", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("namespace", StringComparison.InvariantCultureIgnoreCase)
                || p.Equals("abstract", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                return "__" + p;
            }

            return p;
        }


        public static string CapitalizeWord(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "";

            return p.Substring(0, 1).ToUpper() + p.Substring(1);
        }

        private static string DecapitalizeWord(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "";

            return p.Substring(0, 1).ToLower() + p.Substring(1);
        }

        public static string Capitalize(string p, bool capitalizeFirstWord)
        {
            var parts = p.Split(' ', '_');

            for (int i = 0; i < parts.Count(); i++)
                parts[i] = i != 0 || capitalizeFirstWord ? CapitalizeWord(parts[i]) : DecapitalizeWord(parts[i]);

            return string.Join("_", parts);
        }

        public static string GetProperEntityName(string entityName)
        {
                return Clean(Capitalize(entityName, true));
        }
        public static string GetProperHybridName(string displayName, string logicalName)
        {
            if (logicalName.Contains("_"))
            {
                return logicalName;
            }
            else
            {
                return Clean(Capitalize(displayName, true));
            }
        }
        public static string GetProperHybridFieldName(string displayName, CrmCodeGenerator.VSPackage.Model.CrmPropertyAttribute attribute)
        {
            if (attribute != null && attribute.LogicalName.Contains("_"))
            {
                return attribute.LogicalName;
            }
            else
            {
                return displayName;
            }
        }

        public static string GetProperVariableName(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return "Empty";

            return Clean(Capitalize(p, true));
        }

        public static string GetPluralName(string p)
        {
            if (p.EndsWith("y"))
                return p.Substring(0, p.Length - 1) + "ies";

            if (p.EndsWith("s"))
                return p;

            return p + "s";
        }

        public static string GetEntityPropertyPrivateName(string p)
        {
            return "_" + Clean(Capitalize(p, false));
        }
    }
}
