using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace ExpressionModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            //Utility.ParseExpression(e);
            Console.WriteLine("working");
            //var s = new string[] { "P", "T" };
            var t = new string[] { "S" };
            var d = new int[] { 1, 2 };
            //Expression<Func<Employee, bool>> e = c => t.Contains(c.Category) && c.DesignationId == 2;
            Expression<Func<Employee, bool>> e = c => t.Contains(c.Category) && d.Contains(c.DesignationId);
            //  Expression<Func<Employee, bool>> e = c => c.Status != "T";

            var modif = new EmployeeChangeSetExpressionVisitor();

            Console.WriteLine(e.ToString());
            var modified = (Expression<Func<Employee, bool>>)modif.Modify(e);
            Console.WriteLine(modified.ToString());
            IEnumerable<Employee> employees = Enumerable.Empty<Employee>();


            using (var context = new TempContext())
            {
                employees = context.Employees.Include(c => c.EmployeeChangeSets).Where(modified).ToList();
            }

            if (employees.Any())
                foreach (var item in employees)
                {
                    EmployeeEntityFiller.Fill(item);
                    Console.WriteLine(string.Format("Name:{0}\n ,STatus: {1}\n,Category:{2},\n DesignationID:{3} \n\n", item.Name, item.Status, item.Category, item.DesignationId));

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
        [NotMapped, IncludedInChangeSet("S", DatePropertyName = "Date")]
        public string Status { get; set; }
        public string LastName { get; set; }
        public string Category { get; set; }
        [NotMapped, IncludedInChangeSet("D")]
        public int DesignationId { get; set; }
        public ICollection<EmployeeChangeSet> EmployeeChangeSets { get; set; }
    }
    
    public class EmployeeChangeSet
    {
        public int ID { get; set; }
        [Index("ind1",1)]
        public string ContextType { get; set; }
        [Index("ind1", 2)]
        public string ContextValue { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeID { get; set; }
    }



}
