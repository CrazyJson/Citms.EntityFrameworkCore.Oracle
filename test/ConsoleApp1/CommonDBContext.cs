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
            optionsBuilder.UseOracle("DATA SOURCE=192.168.0.245:1521/tjims;PASSWORD=citms;PERSIST SECURITY INFO=True;USER ID=hsepp");
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Spotting> Spotting { get; set; }
    }
}
