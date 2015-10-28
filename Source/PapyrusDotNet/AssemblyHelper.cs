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
	
	Copyright 2015, Karl Patrik Johansson, zerratar@gmail.com
 */

namespace PapyrusDotNet
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;

    public class AssemblyHelper
    {
        public static List<AssemblyDefinition> GetAssemblyReferences(AssemblyDefinition asm)
        {
            var inputFileInfo = new System.IO.FileInfo(Program.inputFile);
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

        public static List<GenericTypeReference> GetAllReferences(TypeDefinition type, AssemblyDefinition asm)
        {
            var gReferences = new List<GenericTypeReference>();
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

        public static List<GenericTypeReference> GetAllGenericReferences(TypeDefinition type, AssemblyDefinition asm)
        {
            var gReferences = new List<GenericTypeReference>();
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

        public static void GetReferences(TypeDefinition type, ModuleDefinition mod, List<GenericTypeReference> gReferences)
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
                                        gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                            gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                        gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                gReferences.Add(new GenericTypeReference(usedType, t.FullName));
                            }
                        }

                    }
                }
            }
        }

        public static void GetGenericReferences(TypeDefinition type, ModuleDefinition mod, List<GenericTypeReference> gReferences, TypeDefinition ignore = null)
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
                    List<GenericTypeReference> tRefs = new List<GenericTypeReference>();
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
                                        gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                            gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                        gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                gReferences.Add(new GenericTypeReference(usedType, t.FullName));
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
                                gReferences.Add(new GenericTypeReference(usedType, t.FullName));
                            }
                        }
                    }
                }
            }
        }
    }
}