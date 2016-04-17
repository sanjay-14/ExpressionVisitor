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
    class Program
    {
        static void Main(string[] args)
        {
            //Utility.ParseExpression(e);
            Console.WriteLine("working");
            var s = new string[] { "P", "T" };
            var t = new string[] { "S" };
            Expression<Func<Employee, bool>> e = c => t.Contains(c.Category) && c.Status == "T";
            //  Expression<Func<Employee, bool>> e = c => c.Status != "T";

            var modif = new ExpressionFilter();

            Console.WriteLine(e.ToString());
            var modified = (Expression<Func<Employee, bool>>)modif.Modify(e);
            Console.WriteLine(modified.ToString());
            IEnumerable<Employee> employees = Enumerable.Empty<Employee>();


            using (var context = new TempContext())
            {
                employees = context.Employees.Where(modified).ToList();
            }

            if (employees.Any())
                foreach (var item in employees)
                {
                    Console.WriteLine(string.Format("{0} ,{1},{2}", item.Name, item.Status, item.Category));
                }

            //var combined = FullName.TryCombiningExpressions(c => c.FirstName == "Dog", c => c.LastName == "Boy");

            //Console.WriteLine("Dog Boy should be true: {0}", combined(new FullName { FirstName = "Dog", LastName = "Boy" }));
            //Console.WriteLine("Cat Boy should be false: {0}", combined(new FullName { FirstName = "Cat", LastName = "Boy" }));


            Console.ReadLine();

        }
    }

    public static class Utility
    {
        public static void ParseExpression<T>(Expression<Func<T, bool>> exp) where T : class
        {
            ParameterExpression param = (ParameterExpression)exp.Parameters[0];
            BinaryExpression operation = (BinaryExpression)exp.Body;
            ParameterExpression left = (ParameterExpression)operation.Left;
            ConstantExpression right = (ConstantExpression)operation.Right;

            Console.WriteLine("Decomposed expression: {0} => {1} {2} {3}",
                              param.Name, left.Name, operation.NodeType, right.Value);
        }
    }

    public class Employee
    {
        public Employee()
        {
            //EmployeeChangeSets = new List<Employee>();
        }
        public int ID { get; set; }
        public string Name { get; set; }
        [NotMapped, IncludeInChangeSet("S", DatePropertyName = "Date")]
        public string Status { get; set; }
        public string Category { get; set; }
        public ICollection<EmployeeChangeSet> EmployeeChangeSets { get; set; }
    }

    public class EmployeeChangeSet
    {

        public int ID { get; set; }
        public string ContextType { get; set; }
        public string ContextValue { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }
    }

    public class ExpressionFilter : ExpressionVisitor
    {
        ParameterExpression _paramenterExpression;
        public Expression Modify<T, Y>(Expression<Func<T, Y>> expression) where T : class
        {
            _paramenterExpression = expression.Parameters[0];
            return Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //think how to add contains detection....tomorrow.
            var valueExpression = GetValueExpression(node);
            return EmployeeChangeSetExpression("S", valueExpression);
            return base.VisitMethodCall(node);
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

                // if left expression is removed for expression then only right expression will be sent only.
                if (left.NodeType == ExpressionType.Default)
                {
                    return right;
                }
                // if right expression is removed for expression then only left expression will be sent only.
                if (right.NodeType == ExpressionType.Default)
                {
                    return left;
                }
                return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
            }
            return FilterExpression(node);
        }

        private Expression FilterExpression(BinaryExpression node)
        {
            var memberExp = node.Left as MemberExpression;
            if (memberExp != null && IsNotMappedProperty(memberExp))
            {
                var contextValue = GetContextType(memberExp);
                if (!string.IsNullOrEmpty(contextValue))
                {
                    var valueExpression = GetValueExpression(node);
                    return EmployeeChangeSetExpression(contextValue, valueExpression);
                }
                else
                {
                    return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(true, typeof(bool)), Expression.Constant(true, typeof(bool)), false, null);
                }
            }
            //if (node.NodeType != ExpressionType.AndAlso
            //    && node.NodeType != ExpressionType.OrElse
            //    && node.ToString().Contains("Status"))
            //{
            //var valueExpression = GetValueExpression(node);
            //return EmployeeChangeSetExpression("S", valueExpression);
            //think how to segregte NotMapped & ChangeSet Dependand Properties.
            //return Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(true, typeof(bool)), Expression.Constant(true, typeof(bool)), false, null);
            //}
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

        private bool IsNotMappedProperty(MemberExpression memeberExpression)
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

        //private Expression EmployeeChangeSetExpression(string contextType, BinaryExpression binaryExpression)
        //{
        //    var employeeChangeSetProperty = Expression.MakeMemberAccess(_paramenterExpression, typeof(Employee).GetMember("EmployeeChangeSets").FirstOrDefault());
        //    Expression<Func<EmployeeChangeSet, bool>> fixedExpression = x => x.ContextType == contextType && x.Date <= DateTime.Now;
        //    var valueExpression = GetValueExpression(binaryExpression);
        //    var completeExpression = fixedExpression.CombineWithAndAlso(valueExpression);

        //    var anyMethodInfo = GetMethodInfo("Any", 1);
        //    var genericMethodInfo = anyMethodInfo.MakeGenericMethod(typeof(EmployeeChangeSet));
        //    return Expression.Call(genericMethodInfo, employeeChangeSetProperty, completeExpression);//OKResult
        //}

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

        //seperate it new file.
        private MethodInfo GetEnumerbleMethodInfo(string name, int parameterLength)
        {
            return typeof(System.Linq.Enumerable).GetMethods().Where(c => c.Name == name && c.GetParameters().Length > parameterLength).FirstOrDefault();
        }
    }

}
