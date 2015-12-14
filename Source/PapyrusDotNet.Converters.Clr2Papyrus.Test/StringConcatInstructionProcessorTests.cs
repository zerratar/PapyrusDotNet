using System;
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors;

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    [TestClass]
    public class StringConcatInstructionProcessorTests
    {
        [TestMethod]
        public void StringConcatInstructionProcessor_Process()
        {
            var testModule = CreateInternal<ModuleDefinition>();


            var concat = new StringConcatInstructionProcessor(new MockClr2PapyrusInstructionProcessor());
            var stringType = testModule.Import(typeof(string));
            var methodReference = new MethodReference("System.String.Concat", stringType);

            methodReference.Parameters.Add(
                    new ParameterDefinition(stringType)
                );

            methodReference.Parameters.Add(
                    new ParameterDefinition(stringType)
                );

            var instruction = CreateInternal<Instruction>(OpCodes.Call, methodReference);

            //var output = concat.Process(instruction, methodReference, new List<object>(
            //    new object[]
            //    {
            //        new EvaluationStackItem
            //        {
                        
            //        }
            //    }
            //    ));

        }

        public T CreateInternal<T>(params object[] parameters)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            CultureInfo culture = null; // use InvariantCulture or other if you prefer
            return (T)Activator.CreateInstance(typeof(T), flags, null, parameters, culture);
        }
    }
}