using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp1
{
    ///<summary>
    ///组织机构表
    ///</summary>
    [Table("SYS_DEPARTMENT")]
	public class  Department
    {
	///<summary>
	    ///单位GUID 
	    ///</summary> 
	    [Key,Column("DEPARTMENTID", TypeName = "VARCHAR2")]
        public string DepartmentId { get; set; }
	      
		///<summary>
	    ///单位简称 
	    ///</summary> 
	    [Column("BUNAME")]
        public string BuName { get; set; }
	      
		///<summary>
	    ///单位全称 
	    ///</summary> 
	    [Column("BUFULLNAME")]
        public string BuFullName { get; set; }
	      
		///<summary>
	    ///单位代码 
	    ///</summary> 
	    [Column("BUCODE")]
        public string BuCode { get; set; }
	      
		///<summary>
	    ///层级代码 
	    ///</summary> 
	    [Column("HIERARCHYCODE",TypeName = "NVARCHAR2")]
        public string HierarchyCode { get; set; }
	      
		///<summary>
	    ///父级GUID 
	    ///</summary> 
	    [Column("PARENTGUID", TypeName = "VARCHAR2")]
        public string ParentGuid { get; set; }
	      
		///<summary>
	    ///网址 
	    ///</summary> 
	    [Column("WEBSITE")]
        public string WebSite { get; set; }
	      
		///<summary>
	    ///传真 
	    ///</summary> 
	    [Column("FAX")]
        public string Fax { get; set; }
	      
		///<summary>
	    ///公司地址 
	    ///</summary> 
	    [Column("COMPANYADDR")]
        public string CompanyAddr { get; set; }
	      
		///<summary>
	    ///营业执照 
	    ///</summary> 
	    [Column("CHARTER")]
        public string Charter { get; set; }
	      
		///<summary>
	    ///法人代表 
	    ///</summary> 
	    [Column("CORPORATIONDEPUTY")]
        public string CorporationDeputy { get; set; }
	      
		///<summary>
	    ///创建时间 
	    ///</summary> 
	    [Column("CREATEDON", TypeName = "DATE")]
        public DateTime?  CreatedOn { get; set; }
	      
		///<summary>
	    ///修改时间 
	    ///</summary> 
	    [Column("MODIFIEDON", TypeName = "DATE")]
        public DateTime?  ModifiedOn { get; set; }
	      
		///<summary>
	    ///创建人 
	    ///</summary> 
	    [Column("CREATEDBY", TypeName = "VARCHAR2")]
        public string CreatedBy { get; set; }
	      
		///<summary>
	    ///说明 
	    ///</summary> 
	    [Column("COMMENTS")]
        public string Comments { get; set; }
	      
		///<summary>
	    ///修改人 
	    ///</summary> 
	    [Column("MODIFIEDBY", TypeName = "VARCHAR2")]
        public string ModifiedBy { get; set; }
	      
		///<summary>
	    ///是否末级公司 
	    ///</summary> 
	    [Column("ISENDCOMPANY")]
        public bool?  IsEndCompany { get; set; }
	      
		///<summary>
	    ///是否公司 
	    ///</summary> 
	    [Column("ISCOMPANY")]
        public bool?  IsCompany { get; set; }
	      
		///<summary>
	    ///层级数 
	    ///</summary> 
	    [Column("BULEVEL")]
        public  double?  BuLevel { get; set; }
	      
		///<summary>
	    ///组织类型 
	    ///</summary> 
	    [Column("BUTYPE")]
        public  double?  BuType { get; set; }
	      
		///<summary>
	    ///排序代码 
	    ///</summary> 
	    [Column("ORDERCODE")]
        public string OrderCode { get; set; }
	      
		///<summary>
	    ///排序层级代码 
	    ///</summary> 
	    [Column("ORDERHIERARCHYCODE")]
        public string OrderHierarchyCode { get; set; }
	      
		///<summary>
	    ///单位所属区域编码 
	    ///</summary> 
	    [Column("AREACODE", TypeName = "VARCHAR2")]
        public string AreaCode { get; set; }
	      
	}  
}

