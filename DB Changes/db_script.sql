--create database erpdb;

--create table ProductCategorys (ProductCategoryId integer primary key identity, ProductCategoryName varchar(50));
--create table ProductTypes (ProductTypeId integer primary key identity, ProductTypeName varchar(50), ProductCategoryId integer FOREIGN KEY REFERENCES ProductCategorys(ProductCategoryId));

--create table ProductDetails (ProductId integer primary key identity, ProductTypeId integer FOREIGN KEY REFERENCES ProductTypes(ProductTypeId), InputCode varchar(50), InputMaterialDesc varchar(1000), OutputCode varchar(50), OutputMaterialDesc varchar(1000), ProjectName varchar(1000), SplitRatio integer, CreateDate datetime, EditDate datetime);
--CREATE UNIQUE INDEX unq_proddet_inputcode ON ProductDetails(InputCode) WHERE InputCode IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_outputcode ON ProductDetails(OutputCode) WHERE OutputCode IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_inputmatdesc ON ProductDetails(InputMaterialDesc) WHERE InputMaterialDesc IS NOT NULL;
--CREATE UNIQUE INDEX unq_proddet_outputmatdesc ON ProductDetails(OutputMaterialDesc) WHERE OutputMaterialDesc IS NOT NULL;

--create table ProductMappings (ProductMappingId integer primary key identity, ProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), MappingProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), CreateDate datetime, EditDate datetime);
--CREATE UNIQUE INDEX unq_prodmap ON ProductMappings(ProductId, MappingProductId);

--create table ChallanDetails (ChallanId integer primary key identity, ChallanNo varchar(50) NOT NULL UNIQUE, ChallanDate datetime, CreateDate datetime, EditDate datetime);
--create table ChallanProducts (ChallanProductId integer primary key identity, ChallanId integer FOREIGN KEY REFERENCES ChallanDetails(ChallanId), ProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), InputQuantity integer, CreateDate datetime, EditDate datetime);

--create table PODetails (POId integer primary key identity, PONo varchar(50) NOT NULL UNIQUE, PODate datetime, CreateDate datetime, EditDate datetime);
--create table POProducts (POProductId integer primary key identity, POId integer FOREIGN KEY REFERENCES PODetails(POId), ProductId integer FOREIGN KEY REFERENCES ProductDetails(ProductId), InputQuantity integer, CreateDate datetime, EditDate datetime);

