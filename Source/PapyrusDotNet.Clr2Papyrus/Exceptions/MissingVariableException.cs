using System;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Exceptions
{
    public class MissingVariableException : Exception
    {
        public MissingVariableException(string variableName)
        {
            Name = variableName;
        }
        public string Name { get; set; }
    }
}