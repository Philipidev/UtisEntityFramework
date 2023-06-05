using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtisEntityFramework
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string name) =>
            char.ToLowerInvariant(name[0]) + name[1..];
    }
}
