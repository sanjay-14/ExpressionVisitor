using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionModifier
{
    public static class ExpressionThroughAPI
    {
        public static Expression GetExp()
        {
            ParameterExpression argParam = Expression.Parameter(typeof(EmployeeChangeSet), "s");
            Expression nameProperty = Expression.Property(argParam, "ContextValue");

            var val1 = Expression.Constant("S");

            Expression e1 = Expression.Equal(nameProperty, val1);
            var lambda = Expression.Lambda<Func<EmployeeChangeSet, bool>>(e1, argParam);
            return lambda;
        }

        public static Expression GetExpression()
        {
            ParameterExpression param = Expression.Parameter(typeof(EmployeeChangeSet), "e");
            ConstantExpression constValue = Expression.Constant("S");
            var Context = Expression.MakeMemberAccess(param, typeof(EmployeeChangeSet).GetMember("ContextType").FirstOrDefault());
            var exp1 = Expression.Equal(Context, constValue);

            var Date = Expression.MakeMemberAccess(param, typeof(EmployeeChangeSet).GetMember("Date").FirstOrDefault());
            ConstantExpression dateValue = Expression.Constant(DateTime.Now);
            var exp2 = Expression.LessThanOrEqual(Date, dateValue);
            var ex = Expression.AndAlso(exp1, exp2);
            //return Expression.MakeBinary(ExpressionType.AndAlso, exp1, exp2);
            return Expression.AndAlso(exp1, exp2);
        }

        public static Expression GetCollection()
        {
            ParameterExpression param = Expression.Parameter(typeof(Employee), "e");
            return GetCollection(param);
        }

        public static Expression GetCollection(ParameterExpression paramExp)
        {
            var compositeExp = GetExpression();
            var Context = Expression.MakeMemberAccess(paramExp, typeof(Employee).GetMember("EmployeeChangeSets").FirstOrDefault());
            Expression<Func<EmployeeChangeSet, bool>> e = x => x.ContextType == "S";

            var anyMethod = typeof(System.Linq.Enumerable).GetMethods().Where(c => c.Name == "Any" && c.GetParameters().Length > 1).FirstOrDefault();
            //var ef = ((Expression<Func<EmployeeChangeSet, bool>>)compositeExp);
            var cast = anyMethod.MakeGenericMethod(typeof(EmployeeChangeSet));
            //return Expression.Call(cast, Context, compositeExp);//OK TESTING


            return Expression.Call(cast, Context, e);//OKResult
            //return EnumerableMethodCaller.CallEnumerableMethod(Context, "Any", e);
        }


    }



    public static class EnumerableMethodCaller
    {
        public static object CallMethod(object collection, string methodName, string parameter)
        {
            Expression call = CallEnumerableMethod(Expression.Constant(collection), methodName, parameter);

            return Expression.Lambda(call).Compile().DynamicInvoke(); ;
        }

        public static MethodBase GetGenericMethod(Type type, string name, Type[] typeArgs, Type[] argTypes, BindingFlags flags)
        {
            int typeArity = typeArgs.Length;
            var methods = type.GetMethods()
              .Where(m => m.Name == name)
              .Where(m => m.GetGenericArguments().Length == typeArity)
              .Select(m => m.MakeGenericMethod(typeArgs));

            return Type.DefaultBinder.SelectMethod(flags, methods.ToArray(), argTypes, null);
        }

        static bool IsIEnumerable(Type type)
        {
            return type.IsGenericType
              && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        static Type GetIEnumerableImpl(Type type)
        {
            // Get IEnumerable implementation. Either type is IEnumerable<T> for some T, 
            // or it implements IEnumerable<T> for some T. We need to find the interface.
            if (IsIEnumerable(type))
                return type;
            Type[] t = type.FindInterfaces((m, o) => IsIEnumerable(m), null);
            Debug.Assert(t.Length == 1);
            return t[0];
        }

        public static Expression CallEnumerableMethod(Expression collection, string methodName, object parameter)
        {
            Type cType = GetIEnumerableImpl(collection.Type);
            collection = Expression.Convert(collection, cType);
            Type elemType = cType.GetGenericArguments()[0];

            MethodInfo anyMethod = (MethodInfo)
              GetGenericMethod(typeof(Enumerable), methodName, new[] { elemType },
                new[] { cType, parameter.GetType() }, BindingFlags.Static);

            return Expression.Call(anyMethod, collection, Expression.Constant(parameter));
        }

        public static MethodInfo GetGenericMethodInfo(Expression collection, string methodName, object parameter)
        {
            Type cType = GetIEnumerableImpl(collection.Type);
            collection = Expression.Convert(collection, cType);
            Type elemType = cType.GetGenericArguments()[0];

            MethodInfo anyMethod = (MethodInfo)
              GetGenericMethod(typeof(Enumerable), methodName, new[] { elemType },
                new[] { cType, parameter.GetType() }, BindingFlags.Static);
            return anyMethod;
        }

        public static Expression CallEnumerableMethod(Expression collection, string methodName, Expression parameter)
        {
            Type cType = GetIEnumerableImpl(collection.Type);
            collection = Expression.Convert(collection, cType);
            Type elemType = cType.GetGenericArguments()[0];

            MethodInfo anyMethod = (MethodInfo)
              GetGenericMethod(typeof(Enumerable), methodName, new[] { elemType },
                new[] { cType, parameter.GetType() }, BindingFlags.Static);

            return Expression.Call(anyMethod, collection, parameter);
        }


    }
}
