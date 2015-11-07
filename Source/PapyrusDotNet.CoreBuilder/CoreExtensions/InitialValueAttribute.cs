using System;

namespace PapyrusDotNet.CoreBuilder.CoreExtensions
{
    public class InitialValueAttribute : Attribute
    {
        public InitialValueAttribute(object value)
        {
            InitialValue = value;
        }

        public object InitialValue { get; set; }
    }
}