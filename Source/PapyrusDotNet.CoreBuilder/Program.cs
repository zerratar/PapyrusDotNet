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

namespace PapyrusDotNet.CoreBuilder
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;

	using Mono.Cecil;
	using Mono.Cecil.Cil;

	using PapyrusDotNet.Common;
	using PapyrusDotNet.CoreBuilder.CoreExtensions;

	using PowerArgs;

	public enum PapyrusParseType
	{
		Script,
		Assembly
	}

	class Program
	{
		internal static ModuleDefinition MainModule;

		internal static AssemblyDefinition Core;

		internal static string[] WordList;

		internal static PapyrusParseType ParseType { get; set; }

		internal static List<string> PreservedTypeNames = new List<string>();

		internal static string OutputFileName = "PapyrusDotNet.Core.dll";

		internal static string CoreNamespace = "PapyrusDotNet.Core";

		static void Main(string[] args)
		{
			ParseType = PapyrusParseType.Assembly;



			string inputDirectory = @"C:\The Elder Scrolls V Skyrim\Papyrus Compiler";
			string searchFor = "*.pas";
			string typeName = "assembly";

			try
			{
				var parsed = Args.Parse<PapyrusDotNetArgs>(args);
				if (parsed.InputFolder != null)
				{
					// Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.StringArg, parsed.IntArg);
					if (parsed.InputType.ToLower().Contains("script") || parsed.InputType.ToLower().Contains("psc"))
					{
						ParseType = PapyrusParseType.Script;
						searchFor = "*.psc";
						typeName = "script";
					}

					inputDirectory = parsed.InputFolder;
					if (inputDirectory.Contains("\""))
					{
						inputDirectory = inputDirectory.Replace("\"", "");
					}
					// Console.WriteLine("You entered string '{0}' and int '{1}'", parsed.InputFile, parsed.OutputFolder);
				}
				if (parsed.InputFolder == null)
				{
					Console.WriteLine("No arguments were defined, using default.");
					Console.WriteLine("Input: " + inputDirectory);
					Console.WriteLine("Type: " + typeName);
					Console.WriteLine();
					Console.WriteLine("If you are fine with this, press any key to continue.");
					Console.WriteLine("Or hit CTRL+C to exit.");
					Console.ReadKey();
				}
			}
			catch (ArgException ex)
			{
				Console.WriteLine("Crash on startup :(");
				Console.WriteLine("Exception thrown: " + ex);

				return;
			}





			Console.WriteLine("Loading all " + typeName + " files...");

			// @"C:\The Elder Scrolls V Skyrim\Data\scripts\Source"
			var files = Directory.GetFiles(
				inputDirectory, searchFor, SearchOption.AllDirectories);

			if (File.Exists("wordlist.txt"))
			{
				Console.WriteLine("Loading wordlist...");
				WordList = File.ReadAllLines("wordlist.txt");
			}
			else
			{
				Console.WriteLine("Wordlist was not found, skipping...");
				WordList = new string[0];
			}

			Core = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(CoreNamespace, new Version(1, 0)),
				CoreNamespace, ModuleKind.Dll);

			MainModule = Core.MainModule;

			var papyrusObjects = new List<PapyrusAsmObject>();

			Console.WriteLine("Parsing Papyrus...");
			papyrusObjects.AddRange(files.Select(Parse));


			Console.WriteLine("Adding object references...");
			foreach (var pasObj in papyrusObjects)
				AddedTypeReferences.Add(GetTypeReference(null, pasObj.Name));

			foreach (var pas in papyrusObjects)
				MainModule.Types.Add(TypeDefinitionFromPapyrus(pas));

			Console.WriteLine("Resolving object references...");
			foreach (var t in MainModule.Types)
			{
				foreach (var f in t.Methods)
				{
					var typeDefinition = MainModule.Types.FirstOrDefault(ty => ty.FullName == f.ReturnType.FullName);
					f.ReturnType = GetTypeReference(typeDefinition, f.ReturnType.FullName);
					foreach (var p in f.Parameters)
					{
						var td = MainModule.Types.FirstOrDefault(ty => ty.FullName == p.ParameterType.FullName);
						if (td != null)
							/*	// Most likely a System object.							
								p.ParameterType = GetTypeReference(null, p.ParameterType.FullName);
							else */
							p.ParameterType = GetTypeReference(typeDefinition, td.FullName);
					}
				}
			}

			Console.WriteLine("Importing Papyrus specific attributes...");


			var allAttributesToInclude = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name.ToLower().EndsWith("attribute"));

			bool isOk = true;
			foreach (var attr in allAttributesToInclude)
			{
				isOk = isOk && IncludeType(MainModule, attr);
			}
		
			if (!isOk)
			{
				// dumbidum
			}



			Core.Write(OutputFileName);
			Console.WriteLine(OutputFileName + " successefully generated.");
		}

		private static bool IncludeType(ModuleDefinition MainModule, Type type)
		{
			try
			{
				var reference = MainModule.Import(type);

				var definition = reference.Resolve();

				var newType = new TypeDefinition(CoreNamespace, definition.Name, TypeAttributes.Class);
				newType.IsPublic = true;
				newType.BaseType = definition.BaseType;

				foreach (var field in definition.Fields)
				{
					var newField = new FieldDefinition(field.Name, FieldAttributes.Public, field.FieldType);
					newType.Fields.Add(newField);
				}

				foreach (var field in definition.Events)
				{
					var newField = new EventDefinition(field.Name, EventAttributes.None, field.EventType);
					newType.Events.Add(newField);
				}

				var constructor = definition.Methods.FirstOrDefault(m => m.IsConstructor);
				if (constructor != null)
					MainModule.Import(constructor);

				if (definition.BaseType != null)
				{
					try
					{
						var baseDef = definition.BaseType.Resolve();
						constructor = baseDef.Methods.FirstOrDefault(m => m.IsConstructor);
						if (constructor != null)
							MainModule.Import(constructor);
					}
					catch { }
				}

				// if (constructor != null && !constructor.HasParameters)
				AddEmptyConstructor(newType);

				foreach (var field in definition.Methods)
				{

					if (field.IsConstructor && !field.HasParameters) continue;




					var newField = new MethodDefinition(field.Name, field.Attributes, field.ReturnType);

					var refer = MainModule.Import(field);

					foreach (var fp in field.Parameters)
					{
						if (fp.Name.Contains("<"))
						{

						}
						var newParam = new ParameterDefinition(fp.Name, fp.Attributes, fp.ParameterType);
						newField.Parameters.Add(newParam);
					}
					/*	
						if (field.HasBody)
						{
							foreach (var inst in field.Body.Instructions)
							{
								if (inst.Operand is MethodReference)
									MainModule.Import(inst.Operand as MethodReference);
								if (inst.Operand is FieldReference)
									MainModule.Import(inst.Operand as FieldReference);

							
								// newField.Body.Instructions.Add(inst);
							}
						}*/

					CreateEmptyFunctionBody(ref newField);
					newType.Methods.Add(newField);
				}

				MainModule.Types.Add(newType);
				return true;
			}
			catch { }
			return false;
			/*
			MainModule.Import(
			   typeof(AutoReadOnlyAttribute));

			MainModule.Import(
				typeof(ConditionalAttribute));

			MainModule.Import(
				typeof(HiddenAttribute));*/
		}

		public static TypeDefinition TypeDefinitionFromPapyrus(PapyrusAsmObject input)
		{
			var newType = new TypeDefinition(CoreNamespace, input.Name, TypeAttributes.Class);


			newType.IsPublic = true;
			// newType.DeclaringType = newType;
			if (!string.IsNullOrEmpty(input.ExtendsName))
			{
				newType.BaseType = new TypeReference(CoreNamespace, input.ExtendsName, MainModule, MainModule);
				// newType.DeclaringType = MainModule.Types.FirstOrDefault(t => t.FullName == newType.BaseType.FullName);
				newType.Scope = MainModule;
			}
			else
			{
				newType.BaseType = MainModule.TypeSystem.Object;
				newType.Scope = MainModule;
			}

			Console.WriteLine("Generating Type '" + CoreNamespace + "." + input.Name + "'...");

			foreach (var prop in input.PropertyTable)
			{
				var typeRef = GetTypeReference(null, prop.Type);
				var pro = new PropertyDefinition(prop.Name, PropertyAttributes.HasDefault, typeRef);
				newType.Properties.Add(pro);
			}

			// newType.AddDefaultConstructor();

			AddEmptyConstructor(newType);

			AddVirtualOnInit(newType);

			foreach (var state in input.States)
			{
				TypeReference typeRef = GetTypeReference(null, state.ReturnType);
				// var typeRef = MainModule.TypeSystem.Void;

				var function = new MethodDefinition(state.Name, MethodAttributes.Public, typeRef);
				function.IsStatic = state.IsStatic;
				if (function.IsStatic)
					function.HasThis = false;
				if (!function.IsStatic && state.Name.StartsWith("On"))
					function.IsVirtual = true;
				else function.IsVirtual = false;

				CreateEmptyFunctionBody(ref function);

				foreach (var par in state.Params)
				{
					TypeReference typeRefp = GetTypeReference(null, par.Type);
					// var typeRefp = MainModule.TypeSystem.Object;

					var nPar = new ParameterDefinition(par.Name, ParameterAttributes.None, typeRefp);
					function.Parameters.Add(nPar);
				}
				bool skipAdd = false;
				foreach (var m in newType.Methods)
				{
					if (m.Name == function.Name)
					{
						if (m.Parameters.Count == function.Parameters.Count)
						{
							skipAdd = true;
							for (int pi = 0; pi < m.Parameters.Count; pi++)
							{
								if (m.Parameters[pi].ParameterType.FullName != function.Parameters[pi].ParameterType.FullName) skipAdd = false;
							}
							break;
						}
					}
				}
				if (!skipAdd)
					newType.Methods.Add(function);
			}
			return newType;
		}

		private static void CreateEmptyFunctionBody(ref MethodDefinition function)
		{
			var fnl = function.ReturnType.FullName.ToLower();
			if (fnl.Equals("system.void"))
			{
				// Do nothing	
			}

			else if (fnl.Contains("[]"))
			{
				function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
			}

			else if (fnl.StartsWith("system.string") || fnl.StartsWith("system.object") || fnl.StartsWith(CoreNamespace.ToLower()))
				function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));

			else if (fnl.StartsWith("system.int") || fnl.StartsWith("system.bool") || fnl.StartsWith("system.long") || fnl.StartsWith("system.byte") || fnl.StartsWith("system.short"))
			{
				if (fnl.StartsWith("system.long"))
					function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L));
				else
					function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
			}

			else if (fnl.StartsWith("system.float"))
				function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, 0f));

			else if (fnl.StartsWith("system.double"))
				function.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R8, 0d));


			function.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
		}

		private static void AddVirtualOnInit(TypeDefinition newType)
		{
			if (!newType.Methods.Any(v => v.Name == "OnInit"))
			{
				var methodAttributes = MethodAttributes.Public;
				var method = new MethodDefinition("OnInit", methodAttributes, MainModule.TypeSystem.Void);
				method.IsVirtual = true;



				method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				newType.Methods.Add(method);
			}
		}

		private static TypeReference GetTypeReference(TypeDefinition newType, string fallback = null)
		{
			var typeName = "";
			if (!string.IsNullOrEmpty(fallback))
				typeName = fallback;
			else
				typeName = newType.FullName;

			var ns = GetTypeNamespace(typeName);
			var tn = GetTypeName(typeName);
			var isArray = tn.EndsWith("[]");

			if (ns == "System")
			{

				var propies = MainModule.TypeSystem.GetType().GetProperties().Where(pr => pr.PropertyType == typeof(TypeReference)).ToList();
				foreach (var propy in propies)
				{
					var name = propy.Name;
					if (tn.Replace("[]", "").ToLower() == name.ToLower())
					{
						var val = propy.GetValue(MainModule.TypeSystem, null) as TypeReference;
						return val;
					}
				}
				// fallback
				switch (tn.ToLower())
				{
					case "none":
					case "void":
						return MainModule.TypeSystem.Void;
					case "byte":
					case "short":
					case "int":
					case "long":
					case "int8":
					case "int16":
					case "int32":
					case "int64":
						return MainModule.TypeSystem.Int32;
					case "string":
						return MainModule.TypeSystem.String;
					case "float":
					case "double":
						return MainModule.TypeSystem.Double;
					case "bool":
					case "boolean":
						return MainModule.TypeSystem.Boolean;
					default:
						return MainModule.TypeSystem.Object;
				}
			}
			var tnA = tn.Replace("[]", "");
			var existing = AddedTypeReferences.FirstOrDefault(ty => ty.FullName.ToLower() == (ns + "." + tnA).ToLower());
			if (existing == null)
			{
				var hasTypeOf = MainModule.Types.FirstOrDefault(t => t.FullName.ToLower() == (ns + "." + tnA).ToLower());
				if (hasTypeOf != null)
				{
					var typeRef = new TypeReference(hasTypeOf.Namespace, hasTypeOf.Name, MainModule, MainModule);
					typeRef.Scope = MainModule;
					AddedTypeReferences.Add(typeRef);
					return typeRef;
				}
				else
				{
					if (PreservedTypeNames.Any(n => n.ToLower() == tnA.ToLower()))
					{
						tn = PreservedTypeNames.FirstOrDefault(j => j.ToLower() == tnA.ToLower());
					}
					var typeRef = new TypeReference(ns, tn, MainModule, MainModule);
					typeRef.Scope = MainModule;
					AddedTypeReferences.Add(typeRef);
					return typeRef;
				}

			}

			return existing;
		}

		public static List<TypeReference> AddedTypeReferences = new List<TypeReference>();

		public static void AddEmptyConstructor(TypeDefinition type)
		{
			var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
			var method = new MethodDefinition(".ctor", methodAttributes, MainModule.TypeSystem.Void);

#warning might need to fix this later so that PEVERIFY can verify the outputted library properly.
			// var baseEmptyConstructor = new MethodReference(".ctor", MainModule.TypeSystem.Void, MainModule.TypeSystem.Object);// MainModule.TypeSystem.Object
			// method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
			// method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseEmptyConstructor));



			method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
			type.Methods.Add(method);
		}

		private static string GetTypeName(string p)
		{
			if (p.Contains('.')) p = p.Split('.').LastOrDefault();
			var pl = p.ToLower();

			/*if (p.EndsWith("[]"))
			{
				pl = pl.Replace("[]", "");
			}*/

			if (pl == "boolean")
				return "bool";
			if (pl == "none")
				return "void";

			if (pl == "float" || pl == "int" || pl == "bool" || pl == "string") return pl;

			return p;
		}

		private static string GetTypeNamespace(string p)
		{
			if (p.Contains('.')) p = p.Split('.').LastOrDefault();
			var pl = p.ToLower();

			if (p.EndsWith("[]"))
			{
				pl = pl.Replace("[]", "");
			}

			/* have not added all possible types yet though.. might be a better way of doing it. */
			if (pl == "string" || pl == "int" || pl == "boolean" || pl == "bool" || pl == "none"
			   || pl == "void" || pl == "float" || pl == "short" || pl == "char" || pl == "double"
			   || pl == "int32" || pl == "integer32" || pl == "long" || pl == "uint")
			{
				return "System";
			}
			return CoreNamespace;
		}

		public static PapyrusAsmObject Parse(string file)
		{
			var ext = Path.GetExtension(file);
			if (!string.IsNullOrEmpty(ext))
			{
				if (ext.ToLower().EndsWith("pas"))
				{
					return ParseAsm(file);
				}
				return ParseScript(file);
			}
			return null;
		}

		private static PapyrusAsmObject ParseScript(string file)
		{
			var inputScript = File.ReadAllLines(file);
			var obj = new PapyrusAsmObject();

			throw new NotImplementedException("Parsing from .psc/ Skyrim Papyrus Scripts are not yet supported.");

			//	TypeDefinition def = new TypeDefinition("","", TypeAttributes.);

			return obj;
		}

		public static PapyrusAsmObject ParseAsm(string file)
		{

			var inputScript = System.IO.File.ReadAllLines(file);
			var obj = new PapyrusAsmObject();
			var inVariableTable = false;
			var inPropertyTable = false;
			var inStateTable = false;
			var inFunction = false;
			var inFunctionLocalTable = false;
			var inFunctionParamTable = false;

			PapyrusAsmState latestFunction = null;
			foreach (var line in inputScript)
			{
				var tLine = line.Replace("\t", "").Trim();
				if (tLine.Contains(";"))
					tLine = tLine.Split(';')[0].Trim();

				if (tLine.StartsWith(".variableTable"))
					inVariableTable = true;
				if (tLine.StartsWith(".endVariableTable"))
					inVariableTable = false;

				if (tLine.StartsWith(".propertyTable"))
					inPropertyTable = true;
				if (tLine.StartsWith(".endPropertyTable"))
					inPropertyTable = false;

				if (tLine.StartsWith(".stateTable"))
					inStateTable = true;
				if (tLine.StartsWith(".endStateTable"))
					inStateTable = false;

				if (tLine.StartsWith(".paramTable"))
					inFunctionParamTable = true;
				if (tLine.StartsWith(".endParamTable"))
					inFunctionParamTable = false;

				if (tLine.StartsWith(".localTable"))
					inFunctionLocalTable = true;
				if (tLine.StartsWith(".endLocalTable"))
					inFunctionLocalTable = false;

				if (tLine.StartsWith(".object "))
				{
					//			obj.Name = tLine.Split(' ')[1];
					obj.Name = Path.GetFileNameWithoutExtension(file);



					if (obj.Name.Contains("."))
					{
						obj.Name = obj.Name.Split('.')[0];
					}

					string before = obj.Name;

					if (WordList != null && WordList.Length > 0)
					{
						if (!Char.IsUpper(obj.Name[0]))
						{
							var usedWords = new List<string>();
							var ordered = WordList.OrderByDescending(o => o.Length);
							foreach (var word in ordered)
							{
								if (string.IsNullOrEmpty(word) || word.Length < 4) continue;
								if (obj.Name.ToLower().Contains(word) && !usedWords.Any(s => s.Contains(word)))
								{
									var i = obj.Name.ToLower().IndexOf(word);

									bool skip = false;
									if (i > 0)
									{
										skip = Char.IsUpper(obj.Name[i - 1]);
									}

									if (skip) continue;

									var w = Char.ToUpper(word[0]) + word.Substring(1);
									obj.Name = obj.Name.Replace(word, w);
									usedWords.Add(word);
								}
							}
						}

					}
					if (!Char.IsUpper(obj.Name[0]))
					{
						obj.Name = Char.ToUpper(obj.Name[0]) + obj.Name.Substring(1);
					}
					var theBefore = before;
					var theAfter = obj.Name;



					if (tLine.Split(' ').Length > 2)
					{
						obj.ExtendsName = tLine.Split(' ')[2];
					}
					if (tLine.Contains("extends"))
					{
						obj.ExtendsName = tLine.Split(' ')[3];// Parse(@"C:\The Elder Scrolls V Skyrim\Papyrus Compiler\" + tLine.Split(' ')[3] + ".disassemble.pas");
					}
				}

				if (inVariableTable)
				{

				}
				else if (inPropertyTable)
				{

				}
				else if (inStateTable)
				{
					if (tLine.StartsWith(".state") || tLine.StartsWith(".endState")) continue;
					if (tLine.StartsWith(".function "))
					{
						inFunction = true;
						latestFunction = new PapyrusAsmState();

						if (tLine.Contains(" static")) latestFunction.IsStatic = true;
						latestFunction.Name = tLine.Split(' ')[1];
					}
					if (tLine.StartsWith(".endFunction"))
					{
						inFunction = false;
						obj.States.Add(latestFunction);
					}
					if (inFunctionLocalTable && latestFunction != null)
					{
						if (tLine.StartsWith(".local "))
						{
							latestFunction.LocalTable.Add(new PapyrusAsmVariable(tLine.Split(' ')[1], tLine.Split(' ')[2]));
						}
					}
					if (inFunctionParamTable && latestFunction != null)
					{
						if (tLine.StartsWith(".param "))
						{
							latestFunction.Params.Add(new PapyrusAsmVariable(tLine.Split(' ')[1], tLine.Split(' ')[2]));
						}
					}
					if (tLine.StartsWith(".return ") && latestFunction != null)
					{
						latestFunction.ReturnType = tLine.Split(' ')[1];
					}
				}
			}
			return obj;
		}
	}

	public class PapyrusAsmObject
	{
		public List<PapyrusAsmVariable> VariableTable { get; set; }
		public List<PapyrusAsmVariable> PropertyTable { get; set; }
		public List<PapyrusAsmState> States { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public PapyrusAsmObject Extends { get; set; }
		public PapyrusAsmObject()
		{
			VariableTable = new List<PapyrusAsmVariable>();
			PropertyTable = new List<PapyrusAsmVariable>();
			States = new List<PapyrusAsmState>();

		}

		public string ExtendsName { get; set; }
	}

	public class PapyrusAsmState
	{
		public string Name { get; set; }
		public bool IsStatic { get; set; }
		public string ReturnType { get; set; }
		public List<PapyrusAsmVariable> Params { get; set; }
		public List<PapyrusAsmVariable> LocalTable { get; set; }

		public PapyrusAsmState()
		{
			Params = new List<PapyrusAsmVariable>();
			LocalTable = new List<PapyrusAsmVariable>();

		}

	}
	public class PapyrusAsmVariable
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public PapyrusAsmVariable(string name, string type)
		{
			this.Name = name;
			this.Type = type;

			if (type.Contains('.'))
			{
				var n = type.Substring(type.LastIndexOf('.'));
				this.Type = type.Remove(type.LastIndexOf('.')) + Char.ToUpper(n[0]) + n.Substring(1);
			}
			else
			{
				this.Type = Char.ToUpper(type[0]) + type.Substring(1);
			}
		}
	}
}
