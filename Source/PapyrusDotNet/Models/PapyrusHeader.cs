using System.Linq;
using System.Text;

namespace PapyrusDotNet.Models
{
    public class PapyrusHeader
    {
        public PapyrusInfo Info { get; set; }
        public PapyrusUserFlags UserFlagRef { get; set; }

        public PapyrusHeader()
        {
            Info = new PapyrusInfo();
            UserFlagRef = new PapyrusUserFlags();
        }
    }
}
