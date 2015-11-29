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

using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PapyrusDotNet.CoreBuilder.Interfaces;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;

#endregion

namespace PapyrusDotNet.CoreBuilder
{
    /// <summary>
    ///     A class that extends the <see cref="TypeDefinition" />
    ///     class with features similar to the features in the
    ///     System.Reflection.Emit namespace.
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        ///     Adds a default constructor to the target type.
        /// </summary>
        /// <param name="targetType">The type that will contain the default constructor.</param>
        /// <returns>The default constructor.</returns>
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType,
            IPapyrusCilAssemblyBuilder libraryGenerator)
        {
            var parentType = typeof (object);

            return AddDefaultConstructor(targetType, parentType, libraryGenerator);
        }

        /// <summary>
        ///     Adds a default constructor to the target type.
        /// </summary>
        /// <param name="parentType">
        ///     The base class that contains the default constructor that will be used for constructor
        ///     chaining..
        /// </param>
        /// <param name="targetType">The type that will contain the default constructor.</param>
        /// <returns>The default constructor.</returns>
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType, Type parentType,
            IPapyrusCilAssemblyBuilder libraryGenerator)
        {
            var module = libraryGenerator.MainModule; // targetType.Module;
            var voidType = module.Import(typeof (void));
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig
                                   | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;


            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objectConstructor = parentType.GetConstructor(flags, null, new Type[0], null);

            // Revert to the System.Object constructor
            // if the parent type does not have a default constructor
            if (objectConstructor == null)
                objectConstructor = typeof (object).GetConstructor(new Type[0]);

            var baseConstructor = module.Import(objectConstructor);

            // Define the default constructor
            var ctor = new MethodDefinition(".ctor", methodAttributes, voidType)
            {
                CallingConvention = MethodCallingConvention.StdCall,
                ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed
            };

            var IL = ctor.Body.Instructions;

            // Call the constructor for System.Object, and exit
            IL.Add(Instruction.Create(OpCodes.Ldarg_0));
            IL.Add(Instruction.Create(OpCodes.Call, baseConstructor));
            IL.Add(Instruction.Create(OpCodes.Ret));

            targetType.Methods.Add(ctor);

            return ctor;
        }

        /*/// <summary>
		/// Adds a new method to the <paramref name="typeDef">target type</paramref>.
		/// </summary>
		/// <param name="typeDef">The type that will hold the newly-created method.</param>
		/// <param name="attributes">The <see cref="Mono.Cecil.MethodAttributes"/> parameter that describes the characteristics of the method.</param>
		/// <param name="methodName">The name to be given to the new method.</param>
		/// <param name="returnType">The method return type.</param>        
		/// <param name="parameterTypes">The list of argument types that will be used to define the method signature.</param>
		/// <param name="genericParameterTypes">The list of generic argument types that will be used to define the method signature.</param>
		/// <returns>A <see cref="MethodDefinition"/> instance that represents the newly-created method.</returns>
		public static MethodDefinition DefineMethod(this TypeDefinition typeDef, string methodName,
			Mono.Cecil.MethodAttributes attributes, Type returnType, Type[] parameterTypes, Type[] genericParameterTypes)
		{
			var method = new MethodDefinition(methodName, attributes, null);

			typeDef.Methods.Add(method);

			//Match the generic parameter types
			foreach (var genericParameterType in genericParameterTypes)
			{
				method.AddGenericParameter(genericParameterType);
			}

			// Match the parameter types
			method.impParameters.Add(parameterTypes);

			// Match the return type
			method.SetReturnType(returnType);

			return method;
		}
		*/

        /// <summary>
        ///     Adds a rewritable property to the <paramref name="typeDef">target type</paramref>.
        /// </summary>
        /// <param name="typeDef">The target type that will hold the newly-created property.</param>
        /// <param name="propertyName">The name of the property itself.</param>
        /// <param name="propertyType">The <see cref="System.Type" /> instance that describes the property type.</param>
        public static void AddProperty(this TypeDefinition typeDef, string propertyName, Type propertyType)
        {
            var module = typeDef.Module;
            var typeRef = module.Import(propertyType);
            typeDef.AddProperty(propertyName, typeRef);
        }

        /// <summary>
        ///     Adds a rewritable property to the <paramref name="typeDef">target type</paramref>.
        /// </summary>
        /// <param name="typeDef">The target type that will hold the newly-created property.</param>
        /// <param name="propertyName">The name of the property itself.</param>
        /// <param name="propertyType">The <see cref="TypeReference" /> instance that describes the property type.</param>
        public static void AddProperty(this TypeDefinition typeDef, string propertyName,
            TypeReference propertyType)
        {
            #region Add the backing field

            var fieldName = string.Format("__{0}_backingField", propertyName);
            var actualField = new FieldDefinition(fieldName,
                FieldAttributes.Private, propertyType);


            typeDef.Fields.Add(actualField);

            #endregion

            FieldReference backingField = actualField;
            if (typeDef.GenericParameters.Count > 0)
                backingField = GetBackingField(fieldName, typeDef, propertyType);

            var getterName = string.Format("get_{0}", propertyName);
            var setterName = string.Format("set_{0}", propertyName);


            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig |
                                                MethodAttributes.SpecialName | MethodAttributes.NewSlot |
                                                MethodAttributes.Virtual;

            var module = typeDef.Module;
            var voidType = module.Import(typeof (void));

            // Implement the getter and the setter
            var getter = AddPropertyGetter(propertyType, getterName, attributes, backingField);
            var setter = AddPropertySetter(propertyType, attributes, backingField, setterName, voidType);

            typeDef.AddProperty(propertyName, propertyType, getter, setter);
        }

        /// <summary>
        ///     Adds a rewriteable property to the <paramref name="typeDef">target type</paramref>
        ///     using an existing <paramref name="getter" /> and <paramref name="setter" />.
        /// </summary>
        /// <param name="typeDef">The target type that will hold the newly-created property.</param>
        /// <param name="propertyName">The name of the property itself.</param>
        /// <param name="propertyType">The <see cref="TypeReference" /> instance that describes the property type.</param>
        /// <param name="getter">The property getter method.</param>
        /// <param name="setter">The property setter method.</param>
        public static void AddProperty(this TypeDefinition typeDef, string propertyName, TypeReference propertyType,
            MethodDefinition getter, MethodDefinition setter)
        {
            var newProperty = new PropertyDefinition(propertyName,
                PropertyAttributes.Unused, propertyType)
            {
                GetMethod = getter,
                SetMethod = setter
            };

            typeDef.Methods.Add(getter);
            typeDef.Methods.Add(setter);
            typeDef.Properties.Add(newProperty);
        }

        /// <summary>
        ///     Creates a property getter method implementation with the
        ///     <paramref name="propertyType" /> as the return type.
        /// </summary>
        /// <param name="propertyType">Represents the <see cref="TypeReference">return type</see> for the getter method.</param>
        /// <param name="getterName">The getter method name.</param>
        /// <param name="attributes">The method attributes associated with the getter method.</param>
        /// <param name="backingField">The field that will store the instance that the getter method will retrieve.</param>
        /// <returns>A <see cref="MethodDefinition" /> representing the getter method itself.</returns>
        private static MethodDefinition AddPropertyGetter(TypeReference propertyType,
            string getterName, MethodAttributes attributes, FieldReference backingField)
        {
            var getter = new MethodDefinition(getterName, attributes, propertyType)
            {
                IsPublic = true,
                ImplAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL
            };

            var IL = getter.Body.Instructions;
            IL.Add(Instruction.Create(OpCodes.Ldarg_0));
            IL.Add(Instruction.Create(OpCodes.Ldfld, backingField));
            IL.Add(Instruction.Create(OpCodes.Ret));

            return getter;
        }

        /// <summary>
        ///     Creates a property setter method implementation with the
        ///     <paramref name="propertyType" /> as the setter parameter.
        /// </summary>
        /// <param name="propertyType">Represents the <see cref="TypeReference">parameter type</see> for the setter method.</param>
        /// <param name="attributes">The method attributes associated with the setter method.</param>
        /// <param name="backingField">The field that will store the instance for the setter method.</param>
        /// <param name="setterName">The method name of the setter method.</param>
        /// <param name="voidType">The <see cref="TypeReference" /> that represents <see cref="Void" />.</param>
        /// <returns>A <see cref="MethodDefinition" /> that represents the setter method itself.</returns>
        private static MethodDefinition AddPropertySetter(TypeReference propertyType, MethodAttributes attributes,
            FieldReference backingField, string setterName, TypeReference voidType)
        {
            var setter = new MethodDefinition(setterName, attributes, voidType)
            {
                IsPublic = true,
                ImplAttributes = MethodImplAttributes.Managed | MethodImplAttributes.IL
            };

            setter.Parameters.Add(new ParameterDefinition(propertyType));

            var IL = setter.Body.Instructions;
            IL.Add(Instruction.Create(OpCodes.Ldarg_0));
            IL.Add(Instruction.Create(OpCodes.Ldarg_1));
            IL.Add(Instruction.Create(OpCodes.Stfld, backingField));
            IL.Add(Instruction.Create(OpCodes.Ret));

            return setter;
        }

        /// <summary>
        ///     Resolves the backing field for a generic type declaration.
        /// </summary>
        /// <param name="fieldName">The name of the field to reference.</param>
        /// <param name="typeDef">The type that holds the actual field.</param>
        /// <param name="propertyType">The <see cref="TypeReference" /> that describes the property type being referenced.</param>
        /// <returns>A <see cref="FieldReference" /> that points to the actual backing field.</returns>
        private static FieldReference GetBackingField(string fieldName, TypeDefinition typeDef,
            TypeReference propertyType)
        {
            // If the current type is a generic type, 
            // the current generic type must be resolved before
            // using the actual field
            var declaringType = new GenericInstanceType(typeDef);
            foreach (var parameter in typeDef.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }

            return new FieldReference(fieldName, declaringType, propertyType);
            ;
        }
    }
}