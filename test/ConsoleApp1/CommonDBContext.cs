using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
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
            optionsBuilder.UseOracle("DATA SOURCE=127.0.0.1:1521/orcl;PASSWORD=95938;PERSIST SECURITY INFO=True;USER ID=hsepp");
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Spotting> Spotting { get; set; }
    }
}
