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

            var modif = new EmployeeChangeSetExpressionVisitor();

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
        [NotMapped
        , IncludeInChangeSet("S", DatePropertyName = "Date")
        
        ]
        public string Status { get; set; }
        public string Category { get; set; }
        [NotMapped]
        public int DesignationId { get; set; }
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



}
