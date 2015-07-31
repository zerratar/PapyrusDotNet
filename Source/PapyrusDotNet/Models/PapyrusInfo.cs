namespace PapyrusDotNet.Models
{
    using System;

    using PapyrusDotNet.Common;

    public class PapyrusInfo
    {
        public string Source { get; set; }
        public int ModifyTime { get; set; }
        public int CompileTime { get; set; }
        public string User { get; set; }
        public string Computer { get; set; }

        public PapyrusInfo()
        {
            Source = "PapyrusDotNet-Generated.psc";
            ModifyTime = Utility.ConvertToTimestamp(DateTime.Now);
            CompileTime = ModifyTime;
            User = Environment.UserName;
            Computer = Environment.MachineName;
        }

        public override string ToString()
        {
            string output = "";
            output += ".info" + Environment.NewLine;
            output += "\t.source \"" + Source + "\"" + Environment.NewLine;
            output += "\t.modifyTime " + ModifyTime + Environment.NewLine;
            output += "\t.compileTime " + CompileTime + Environment.NewLine;
            output += "\t.user \"" + User + "\"" + Environment.NewLine;
            output += "\t.computer \"" + Computer + "\"" + Environment.NewLine;
            output += ".endInfo" + Environment.NewLine;
            return output;
        }
    }
}