--create table VendorChallans (VendorChallanNo integer primary key identity, VendorChallanDate datetime, IsNg integer, CreateDate datetime, EditDate datetime);
--create table OutStocks (OutStockId integer primary key identity, VendorChallanNo integer FOREIGN KEY REFERENCES VendorChallans(VendorChallanNo), OutputQuantity integer, CreateDate datetime, EditDate datetime);
--create table OutAccs (OutAccId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), OutputQuantity integer, CreateDate datetime, EditDate datetime);
--create table OutAssemblys (OutAssemblyId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), OutputQuantity integer, CreateDate datetime, EditDate datetime);
--create table ChallanDeductions (ChallanDeductionId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table AccChallanDeductions (AccChallanDeductionId integer primary key identity, OutAccId integer FOREIGN KEY REFERENCES OutAccs(OutAccId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table AssemblyChallanDeductions (AssemblyChallanDeductionId integer primary key identity, OutAssemblyId integer FOREIGN KEY REFERENCES OutAssemblys(OutAssemblyId), ChallanProductId integer FOREIGN KEY REFERENCES ChallanProducts(ChallanProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table PODeductions (PODeductionId integer primary key identity, OutStockId integer FOREIGN KEY REFERENCES OutStocks(OutStockId), POProductId integer FOREIGN KEY REFERENCES POProducts(POProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table AccPODeductions (AccPODeductionId integer primary key identity, OutAccId integer FOREIGN KEY REFERENCES OutAccs(OutAccId), POProductId integer FOREIGN KEY REFERENCES POProducts(POProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);
--create table AssemblyPODeductions (AssemblyPODeductionId integer primary key identity, OutAssemblyId integer FOREIGN KEY REFERENCES OutAssemblys(OutAssemblyId), POProductId integer FOREIGN KEY REFERENCES POProducts(POProductId), OutQuantity integer, CreateDate datetime, EditDate datetime);


--insert into ProductCategorys (ProductCategoryName) values ('Main');
--insert into ProductCategorys (ProductCategoryName) values ('Assembly');
--insert into ProductCategorys (ProductCategoryName) values ('Accessories');

--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('PU', 1);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('RING', 2);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('SPACER', 2);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('BOX', 3);
--insert into ProductTypes (ProductTypeName, ProductCategoryId) values ('PALLET', 3);



--drop table PODeductions;
--drop table AccPODeductions;
--drop table AssemblyPODeductions;

--drop table ChallanDeductions;
--drop table AccChallanDeductions;
--drop table AssemblyChallanDeductions;

--drop table OutAccs;
--drop table OutAssemblys;
--drop table OutStocks;
--drop table VendorChallans;

--drop table ChallanProducts;
--drop table ChallanDetails;

--drop table POProducts;
--drop table PODetails;

--drop table ProductDetails;
--drop table ProductTypes;
--drop table ProductCategorys;



--delete from PODeductions;
--delete from AccPODeductions;
--delete from AssemblyPODeductions;

--delete from ChallanDeductions;
--delete from AccChallanDeductions;
--delete from AssemblyChallanDeductions;

--delete from OutAccs;
--delete from OutAssemblys;
--delete from OutStocks;
--delete from VendorChallans;

--delete from ChallanProducts;
--delete from ChallanDetails;

--delete from POProducts;
--delete from PODetails;

--delete from ProductMappings;
--delete from ProductDetails;



select * from ProductCategorys;
select * from ProductTypes;

select * from ProductDetails;
select * from ProductMappings;

--update ProductDetails set InputMaterialDesc = 'Split Product', OutputMaterialDesc = 'Split Product' where ProductId = 12;

--INSERT INTO ProductMappings (ProductId, MappingProductId, CreateDate, EditDate) values (1, 9, GETDATE(), GETDATE());
--INSERT INTO ProductMappings (ProductId, MappingProductId, CreateDate, EditDate) values (1, 11, GETDATE(), GETDATE());

select * from VendorChallans;
select * from OutStocks;
select * from ChallanDeductions;
select * from PODeductions;

select * from OutAccs;
select * from AccChallanDeductions;
select * from AccPODeductions;

select * from OutAssemblys;
select * from AssemblyChallanDeductions;
select * from AssemblyPODeductions;

select * from ChallanDetails;
select * from ChallanProducts;

select * from PODetails;
select * from POProducts;

select * from ProductMappings;

--update PODetails set PONo = '8499975750' where POId = 1;



--delete from ChallanDeductions where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56);
--delete from AccChallanDeductions where OutAccId in (select OutAccId from OutAccs where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56));
--delete from AssemblyChallanDeductions where OutAssemblyId in (select OutAssemblyId from OutAssemblys where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56));

--delete from PODeductions where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56);
--delete from AccPODeductions where OutAccId in (select OutAccId from OutAccs where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56));
--delete from AssemblyPODeductions where OutAssemblyId in (select OutAssemblyId from OutAssemblys where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56));

--delete from OutAccs where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56);
--delete from OutAssemblys where OutStockId in (select OutStockId from OutStocks where VendorChallanNo = 56);
--delete from OutStocks where VendorChallanNo = 56;

--delete from VendorChallans where VendorChallanNo = 56;



select * from VendorChallans;
select * from ChallanProducts;
select * from POProducts;

select * from ProductDetails where ProductId = 21;

select * from OutStocks;

select * from ChallanDeductions;
select * from PODeductions;

--update POProducts set InputQuantity = 100 where POProductId = 40;