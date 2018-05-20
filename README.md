# Citms.EntityFrameworkCore.Oracle
Entity, Framework, EF, Core, Data, O/RM, entity-framework-core,Oracle

Citms.EntityFrameworkCore.Oracle is an Entity Framework Core provider built on top of [Oracle.ManagedDataAccess.Core](http://www.oracle.com/technetwork/licenses/ea-license-152003.html). It allows us to use the Entity Framework Core ORM with Oracle.  Async functions in this library properly implement Async I/O at the lowest level.


## Getting Started

Here is a console application sample for accessing a Oracle database using Entity Framework:


② Put `Citms.EntityFrameworkCore.Oracle` into your project's `.csproj` file
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
	<PackageReference Include="Citms.EntityFrameworkCore.Oracle" Version="1.0.0" />
  </ItemGroup>
  
</Project>
```

③ Implement some models, DbContext in `Program.cs`. Then overriding the OnConfiguring of DbContext to use Oracle database. Besides. Finally to invoking Oracle with EF Core in your Main() method.

```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OracleTest
{
    public class User
    {
        public int UserId { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }
    }

    public class Blog
    {
        public Guid Id { get; set; }

        [MaxLength(32)]
        public string Title { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Content { get; set; }

    }

    public class MyContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseOracle(@"DATA SOURCE=127.0.0.1:1521/tjims;PASSWORD=test;PERSIST SECURITY INFO=True;USER ID=test;");
    }

    public class Program
    {
        public static void Main()
        {
            using (var context = new MyContext())
            {
                // Create database
                context.Database.EnsureCreated();

                // Init sample data
                var user = new User { Name = "Yuuko" };
                context.Add(user);
                var blog1 = new Blog {
                    Title = "Title #1",
                    UserId = user.UserId,
                    Tags = new List<string>() { "ASP.NET Core", "Oracle", "Citms" }
                };
                context.Add(blog1);
                var blog2 = new Blog
                {
                    Title = "Title #2",
                    UserId = user.UserId,
                    Tags = new List<string>() { "ASP.NET Core", "Oracle" }
                };
                context.Add(blog2);
                context.SaveChanges();

                context.SaveChanges();

                context.SaveChanges();

                // Output data
                var ret = context.Blogs
                    .Where(x => x.Tags.Object.Contains("Citms"))
                    .ToList();
                foreach (var x in ret)
                {
                    Console.WriteLine($"{ x.Id } { x.Title }");                
                    Console.WriteLine();
                }
            }
            Console.Read();
        }
    }
}
```

## Contribute

One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

## License

[MIT](https://github.com/CrazyJson/Citms.EntityFrameworkCore.Oracle/blob/master/LICENSE)
