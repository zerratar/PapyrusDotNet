namespace PapyrusDotNet.Models
{
    using System;
    using System.Collections.Generic;

    public class PapyrusUserFlags
    {
        public Dictionary<string, int> Flags { get; set; }

        public PapyrusUserFlags()
        {
            Flags = new Dictionary<string, int>();
            Flags.Add("conditional", 1);
            Flags.Add("hidden", 0);
        }

        public override string ToString()
        {
            string output = "";
            output += ".userFlagsRef" + Environment.NewLine;
            foreach (var flag in Flags)
            {
                output += "\t.flag " + flag.Key + " " + flag.Value + Environment.NewLine;
            }
            output += ".endUserFlagsRef" + Environment.NewLine;
            return output;
        }
    }
}