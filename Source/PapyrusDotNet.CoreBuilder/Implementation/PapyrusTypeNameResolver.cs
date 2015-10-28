using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PapyrusDotNet.CoreBuilder.Interfaces;

namespace PapyrusDotNet.CoreBuilder.Implementation
{
    public class PapyrusTypeNameResolver : IPapyrusTypeNameResolver
    {
        public string Resolve(string typeName)
        {
            if (typeName.Contains('.')) typeName = typeName.Split('.').LastOrDefault();
            if (typeName != null)
            {
                var typeNameLower = typeName.ToLower();

                /*if (p.EndsWith("[]"))
            {
                pl = pl.Replace("[]", "");
            }*/

                if (typeNameLower == "boolean")
                    return "bool";
                if (typeNameLower == "none")
                    return "void";

                if (typeNameLower == "float" || typeNameLower == "int" || typeNameLower == "bool" || typeNameLower == "string") return typeNameLower;
            }

            return typeName;
        }
    }
}
