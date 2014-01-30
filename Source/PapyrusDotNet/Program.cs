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
		static void Main(string[] args)
		{

			string outputFolder = @".\output";
			string inputFile = @"..\Examples\Example1\bin\Debug\Example1.dll";
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
							papyrus += CreatePapyrusInfo(CurrentAssembly);
							papyrus += CreatePapyrusUserflagRef();
							papyrus += CreatePapyrusObjectTable(type);
							if (!outputPASFiles.ContainsKey(GetPapyrusBaseType(type) + ".pas"))
								outputPASFiles.Add(GetPapyrusBaseType(type) + ".pas", papyrus);
						}
					}
				}

			}

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



		public static string CreatePapyrusObjectTable(TypeDefinition type)
		{
			string papyrus = "";

			papyrus += ".objectTable" + Environment.NewLine;
			papyrus += "\t.object " + GetPapyrusBaseType(type) + (type.BaseType != null ? " " + GetPapyrusBaseType(type.BaseType) : "") + Environment.NewLine;

			var props = Utility.GetFlagsAndProperties(type);

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
				papyrus += Utility.StringIndent(3, ".property " + propName + " " + propField.TypeName + autoMarker);
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



		public static string GetPapyrusBaseType(TypeReference typeRef)
		{
			if (typeRef.Name == "Object") return "";

			if (typeRef.Namespace.ToLower().StartsWith("papyrusdotnet.core."))
			{
				return "DotNet" + typeRef.Name;
			}
			if (typeRef.Namespace.StartsWith("PapyrusDotNet.Core"))
			{
				return typeRef.Name;
			}

			if (string.IsNullOrEmpty(typeRef.Namespace))
				return typeRef.Name;

			return typeRef.Namespace.Replace(".", "_") + "_" + typeRef.Name;
		}
		public static string GetPapyrusBaseType(string p)
		{
			switch (p)
			{
				case "Object":
					return "";
			}
			return p;
		}

		public static int ConvertToTimestamp(DateTime value)
		{
			//create Timespan by subtracting the value provided from
			//the Unix Epoch
			TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

			//return the total seconds (which is a UNIX timestamp)
			return (int)span.TotalSeconds;
		}

		public static string CreatePapyrusInfo(AssemblyDefinition asm)
		{
			string output = "";
			output += ".info" + Environment.NewLine;
			output += "\t.source \"PapyrusDotNet-Generated.psc\"" + Environment.NewLine;
			output += "\t.modifyTime " + ConvertToTimestamp(DateTime.Now) + Environment.NewLine;
			output += "\t.compileTime " + ConvertToTimestamp(DateTime.Now) + Environment.NewLine;
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
