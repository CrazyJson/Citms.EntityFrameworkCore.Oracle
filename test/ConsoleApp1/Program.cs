using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CommonDBContext db = new CommonDBContext())
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 2001; i++)
                {
                    sb.Append(0);
                }
                var mytest = new Mytest
                {
                    Id = Guid.NewGuid().ToString("N"),
                    AA = sb.ToString()
                };
                db.Entry(mytest).State = EntityState.Added;
                db.SaveChanges();

                //int total = db.Spotting.Count();
                //var x1 = db.Spotting.GroupBy(e => e.DepartmentId).Select(e => new { e.Key, count = e.Count() }).ToList();

                //Console.WriteLine("路口总行数：{0}", total);
                //var fDisItem = db.Spotting.FirstOrDefault(e => e.Disabled == true);
                //Console.WriteLine("第一条禁用路口：{0}", fDisItem.SpottingName);
                ////分页查询演示
                //var pageList = db.Spotting.Where(e => e.Disabled == true)
                //    .OrderBy(e => e.SpottingName).Skip(10).Take(20).ToList();
                //Console.WriteLine("分页查询禁用路口：{0}", pageList.Count);
                //var list = db.Spotting.ToList();
                //string[] arrSpottingNo = new string[] { "123", "34" };
                //db.Spotting.Where(e => arrSpottingNo.Contains(e.SpottingNo)).ToList();

                ////var cmd = db.Database.GetDbConnection().CreateCommand();
                ////cmd.CommandType = CommandType.StoredProcedure;
                ////cmd.CommandText = "procName";
                ////OracleParameter guest_type = new OracleParameter(":i_guest_type", OracleDbType.Int32);
                ////guest_type.Value = 10;
                ////guest_type.Direction = ParameterDirection.Input;
                ////cmd.Parameters.Add(guest_type);
                ////cmd.Connection.Open();
                //////int retCode = cmd.ExecuteNonQuery();
                ////var reader = cmd.ExecuteReader();
                ////cmd.Connection.Close();



                //string minSpottingNo = db.Spotting.Min(e => e.SpottingNo);
                //string maxSpottingNo = db.Spotting.Max(e => e.SpottingNo);
                //string[] arrAreaCode = db.Spotting.Select(e => e.AreaCode).Distinct().ToArray();
                //db.Spotting.Average(e => e.Longitude);
                //var dt = DateTime.Now.AddDays(-100);
                ////日期过滤
                //db.Spotting.Where(e =>
                //    e.Createdtime >= dt && e.Createdtime <= DateTime.Now && e.Disabled == true).ToList();
                //var listSpotting = new List<Spotting>();
                //for (int i = 0; i < 1; i++)
                //{
                //    var itemNew = new Spotting
                //    {
                //        SpottingId = Guid.NewGuid().ToString("N"),
                //        SpottingName = "test",
                //        SpottingNo = "test",
                //        Creator = "admin",
                //        SourceKind = "local",
                //        ApplicationName = "ims",
                //        //Createdtime = DateTime.Now,
                //        DepartmentId = Guid.NewGuid().ToString("N")
                //    };
                //    listSpotting.Add(itemNew);
                //}
                //db.AddRange(listSpotting);
                //db.SaveChanges();

                //db.RemoveRange(db.Spotting.Where(e => listSpotting.Select(t => t.SpottingId).Contains(e.SpottingId)));
                //db.SaveChanges();
                ////db.Entry(itemNew).State = EntityState.Added;
                ////Console.WriteLine("新增一条路口Id：{0} 数据", itemNew.SpottingId);

                //////var itemModify = db.Spotting.Find(itemNew.SpottingId);
                //////itemModify.SpottingName = "testModify";
                //////db.Entry(itemModify).State = EntityState.Modified;
                //////Console.WriteLine("修改路口Id：{0} 数据的名称为：{1}", itemNew.SpottingId,itemModify.SpottingName);

                ////var dItem = db.Spotting.Find(itemNew.SpottingId);
                ////db.Remove(dItem);
                ////Console.WriteLine("删除路口Id：{0} 数据", itemNew.SpottingId);

                ////关联查询
                //var x = (from p in db.Spotting
                //         join q in db.Department
                //         on p.DepartmentId equals q.DepartmentId
                //         select new { p.SpottingName, p.SpottingId, p.DepartmentId, q.BuName }).OrderBy(e => e.SpottingName)
                //         .Skip(10).Take(20).ToList();

                //db.SaveChanges();
            }
            Console.Read();
        }
    }
}
