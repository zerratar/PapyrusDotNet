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
//     Copyright 2016, Karl Patrik Johansson, zerratar@gmail.com

#region

using System;
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.Converters.Clr2Papyrus.Implementations.Processors;

#endregion

namespace PapyrusDotNet.Converters.Clr2Papyrus.Test
{
    [TestClass]
    public class StringConcatInstructionProcessorTests
    {
        [TestMethod]
        public void StringConcatInstructionProcessor_Process()
        {
            var testModule = CreateInternal<ModuleDefinition>();


            var concat = new StringConcatProcessor();
            var stringType = testModule.Import(typeof (string));
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
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            CultureInfo culture = null; // use InvariantCulture or other if you prefer
            return (T) Activator.CreateInstance(typeof (T), flags, null, parameters, culture);
        }
    }
}