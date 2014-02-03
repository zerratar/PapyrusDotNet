/*
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
using Mono;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PapyrusDotNet
{
	using PapyrusDotNet.Common;

	using PowerArgs;

	public class PapyrusDotNetArgs
	{
		[ArgShortcut("o")]
		[ArgPosition(1)]
		public string OutputFolder { get; set; }

		[ArgShortcut("i")]
		[ArgPosition(0)]
		public string InputFile { get; set; }
	}
	class Program
	{
		public static bool HandleConstructorAsOnInit = false;

		private static List<PapyrusVariableReference> Fields;
		public static AssemblyDefinition CurrentAssembly;

		public static string outputFolder;

		public static string inputFile;
		static void Main(string[] args)
		{

			outputFolder = @".\output";
			inputFile = @"..\Examples\Example1\bin\Debug\Example1.dll";
			try
			{
				var parsed = Args.Parse<PapyrusDotNetArgs>(args);
				if (parsed.InputFile != null)
				{
					// Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
					if (parsed.OutputFolder != null)
						outputFolder = parsed.OutputFolder;
					inputFile = parsed.InputFile;
					if (outputFolder.Contains("\""))
					{
						outputFolder = outputFolder.Replace("\"", "");
					}

					if (inputFile.Contains("\""))
					{
						inputFile = inputFile.Replace("\"", "");
					}

					Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.InputFile, parsed.OutputFolder);
				}
			}
			catch (ArgException ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ArgUsage.GetUsage<PapyrusDotNetArgs>());
				// return;
			}


			Dictionary<string, string> outputPASFiles = new Dictionary<string, string>();
			CurrentAssembly = AssemblyDefinition.ReadAssembly(inputFile);

			var asmReferences = GetAssemblyReferences(CurrentAssembly);

			foreach (var asm in asmReferences)
			{
				GeneratePapyrusFromAssembly(asm, ref outputPASFiles, CurrentAssembly);
			}


			GeneratePapyrusFromAssembly(CurrentAssembly, ref outputPASFiles);

			foreach (var pas in outputPASFiles)
			{
				try
				{
					if (!System.IO.Directory.Exists(outputFolder)) System.IO.Directory.CreateDirectory(outputFolder);

					Console.WriteLine("Saving " + System.IO.Path.Combine(outputFolder, pas.Key) + "...");
					System.IO.File.WriteAllText(System.IO.Path.Combine(outputFolder, pas.Key), pas.Value);
				}
				catch (Exception exc)
				{
					Console.WriteLine("----------------");
					Console.WriteLine(outputFolder);
					Console.WriteLine(pas.Key);
					Console.WriteLine(exc);
					Console.ReadKey();
				}
			}
		}

		private static void GeneratePapyrusFromAssembly(AssemblyDefinition CurrentAssembly, ref Dictionary<string, string> outputPASFiles, AssemblyDefinition mainAssembly = null)
		{
			foreach (var module in CurrentAssembly.Modules)
			{
				if (module.HasTypes)
				{
					var types = module.Types;
					foreach (var type in types)
					{
						if (!type.FullName.ToLower().Contains("<module>"))
						{
							Console.WriteLine("Generating Papyrus Asm for " + type.FullName);
							Fields = new List<PapyrusVariableReference>();
							string papyrus = "";

							var properties = Utility.GetFlagsAndProperties(type);
							if (properties.IsGeneric)
							{
								// Get all usages to know which generic types are necessary to be generated.
								// then for each of those, we will generate a new class to represent it.
								// replacing all 'T' with the types used.


								var references = GetAllGenericReferences(type, mainAssembly != null ? mainAssembly : CurrentAssembly);

								var ref2 = GetAllGenericReferences(type, CurrentAssembly);

								var ref3 = GetAllReferences(type, CurrentAssembly);




								foreach (var r in references)
								{
									if (r.Type == "T") continue;

									papyrus = "";
									papyrus += CreatePapyrusInfo(CurrentAssembly);
									papyrus += CreatePapyrusUserflagRef();
									papyrus += CreatePapyrusObjectTable(type, r.Type);

									var genericName = Utility.GetPapyrusBaseType(type);

									genericName = genericName.Replace("`1", "_" + Utility.GetPapyrusBaseType(r.Type));

									if (!outputPASFiles.ContainsKey(genericName + ".pas"))
									{
										outputPASFiles.Add(genericName + ".pas", papyrus);
									}
								}
							}
							else
							{
								papyrus += CreatePapyrusInfo(CurrentAssembly);
								papyrus += CreatePapyrusUserflagRef();
								papyrus += CreatePapyrusObjectTable(type);

								if (!outputPASFiles.ContainsKey(Utility.GetPapyrusBaseType(type) + ".pas"))
								{
									outputPASFiles.Add(Utility.GetPapyrusBaseType(type) + ".pas", papyrus);
								}
							}
						}
					}
				}
			}
		}

		public static List<AssemblyDefinition> GetAssemblyReferences(AssemblyDefinition asm)
		{
			var inputFileInfo = new System.IO.FileInfo(inputFile);
			var assemblyReferences = new List<AssemblyDefinition>();
			foreach (var mod in asm.Modules)
			{
				var asmReferences =
					// We do not want to get any from mscorlib, PapyrusDotNet.Core, or any other System libs
						mod.AssemblyReferences.Where(re => re.Name != "mscorlib" && re.Name != "PapyrusDotNet.Core" && !re.Name.StartsWith("System")).ToList();

				foreach (var asr in asmReferences)
				{
					var refLibName = asr.Name + ".dll";
					if (!assemblyReferences.Any(a => a.Name.Name == asr.Name))
					{
						if (inputFileInfo.Directory != null)
						{
							var refLib = inputFileInfo.Directory.FullName + @"\" + refLibName;

							var refAsm = AssemblyDefinition.ReadAssembly(refLib);

							assemblyReferences.Add(refAsm);

						}
					}
				}
			}
			return assemblyReferences;
		}

		private static List<GenericReference> GetAllReferences(TypeDefinition type, AssemblyDefinition asm)
		{
			var gReferences = new List<GenericReference>();
			var assemblyReferences = GetAssemblyReferences(asm);

			foreach (var mod in asm.Modules)
			{
				GetReferences(type, mod, gReferences);
			}

			foreach (var ar in assemblyReferences)
			{
				foreach (var mod in ar.Modules)
				{
					GetReferences(type, mod, gReferences);
				}
			}

			return gReferences;
		}

		private static List<GenericReference> GetAllGenericReferences(TypeDefinition type, AssemblyDefinition asm)
		{
			var gReferences = new List<GenericReference>();
			var assemblyReferences = GetAssemblyReferences(asm);

			foreach (var mod in asm.Modules)
			{
				GetGenericReferences(type, mod, gReferences);
			}

			foreach (var ar in assemblyReferences)
			{
				foreach (var mod in ar.Modules)
				{
					GetGenericReferences(type, mod, gReferences);
				}
			}

			return gReferences;
		}

		private static void GetReferences(TypeDefinition type, ModuleDefinition mod, List<GenericReference> gReferences)
		{
			foreach (var t in mod.Types)
			{
				if (t == type)
				{
					continue;
				}

				if (t.HasMethods)
				{
					foreach (var m in t.Methods)
					{
						foreach (var mp in m.Parameters)
						{
							if (mp.ParameterType.IsGenericInstance || mp.ParameterType.Name == "T")
							{
								var name = mp.ParameterType.Name;
								var targetName = type.Name;
								if (name == targetName && mp.ParameterType.FullName.Contains("<"))
								{
									var usedType = mp.ParameterType.FullName.Split('<')[1].Split('>')[0];
									if (!gReferences.Any(j => j.Type == usedType))
									{
										gReferences.Add(new GenericReference(usedType, t.FullName));
									}
								}

							}
						}

						if (m.HasBody)
						{
							foreach (var v in m.Body.Variables)
							{
								if (v.VariableType.IsGenericInstance || v.VariableType.Name == "T")
								{
									var name = v.VariableType.Name;
									var targetName = type.Name;
									if (name == targetName && v.VariableType.FullName.Contains("<"))
									{
										var usedType = v.VariableType.FullName.Split('<')[1].Split('>')[0];
										if (!gReferences.Any(j => j.Type == usedType))
										{
											gReferences.Add(new GenericReference(usedType, t.FullName));
										}
									}

								}
							}
						}
						foreach (var gen in m.GenericParameters)
						{
							if (gen.IsGenericInstance || gen.Type.ToString() == "T")
							{
								var name = gen.Name;
								var targetName = type.Name;
								if (name == targetName && gen.FullName.Contains("<"))
								{
									var usedType = gen.FullName.Split('<')[1].Split('>')[0];
									if (!gReferences.Any(j => j.Type == usedType))
									{
										gReferences.Add(new GenericReference(usedType, t.FullName));
									}
								}

							}
						}
					}
				}

				foreach (var gen in t.GenericParameters)
				{
					if (gen.IsGenericInstance || gen.Type.ToString() == "T")
					{
						var name = gen.Name;
						var targetName = type.Name;
						if (name == targetName && gen.FullName.Contains("<"))
						{
							var usedType = gen.FullName.Split('<')[1].Split('>')[0];
							if (!gReferences.Any(j => j.Type == usedType))
							{
								gReferences.Add(new GenericReference(usedType, t.FullName));
							}
						}

					}
				}
				foreach (var f in t.Fields)
				{
					if (f.FieldType.IsGenericInstance || f.FieldType.Name == "T")
					{
						var name = f.FieldType.Name;
						var targetName = type.Name;
						if (name == targetName && f.FieldType.FullName.Contains("<"))
						{
							var usedType = f.FieldType.FullName.Split('<')[1].Split('>')[0];
							if (!gReferences.Any(j => j.Type == usedType))
							{
								gReferences.Add(new GenericReference(usedType, t.FullName));
							}
						}

					}
				}
			}
		}

		private static void GetGenericReferences(TypeDefinition type, ModuleDefinition mod, List<GenericReference> gReferences, TypeDefinition ignore = null)
		{
			foreach (var t in mod.Types)
			{
				if ((ignore != null && ignore == t)) return;
				if (t == type)
				{
					continue;
				}

				if (t.HasGenericParameters)
				{
					// time to backtrack!
					// we need to know if this type has been referenced before
					// and by using what type.
					// the same type will be expected by the target type.
					List<GenericReference> tRefs = new List<GenericReference>();
					GetGenericReferences(t, mod, tRefs, type);
					// Now, we need to know if this assembly
					// is actually referenced, altough we can just 
					// be sure and generate em all.
					// easiest way for now. lol xD
					gReferences.AddRange(tRefs);
				}

				if (t.HasMethods)
				{
					foreach (var m in t.Methods)
					{
						foreach (var mp in m.Parameters)
						{
							if (mp.ParameterType.IsGenericInstance)
							{
								var name = mp.ParameterType.Name;
								var targetName = type.Name;
								if (name == targetName && mp.ParameterType.FullName.Contains("<"))
								{
									var usedType = mp.ParameterType.FullName.Split('<')[1].Split('>')[0];
									if (!gReferences.Any(j => j.Type == usedType))
									{
										gReferences.Add(new GenericReference(usedType, t.FullName));
									}
								}
							}
						}

						if (m.HasBody)
						{
							foreach (var v in m.Body.Variables)
							{
								if (v.VariableType.IsGenericInstance)
								{
									var name = v.VariableType.Name;
									var targetName = type.Name;
									if (name == targetName && v.VariableType.FullName.Contains("<"))
									{
										var usedType = v.VariableType.FullName.Split('<')[1].Split('>')[0];
										if (!gReferences.Any(j => j.Type == usedType))
										{
											gReferences.Add(new GenericReference(usedType, t.FullName));
										}
									}
								}
							}
						}
						foreach (var gen in m.GenericParameters)
						{
							if (gen.IsGenericInstance)
							{
								var name = gen.Name;
								var targetName = type.Name;
								if (name == targetName && gen.FullName.Contains("<"))
								{
									var usedType = gen.FullName.Split('<')[1].Split('>')[0];
									if (!gReferences.Any(j => j.Type == usedType))
									{
										gReferences.Add(new GenericReference(usedType, t.FullName));
									}
								}
							}
						}
					}
				}

				foreach (var gen in t.GenericParameters)
				{
					if (gen.IsGenericInstance)
					{
						var name = gen.Name;
						var targetName = type.Name;
						if (name == targetName && gen.FullName.Contains("<"))
						{
							var usedType = gen.FullName.Split('<')[1].Split('>')[0];
							if (!gReferences.Any(j => j.Type == usedType))
							{
								gReferences.Add(new GenericReference(usedType, t.FullName));
							}
						}
					}
				}
				foreach (var f in t.Fields)
				{
					if (f.FieldType.IsGenericInstance)
					{
						var name = f.FieldType.Name;
						var targetName = type.Name;
						if (name == targetName && f.FieldType.FullName.Contains("<"))
						{
							var usedType = f.FieldType.FullName.Split('<')[1].Split('>')[0];
							if (!gReferences.Any(j => j.Type == usedType))
							{
								gReferences.Add(new GenericReference(usedType, t.FullName));
							}
						}
					}
				}
			}
		}

		public class GenericReference
		{
			public string SourceClass { get; set; }
			public string Type { get; set; }
			public GenericReference(string t, string c = null)
			{
				Type = t;
				SourceClass = c;
			}
		}


		public static string CreatePapyrusObjectTable(TypeDefinition type, string replaceGenericWithType)
		{
			var ptype = Utility.GetPapyrusBaseType(replaceGenericWithType);
			var papyrus = CreatePapyrusObjectTable(type);

			var pap = papyrus.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

			for (int j = 0; j < pap.Length; j++)
			{
				if (pap[j].EndsWith(" T")||pap[j].EndsWith("_T"))
				{
					pap[j] = pap[j].Remove(pap[j].LastIndexOf('T')) + ptype;
				}


				

				if (pap[j].Contains(".object ") && pap[j].Contains("`1"))
				{
					pap[j] = pap[j].Replace("`1", "_" + ptype);
				}
				else if (pap[j].Contains("`1"))
				{
					pap[j] = pap[j].Replace("`1", "_" + ptype);
				}
			}

			papyrus = string.Join(Environment.NewLine, pap);

			return papyrus;
		}

		public static string CreatePapyrusObjectTable(TypeDefinition type)
		{
			string papyrus = "";

			papyrus += ".objectTable" + Environment.NewLine;
			papyrus += "\t.object " + Utility.GetPapyrusBaseType(type) + (type.BaseType != null ? " " + Utility.GetPapyrusBaseType(type.BaseType) : "") + Environment.NewLine;

			var props = Utility.GetFlagsAndProperties(type);

			if (props.IsGeneric)
			{

			}

			papyrus += "\t\t.userFlags " + props.UserFlagsValue + Environment.NewLine;
			papyrus += "\t\t.docString \"\"" + Environment.NewLine;
			papyrus += "\t\t.autoState" + Environment.NewLine;

			papyrus += "\t\t.variableTable" + Environment.NewLine;
			foreach (var variable in type.Fields)
			{
				var varProps = Utility.GetFlagsAndProperties(variable);



				var n = variable.Name.Replace('<', '_').Replace('>', '_');
				var t = variable.FieldType;
				var v = variable.InitialValue;
				string initialValue = "";
				string fieldType = Utility.GetPapyrusReturnType(t.Name, t.Namespace);


				if (variable.FieldType.IsGenericInstance)
				{
					fieldType = fieldType.Replace("`1", "_" + Utility.GetPapyrusBaseType(t.FullName.Split('<')[1].Split('>')[0]));
				}

				papyrus += Utility.StringIndent(3, ".variable ::" + n + " " + fieldType);

				var userFlagsVal = varProps.UserFlagsValue;
				if (varProps.IsHidden && varProps.IsProperty)
				{
					userFlagsVal -= 1;
					// The hidden flag only effects the Property field, declared after the variable field
				}

				papyrus += Utility.StringIndent(4, ".userFlags " + userFlagsVal);

				if (v != null && v.Length > 0)
				{
					if (t.FullName.ToLower().Contains("system.string"))
					{
						initialValue = "\"" + UTF8Encoding.Default.GetString(v) + "\"";
					}

					if (t.FullName.ToLower().Contains("system.bool"))
					{
						initialValue = (v[0] == 1).ToString();
					}
				}
				else initialValue = "None";

				if (varProps.InitialValue != null) initialValue = varProps.InitialValue;

				papyrus += Utility.StringIndent(4, ".initialValue " + initialValue);
				papyrus += Utility.StringIndent(3, ".endVariable");

				var newVar = new PapyrusVariableReference("::" + n, fieldType);
				newVar.Properties = varProps;
				Fields.Add(newVar);
			}
			papyrus += "\t\t.endVariableTable" + Environment.NewLine;


			papyrus += "\t\t.propertyTable" + Environment.NewLine;

			foreach (var propField in Fields.Where(f => f.Properties.IsProperty))
			{
				string autoMarker = propField.Properties.IsAuto ? " auto" : "";

				var propName = Utility.GetPropertyName(propField.Name);
				var fieldType = propField.TypeName;
				if (propField.Properties.IsGeneric)
				{
					// fieldType = fieldType.Replace("`1", "_" + Utility.GetPapyrusBaseType(prop.FullName.Split('<')[1].Split('>')[0]));
				}


				papyrus += Utility.StringIndent(3, ".property " + propName + " " + fieldType + autoMarker);
				papyrus += Utility.StringIndent(4, ".userFlags " + propField.Properties.UserFlagsValue);
				papyrus += Utility.StringIndent(4, ".docString \"\"");
				papyrus += Utility.StringIndent(4, ".autoVar " + propField.Name);
				papyrus += Utility.StringIndent(3, ".endProperty");
			}

			papyrus += "\t\t.endPropertyTable" + Environment.NewLine;

			papyrus += "\t\t.stateTable" + Environment.NewLine;
			papyrus += "\t\t\t.state" + Environment.NewLine;
			var methods = type.Methods;
			foreach (var method in methods)
			{
				papyrus += CreatePapyrusMethod(type, method);
			}

			papyrus += "\t\t\t.endState" + Environment.NewLine;
			papyrus += "\t\t.endStateTable" + Environment.NewLine;
			papyrus += "\t.endObject" + Environment.NewLine;
			papyrus += ".endObjectTable";


			var rows = papyrus.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
			var definedLabels = new Dictionary<int, string>();
			int rowI = 0;

			List<Utility.CodeBlock> codeBlocks = new List<Utility.CodeBlock>();
			Utility.CodeBlock latestCodeBlock = null;

			foreach (var row in rows)
			{
				if (row.Replace("\t", "").Trim().StartsWith(".code"))
				{
					latestCodeBlock = new Utility.CodeBlock();
					latestCodeBlock.StartRow = rowI;
				}
				else if (row.Replace("\t", "").Trim().StartsWith(".endCode"))
				{
					if (latestCodeBlock != null)
					{
						latestCodeBlock.EndRow = rowI;
						codeBlocks.Add(latestCodeBlock);
					}

				}
				else if (latestCodeBlock != null)
				{
					if (row.Replace("\t", "").StartsWith("_") && row.Trim().EndsWith(":"))
					{
						latestCodeBlock.Labels.Add(new PapyrusLabelDefinition(rowI, row.Replace("\t", "").Trim()));
					}
					if (row.Replace("\t", "").Contains("_label") && !row.Contains(":") && row.ToLower().Contains("jump"))
					{
						latestCodeBlock.UsedLabels.Add(new PapyrusLabelReference(row.Substring(row.IndexOf("_label")).Split(' ')[0] + ":", rowI));
					}
				}
				rowI++;
			}

			Dictionary<int, string> labelsToRemove = new Dictionary<int, string>();

			foreach (var block in codeBlocks)
			{
				foreach (var lbl in block.Labels)
				{
					bool isCalled = false;
					foreach (var ulbl in block.UsedLabels)
					{
						if (lbl.Name == ulbl.Name)
						{
							isCalled = true;
						}
					}
					if (!isCalled)
					{
						labelsToRemove.Add(lbl.Row, lbl.Name);
					}
				}
			}
			/*
			var ordered = labelsToRemove.OrderByDescending(i => i.Key).ToArray();

			foreach (var row in ordered)
			{
				rows.RemoveAt(row.Key);
			}
			*/
			return string.Join(Environment.NewLine, rows.ToArray());
		}




		public static string CreatePapyrusInfo(AssemblyDefinition asm)
		{
			string output = "";
			output += ".info" + Environment.NewLine;
			output += "\t.source \"PapyrusDotNet-Generated.psc\"" + Environment.NewLine;
			output += "\t.modifyTime " + Utility.ConvertToTimestamp(DateTime.Now) + Environment.NewLine;
			output += "\t.compileTime " + Utility.ConvertToTimestamp(DateTime.Now) + Environment.NewLine;
			output += "\t.user \"" + Environment.UserName + "\"" + Environment.NewLine;
			output += "\t.computer \"" + Environment.MachineName + "\"" + Environment.NewLine;
			output += ".endInfo" + Environment.NewLine;
			return output;
		}

		public static string CreatePapyrusUserflagRef()
		{
			string output = "";
			output += ".userFlagsRef" + Environment.NewLine;
			output += "\t.flag conditional 1" + Environment.NewLine;
			output += "\t.flag hidden 0" + Environment.NewLine;
			output += ".endUserFlagsRef" + Environment.NewLine;
			return output;
		}


		public static string GetPapyrusType(TypeReference reference)
		{

			switch (reference.Name)
			{
				// for now...
				default:
					return "";
			}

			return reference.Name;
		}

		static string CreatePapyrusMethod(TypeDefinition type, MethodDefinition method)
		{
			// We wont handle any Constructors
			if (!HandleConstructorAsOnInit && method.IsConstructor) return "";

			string output = "";

			var functionWriter = new PapyrusFunctionWriter(CurrentAssembly, type, Fields);
			var function = functionWriter.CreateFunction(method);
			return function;


		}
		// st codes are to store to the stack
		// ld is to load from stack and assign its value to either function or variable
		// ld is also used for ret when returning a value
	}
}
