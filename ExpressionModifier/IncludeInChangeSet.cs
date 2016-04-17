using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionModifier
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IncludedInChangeSetAttribute : Attribute
    {
        public IncludedInChangeSetAttribute(string contextType)
        {
            ContextType = contextType;
        }
        public string ContextType { get; set; }
        public string DatePropertyName { get; set; }
    }
}
