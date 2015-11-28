using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PapyrusDotNet.Common.Extensions
{
    public enum FindFilterOptions
    {
        NameEndsWith,
        NameStartsWith,
        NameContains
    }
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> FindTypes(this Assembly asm, string search, FindFilterOptions options = FindFilterOptions.NameEndsWith)
        {
            switch (options)
            {
                case FindFilterOptions.NameEndsWith:
                    return from t in asm.GetTypes()
                           where t.Name.ToLower().EndsWith(search)
                           select t;
                case FindFilterOptions.NameContains:
                    return from t in asm.GetTypes()
                           where t.Name.ToLower().Contains(search)
                           select t;
                case FindFilterOptions.NameStartsWith:
                    return from t in asm.GetTypes()
                           where t.Name.ToLower().StartsWith(search)
                           select t;
            }
            return from t in asm.GetTypes()
                   where t.Name.ToLower().EndsWith(search)
                   select t;
        }
    }
}
