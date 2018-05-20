using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConsoleApp1
{
    ///<summary>
    ///道路点位表 Spottings 有"s"后辍，与现有的Spotting区别开来
    ///</summary>
    [Table("COMMON_SPOTTING")]
    public class Spotting
    {
        ///<summary>
        ///点位ID 
        ///</summary> 
        [Key, Column("SPOTTINGID", TypeName = "VARCHAR2")]
        public string SpottingId { get; set; }

        ///<summary>
        ///点位编号(可以为厂家分配的点位编号) 
        ///</summary> 
        [Column("SPOTTINGNO", TypeName = "VARCHAR2"),Required]
        public string SpottingNo { get; set; }

        ///<summary>
        ///点位名称 
        ///</summary> 
        [Column("SPOTTINGNAME")]
        [Required]
        public string SpottingName { get; set; }

        ///<summary>
        ///上传六合一标准代码 
        ///</summary> 
        [Column("UNIQUECODE", TypeName = "VARCHAR2")]
        public string UniqueCode { get; set; }

        ///<summary>
        ///所在道路ID 
        ///</summary> 
        [Column("ROADID", TypeName = "VARCHAR2")]
        public string RoadId { get; set; }

        ///<summary>
        ///经度坐标值 
        ///</summary> 
        [Column("LONGITUDE")]
        public double? Longitude { get; set; }

        ///<summary>
        ///纬度坐标值 
        ///</summary> 
        [Column("LATITUDE")]
        public double? Latitude { get; set; }

        ///<summary>
        ///所在管理部门 
        ///</summary> 
        [Column("DEPARTMENTID", TypeName = "VARCHAR2")]
        [Required]
        public string DepartmentId { get; set; }

        ///<summary>
        ///来源类型 
        ///</summary> 
        [Column("SOURCEKIND", TypeName = "VARCHAR2"), Required]
        public string SourceKind { get; set; }

        ///<summary>
        ///创建用户ID 
        ///</summary> 
        [Column("CREATOR", TypeName = "VARCHAR2")]
        public string Creator { get; set; }

        ///<summary>
        ///创建时间 
        ///</summary> 
        [Column("CREATEDTIME", TypeName = "DATE")]
        public DateTime? Createdtime { get; set; }

        ///<summary>
        ///修改人 
        ///</summary> 
        [Column("MODIFIER", TypeName = "VARCHAR2")]
        public string Modifier { get; set; }

        ///<summary>
        ///修改时间 
        ///</summary> 
        [Column("MODIFIEDTIME", TypeName = "DATE")]
        public DateTime? ModifiedTime { get; set; }

        ///<summary>
        ///保留标记 
        ///</summary> 
        [Column("FLAGS", TypeName = "VARCHAR2")]
        public string Flags { get; set; }

        ///<summary>
        ///备注 
        ///</summary> 
        [Column("REMARK")]
        public string Remark { get; set; }

        ///<summary>
        ///应用名称 
        ///</summary> 
        [Column("APPLICATIONNAME", TypeName = "VARCHAR2")]
        public string ApplicationName { get; set; }

        ///<summary>
        ///所在地区编号（行政区划代码） 
        ///</summary> 
        [Column("AREACODE", TypeName = "VARCHAR2")]
        public string AreaCode { get; set; }

        ///<summary>
        ///拼音简称 
        ///</summary> 
        [Column("BOPOMOFO", TypeName = "VARCHAR2")]
        public string Bopomofo { get; set; }

        ///<summary>
        ///点位类型(字典表字典 ，Kind 为 1003 ， 十字路口/丁字路口/圆形转盘/其它) 
        ///</summary> 
        [Column("SPOTTINGTYPE", TypeName = "VARCHAR2")]
        public string SpottingType { get; set; }

        ///<summary>
        ///逻辑删除标记(0 正常数据， 1 逻辑删除) 
        ///</summary> 
        [Column("VIRTUALDELETEFLAG")]
        public double? VirtualDeleteFlag { get; set; }

        ///<summary>
        ///是否停用(0 未停用， 1 停用)，默认为0 
        ///</summary> 
        [Column("DISABLED")]
        public bool? Disabled { get; set; }

    }
}
