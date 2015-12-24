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

        public void PropertySet(int value)
        {
            var test = 9292;

            PropertyInt = 1000;

            PropertyInt = test;

            PropertyInt = value;
            
            // BUG HERE
            //for (PropertyInt = 0; PropertyInt < test; PropertyInt++)
            //{
            //    Debug.MessageBox("Hello There!");
            //}
        }

        public int PropertyGet()
        {
            var test = PropertyInt;

            // BUG HERE
            //for (var i = 0; i < PropertyInt; i++)
            //{
            //    Debug.MessageBox("Hello There: " + i);
            //}

            return test;
        }
    }
}