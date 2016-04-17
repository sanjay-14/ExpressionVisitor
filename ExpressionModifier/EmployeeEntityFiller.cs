using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionModifier
{
    public static class EmployeeEntityFiller
    {
        public static void Fill(Employee employee)
        {
            var contextProperties = getPropertyByAttribute<IncludedInChangeSetAttribute>(employee);
            foreach (var property in contextProperties)
            {
                var attri = property.GetCustomAttributes(typeof(IncludedInChangeSetAttribute), false).SingleOrDefault();
                if (attri == null) return;

                var attr = attri as IncludedInChangeSetAttribute;
                var employeeChangeSet = getEmployeeChangeSet(employee, attr.ContextType);
                setPropertyValue(employee, property, employeeChangeSet.ContextValue);
            }
        }

        private static void setPropertyValue(object propertyOwnerObject, PropertyInfo property, object newValue)
        {
            var propertyType = property.PropertyType;
            object propertyNewValue = newValue;
            if (propertyType.IsPrimitive && propertyNewValue.GetType() != propertyType)
            {
                propertyNewValue = Convert.ChangeType(propertyNewValue, propertyType);
            }
            property.SetValue(propertyOwnerObject, propertyNewValue);
        }

        private static EmployeeChangeSet getEmployeeChangeSet(Employee employee, string contextType)
        {
            return employee.EmployeeChangeSets.OrderByDescending(c => c.Date).FirstOrDefault(c => c.ContextType == contextType && c.Date <= DateTime.Now);
        }

        private static IEnumerable<PropertyInfo> getPropertyByAttribute<T>(Employee employee) where T : Attribute
        {
            return employee.GetType().GetProperties().Where(c => c.CustomAttributes.Any(a => a.AttributeType == typeof(T)));
        }
    }
}
