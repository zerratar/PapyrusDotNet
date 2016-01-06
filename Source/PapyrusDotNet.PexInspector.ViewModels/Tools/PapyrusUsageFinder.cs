using System.Collections.Generic;
using PapyrusDotNet.PapyrusAssembly;
using System.Linq;

namespace PapyrusDotNet.PexInspector.ViewModels.Tools
{
    public class PapyrusUsageFinder : IPapyrusUsageFinder
    {
        private readonly IList<PapyrusAssemblyDefinition> haystack;

        /// <summary>
        /// Initializes a new instance of the <see cref="PapyrusUsageFinder"/> class.
        /// </summary>
        /// <param name="haystack">The haystack.</param>
        public PapyrusUsageFinder(IList<PapyrusAssemblyDefinition> haystack)
        {
            this.haystack = haystack;
        }

        /// <summary>
        /// Finds the method usage.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public IFindResult FindMethodUsage(string methodName)
        {
            var result = new FindResult(); result.SearchText = methodName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode != PapyrusOpCodes.Callmethod && i.OpCode != PapyrusOpCodes.Callparent &&
                                    i.OpCode != PapyrusOpCodes.Callstatic)
                                    continue;

                                var m2 = methodName.ToLower();
                                var a = i.GetArg(0).ToLower();
                                var b = i.GetArg(1).ToLower();
                                var c = "";
                                if (i.Arguments.Count > 2)
                                    c = i.GetArg(2).ToLower();

                                if (c == m2 || a == m2 || b == m2)
                                {
                                    result.AddResult(
                                        t, s, m, i, methodName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + methodName + "(" + string.Join(", ", i.OperandArguments.Select(j => j.Value)) + ")");
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the property usage.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public IFindResult FindPropertyUsage(string propertyName)
        {
            var result = new FindResult(); result.SearchText = propertyName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode != PapyrusOpCodes.PropGet && i.OpCode != PapyrusOpCodes.PropSet)
                                    continue;

                                var m2 = propertyName.ToLower();
                                var a = i.GetArg(0).ToLower();
                                var b = i.GetArg(1).ToLower();
                                var c = "";
                                if (i.Arguments.Count > 2)
                                    c = i.GetArg(2).ToLower();

                                if (c == m2 || a == m2 || b == m2)
                                {
                                    result.AddResult(
                                        t, s, m, i, propertyName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + propertyName);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the field usage.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public IFindResult FindFieldUsage(string fieldName)
        {
            var result = new FindResult(); result.SearchText = fieldName;
            foreach (var asm in haystack)
            {
                foreach (var t in asm.Types)
                {
                    foreach (var s in t.States)
                    {
                        foreach (var m in s.Methods)
                        {
                            foreach (var i in m.Body.Instructions)
                            {
                                if (i.OpCode == PapyrusOpCodes.Nop)
                                    continue;

                                var m2 = fieldName.ToLower();

                                if (i.OperandArguments.Any(a => a.GetStringRepresentation().ToLower() == m2) || i.Arguments.Any(a => a.GetStringRepresentation().ToLower() == m2))
                                {
                                    result.AddResult(
                                        t, s, m, i, fieldName,
                                        t.Name + "->" + s.Name + "->" + m.Name.Value + "-> L_" + i.Offset + ": " +
                                        i.OpCode + " - " + fieldName);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

    }
}