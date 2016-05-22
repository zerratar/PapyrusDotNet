using System;
using System.Collections.Generic;
using System.Linq;

namespace PapyrusDotNet.Common
{
    public class IoCContainer
    {
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();

        private readonly Dictionary<Type, Type> typeLookup = new Dictionary<Type, Type>();

        private readonly Dictionary<Type, object> customTypeRegister = new Dictionary<Type, object>();

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

        public void RegisterCustom<TInterface>(Func<TInterface> func)
        {
            var t = typeof(TInterface);
            if (customTypeRegister.ContainsKey(t))
            {
                customTypeRegister[t] = func;
            }
            else
            {
                customTypeRegister.Add(t, func);
            }
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private T CreateInstanceOf<T>()
        {
            return (T)CreateInstanceOf(typeof(T));
        }

        private object Resolve(Type type)
        {
            if (instances.ContainsKey(type))
                return instances[type];
            var inst = CreateInstanceOf(type);
            instances.Add(type, inst);
            return inst;
        }

        private object CreateInstanceOf(Type type)
        {
            if (customTypeRegister.ContainsKey(type))
            {
                var func = (Delegate)customTypeRegister[type];
                if (func != null)
                    return func.DynamicInvoke();
            }

            var i = typeLookup[type];
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