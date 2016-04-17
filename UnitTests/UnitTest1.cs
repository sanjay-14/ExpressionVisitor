using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using ExpressionModifier;

namespace UnitTests
{
    [TestClass]
    public class ExpressionFilterUnitTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            Expression<Func<Employee, bool>> e = x => x.Status == "S" && x.Category == "T";
            ExpressionConvertor modif = new ExpressionConvertor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Remove_Status_Expresion_With_Single_Status_Condition()
        {
            Expression<Func<Employee, bool>> e = x => x.Status == "S";
            var modif = new ExpressionFilter();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

        [TestMethod]
        public void Remove_Status_Expresion_With_And_Operator_Mulitple_Condition()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && x.Status == "S";
            var modif = new ExpressionFilter();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

        [TestMethod]
        public void Remove_Status_Expresion_With_And_Operator_Three_Condition()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && (x.Status == "S" ||  x.Category == "S") ;
            var modif = new ExpressionFilter();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }


        [TestMethod]
        public void Remove_Status_Expresion_With_OR_Operator_Mulitple_Condition()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 || x.Status == "S";
            var modif = new ExpressionFilter();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

    }
}
