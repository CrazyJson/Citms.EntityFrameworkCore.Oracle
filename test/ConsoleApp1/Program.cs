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
                db.Database.Query<Spotting>("select * from common_spotting where spottingId=:a", 
                    new { a = "123" });
                db.Database.ToSingle<Spotting>("select * from common_spotting where spottingId=:a",
                    new { a = "123" });

                int total = db.Spotting.Count();
                Console.WriteLine("路口总行数：", total);
                var fDisItem = db.Spotting.FirstOrDefault(e => e.Disabled == true);
                Console.WriteLine("第一条禁用路口：", fDisItem.SpottingName);
                //分页查询演示
                var pageList = db.Spotting.Where(e => e.Disabled == true)
                    .OrderBy(e => e.SpottingName).Skip(10).Take(20).ToList();
                Console.WriteLine("分页查询禁用路口：", pageList.Count);
                var list = db.Spotting.ToList();
                string[] arrSpottingNo = new string[] { "123", "34" };
                db.Spotting.Where(e => arrSpottingNo.Contains(e.SpottingNo)).ToList();

                var item = db.Spotting.Find("a5563b53d23548179ed857ac3820df73");
                var itemNoTrack = db.Spotting.AsNoTracking().FirstOrDefault(e => e.SpottingId == "a5563b53d23548179ed857ac3820df73");
                string minSpottingNo = db.Spotting.Min(e => e.SpottingNo);
                string maxSpottingNo = db.Spotting.Max(e => e.SpottingNo);
                string[] arrAreaCode = db.Spotting.Select(e => e.AreaCode).Distinct().ToArray();
                db.Spotting.Average(e => e.Longitude);
                var dt = DateTime.Now.AddDays(-100);
                //日期过滤
                db.Spotting.Where(e =>
                    e.Createdtime >= dt && e.Createdtime <= DateTime.Now && e.Disabled == true).ToList();
                item.SpottingName = "test";
                item.Createdtime = DateTime.Now;
                item.SpottingId = Guid.NewGuid().ToString("N");
                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                var dItem = db.Spotting.Find(item.SpottingId);
                db.Remove(dItem);
                //关联查询
                var x = (from p in db.Spotting
                         join q in db.Department
                         on p.DepartmentId equals q.DepartmentId
                         select new { p.SpottingName, p.SpottingId, p.DepartmentId, q.BuName }).OrderBy(e => e.SpottingName)
                         .Skip(10).Take(20).ToList();

                db.SaveChanges();
            }
            Console.Read();
        }
    }
}
