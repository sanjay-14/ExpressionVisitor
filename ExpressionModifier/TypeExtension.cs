using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    public static class TypeExtension
    {

        public static MethodInfo GetAnyMethodInfo()
        {
            var enumerableType = typeof(Enumerable);
            var bar =
            (
                from m in enumerableType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                where m.Name == "Any"
                let p = m.GetParameters()
                where p.Length == 2
                    && p[0].ParameterType.IsGenericType
                    && p[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && p[1].ParameterType.IsGenericType
                    && p[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                select m
            ).SingleOrDefault();
            return bar;
        }

        public static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly,
         Type extendedType)
        {
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
            return query;
        }

        public static MethodInfo[] GetExtensionMethods(this Type t)
        {
            List<Type> AssTypes = new List<Type>();

            foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssTypes.AddRange(item.GetTypes());
            }

            var query = from type in AssTypes
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == t
                        select method;
            return query.ToArray<MethodInfo>();
        }

        /// <summary>
        /// Extends the System.Type-type to search for a given extended MethodeName.
        /// </summary>
        /// <param name="MethodeName">Name of the Methode</param>
        /// <returns>the found Methode or null</returns>
        public static MethodInfo GetExtensionMethod(this Type t, string MethodeName)
        {
            var mi = from methode in t.GetExtensionMethods()
                     where methode.Name == MethodeName
                     select methode;
            if (mi.Count<MethodInfo>() <= 0)
                return null;
            else
                return mi.First<MethodInfo>();
        }
    }
}