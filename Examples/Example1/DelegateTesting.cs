using PapyrusDotNet.Core;

namespace Example1
{
    public class DelegateTesting
    {
        public delegate void AnotherDelegate(string input);


        // -----------
        // The following code works.
        // -----------
        public delegate void HelloThereDelegate();

        public delegate void HorribleDelegate();

        public delegate void SecondDelegate();

        public void UtilizeDelegate()
        {
            HelloThereDelegate awesome = () => { Debug.Trace("Awesome was used!", 0); };

            HelloThereDelegate secondAwesome = () => { Debug.Trace("Second awesome was used!", 0); };

            awesome();

            secondAwesome();
        }

        public void UtilizeDelegate2()
        {
            var whatHorrorLiesHere = "test123";

            SecondDelegate arrr = () => { Debug.Trace("UtilizeDelegate2 was used!" + whatHorrorLiesHere, 0); };

            arrr();
        }

        public void UtilizeDelegate3()
        {
            var horror = "test";

            AnotherDelegate awesome = s => { Debug.Trace("UtilizeDelegate3 was used!" + s, 0); };

            awesome(horror);
        }

        public void UtilizeDelegate4()
        {
            var magic = "helloo";
            HorribleDelegate awesome = () =>
            {
                AnotherDelegate awe2 = s => { Debug.Trace("UtilizeDelegate4 was used!" + s, 0); };

                awe2(magic);
            };
            awesome();
        }
    }
}