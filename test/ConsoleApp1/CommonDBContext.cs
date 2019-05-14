using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class CommonDBContext : DbContext
    {
        //public CommonDBContext(DbContextOptions options) : base(options)
        //{
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var logger = new LoggerFactory();
            logger.AddConsole();
            optionsBuilder.UseLoggerFactory(logger);
            optionsBuilder.UseOracle("DATA SOURCE=192.168.0.151:1521/orcl;PASSWORD=citms;PERSIST SECURITY INFO=True;USER ID=tjepp");
            base.OnConfiguring(optionsBuilder);



        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Spotting>().Property(t => t.Createdtime).ValueGeneratedOnAdd();
            modelBuilder.Entity<Spotting>().Property(t => t.ModifiedTime).ValueGeneratedOnAdd();
        }

        public DbSet<Spotting> Spotting { get; set; }

        public DbSet<Department> Department { get; set; }
    }
}
