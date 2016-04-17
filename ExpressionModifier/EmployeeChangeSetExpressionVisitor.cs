using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionModifier
{
    public class EmployeeChangeSetExpressionVisitor : ExpressionVisitor
    {
        ParameterExpression _paramenterExpression;
        public Expression Modify<T, Y>(Expression<Func<T, Y>> expression) where T : class
        {
            _paramenterExpression = expression.Parameters[0];
            return Visit(expression);
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //to handle contain methed of properties whome has IncludedChangeSetAttribute.
            var memeberExpression = node.Arguments[1] as MemberExpression;
            if (memeberExpression != null && IsNotMappedMember(memeberExpression) && node.Method.Name.Equals("Contains"))
            {
                var contextType = GetContextType(memeberExpression);
                if (!string.IsNullOrEmpty(contextType))//if contextType attribute not defined on attribute the empty exprssion will be provided
                {
                    var valueExpression = GetValueExpression(node);
                    return employeeChangeSetExpression(contextType, valueExpression);
                }
                else
                {
                    return EmptyExpression();
                }
            }

            return base.VisitMethodCall(node);
        }
        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse)
            {
                Expression left = this.Visit(node.Left);
                Expression right = this.Visit(node.Right);
                return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
            }
            return TransformVisitBinaryExpression(node);
        }
        
        private Expression EmptyExpression()
        {
            return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(true, typeof(bool)), Expression.Constant(true, typeof(bool)), false, null);
        }
        //protected override Expression VisitLambda<T>(Expression<T> node)
        //{//to watch sub lamda expression in expression 
        //    var lamda = base.VisitLambda<T>(node);
        //    return lamda;
        //}

        private Expression TransformVisitBinaryExpression(BinaryExpression node)
        {
            var memberExp = node.Left as MemberExpression;
            if (memberExp != null && IsNotMappedMember(memberExp))
            {
                var contextType = GetContextType(memberExp);
                if (!string.IsNullOrEmpty(contextType))
                {
                    var valueExpression = GetValueExpression(node);
                    return employeeChangeSetExpression(contextType, valueExpression);
                }
                else
                {
                    return EmptyExpression();
                }
            }
            return base.VisitBinary(node);
        }

        private Expression employeeChangeSetExpression(string contextType, Expression<Func<EmployeeChangeSet, bool>> valueExpression)
        {
            var employeeChangeSetProperty = Expression.MakeMemberAccess(_paramenterExpression, typeof(Employee).GetMember("EmployeeChangeSets").FirstOrDefault());
            Expression<Func<EmployeeChangeSet, bool>> fixedExpression = x => x.ContextType == contextType && x.Date <= DateTime.Now;
            var completeExpression = fixedExpression.CombineWithAndAlso(valueExpression);
            var anyMethodInfo = GetEnumerbleMethodInfo("Any", 1);
            var genericMethodInfo = anyMethodInfo.MakeGenericMethod(typeof(EmployeeChangeSet));
            return Expression.Call(genericMethodInfo, employeeChangeSetProperty, completeExpression);//OKResult
        }

        private bool IsNotMappedMember(MemberExpression memeberExpression)
        {
            var attribute = memeberExpression.Member.GetCustomAttribute<NotMappedAttribute>();
            return (attribute != null);
        }

        private string GetContextType(MemberExpression memeberExpression)
        {
            var attribute = memeberExpression.Member.GetCustomAttribute<IncludedInChangeSetAttribute>();
            if (attribute != null)
            {
                return attribute.ContextType;
            }
            return null;
        }

        private Expression<Func<EmployeeChangeSet, bool>> GetValueExpression(MethodCallExpression methodCall)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(EmployeeChangeSet), "m");
            Expression contextValueProperty = Expression.Property(parameterExpression, "ContextValue");
            if (!methodCall.Arguments.Any()) throw new ArgumentNullException();

            var containingArray = methodCall.Arguments[0];
            Expression convertedToStringArray;
            if (isConversionToStringRequried(containingArray as MemberExpression))
            {
                convertedToStringArray = tryToConvertInStringArray(containingArray as MemberExpression);
                if (convertedToStringArray == null) throw new ArgumentNullException("String array conversion failed");
            }
            else
            {
                convertedToStringArray = containingArray;
            }

            var containsMethodInfo = GetEnumerbleMethodInfo("Contains", 1);
            var genericMethodInfo = containsMethodInfo.MakeGenericMethod(typeof(string));
            var method = Expression.Call(genericMethodInfo, convertedToStringArray, contextValueProperty);
            return Expression.Lambda<Func<EmployeeChangeSet, bool>>(method, parameterExpression);
        }

        private static Expression<Func<EmployeeChangeSet, bool>> GetValueExpression(BinaryExpression exp)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(EmployeeChangeSet), "s");
            Expression contextValueProperty = Expression.Property(parameterExpression, "ContextValue");

            Expression e1 = Expression.MakeBinary(exp.NodeType, contextValueProperty, Expression.Constant(exp.Right.ToString()));
            var lambda = Expression.Lambda<Func<EmployeeChangeSet, bool>>(e1, parameterExpression);
            return lambda;
        }

        //move in seperate it new file.
        private MethodInfo GetEnumerbleMethodInfo(string name, int parameterLength)
        {
            return typeof(System.Linq.Enumerable).GetMethods().Where(c => c.Name == name && c.GetParameters().Length > parameterLength).FirstOrDefault();
        }

        private ConstantExpression tryToConvertInStringArray(MemberExpression memberExpression)
        {
            object container = ((ConstantExpression)memberExpression.Expression).Value;
            var member = memberExpression.Member;
            if (member is FieldInfo)
            {
                object value = ((FieldInfo)member).GetValue(container);
                if (value is Array)
                {
                    var convertedToString = ConvertToStringArray(value as Array);
                    return Expression.Constant(convertedToString);
                }
            }
            if (member is PropertyInfo)
            {
                object value = ((PropertyInfo)member).GetValue(container, null);
                if (value is Array)
                {
                    var convertedToString = ConvertToStringArray(value as Array);
                    return Expression.Constant(convertedToString);
                }
            }

            return null;
        }

        private bool isConversionToStringRequried(MemberExpression memberExpression)
        {
            object container = ((ConstantExpression)memberExpression.Expression).Value;
            var member = memberExpression.Member;

            if (member is FieldInfo)
            {
                object value = ((FieldInfo)member).GetValue(container);
                return value.GetType().GetElementType() != typeof(string);
            }
            if (member is PropertyInfo)
            {
                object value = ((FieldInfo)member).GetValue(container);
                return value.GetType().GetElementType() != typeof(string);
            }

            return false;
        }

        private Array ConvertToStringArray(Array value)
        {
            var list = new List<string>();
            foreach (var item in value)
            {
                list.Add(item.ToString());
            }
            return list.ToArray();
        }
    }


}
