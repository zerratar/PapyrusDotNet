﻿/*
    This file is part of PapyrusDotNet.

    PapyrusDotNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PapyrusDotNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with PapyrusDotNet.  If not, see <http://www.gnu.org/licenses/>.
	
	Copyright 2014, Karl Patrik Johansson, zerratar@gmail.com
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PapyrusDotNet
{
	using Mono.Cecil;
	using Mono.Cecil.Cil;

	using PapyrusDotNet.Common;

	public class PapyrusFunctionWriter
	{
		private AssemblyDefinition Assembly;

		private TypeDefinition Type;

		private Stack<EvaluationStackItem> EvaluationStack;

		private List<PapyrusVariableReference> Parameters;

		private List<PapyrusVariableReference> Variables;

		private List<PapyrusVariableReference> TempVariables;

		private List<PapyrusVariableReference> Fields;

		private bool skipNextInstruction = false;

		private MethodDefinition ThisMethod;

		public PapyrusFunctionWriter(AssemblyDefinition assembly, TypeDefinition type, List<PapyrusVariableReference> fields)
		{
			this.Assembly = assembly;
			this.Type = type;
			this.EvaluationStack = new Stack<EvaluationStackItem>();
			this.Parameters = new List<PapyrusVariableReference>();
			this.Variables = new List<PapyrusVariableReference>();
			this.TempVariables = new List<PapyrusVariableReference>();
			this.Fields = fields;
		}

		public string CreateFunction(MethodDefinition method)
		{
			this.ThisMethod = method;
			var returnType = Utility.GetPapyrusReturnType(method.ReturnType);
			var staticMarker = method.IsStatic ? " static" : "";

			// Native methods or functions can be declared			
			foreach (var attr in method.CustomAttributes)
			{
				if (attr.AttributeType.Name.Equals("NativeAttribute"))
				{
					method.IsNative = true;
					break;
				}
			}

			var nativeMarker = method.IsNative ? " native" : "";

			int indentDepth = 4;

			string output = Utility.StringIndent(indentDepth++, ".function " + method.Name + staticMarker + nativeMarker);
			output += Utility.StringIndent(indentDepth, ".userFlags 0");
			output += Utility.StringIndent(indentDepth, ".docString \"\"");
			output += Utility.StringIndent(indentDepth, ".return " + returnType);
			output += Utility.StringIndent(indentDepth++, ".paramTable");

			foreach (var parameter in method.Parameters)
			{
				output += Utility.StringIndent(indentDepth, ParseParameter(parameter));
			}

			output += Utility.StringIndent(--indentDepth, ".endParamTable");
			output += Utility.StringIndent(indentDepth++, ".localTable");

			if (method.HasBody)
			{
				if (HasVoidCalls(method.Body))
					output += Utility.StringIndent(indentDepth, ".local ::NoneVar None");

				foreach (var variable in method.Body.Variables)
					output += Utility.StringIndent(indentDepth, ParseLocalVariable(variable));
			}

			output += Utility.StringIndent(--indentDepth, ".endLocalTable");
			output += Utility.StringIndent(indentDepth++, ".code");
			if (method.HasBody)
			{
				foreach (var instruction in method.Body.Instructions)
				{
					output += Utility.StringIndent(indentDepth - 1, "_label" + instruction.Offset + ":");

					if (skipNextInstruction)
					{
						skipNextInstruction = false;
						continue;
					}

					if (SkipToOffset > 0)
					{
						if (instruction.Offset <= SkipToOffset)
						{
							continue;
						}
						SkipToOffset = -1;
					}

					var value = ParseInstruction(instruction);
					if (!string.IsNullOrEmpty(value))
					{
						if (value.Contains(Environment.NewLine))
						{
							var rows = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
							foreach (var row in rows)
								output += Utility.StringIndent(indentDepth, row);
						}
						else
							output += Utility.StringIndent(indentDepth, value);
					}
				}
			}
			output += Utility.StringIndent(--indentDepth, ".endCode");
			output += Utility.StringIndent(--indentDepth, ".endFunction");

			// Post Enhancements
			output = Utility.RemoveUnusedLabels(output);
			output = Utility.RemoveUnnecessaryLabels(output);
			output = Utility.InjectTempVariables(output, indentDepth + 2, TempVariables);

			return output;
		}

		private bool HasVoidCalls(MethodBody body)
		{
			foreach (var s in body.Instructions)
			{
				if (Utility.IsCallMethod(s.OpCode.Code))
				{
					var mRef = s.Operand as MethodReference;
					if (mRef != null)
					{
						if (IsVoid(mRef.ReturnType)) return true;
					}
				}
			}
			return false;
		}

		private string ParseParameter(ParameterDefinition parameter)
		{
			var name = parameter.Name;
			var type = parameter.ParameterType.FullName;
			var typeN = parameter.ParameterType.Name;
			Parameters.Add(new PapyrusVariableReference(name, type));

			return ".param " + name + " " + Utility.GetPapyrusReturnType(typeN, parameter.ParameterType.Namespace);
		}

		private string ParseLocalVariable(Mono.Cecil.Cil.VariableDefinition variable)
		{
			var name = variable.Name;
			var type = variable.VariableType.FullName;
			var typeN = variable.VariableType.Name;

			if (string.IsNullOrEmpty(name))
				name = "V_" + Variables.Count;

			Variables.Add(new PapyrusVariableReference(name, type));

			return ".local " + name + " " + Utility.GetPapyrusReturnType(typeN, variable.VariableType.Namespace);
		}

		private string ParseInstruction(Mono.Cecil.Cil.Instruction instruction)
		{
			// ArrayGetElement	<outputVarName>	<arrayName>	<int:index>
			// ArraySetElement <arrayName> <int:index> <valueOrVariable>?
			// ArrayLength	<outputVarName>	<arrayName>

			if (Utility.IsLoadElement(instruction.OpCode.Code))
			{
				var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
				if (EvaluationStack.Count >= popCount)
				{
					var itemIndex = EvaluationStack.Pop();
					var itemArray = EvaluationStack.Pop();

					string tar = "";
					var idx = -1;
					var oidx = "0";
					if (itemIndex.Value is PapyrusVariableReference)
						oidx = (itemIndex.Value as PapyrusVariableReference).Name;
					else if (itemIndex.Value != null)
						idx = int.Parse(itemIndex.Value.ToString());

					if (idx > 128) idx = 128;
					if (idx != -1)
						oidx = idx.ToString();

					if (itemArray.Value is PapyrusVariableReference)
						tar = (itemArray.Value as PapyrusVariableReference).Name;
					else
					{ /* Unsupported */ }


					// Supports:
					// var obj = array[x]

					// Not yet supporting? note: i have not tried it yet.
					// Function(array[x],...)
					int targetVariableIndex = 0;
					var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
					if (tarIn != null)
					{

						return "ArrayGetElement " + AllVariables[targetVariableIndex].Name + " " + tar + " " + oidx;
					}



				}
			}

			if (Utility.IsStoreElement(instruction.OpCode.Code))
			{
				var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
				if (EvaluationStack.Count >= popCount)
				{
					var newValue = EvaluationStack.Pop();
					var itemIndex = EvaluationStack.Pop();
					var itemArray = EvaluationStack.Pop();

					string val = "";
					string tar = "";
					var idx = -1;
					var oidx = "0";
					if (itemIndex.Value is PapyrusVariableReference)
						oidx = (itemIndex.Value as PapyrusVariableReference).Name;
					else if (itemIndex.Value != null)
						idx = int.Parse(itemIndex.Value.ToString());

					if (idx > 128) idx = 128;
					if (idx != -1)
						oidx = idx.ToString();
					if (itemArray.Value is PapyrusVariableReference)
						tar = (itemArray.Value as PapyrusVariableReference).Name;
					else
					{ /* Unsupported */ }

					if (newValue.Value is PapyrusVariableReference)
						val = (newValue.Value as PapyrusVariableReference).Name;

					else if (newValue.Value != null) val = newValue.Value.ToString();

					return "ArraySetElement " + tar + " " + oidx + " " + val;

				}
			}

			if (Utility.IsLoadLength(instruction.OpCode.Code))
			{
				var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
				if (EvaluationStack.Count >= popCount)
				{
					var val = EvaluationStack.Pop();
					if (val.TypeName.EndsWith("[]"))
					{
						if (val.Value is PapyrusVariableReference)
						{
							int variableIndex = 0;
							var storeInstruction = GetNextStoreLocalVariableInstruction(instruction, out variableIndex);
							if (storeInstruction != null)
							{
								return "ArrayLength " + AllVariables[variableIndex].Name + " " + (val.Value as PapyrusVariableReference).Name;
							}
						}
						else
						{
							// NOT SUPPORTED
						}
					}
					else
					{
						// size of ?
					}
				}
				// ArrayLength <outputVariableName> <arrayName>
			}

			if (Utility.IsNewArrayInstance(instruction.OpCode.Code))
			{
				var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
				if (EvaluationStack.Count >= popCount)
				{
					var val = EvaluationStack.Pop();

					int targetVariableIndex = 0;
					var tarIn = GetNextStoreLocalVariableInstruction(instruction, out targetVariableIndex);
					if (tarIn != null)
					{
						return "ArrayCreate " + AllVariables[targetVariableIndex].Name + " " + val.Value;
					}
				}
			}
			if (Utility.IsNewObjectInstance(instruction.OpCode.Code))
			{
				var popCount = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);
				// the obj objN = new obj
				// is not supported by Papyrus, rather they are automatically instanced so 
				// this opCode should be ignored, but we should still pop the values from the stack
				// to maintain correct work flow.

				for (int pops = 0; pops < popCount; pops++)
				{
					if (EvaluationStack.Count > 0)
						EvaluationStack.Pop();
				}
			}
			if (Utility.IsLoadArgs(instruction.OpCode.Code))
			{
				var index = IntValue(instruction);
				if (ThisMethod.IsStatic && (int)index == 0)
				{
					EvaluationStack.Push(new EvaluationStackItem { IsThis = true, Value = this.Type, TypeName = this.Type.FullName });
				}
				else
				{
					if (!ThisMethod.IsStatic && index > 0) index--;
					if (index < Parameters.Count)
					{
						EvaluationStack.Push(new EvaluationStackItem { Value = Parameters[(int)index], TypeName = Parameters[(int)index].TypeName });
					}
				}

			}
			if (Utility.IsLoadInteger(instruction.OpCode.Code))
			{
				var index = IntValue(instruction);
				EvaluationStack.Push(new EvaluationStackItem { Value = index, TypeName = "Int" });
			}

			if (Utility.IsLoadNull(instruction.OpCode.Code))
			{
				EvaluationStack.Push(new EvaluationStackItem { Value = "None", TypeName = "None" });
			}

			if (Utility.IsLoadField(instruction.OpCode.Code))
			{
				if (instruction.Operand is FieldReference)
				{
					var fref = instruction.Operand as FieldReference;

					var definedField = this.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
					if (definedField != null)
					{
						EvaluationStack.Push(new EvaluationStackItem { Value = definedField, TypeName = definedField.TypeName });
					}
				}
			}


			if (Utility.IsLoadLocalVariable(instruction.OpCode.Code))
			{
				var index = IntValue(instruction);
				if (index < AllVariables.Count)
				{
					EvaluationStack.Push(new EvaluationStackItem { Value = AllVariables[(int)index], TypeName = AllVariables[(int)index].TypeName });
				}
			}

			if (Utility.IsLoadString(instruction.OpCode.Code))
			{
				var value = Utility.GetString(instruction.Operand);

				EvaluationStack.Push(new EvaluationStackItem { Value = "\"" + value + "\"", TypeName = "String" });
			}



			if (Utility.IsStoreLocalVariable(instruction.OpCode.Code) || Utility.IsStoreField(instruction.OpCode.Code))
			{

				if (instruction.Operand is FieldReference)
				{
					var fref = instruction.Operand as FieldReference;
					// if the EvaluationStack.Count == 0
					// The previous instruction might have been a call that returned a value
					// Something we did not store...
					if (EvaluationStack.Count > 0)
					{
						var obj = EvaluationStack.Pop();

						var definedField = this.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
						if (definedField != null)
						{
							if (obj.Value is PapyrusVariableReference)
							{
								var varRef = obj.Value as PapyrusVariableReference;
								definedField.Value = varRef.Value;
								return "Assign " + definedField.Name + " " + varRef.Name;
							}
							definedField.Value = Utility.TypeValueConvert(definedField.TypeName, obj.Value);
							return "Assign " + definedField.Name + " " + definedField.Value;
						}
					}
				}
				var index = IntValue(instruction);
				if (index < AllVariables.Count)
				{
					if (EvaluationStack.Count > 0)
					{
						var heapObj = EvaluationStack.Pop();
						if (heapObj.Value is PapyrusVariableReference)
						{
							var varRef = heapObj.Value as PapyrusVariableReference;
							AllVariables[(int)index].Value = varRef.Value;
							return "Assign " + AllVariables[(int)index].Name + " " + varRef.Name;
						}


						AllVariables[(int)index].Value = Utility.TypeValueConvert(AllVariables[(int)index].TypeName, heapObj.Value);
					}
					var valout = AllVariables[(int)index].Value;
					var valoutStr = valout + "";
					if (string.IsNullOrEmpty(valoutStr)) valoutStr = "None";
					return "Assign " + AllVariables[(int)index].Name + " " + valoutStr;
				}

			}

			if (Utility.IsMath(instruction.OpCode.Code))
			{
				if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
				{
					// should be 2.
					// Make sure we have a temp variable if necessary
					string concatTargetVar = GetTargetVariable(instruction, null, "Int");

					// Equiviliant Papyrus: StrCat <output> <val1> <val2>

					var value = GetConditional(instruction);
					var retVal = Utility.GetPapyrusMathOp(instruction.OpCode.Code) + " " + value;

					return retVal;
				}
			}

			if (Utility.IsCallMethod(instruction.OpCode.Code))
			{
				if (instruction.Operand is MethodReference)
				{



					var methodRef = instruction.Operand as MethodReference;

					if (methodRef.Name.ToLower().Contains("concat"))
					{
						// Make sure we have a temp variable if necessary
						string concatTargetVar = GetTargetVariable(instruction, methodRef);

						// Equiviliant Papyrus: StrCat <output> <val1> <val2>

						var para = methodRef.Parameters;
						var popCount = para.Count;

						var targetVariable = EvaluationStack.Pop();
						var concatValues = new List<EvaluationStackItem>();
						for (int idx = 0; idx < popCount; idx++)
						{
							concatValues.Add(EvaluationStack.Pop());
						}


						var leftOverCount = concatValues.Count % 2;

						List<string> strCats = new List<string>();

						var outputStr = "";

						for (int j = concatValues.Count - 1; j >= 0; j--)
						{
							if (concatValues[j].Value is PapyrusVariableReference)
							{
								if (!concatValues[j].TypeName.ToLower().Contains("string"))
									strCats.Add("Cast " + concatTargetVar + " " + (concatValues[j].Value as PapyrusVariableReference).Name);

								strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " + (concatValues[j].Value as PapyrusVariableReference).Name);
							}
							else
							{
								strCats.Add("StrCat " + concatTargetVar + " " + concatTargetVar + " " + concatValues[j].Value);
							}
						}
						var possibleMethodCall = instruction.Next;
						if (possibleMethodCall.OpCode.Code == Code.Nop)
						{
							possibleMethodCall = possibleMethodCall.Next;
						}
						if (possibleMethodCall != null && Utility.IsCallMethod(possibleMethodCall.OpCode.Code))
						{
							EvaluationStack.Push(targetVariable);
						}

						return string.Join(Environment.NewLine, strCats.ToArray());
					}

					if (methodRef.Name.ToLower().Contains("op_equal") ||
						methodRef.Name.ToLower().Contains("op_inequal"))
					{
						InvertedBranch = methodRef.Name.ToLower().Contains("op_inequal");
						SkipToOffset = instruction.Next.Offset;
						// EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
						return "";
					}

					bool isCalledByThis = false;
					var param = new List<EvaluationStackItem>();

					var itemsToPop = 0;
					if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Varpop) itemsToPop = methodRef.Parameters.Count;
					else itemsToPop = Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop);

					for (int j = 0; j < itemsToPop; j++)
					{
						if (EvaluationStack.Count > 0)
						{
							var parameter = EvaluationStack.Pop();
							if (parameter.IsThis && EvaluationStack.Count > methodRef.Parameters.Count
								|| methodRef.CallingConvention == MethodCallingConvention.ThisCall)
							{

								isCalledByThis = true;
								// this.CallMethod();
							}

							param.Insert(0, parameter);
						}
					}
					if (!isCalledByThis && EvaluationStack.Count > 0)
					{
						param.Insert(0, EvaluationStack.Pop());
					}

					MethodDefinition definition = null;
					foreach (var ty in Assembly.MainModule.Types)
					{
						definition = ty.Methods.FirstOrDefault(f => f.Name == methodRef.Name);
						if (definition != null) break;
					}

					try
					{
						if (definition == null)
						{
							definition = methodRef.Resolve();
						}
					}
					catch
					{

					}

					string targetVar = GetTargetVariable(instruction, methodRef);


					if (definition != null)
					{
						if (definition.IsStatic)
						{
							var callerType = "self";
							if (methodRef.Parameters.Count != param.Count)
							{
								var caller = param[0];
								param.Remove(caller);
								callerType = caller.TypeName;
								if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();
							}
							else
							{
								var fName = methodRef.FullName;
								if (fName.Contains("::"))
								{
									var values = fName.Split(new string[] { "::" }, StringSplitOptions.None)[0];
									if (values.Contains("."))
									{
										values = values.Split('.').LastOrDefault();
										if (!string.IsNullOrEmpty(values))
										{
											callerType = values;
										}
									}
								}
							}

							return "CallStatic " + callerType + " " + definition.Name + " " + targetVar + " " + FormatParameters(methodRef, param); //definition;
						}
					}

					if (isCalledByThis)
					{
						var callerType = "self";
						if (methodRef.Parameters.Count != param.Count)
						{
							var caller = param[0];
							param.Remove(caller);
							callerType = caller.TypeName;
							if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();
						}

						return "CallMethod " + methodRef.Name + " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
					}
					else
					{
						var callerType = "self";
						if (methodRef.Parameters.Count != param.Count && param.Count > 0)
						{
							var caller = param[0];
							param.Remove(caller);
							callerType = caller.TypeName;
							if (callerType.Contains(".")) callerType = callerType.Split('.').LastOrDefault();

							if (callerType.ToLower() == Type.Name.ToLower()) callerType = "self";
							if (caller.Value is PapyrusVariableReference)
							{
								callerType = (caller.Value as PapyrusVariableReference).Name;
							}
						}
						return "CallMethod " + methodRef.Name + " " + callerType + " " + targetVar + " " + FormatParameters(methodRef, param);
					}
				}
			}

			if (instruction.OpCode.Code == Code.Ret)
			{
				if (IsVoid(this.ThisMethod.ReturnType))
				{
					return "Return None";
				}

				if (EvaluationStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
				{
					var topValue = EvaluationStack.Pop();
					if (topValue.Value is PapyrusVariableReference)
					{
						var variable = topValue.Value as PapyrusVariableReference;
						return "Return " + variable.Name;
					}
				}
				else
				{
					return "Return None";
				}
			}

			if (Utility.IsConditional(instruction.OpCode.Code))
			{
				if (Utility.IsGreaterThan(instruction.OpCode.Code))
				{
					var outputVal = GetConditional(instruction);
					if (!string.IsNullOrEmpty(outputVal))
					{
						return "CompareGT " + outputVal;
					}
				}
				else if (Utility.IsLessThan(instruction.OpCode.Code))
				{
					var outputVal = GetConditional(instruction);
					if (!string.IsNullOrEmpty(outputVal))
					{
						return "CompareLT " + outputVal;
					}
				}
				else if (Utility.IsEqualTo(instruction.OpCode.Code))
				{
					var outputVal = GetConditional(instruction);
					if (!string.IsNullOrEmpty(outputVal))
					{
						return "CompareEQ " + outputVal;
					}
				}

			}

			if (Utility.IsBranchConditional(instruction.OpCode.Code))
			{
				var heapStack = EvaluationStack;
			}
			else if (Utility.IsBranch(instruction.OpCode.Code))
			{
				var heapStack = EvaluationStack;
				var target = instruction.Operand;

				var targetVal = "";

				if (target is Instruction)
				{
					targetVal = "_label" + (target as Instruction).Offset;
				}
				else if (target != null)
				{
					targetVal = target.ToString();
				}

				if (heapStack.Count >= Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
				{
					var value = heapStack.Pop();
					var compareVal = "";
					if (value.Value is PapyrusVariableReference)
					{
						var variable = value.Value as PapyrusVariableReference;
						compareVal = variable.Name;
					}
					else compareVal = value.Value.ToString();



					if (instruction.OpCode.Code == Code.Brtrue || instruction.OpCode.Code == Code.Brtrue_S)
					{
						if (InvertedBranch)
						{
							InvertedBranch = false;
							return "JumpT " + compareVal + " " + targetVal;
						}

						return "JumpF " + compareVal + " " + targetVal;
					}
					if (instruction.OpCode.Code == Code.Brfalse || instruction.OpCode.Code == Code.Brfalse_S)
					{
						if (InvertedBranch)
						{
							InvertedBranch = false;
							return "JumpF " + compareVal + " " + targetVal;
						}
						return "JumpT " + compareVal + " " + targetVal;
					}
				}
				return "Jump " + targetVal;
			}


			return "";
		}

		public PapyrusVariableReference GetFieldFromSTFLD(Instruction whereToPlace)
		{
			if (Utility.IsStoreField(whereToPlace.OpCode.Code))
			{

				if (whereToPlace.Operand is FieldReference)
				{
					var fref = whereToPlace.Operand as FieldReference;
					// if the EvaluationStack.Count == 0
					// The previous instruction might have been a call that returned a value
					// Something we did not store...
					var definedField = this.Fields.FirstOrDefault(f => f.Name == "::" + fref.Name.Replace('<', '_').Replace('>', '_'));
					if (definedField != null)
					{
						return definedField;
					}
				}
			}
			return null;
		}

		private string GetTargetVariable(Instruction instruction, MethodReference methodRef, string fallbackType = null)
		{
			string targetVar = null;
			var whereToPlace = instruction.Next;
			if (whereToPlace != null && (Utility.IsStoreLocalVariable(whereToPlace.OpCode.Code) || Utility.IsStoreField(whereToPlace.OpCode.Code)))
			{

				if (Utility.IsStoreField(whereToPlace.OpCode.Code))
				{
					var fieldData = GetFieldFromSTFLD(whereToPlace);
					if (fieldData != null)
					{
						targetVar = fieldData.Name;
					}
				}
				else
				{
					var index = IntValue(whereToPlace);
					if (index < AllVariables.Count)
					{
						targetVar = AllVariables[(int)index].Name;
					}
				}
				skipNextInstruction = true;

				// else 
				//EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });
			}
			else if (whereToPlace != null && (Utility.IsLoad(whereToPlace.OpCode.Code) || Utility.IsCallMethod(whereToPlace.OpCode.Code)))
			{
				// Most likely this function call have a return value other than Void
				// and is used for an additional method call, witout being assigned to a variable first.

				// EvaluationStack.Push(new EvaluationStackItem { IsMethodCall = true, Value = methodRef, TypeName = methodRef.ReturnType.FullName });

				var tVar = CreateTempVariable(!string.IsNullOrEmpty(fallbackType) ? fallbackType : methodRef.ReturnType.FullName);
				targetVar = tVar.Name;
				EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
			}
			else
			{
				targetVar = "::NoneVar";
			}
			return targetVar;
		}

		private PapyrusVariableReference CreateTempVariable(string p)
		{
			string Namespace = "";
			string Name = "";
			if (p.Contains("."))
			{
				Namespace = p.Remove(p.LastIndexOf('.'));
				Name = p.Split('.').LastOrDefault();
			}
			else
			{
				Name = p;
			}
			var varname = "::temp" + TempVariables.Count;
			var type = Utility.GetPapyrusReturnType(Name, Namespace);
			var def = ".local " + varname + " " + type;
			var varRef = new PapyrusVariableReference(varname, type, def);
			TempVariables.Add(varRef);
			return varRef;
		}

		private bool InvertedBranch = false;

		private int SkipToOffset = -1;

		public List<PapyrusVariableReference> AllVariables
		{
			get
			{
				var var1 = new List<PapyrusVariableReference>();
				if (Variables != null)
					var1.AddRange(Variables);
				if (TempVariables != null)
					var1.AddRange(TempVariables);
				if (Fields != null)
					var1.AddRange(Fields);
				return var1;
			}
		}

		public string GetConditional(Instruction instruction)
		{
			var code = instruction.OpCode.Code;
			var heapStack = EvaluationStack;


			if (heapStack.Count >= 2)//Utility.GetStackPopCount(instruction.OpCode.StackBehaviourPop))
			{
				var obj1 = heapStack.Pop();
				var obj2 = heapStack.Pop();
				var vars = AllVariables;
				var varIndex = 0;

				string value1 = "";
				string value2 = "";

				if (obj1.Value is PapyrusVariableReference) value1 = (obj1.Value as PapyrusVariableReference).Name;
				else
					value1 = obj1.Value.ToString();

				if (obj2.Value is PapyrusVariableReference) value2 = (obj2.Value as PapyrusVariableReference).Name;
				else
					value2 = obj2.Value.ToString();

				// if (Utility.IsGreaterThan(code) || Utility.IsLessThan(code))
				{
					var next = instruction.Next;
					while (next != null && !Utility.IsStoreLocalVariable(next.OpCode.Code) && !Utility.IsStoreField(next.OpCode.Code) && !Utility.IsCallMethod(next.OpCode.Code))
					{
						next = next.Next;
					}
					//	if (next != null && next.Operand is MethodReference)
					if (instruction.Operand is MethodReference)
					{
						// if none found, create a temp one.
						var methodRef = instruction.Operand as MethodReference;
						var tVar = CreateTempVariable(methodRef.ReturnType.FullName);
						var targetVar = tVar.Name;
						EvaluationStack.Push(new EvaluationStackItem { Value = tVar, TypeName = tVar.TypeName });
						return targetVar + " " + value2 + " " + value1;
					}

					if (next == null)
					{


						// No intentions to store this value into a variable, 
						// Its to be used in a function call.
						return "NULLPTR " + value2 + " " + value1;
					}

					SkipToOffset = next.Offset;
					if (next.Operand is FieldReference)
					{
						var field = GetFieldFromSTFLD(next);
						if (field != null)
							return field.Name + " " + value2 + " " + value1;
					}


					varIndex = (int)IntValue(next);

				}

				return vars[varIndex].Name + " " + value2 + " " + value1;
			}
			return null;
		}

		public Instruction GetNextStoreLocalVariableInstruction(Instruction input, out int varIndex)
		{
			varIndex = -1;
			var next = input.Next;
			while (next != null && !Utility.IsStoreLocalVariable(next.OpCode.Code))
			{
				next = next.Next;
			}
			if (next != null)
			{
				varIndex = (int)IntValue(next);
				SkipToOffset = next.Offset;
			}
			return next;
		}


		public bool HasMethod(MethodReference methodRef)
		{
			if (this.Type.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
			if (this.Type.BaseType != null)
			{
				try
				{
					var typeDef = this.Type.BaseType.Resolve();
					if (typeDef != null)
					{
						if (typeDef.Methods.Any(m => m.FullName == methodRef.FullName)) return true;
					}
				}
				catch { }
				//.m
			}
			return false;
		}

		public string FormatParameters(MethodReference methodRef, List<EvaluationStackItem> parameters)
		{
			var outp = new List<string>();
			if (parameters != null && parameters.Count > 0)
			{
				int index = 0;
				foreach (var item in parameters)
				{
					if (item.Value is PapyrusVariableReference)
					{
						outp.Add((item.Value as PapyrusVariableReference).Name);
						continue;
					}
					if (item.TypeName.ToLower().Equals("int"))
					{
						if (methodRef.Parameters[index].ParameterType == Assembly.MainModule.TypeSystem.Boolean)
						{
							var val = int.Parse(item.Value.ToString()) == 1;
							outp.Add(val.ToString());
							continue;
						}
						outp.Add(item.Value.ToString());
					}
					if (item.TypeName.ToLower().Equals("string"))
					{
						if (!item.Value.ToString().Contains("\""))
							outp.Add("\"" + item.Value + "\"");
						else
							outp.Add(item.Value.ToString());
					}
					index++;
				}
			}
			return string.Join(" ", outp.ToArray());
		}

		private bool IsVoid(TypeReference typeReference)
		{
			return (typeReference.FullName.ToLower().Equals("system.void") || typeReference.Name.ToLower().Equals("void"));
		}



		public double IntValue(Instruction instruction)
		{
			double index = Utility.GetCodeIndex(instruction.OpCode.Code);
			if (index == -1)
			{
				if (instruction.Operand is ParameterReference)
				{
					// not yet implemented
					return 0;
				}
				if (instruction.Operand is FieldReference)
				{
					// not yet implemented
					return 0;
				}
				if (instruction.Operand is VariableReference)
				{
					var v = instruction.Operand as VariableReference;
					if (v != null)
					{
						return Array.IndexOf(AllVariables.ToArray(), AllVariables.FirstOrDefault(va => va.Name == "V_" + v.Index));
					}
				}
				else if (instruction.Operand is double || instruction.Operand is long || instruction.Operand is float || instruction.Operand is decimal)
					index = double.Parse(instruction.Operand.ToString());
				else
					index = int.Parse(instruction.Operand.ToString());
			}
			return index;
		}
	}

}