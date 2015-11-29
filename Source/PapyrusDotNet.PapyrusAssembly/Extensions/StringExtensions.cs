using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PapyrusDotNet.PapyrusAssembly.Classes;

namespace PapyrusDotNet.PapyrusAssembly.Extensions
{
    public static class StringExtensions
    {
        public static PapyrusStringRef Ref(this string s, PapyrusAssemblyDefinition assembly)
        {
            return new PapyrusStringRef(assembly, s);
        }
    }
}
