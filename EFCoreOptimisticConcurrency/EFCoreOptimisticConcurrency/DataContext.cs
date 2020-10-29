using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFCoreOptimisticConcurrency
{
    public class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=EFCoreOptimisticConcurrency;Trusted_Connection=True;");
        }

        public DbSet<Product> Products { get; set; }
    }
}
