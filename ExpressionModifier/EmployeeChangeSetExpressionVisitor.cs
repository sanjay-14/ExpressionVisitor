using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            //to handle contain methed of required properties.
            var memeberExpression = node.Arguments[1] as MemberExpression;
            if (memeberExpression != null && IsNotMappedMember(memeberExpression) && node.Method.Name.Equals("Contains"))
            {
                var contextValue = GetContextType(memeberExpression);
                if (!string.IsNullOrEmpty(contextValue))
                {
                    var valueExpression = GetValueExpression(node);
                    return EmployeeChangeSetExpression(contextValue, valueExpression);
                }
                else
                {
                    return EmptyExpression();
                }
            }

            return base.VisitMethodCall(node);
        }

        private Expression EmptyExpression()
        {
            return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(true, typeof(bool)), Expression.Constant(true, typeof(bool)), false, null);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var lamda = base.VisitLambda<T>(node);
            return lamda;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse)
            {
                Expression left = this.Visit(node.Left);
                Expression right = this.Visit(node.Right);
                return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
            }
            return TransformExpression(node);
        }

        private Expression TransformExpression(BinaryExpression node)
        {
            var memberExp = node.Left as MemberExpression;
            if (memberExp != null && IsNotMappedMember(memberExp))
            {
                var contextValue = GetContextType(memberExp);
                if (!string.IsNullOrEmpty(contextValue))
                {
                    var valueExpression = GetValueExpression(node);
                    return EmployeeChangeSetExpression(contextValue, valueExpression);
                }
                else
                {
                    return EmptyExpression();
                }
            }
            return base.VisitBinary(node);
        }

        private Expression EmployeeChangeSetExpression(string contextType, Expression<Func<EmployeeChangeSet, bool>> valueExpression)
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
            var attribute = memeberExpression.Member.GetCustomAttribute<IncludeInChangeSetAttribute>();
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
            var arg = methodCall.Arguments[0];
            var anyMethodInfo = GetEnumerbleMethodInfo("Contains", 1);
            var genericMethodInfo = anyMethodInfo.MakeGenericMethod(typeof(string));
            var method = Expression.Call(genericMethodInfo, arg, contextValueProperty);
            //return method;
            var lambda = Expression.Lambda<Func<EmployeeChangeSet, bool>>(method, parameterExpression);
            return lambda;
        }

        private static Expression<Func<EmployeeChangeSet, bool>> GetValueExpression(BinaryExpression exp)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(EmployeeChangeSet), "s");
            Expression contextValueProperty = Expression.Property(parameterExpression, "ContextValue");

            Expression e1 = Expression.MakeBinary(exp.NodeType, contextValueProperty, exp.Right);
            var lambda = Expression.Lambda<Func<EmployeeChangeSet, bool>>(e1, parameterExpression);
            return lambda;
        }

        //move in seperate it new file.
        private MethodInfo GetEnumerbleMethodInfo(string name, int parameterLength)
        {
            return typeof(System.Linq.Enumerable).GetMethods().Where(c => c.Name == name && c.GetParameters().Length > parameterLength).FirstOrDefault();
        }
    }
}
