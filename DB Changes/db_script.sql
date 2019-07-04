--create database erpdb;

--create table ProductCategorys (ProductCategoryId integer primary key identity, ProductCategoryName varchar(50));
--create table ProductTypes (ProductTypeId integer primary key identity, ProductTypeName varchar(50), ProductCategoryId integer FOREIGN KEY REFERENCES ProductCategorys(ProductCategoryId));

--create table ProductDetails (ProductId integer primary key identity, ProductTypeId integer FOREIGN KEY REFERENCES ProductTypes(ProductTypeId), InputCode varchar(50), InputMaterialDesc varchar(1000), OutputCode varchar(50), OutputMaterialDesc varchar(1000), ProjectName varchar(1000), SplitRatio integer, CreateDate datetime, EditDate datetime);
--CREATE UNIQUE INDEX unq_proddet_inputcode 
--  ON ProductDetails(InputCode) 
--  WHERE InputCode IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_outputcode 
--  ON ProductDetails(OutputCode) 
--  WHERE OutputCode IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_inputmatdesc 
--  ON ProductDetails(InputMaterialDesc) 
--  WHERE InputMaterialDesc IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_outputmatdesc 
--  ON ProductDetails(OutputMaterialDesc) 
--  WHERE OutputMaterialDesc IS NOT NULL;

--create table ChallanDetails (ChallanId integer primary key identity, ChallanNo varchar(50) NOT NULL UNIQUE, ChallanDate datetime, CreateDate datetime, EditDate datetime);
--create table ChallanProducts (ChallanProductId integer primary key identity, ChallanId integer FOREIGN KEY REFERENCES ChallanDetails(ChallanId), ProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), InputQuantity integer, CreateDate datetime, EditDate datetime);

----create table PODetails (POChallanId integer primary key identity, ChallanNo varchar(50) NOT NULL UNIQUE, ChallanDate datetime, CreateDate datetime, EditDate datetime);
----create table POProducts (POChallanProductId integer primary key identity, ChallanId integer FOREIGN KEY REFERENCES ChallanDetails(ChallanId), ProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), InputQuantity integer, CreateDate datetime, EditDate datetime);

--create table VendorChallans (VendorChallanNo integer primary key identity, VendorChallanDate datetime, CreateDate datetime, EditDate datetime);
--create table OutStocks (OutStockId integer primary key identity, VendorChallanNo integer FOREIGN KEY REFERENCES VendorChallans(VendorChallanNo), OutputQuantity integer, CreateDate datetime, EditDate datetime);
--create table OutAccs (OutAccId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), OutputQuantity integer, CreateDate datetime, EditDate datetime);
--create table ChallanDeductions (ChallanDeductionId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table AccChallanDeductions (AccChallanDeductionId integer primary key identity, OutAccId integer FOREIGN KEY REFERENCES OutAccs(OutAccId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
----create table PODeductions (PODeductionId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
----create table AccPODeductions (AccPODeductionId integer primary key identity, OutAccId integer FOREIGN KEY REFERENCES OutAccs(OutAccId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);



--insert into ProductCategorys (ProductCategoryName) values ('Main');
--insert into ProductCategorys (ProductCategoryName) values ('Assembly');
--insert into ProductCategorys (ProductCategoryName) values ('Accessories');

--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('PU', 1);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('RING', 2);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('SPACER', 2);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('BOX', 3);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('PALLET', 3);



--drop table ChallanDeductions;
--drop table AccChallanDeductions;
--drop table OutAccs;
--drop table OutStocks;
--drop table VendorChallans;

--drop table ChallanProducts;
--drop table ChallanDetails;

--drop table ProductDetails;
--drop table ProductTypes;
--drop table ProductCategorys;



--delete from ChallanDeductions;
--delete from AccChallanDeductions;
--delete from OutAccs;
--delete from OutStocks;
--delete from VendorChallans;

--delete from ChallanProducts;
--delete from ChallanDetails;

--delete from ProductDetails;



select * from ProductCategorys;
select * from ProductTypes;

select * from ProductDetails;

select * from VendorChallans;
select * from OutStocks;
select * from ChallanDeductions;

select * from OutAccs;
select * from AccChallanDeductions;

select * from ChallanDetails;
select * from ChallanProducts;