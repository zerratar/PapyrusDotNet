using System;
using System.Collections.Generic;
using System.Linq;

namespace PapyrusDotNet.PexInspector
{
    public class IoCContainer
    {
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();

        private readonly Dictionary<Type, Type> typeLookup = new Dictionary<Type, Type>();
        
        public IoCContainer Register<TInterface, TImpl>()
        {
            var i = typeof(TInterface);
            var t = typeof(TImpl);
            if (typeLookup.ContainsKey(i))
                typeLookup[i] = t;
            else
                typeLookup.Add(i, t);
            return this;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private T CreateInstanceOf<T>()
        {
            return (T)CreateInstanceOf(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (instances.ContainsKey(type))
                return instances[type];
            var inst = CreateInstanceOf(type);
            instances.Add(type, inst);
            return inst;
        }

        private object CreateInstanceOf(Type type)
        {
            var f = type;
            var i = typeLookup[f];
            var ctors = i.GetConstructors();
            var lessStrict = ctors.OrderBy(j => j.GetParameters().Length).FirstOrDefault();
            if (lessStrict == null)
                throw new MissingMemberException(i.FullName, "Constructor");

            var param = lessStrict.GetParameters();
            var paramObjects = param.Select(t => Resolve(t.ParameterType)).ToArray();
            return lessStrict.Invoke(paramObjects);
        }
    }
}