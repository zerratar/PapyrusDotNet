using System.Collections.Generic;
using PapyrusDotNet.Common.Interfaces;

namespace PapyrusDotNet.Converters.Papyrus2CSharp
{
    public class MultiCSharpOutput : IAssemblyOutput
    {
        private readonly IEnumerable<CSharpOutput> outputs;

        public MultiCSharpOutput(IEnumerable<CSharpOutput> outputs)
        {
            this.outputs = outputs;
        }

        public void Save(string output)
        {
            foreach (var o in outputs)
            {
                o.Save(output);
            }
        }
    }
    public class CSharpOutput : IAssemblyOutput
    {
        private readonly string outputFileName;
        private readonly string outputFileContent;

        public CSharpOutput(string outputFileName, string outputFileContent)
        {
            this.outputFileName = outputFileName;
            this.outputFileContent = outputFileContent;
        }

        public void Save(string output)
        {
            var filePath =
                System.IO.Path.Combine(output, outputFileName);
            System.IO.File.WriteAllText(filePath, outputFileContent);
        }
    }
}