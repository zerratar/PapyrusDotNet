//     This file is part of PapyrusDotNet.
// 
//     PapyrusDotNet is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     PapyrusDotNet is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
//  
//     Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com

#region

using PapyrusDotNet.Core;

#endregion

namespace Fallout4Example
{
    public class StructInstructionsTest : ObjectReference
    {
        public struct MyTestStruct
        {
            public string StructString;
            public int StructInteger;
        }

        public int MethodIntValue()
        {
            return 0;
        }

        public int PropertyIntValue => 0;

        public int SetPropertyIntValue { get; set; }

        public int FieldIntValue = 0;

        private MyTestStruct myTestStruct_var;

        private Form form;

        private ObjectReference objRef;

        //// Works
        //public bool Is_Test()
        //{
        //    var isit = form is ObjectReference;
        //    return isit;
        //}

        public bool Is2_Test()
        {
            // Whenever the object is guaranteed to be of the same type, 
            // it doesnt work (currently)
            var isit = objRef is Form;

            //// Works
            //var isit = objRef != null;
            return isit;
        }


        //// Does not work
        //public Form As_Test()
        //{
        //    var isit = objRef as Form;
        //    return isit;
        //}


        public void StructSet_StructInteger(int value)
        {
            var test = 9292;

            // Works
            myTestStruct_var.StructInteger = 1000;

            // Works
            myTestStruct_var.StructInteger = test;

            // Works
            myTestStruct_var.StructInteger = FieldIntValue;

            // Works
            myTestStruct_var.StructInteger = value;

            // Works
            myTestStruct_var.StructInteger = MethodIntValue();

            // Works
            myTestStruct_var.StructInteger = PropertyIntValue;
        }


        // This is not ready:
        public int StructGet_StructInteger()
        {
            // Works
            var test = myTestStruct_var.StructInteger;

            // Works
            FieldIntValue = myTestStruct_var.StructInteger;

            // Works
            SetPropertyIntValue = myTestStruct_var.StructInteger;

            // Works
            StructSet_StructInteger(myTestStruct_var.StructInteger);

            // Works
            return myTestStruct_var.StructInteger;
            // return test;
        }
    }

#if false
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

            for (PropertyInt = 0; PropertyInt < test; PropertyInt++)
            {
                Debug.MessageBox("Hello There!");
            }
        }

        public int PropertyGet()
        {
            var test = PropertyInt;

            for (var i = 0; i < PropertyInt; i++)
            {
                Debug.MessageBox("Hello There: " + i);
            }

            return test;
        }
    }
#endif
}