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
            modelBuilder.Configurations.Add(new EmployeeChangeSetMap());
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

    public class EmployeeChangeSetMap : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<EmployeeChangeSet>
    {
        public EmployeeChangeSetMap()
        {
            this.HasKey(c => c.ID);
            this.Property(c => c.ContextValue).HasMaxLength(2).IsRequired();
            this.Property(c => c.ContextType).HasMaxLength(2).IsRequired();
        }
    }
}
