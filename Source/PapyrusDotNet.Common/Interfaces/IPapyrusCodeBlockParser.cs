using System.Collections.Generic;

namespace PapyrusDotNet.Common.Interfaces
{
    public interface IPapyrusCodeBlockParser : IUtility
    {
        IEnumerable<IPapyrusCodeBlock> ParseCodeBlocks(IEnumerable<string> rows);
        IPapyrusCodeBlock ParseCodeBlock(string codeBlock);
    }
}