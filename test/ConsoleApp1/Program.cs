using System;
using System.Linq;
using System.Data.SqlClient;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CommonDBContext db = new CommonDBContext())
            {
                int i = db.Spotting.Count();

                var list = db.Spotting.ToList();
            }
            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
