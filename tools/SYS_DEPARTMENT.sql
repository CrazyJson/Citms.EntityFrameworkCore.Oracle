---------------------------数据字典生成工具(V1.0)--------------------------------
declare  tableExist number;
begin
    select count(1) into tableExist from user_tables where upper(table_name)=upper('SYS_DEPARTMENT') ;
    if tableExist = 0  then
        execute immediate '
            CREATE TABLE SYS_DEPARTMENT(
                DEPARTMENTID VARCHAR2(32) DEFAULT sys_guid()   NOT NULL  ,
                BUNAME NVARCHAR2(50)   ,
                BUFULLNAME NVARCHAR2(100)   ,
                BUCODE NVARCHAR2(50)   ,
                HIERARCHYCODE NVARCHAR2(500)   ,
                PARENTGUID VARCHAR2(32)   ,
                WEBSITE NVARCHAR2(50)   ,
                FAX NVARCHAR2(20)   ,
                COMPANYADDR NVARCHAR2(100)   ,
                CHARTER NVARCHAR2(50)   ,
                CORPORATIONDEPUTY NVARCHAR2(20)   ,
                CREATEDON DATE   ,
                MODIFIEDON DATE   ,
                CREATEDBY VARCHAR2(32)   ,
                COMMENTS NVARCHAR2(500)   ,
                MODIFIEDBY VARCHAR2(32)   ,
                ISENDCOMPANY NUMBER(1,0) DEFAULT 0   ,
                ISCOMPANY NUMBER(1,0) DEFAULT 0   ,
                BULEVEL INTEGER DEFAULT 0   ,
                BUTYPE NUMBER(3,0) DEFAULT 0   ,
                ORDERCODE NVARCHAR2(20)   ,
                ORDERHIERARCHYCODE NVARCHAR2(500)   ,
                AREACODE VARCHAR2(10)   ,
                SIMPLECODE NVARCHAR2(50)   
		            )
	';

        execute immediate 'comment ON TABLE  SYS_DEPARTMENT IS ''组织机构表''';

        execute immediate 'comment on column SYS_DEPARTMENT.DEPARTMENTID is ''单位GUID''';
        execute immediate 'comment on column SYS_DEPARTMENT.BUNAME is ''单位简称''';
        execute immediate 'comment on column SYS_DEPARTMENT.BUFULLNAME is ''单位全称''';
        execute immediate 'comment on column SYS_DEPARTMENT.BUCODE is ''单位代码''';
        execute immediate 'comment on column SYS_DEPARTMENT.HIERARCHYCODE is ''层级代码''';
        execute immediate 'comment on column SYS_DEPARTMENT.PARENTGUID is ''父级GUID''';
        execute immediate 'comment on column SYS_DEPARTMENT.WEBSITE is ''网址''';
        execute immediate 'comment on column SYS_DEPARTMENT.FAX is ''传真''';
        execute immediate 'comment on column SYS_DEPARTMENT.COMPANYADDR is ''公司地址''';
        execute immediate 'comment on column SYS_DEPARTMENT.CHARTER is ''营业执照''';
        execute immediate 'comment on column SYS_DEPARTMENT.CORPORATIONDEPUTY is ''法人代表''';
        execute immediate 'comment on column SYS_DEPARTMENT.CREATEDON is ''创建时间''';
        execute immediate 'comment on column SYS_DEPARTMENT.MODIFIEDON is ''修改时间''';
        execute immediate 'comment on column SYS_DEPARTMENT.CREATEDBY is ''创建人''';
        execute immediate 'comment on column SYS_DEPARTMENT.COMMENTS is ''说明''';
        execute immediate 'comment on column SYS_DEPARTMENT.MODIFIEDBY is ''修改人''';
        execute immediate 'comment on column SYS_DEPARTMENT.ISENDCOMPANY is ''是否末级公司''';
        execute immediate 'comment on column SYS_DEPARTMENT.ISCOMPANY is ''是否公司''';
        execute immediate 'comment on column SYS_DEPARTMENT.BULEVEL is ''层级数''';
        execute immediate 'comment on column SYS_DEPARTMENT.BUTYPE is ''组织类型''';
        execute immediate 'comment on column SYS_DEPARTMENT.ORDERCODE is ''排序代码''';
        execute immediate 'comment on column SYS_DEPARTMENT.ORDERHIERARCHYCODE is ''排序层级代码''';
        execute immediate 'comment on column SYS_DEPARTMENT.AREACODE is ''单位所属区域编码''';
        execute immediate 'comment on column SYS_DEPARTMENT.SIMPLECODE is ''单位简码''';
        
    end if;
end;
	
