using EmployeeDB.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDB
{
    public class Context: DbContext
    {
        public readonly string SQLServerName = "TURKINA-A-NB";
        public DbSet<Employee> Employees { get; set; }

        public Context(string SQLServerName) 
        { 
            this.SQLServerName = SQLServerName;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Server={SQLServerName};Database=EmployeeDB;Trusted_Connection=true;TrustServerCertificate=true");
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await Employees.AddAsync(employee);
            await SaveChangesAsync();
        }

        public Employee GetEmployeeById(int id)
        {
            return Employees.Find(id);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            Employees.Update(employee);
            await SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(Employee employee)
        {
            Employees.Remove(employee);
            await SaveChangesAsync();
        }
    }
}
