using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    static class IEnumerablesExtensions
    {
        public static T FirstOr<T>(this IEnumerable<T> source, T alternate)
        {
            foreach (T t in source)
                return t;
            return alternate;
        }
    }
}
