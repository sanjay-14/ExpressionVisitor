using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionModifier
{
    public class TempContext : DbContext
    {
        public TempContext()
            : base("TempDbContext")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EmployeeMap());
        }

        public DbSet<Employee> Employees { get; set; }
    }

    public class EmployeeMap : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Employee>
    {
        public EmployeeMap()
        {
            this.HasKey(c => c.ID);
        }
    }
}
