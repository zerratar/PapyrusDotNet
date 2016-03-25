using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PapyrusDotNet.Common.Extensions;
using PapyrusDotNet.PapyrusAssembly;

namespace PapyrusDotNet.PexInspector.ViewModels
{
    public class PexTreeBuilder : IPexTreeBuilder
    {
        private readonly IPexLoader pexLoader;

        public PexTreeBuilder(IPexLoader pexLoader)
        {
            this.pexLoader = pexLoader;
        }

        public ObservableCollection<PapyrusViewModel> BuildPexTree(ObservableCollection<PapyrusViewModel> pexTree, PapyrusViewModel target = null)
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToList();
            var loadedAssemblyNames = pexLoader.GetLoadedAssemblyNames();
            if (target != null)
            {
                var itemIndex = pexTree.IndexOf(target);
                var asm = target.Item as PapyrusAssemblyDefinition;
                if (asm != null)
                {

                    var asmnames = loadedAssemblyNames.Values.ToArray();
                    var asmIndex =
                        loadedAssemblies.IndexOf(i => Enumerable.First<PapyrusTypeDefinition>(i.Types).Name.Value == asm.Types.First().Name.Value);
                    PapyrusViewModel newNode;
                    if (BuildPexTree(asmIndex, asmnames, out newNode)) return pexTree;

                    pexTree.RemoveAt(itemIndex);
                    pexTree.Insert(itemIndex, newNode);
                }
            }
            else
            {
                var asmnames = loadedAssemblyNames.Values.ToArray();
                var rootNodes = new List<PapyrusViewModel>();
                for (var index = 0; index < loadedAssemblies.Count; index++)
                {
                    PapyrusViewModel newNode;
                    if (BuildPexTree(index, asmnames, out newNode))
                        return pexTree; // the tree will be reloaded, so we don't wanna finish it here.
                    if (newNode != null)
                    {
                        rootNodes.Add(newNode);
                    }
                }

                pexTree = new ObservableCollection<PapyrusViewModel>(rootNodes.OrderBy(i => i.Text));
            }

            return pexTree;
        }

        public bool BuildPexTree(int assemblyIndex, string[] asmnames, out PapyrusViewModel root)
        {
            var loadedAssemblies = pexLoader.GetLoadedAssemblies().ToArray();
            var asm = loadedAssemblies[assemblyIndex];
            root = new PapyrusViewModel();
            root.Item = asm;
            root.Text = asmnames[assemblyIndex];
            foreach (var type in asm.Types)
            {
                var typeNode = new PapyrusViewModel(root);
                typeNode.Item = type;
                typeNode.Text = type.Name.Value +
                                (!string.IsNullOrEmpty(type.BaseTypeName?.Value)
                                    ? " : " + type.BaseTypeName.Value
                                    : "");

                if (!string.IsNullOrEmpty(type.BaseTypeName?.Value))
                {
                    if (pexLoader.EnsureAssemblyLoaded(type.BaseTypeName.Value)) return true;
                }

                foreach (var structType in type.NestedTypes.OrderBy(i => i.Name.Value))
                {
                    var structTypeNode = new PapyrusViewModel(typeNode);
                    structTypeNode.Item = structType;
                    structTypeNode.Text = structType.Name.Value;
                    foreach (var field in structType.Fields)
                    {
                        var fieldNode = new PapyrusViewModel(structTypeNode)
                        {
                            Item = field,
                            Text = field.Name.Value + " : " + field.TypeName
                        };

                        if (!string.IsNullOrEmpty(field.TypeName))
                        {
                            if (type.BaseTypeName != null && pexLoader.EnsureAssemblyLoaded(type.BaseTypeName.Value)) return true;
                        }
                    }
                }

                //var statesNode = new PapyrusViewModel(typeNode);
                //statesNode.Item = "states";
                //statesNode.Text = "States";
                foreach (var item in type.States.OrderBy(i => i.Name.Value))
                {
                    var stateNode = new PapyrusViewModel(typeNode);
                    stateNode.Item = item;
                    stateNode.Text = !string.IsNullOrEmpty(item.Name.Value) ? item.Name.Value : "<default>";
                    foreach (var method in item.Methods.OrderBy(i => i.Name.Value))
                    {
                        var m = new PapyrusViewModel(stateNode);
                        m.Item = method;
                        m.Text = method.Name.Value + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;

                        if (!string.IsNullOrEmpty(method.ReturnTypeName.Value))
                        {
                            if (pexLoader.EnsureAssemblyLoaded(method.ReturnTypeName.Value)) return true;
                        }
                    }
                }

                foreach (var field in type.Fields.OrderBy(i => i.Name.Value))
                {
                    var fieldNode = new PapyrusViewModel(typeNode);
                    fieldNode.Item = field;
                    fieldNode.Text = field.Name.Value + " : " + field.TypeName;

                    if (!string.IsNullOrEmpty(field.TypeName))
                    {
                        if (pexLoader.EnsureAssemblyLoaded(field.TypeName)) return true;
                    }
                }

                foreach (var item in type.Properties.OrderBy(i => i.Name.Value))
                {
                    var fieldNode = new PapyrusViewModel(typeNode);
                    fieldNode.Item = item;
                    fieldNode.Text = item.Name.Value + " : " + item.TypeName.Value;

                    if (!string.IsNullOrEmpty(item.TypeName.Value))
                    {
                        if (pexLoader.EnsureAssemblyLoaded(item.TypeName.Value)) return true;
                    }

                    if (item.HasGetter && item.GetMethod != null)
                    {
                        var method = item.GetMethod;
                        var m = new PapyrusViewModel(fieldNode);
                        m.Item = method;
                        m.Text = "Getter" + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;
                    }

                    if (item.HasSetter && item.SetMethod != null)
                    {
                        var method = item.SetMethod;
                        var m = new PapyrusViewModel(fieldNode);
                        m.Item = method;
                        m.Text = "Setter" + GetParameterString(method.Parameters) + " : " +
                                 method.ReturnTypeName.Value;
                    }
                }
            }

            return false;
        }


        private string GetParameterString(IEnumerable<PapyrusParameterDefinition> parameters,
            bool includeParameterNames = false)
        {
            return "(" + string.Join(", ", parameters.Select(p => p.TypeName.Value +
                                                                  (includeParameterNames ? " " + p.Name.Value : ""))) + ")";
        }
    }
}