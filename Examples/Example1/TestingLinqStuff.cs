using PapyrusDotNet.System.Linq;

namespace Example1
{
    public class TestingLinqStuff
    {
        public void FirstTest()
        {
            var listOfStrings = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            //  Console.WriteLine(listOfStrings[0]);



            var theZero = listOfStrings.FirstOrDefault(j => j == "5");

            // Anonymous types are not supported yet.

            //foreach (var res in result)
            //{
            //    Console.WriteLine(res);
            //}

        }
    }
}
