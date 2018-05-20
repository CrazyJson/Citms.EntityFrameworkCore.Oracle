using System;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CommonDBContext db = new CommonDBContext())
            {
                //int i = db.Spotting.Count();

                //var cc1 = db.Spotting.FirstOrDefault(e => e.Disabled == true);
                //var cc = db.Spotting.Where(e => e.Disabled == true).OrderBy(e => e.SpottingName).Skip(10).Take(20).ToList();
                //var list = db.Spotting.ToList();
                //string[] aa = new string[] { "123", "34" };
                //db.Spotting.Where(e => aa.Contains(e.SpottingNo)).ToList();

                //var item = db.Spotting.Find("a5563b53d23548179ed857ac3820df73");
                var item = db.Spotting.AsNoTracking().FirstOrDefault(e => e.SpottingId == "a5563b53d23548179ed857ac3820df73");
                //string cc2 = db.Spotting.Min(e => e.SpottingNo);
                //string cc3 = db.Spotting.Max(e => e.SpottingNo);
                //string[] cc4 = db.Spotting.Select(e=>e.AreaCode).Distinct().ToArray();
                //db.Spotting.Average(e => e.Longitude);
                var dt = DateTime.Now.AddDays(-100);
                db.Spotting.Where(e => e.Createdtime >= dt && e.Createdtime <= DateTime.Now && e.Disabled == true).ToList();
                item.SpottingName = "test";
                item.Createdtime = DateTime.Now;
                item.SpottingId = Guid.NewGuid().ToString("N");
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                var dItem = db.Spotting.Find(item.SpottingId);
                db.Remove(dItem);

                var x = (from p in db.Spotting
                         join q in db.Department
                         on p.DepartmentId equals q.DepartmentId
                         select new { p.SpottingName, p.SpottingId, p.DepartmentId, q.BuName }).OrderBy(e => e.SpottingName)
                         .Skip(10).Take(20).ToList();

                db.SaveChanges();
            }
            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}
