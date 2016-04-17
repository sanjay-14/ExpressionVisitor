using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressionModifier;

namespace UnitTests
{
    [TestClass]
    public class TestExpressionApi
    {
        [TestMethod]
        public void SingleMethodExpression()
        {
            var exp = ExpressionWriter.GetExpression();
            System.Diagnostics.Debug.WriteLine(exp.ToString());
            Assert.IsTrue(exp.ToString().Contains("Context"));
        }

        [TestMethod]
        public void CollectionMethodExpression()
        {
            var exp = ExpressionWriter.GetCollection();
            System.Diagnostics.Debug.WriteLine(exp.ToString());
            Assert.IsTrue(exp.ToString().Contains("Context"));
        }
    
    
    
    }
}
