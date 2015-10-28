using System;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
    public class InitialValueAttribute : Attribute
    {
        public object InitialValue { get; set; }
        public InitialValueAttribute(object value)
        {
            InitialValue = value;
        }
    }
}
