---------------------------数据字典生成工具(V1.0)--------------------------------
declare  tableExist number;
begin
    select count(1) into tableExist from user_tables where upper(table_name)=upper('COMMON_SPOTTING') ;
    if tableExist = 0  then
        execute immediate '
            CREATE TABLE COMMON_SPOTTING(
                SPOTTINGID VARCHAR2(50)  NOT NULL  ,
                SPOTTINGNO VARCHAR2(50)  NOT NULL  ,
                SPOTTINGNAME NVARCHAR2(100)  NOT NULL  ,
                UNIQUECODE VARCHAR2(50)   ,
                ROADID VARCHAR2(50)   ,
                LONGITUDE NUMBER(12,8)   ,
                LATITUDE NUMBER(12,8)   ,
                DEPARTMENTID VARCHAR2(50)  NOT NULL  ,
                SOURCEKIND VARCHAR2(50) DEFAULT ''local''   NOT NULL  ,
                CREATOR VARCHAR2(50)  NOT NULL  ,
                CREATEDTIME DATE DEFAULT sysdate   NOT NULL  ,
                MODIFIER VARCHAR2(50)   ,
                MODIFIEDTIME DATE   ,
                FLAGS VARCHAR2(10)   ,
                REMARK NVARCHAR2(500)   ,
                APPLICATIONNAME VARCHAR2(50) DEFAULT ''Citms.PIS''   NOT NULL  ,
                AREACODE VARCHAR2(50)   ,
                BOPOMOFO VARCHAR2(200)   ,
                SPOTTINGTYPE VARCHAR2(50)   ,
                VIRTUALDELETEFLAG INTEGER DEFAULT 0   ,
                DISABLED NUMBER(1,0) DEFAULT 0   ,
                PUNISHDEPARTMENT VARCHAR2(50)   ,
                DIVISIONCODE VARCHAR2(50)   ,
                APPROVESTATUS INTEGER DEFAULT 0   ,
                APPROVEUSERID VARCHAR2(50)   ,
                APPROVETIME DATE   ,
                APPROVEINFO NVARCHAR2(200)   ,
                MAXWEIGHT NUMBER(12,4)   ,
                MAXHEIGHT NUMBER(12,4)   ,
		PRIMARY KEY(SPOTTINGID) 
            )
	';

        execute immediate 'comment ON TABLE  COMMON_SPOTTING IS ''道路点位表''';

        execute immediate 'comment on column COMMON_SPOTTING.SPOTTINGID is ''点位ID''';
        execute immediate 'comment on column COMMON_SPOTTING.SPOTTINGNO is ''点位编号(可以为厂家分配的点位编号)''';
        execute immediate 'comment on column COMMON_SPOTTING.SPOTTINGNAME is ''点位名称''';
        execute immediate 'comment on column COMMON_SPOTTING.UNIQUECODE is ''上传六合一标准代码''';
        execute immediate 'comment on column COMMON_SPOTTING.ROADID is ''所在道路ID''';
        execute immediate 'comment on column COMMON_SPOTTING.LONGITUDE is ''经度坐标值''';
        execute immediate 'comment on column COMMON_SPOTTING.LATITUDE is ''纬度坐标值''';
        execute immediate 'comment on column COMMON_SPOTTING.DEPARTMENTID is ''所在管理部门''';
        execute immediate 'comment on column COMMON_SPOTTING.SOURCEKIND is ''来源类型''';
        execute immediate 'comment on column COMMON_SPOTTING.CREATOR is ''创建用户ID''';
        execute immediate 'comment on column COMMON_SPOTTING.CREATEDTIME is ''创建时间''';
        execute immediate 'comment on column COMMON_SPOTTING.MODIFIER is ''修改人''';
        execute immediate 'comment on column COMMON_SPOTTING.MODIFIEDTIME is ''修改时间''';
        execute immediate 'comment on column COMMON_SPOTTING.FLAGS is ''保留标记''';
        execute immediate 'comment on column COMMON_SPOTTING.REMARK is ''备注''';
        execute immediate 'comment on column COMMON_SPOTTING.APPLICATIONNAME is ''应用名称''';
        execute immediate 'comment on column COMMON_SPOTTING.AREACODE is ''所属辖区代码''';
        execute immediate 'comment on column COMMON_SPOTTING.BOPOMOFO is ''拼音简称''';
        execute immediate 'comment on column COMMON_SPOTTING.SPOTTINGTYPE is ''点位类型(字典表字典 ，Kind 为 1003 ， 十字路口/丁字路口/圆形转盘/其它)''';
        execute immediate 'comment on column COMMON_SPOTTING.VIRTUALDELETEFLAG is ''逻辑删除标记(0 正常数据， 1 逻辑删除)''';
        execute immediate 'comment on column COMMON_SPOTTING.DISABLED is ''是否停用(0 未停用， 1 停用)，默认为0''';
        execute immediate 'comment on column COMMON_SPOTTING.PUNISHDEPARTMENT is ''处理单位''';
        execute immediate 'comment on column COMMON_SPOTTING.DIVISIONCODE is ''行政区划代码''';
        execute immediate 'comment on column COMMON_SPOTTING.APPROVESTATUS is ''审核状态(0:未审核, 1:审核通过, 2:审核未通过), 默认为未审核状态''';
        execute immediate 'comment on column COMMON_SPOTTING.APPROVEUSERID is ''审核用户代码''';
        execute immediate 'comment on column COMMON_SPOTTING.APPROVETIME is ''审核时间''';
        execute immediate 'comment on column COMMON_SPOTTING.APPROVEINFO is ''审核说明''';
        execute immediate 'comment on column COMMON_SPOTTING.MAXWEIGHT is ''最大限重(KG)''';
        execute immediate 'comment on column COMMON_SPOTTING.MAXHEIGHT is ''最大限高(m)''';
        
    end if;
end;
	
