#if false
using PapyrusDotNet.Core;

namespace Fallout4Example
{
    public class PropertyGetSetTests
    {
        public struct MyTestStruct
        {
            public int StructInteger;
        }

        MyTestStruct s;

        public int PropertyInt { get; set; }

        // Works
        public void PropertySet(int value)
        {
            var test = 9292;

            // Works
            PropertyInt = 1000;

            // Works
            PropertyInt = test;

            // Works
            PropertyInt = value;

            // Works
            for (PropertyInt = 0; PropertyInt < test; PropertyInt++)
            {
                Debug.MessageBox("Hello There!");
            }
        }

        // Works
        public int PropertyGet()
        {
            // Works
            var test = PropertyInt;

            // Works
            for (var i = 0; i < PropertyInt; i++)
            {
                Debug.MessageBox("Hello There: " + i);
            }

            return test;
        }
    }
}
#endif