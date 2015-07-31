namespace Example1
{
    public class DelegateTesting
    {
       

        // -----------
        // The following code works.
        // -----------
        public delegate void HelloThereDelegate();
        public void UtilizeDelegate()
        {
            HelloThereDelegate awesome = () =>
            {
                PapyrusDotNet.Core.Debug.Trace("Awesome was used!", 0);
            };

            HelloThereDelegate secondAwesome = () =>
            {
                PapyrusDotNet.Core.Debug.Trace("Second awesome was used!", 0);
            };


            awesome();

            secondAwesome();
        }

        // -----------
        // The following code does not work.
        // -----------
        public delegate void SecondDelegate();
        public void UtilizeDelegate2()
        {
            // just by exposing one variable to be used inside a delegate,
            // a whole new class is created.. Youch! 
            string horror = "test";

            SecondDelegate awesome = () =>
            {
                PapyrusDotNet.Core.Debug.Trace("Awesome was used!" + horror, 0);
            };

            awesome();
            }
        }
}
