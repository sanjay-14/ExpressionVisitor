using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using ExpressionModifier;
using System.Linq;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class TestEmployeeChangeSetExpressionVisitor
    {
        [TestMethod]
        public void Remove_Status_Expresion_When_Single_Status_Condition_Provided()
        {
            Expression<Func<Employee, bool>> e = x => x.Status == "S";
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

        [TestMethod]
        public void Remove_Status_Expresion_When_And_Operator_With_Two_Conditions_Provided()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && x.Status == "S";
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

        [TestMethod]
        public void Remove_Status_Expresion_When_Incldued_In_Braces_with_OR_Opertor_Provided()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && (x.Status == "S" || x.Category == "S");
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }


        [TestMethod]
        public void Remove_Status_Expresion_When_OR_Operator_Provided()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 || x.Status == "S";
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
        }

        [TestMethod]
        public void Status_Expresion_Removed_And_Any_Expression_Added_Having_Status_Right_Operand()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 || x.Status == "S";
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();

            Assert.IsFalse(modifiedString.Contains("Status"));
            Assert.IsTrue(modifiedString.Contains("Any"));
            Assert.IsTrue(modifiedString.Contains("contextType"));//varible name of IncludeInChangeSet... just to make to test which detect the attribute value
        }

        [TestMethod]
        public void Status_Expresion_Removed_And_Any_Expression_Added_Having_Context_Type_Value()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 || x.Status == "S";
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
            Assert.IsTrue(modifiedString.Contains("Any"));
            Assert.IsTrue(modifiedString.Contains("contextType"));
        }

        [TestMethod]
        public void Status_Expresion_Contains_Clause_Transformed_To_ChangeSet_ContextValue_Contains_Clause()
        {
            var status = new string[] { "P", "S" };
            Expression<Func<Employee, bool>> e = x => status.Contains(x.Status);
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("Status"));
            Assert.IsTrue(modifiedString.Contains("Contains(x.ContextValue)"));
        }

        [TestMethod]
        public void NotMappedProperty_Binary_Expression_Removed()
        {
            Expression<Func<Employee, bool>> e = x => x.ID == 2 || x.DesignationId == 11;
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("DesignationId"));
        }

        [TestMethod]
        public void Int_Type_Array_Transformed_To_String_Array_With_Context_Value_Property()
        {
            var designations = new int[] { 1, 2 };
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && designations.Contains(x.DesignationId);
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("DesignationId"));
            Assert.IsTrue(modifiedString.Contains("ContextValue"));
            Assert.IsTrue(modifiedString.Contains("value(System.String[]).Contains(x.ContextValue))))"));
        }

        [TestMethod]
        public void Int_Type_List_Transformed_To_String_Array_With_Context_Value_Property()
        {
            var designations = new List<int> { 1, 2 };
            Expression<Func<Employee, bool>> e = x => x.ID == 2 && designations.Contains(x.DesignationId);
            var modif = new EmployeeChangeSetExpressionVisitor();
            System.Diagnostics.Debug.WriteLine(e.ToString());
            var modified = modif.Modify(e);
            System.Diagnostics.Debug.WriteLine(modified.ToString());
            var modifiedString = modified.ToString();
            Assert.IsFalse(modifiedString.Contains("DesignationId"));
            Assert.IsTrue(modifiedString.Contains("ContextValue"));
            Assert.IsTrue(modifiedString.Contains("value(System.String[]).Contains(x.ContextValue))))"));
        }

    }
}
