
#if false

using PapyrusDotNet.Core;

namespace Fallout4Example
{


    public class EnumTests : ObjectReference
    {

        // Works
        public enum TestEnum
        {
            A = 1,
            B = 999,
            C = 12
        }

        // Works
        public void EnumVal()
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }
        }

        // Works
        public TestEnum EnumVal2()
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }

            return test;
        }

        // Works
        public TestEnum EnumVal3(TestEnum asd)
        {
            var test = TestEnum.A;

            if (test == TestEnum.C)
            {
                Debug.MessageBox("Hehehehe");
            }

            return asd;
        }

    }
}
#endif