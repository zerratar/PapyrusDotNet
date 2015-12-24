#if false
using PapyrusDotNet.Core;

namespace Fallout4Example
{
    public class LoopTests
    {
        int[] array;

        // Works
        public void ForEachTest()
        {
            foreach (var i in array)
            {
                Debug.MessageBox("i = " + i);
            }
        }

        // Works
        public void ForTest()
        {
            for (int index = 0; index < array.Length; index++)
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
            }
        }

        // Works
        public void WhileTest()
        {
            int index = 0;
            while (index < array.Length)
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
                index++;
            }
        }

        // Works
        public void DoWhileTest()
        {
            int index = 0;
            do
            {
                var i = array[index];
                Debug.MessageBox("i = " + i);
            } while (index++ < array.Length);
        }
    }
}
#endif