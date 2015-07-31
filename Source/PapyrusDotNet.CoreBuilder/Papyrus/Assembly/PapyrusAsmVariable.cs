namespace PapyrusDotNet.CoreBuilder
{
    using System;
    using System.Linq;

    public class PapyrusAsmVariable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArray { get; set; }
        public PapyrusAsmVariable(string name, string type)
        {
            this.Name = name;
            this.Type = type;
            this.IsArray = type.Contains("[]");

            if (type.Contains('.'))
            {
                var n = type.Substring(type.LastIndexOf('.'));
                this.Type = type.Remove(type.LastIndexOf('.')) + Char.ToUpper(n[0]) + n.Substring(1);
            }
            else
            {
                this.Type = Char.ToUpper(type[0]) + type.Substring(1);
            }
        }
    }
}