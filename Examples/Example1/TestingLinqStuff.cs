using PapyrusDotNet.Core;
using PapyrusDotNet.System.Linq;

namespace Example1
{
    public class TestingLinqStuff : ObjectReference
    {
        public void FirstTest()
        {
            var listOfStrings = new[] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

            var numberFive = listOfStrings.FirstOrDefault(j => j == "5");

            Debug.Trace("We selected number " + numberFive, 0);

            var lastNumber = listOfStrings.LastOrDefault(l => l != "");

            Debug.Trace("Our last number is " + lastNumber, 0);
        }
    }
}