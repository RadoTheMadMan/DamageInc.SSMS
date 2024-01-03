
sp_configure 'show advanced options',1
go
reconfigure
go

sp_configure 'Database Mail XPs',1
go
reconfigure
go

create database DMGIncShopSystem collate SQL_Latin1_General_CP1251_CI_AS; 
use DMGIncShopSystem;

create table Users(UserID int identity primary key not null, UserName varchar(100) unique not null,UserDisplayName varchar(100) not null, UserEmail varchar(200) not null, UserPassword varchar(500) unique not null ,UserPhone varchar(50) not null,UserBalance money not null default 0,DateOfRegister date not null,UserProfilePic varbinary(max) not null, isAdmin bit not null, isWorker bit not null, isClient bit not null);
create table ProductCategories(CategoryID int identity primary key not null, CategoryName varchar(200) not null);
create table ProductBrands(BrandID int identity primary key not null, BrandName varchar(100) not null);
create table DeliveryServices(ServiceID int identity primary key not null, ServiceName varchar(50) not null, ServicePrice money not null);
create table DiagnosticTypes(TypeID int identity primary key not null, TypeName varchar(50), TypePrice money);
create table OrderTypes(OrderTypeID int identity primary key not null, TypeName varchar(50) not null);
create table PaymentMethods(PaymentMethodID int identity primary key not null, PaymentMethodName varchar(50) not null);
create table Clients(ClientID int identity primary key not null,UserID int foreign key references Users(UserID) on update cascade on delete cascade default 0, ClientName varchar(100) not null, ClientEmail varchar(100) not null, ClientPhone varchar(50) not null, ClientAddress varchar(100) not null,ClientBalance money not null default 0,ClientProfilePic varbinary(max) not null);
create table Products(ProductID int identity primary key not null, ProductCategoryID int foreign key references ProductCategories(CategoryID) on update cascade not null default 0,ProductBrandID int foreign key references ProductBrands(BrandID) on update cascade not null default 0,ProductName varchar(50) not null,ProductDescription varchar(max) not null,Quantity int not null, Price money not null,ProductArtID varchar(max) not null default '',ProductSerialNumber varchar(max) not null default '',ProductStorageLocation varchar(max) not null default '', DateAdded date not null, DateModified date not null);
create table ProductOrders(OrderID int identity primary key not null, ProductID int foreign key references Products(ProductID) not null default 0,OrderTypeID int foreign key  references OrderTypes(OrderTypeID) not null default 0,ReplacementProductID int foreign key references Products(ProductID) default 0, DesiredQuantity int not null,OrderPrice money not null,ClientID int foreign key references Clients(ClientID) not null default 0, UserID int foreign key references Users(UserID) default 0, DateAdded date not null, DateModified date not null, OrderStatus int not null default 0);
create table ProductImages(TargetProductID int foreign key references Products(ProductID) on update cascade on delete cascade  not null default 0, ImageName varchar(max) default '', Picture varbinary(max) );
create table OrderDeliveries(OrderDeliveryID int identity primary key not null, OrderID int foreign key references ProductOrders(OrderID)on update cascade on delete cascade default 0 ,ServiceID int foreign key references DeliveryServices(ServiceID) on update cascade default 0, DeliveryCargoID varchar(max) not null, TotalPayment money not null, PaymentMethodID int foreign key references PaymentMethods(PaymentMethodID) on update cascade default 0,DateAdded date not null, DateModified date not null, DeliveryStatus int not null default 0);
create table Logs(LogDate date not null default getdate(), LogMessage varchar(max) not null default '[LOG]');

create type UserTable as table (UserID   INT, UserName varchar(100), UserDisplayName varchar(100), UserEmail varchar(200), UserPassword varchar(500), UserPhone varchar(50), UserBalance money, DateOfRegister date, UserProfilePic varbinary(max), isAdmin bit, isWorker bit, isClient bit);
create type ProductCategoryTable as table (CategoryID int, CategoryName varchar(200));
create type ProductBrandTable as table(BrandID int, BrandName varchar(100));
create type DeliveryServiceTable as table(ServiceID int, ServiceName varchar(50), ServicePrice money);
create type DiagnosticTypeTable as table(TypeID int, TypeName varchar(50), TypePrice money);
create type OrderTypeTable as table(OrderTypeID int, TypeName varchar(50));
create type PaymentMethodTable as table(PaymentMethodID int, PaymentMethodName varchar(50));
create type ClientTable as table (ClientID int, UserID int, ClientName varchar(100), ClientEmail varchar(100), ClientPhone varchar(50), ClientAddress varchar(100), ClientBalance money ,ClientProfilePic varbinary(max));
create type ProductTable as table (ProductID int, ProductCategoryID int,ProductBrandID int,ProductName varchar(50),ProductDescription varchar(max), Quantity int, Price money,ProductArtID varchar(max), ProductSerialNumber varchar(max), ProductStorageLocation varchar(max), DateAdded date, DateModified date);
create type ProductOrdersTable as table(OrderID int, ProductID int,OrderTypeID int,ReplacementProductID int,DiagnosticTypeID int,DesiredQuantity int, OrderPrice money, ClientID int, UserID int, DateAdded date, DateModified date, OrderStatus int);
create type ProductImagesTable as table(TargetProductID int, ImageName varchar(max), Picture varbinary(max));
create type OrderDeliveryTable as table(OrderDeliveryID int,OrderID int , ServiceID int, DeliveryCargoID varchar(max), TotalPayment money, PaymentMethodID int ,DateAdded date, DateModified date, DeliveryStatus int);
create type ProductTableWithImages as table(ProductID int, ProductCategoryID int,ProductBrandID int,ProductName varchar(50),ProductDescription varchar(max), Quantity int, Price money,ProductArtID varchar(max) ,ProductSerialNumber varchar(max), ProductStorageLocation varchar(max) , DateAdded date, DateModified date, ProductImages varbinary(max));
create type ProductOrdersExtendedTable as table (OrderID int, ProductName varchar(50), ProductDescription varchar(max), OrderTypeName varchar(50), ReplacementProductName varchar(50), DiagnosticTypeName varchar(50), DesiredQuantity int, OrderPrice money, ClientName varchar(100), ClientEmail varchar(100), ClientPhone varchar(50), ClientAddress varchar(100), UserDisplayName varchar(100), DateAdded date, DateModified date, OrderStatus int);
create type OrderDeliveryExtendedTable as table (OrderDeliveryID int,ProductName varchar(50), ProductDescription varchar(max),DesiredQuantity int, OrderPrice money, ClientName varchar(100), ServiceName varchar(50), ServicePrice money, DeliveryCargoID varchar(max), TotalPayment money, PaymentMethodName varchar(50),DateAdded date ,DateModified date ,DeliveryStatus int);


go
create or alter procedure RegisterWorker (@username varchar(100), @displayname varchar(100), @email varchar(200), @password varchar(500), @phone varchar(50), @registerdate date, @profilepic varbinary(max))
as
if @registerdate is NULL
begin
set @registerdate = GETDATE();
end
insert into Users(UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,DateOfRegister,UserProfilePic,isAdmin,isClient,isWorker) values(@username, @displayname, @email,@password, @phone, @registerdate, @profilepic,0,0,1);
go

go
create or alter procedure RegisterClient (@username varchar(100), @displayname varchar(100), @email varchar(200), @password varchar(500), @phone varchar(50), @address varchar(100), @registerdate date, @profilepic varbinary(max))
as
declare @existingclientid int;
if @registerdate is NULL
begin
set @registerdate = GETDATE();
end
insert into Users(UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,DateOfRegister,UserProfilePic,isAdmin,isClient,isWorker) values(@username, @displayname, @email,@password, @phone, @registerdate, @profilepic,0,1,0);
declare @currentuserid int;
select @currentuserid = UserID from Users where UserName = @username;
select @existingclientid = ClientID from Clients where UserID = @currentuserid;
if @existingclientid is not null or @existingclientid != 0
begin
update Clients set ClientAddress = @address where ClientID =@existingclientid;
end
else
begin
insert into Clients(UserID, ClientName,ClientEmail, ClientPhone, ClientAddress,ClientProfilePic) values(@currentuserid,@displayname,@email,@phone,@address,@profilepic);
end
go

go
create or alter procedure AddClientWithoutRegistering(@clientname varchar(100), @email varchar(200), @phone varchar(50),@address varchar(100), @profilepic varbinary(max))
as
insert into Clients(ClientName,ClientEmail,ClientPhone,ClientAddress,ClientProfilePic) values(@clientname, @email, @phone, @address, @profilepic);
go

go
create or alter procedure RegisterAdmin (@username varchar(100), @displayname varchar(100), @email varchar(200), @password varchar(500), @phone varchar(50), @registerdate date, @profilepic varbinary(max))
as
if @registerdate is NULL
begin
set @registerdate = GETDATE();
end
insert into Users(UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,DateOfRegister,UserProfilePic,isAdmin,isClient,isWorker) values(@username, @displayname, @email,@password, @phone, @registerdate, @profilepic,1,0,0);
go

go
create or alter procedure RegisterUser (@username varchar(100), @displayname varchar(100), @email varchar(200), @password varchar(500), @phone varchar(50), @registerdate date, @profilepic varbinary(max), @isadmin bit, @isclient bit,@isworker bit)
as
if @registerdate is NULL
begin
set @registerdate = GETDATE();
end
insert into Users(UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,DateOfRegister,UserProfilePic,isAdmin,isClient,isWorker) values(@username, @displayname, @email,@password, @phone, @registerdate, @profilepic,@isadmin,@isclient,@isworker);
go

go
create or alter procedure AddPaymentMethod(@method_name varchar(50))
as
insert into PaymentMethods(PaymentMethodName) values(@method_name);
go

go
create or alter procedure AddDeliveryService(@name varchar(50), @price money)
as
insert into DeliveryServices(ServiceName,ServicePrice) values (@name,@price);
go

go
create or alter procedure AddDiagnosticType(@name varchar(50), @price money)
as
insert into DiagnosticTypes(TypeName,TypePrice) values (@name,@price);
go

go
create or alter procedure AddOrderType(@typename varchar(200))
as
insert into OrderTypes(TypeName) values(@typename); 
go

go
create or alter procedure AddProductCategory(@categoryname varchar(200))
as
insert into ProductCategories(CategoryName) values(@categoryname); 
go

go
create or alter procedure AddProductBrand(@brandname varchar(100))
as
insert into ProductBrands(BrandName) values (@brandname);
go

go
create or alter procedure AddProduct(@categoryid int,@brandid int, @name varchar(50), @description varchar(max), @quantity int, @price money,@artid varchar(max), @serialnumber varchar(max), @storagelocation varchar(max))
as
insert into Products(ProductCategoryID,ProductBrandID,ProductName,ProductDescription, Quantity,Price,ProductArtID,ProductSerialNumber,ProductStorageLocation,DateAdded,DateModified) values(@categoryid,@brandid,@name,@description, @quantity, @price, @artid, @serialnumber, @storagelocation,getdate(),getdate());
go

go
create or alter procedure AddProductExtended(@categoryname varchar(200),@brandname varchar(100), @name varchar(50), @description varchar(max), @quantity int, @price money, @artid varchar(max), @serialnumber varchar(max), @storagelocation varchar(max))
as
declare @categoryid as int
declare @brandid as int
select @categoryid = CategoryID from ProductCategories where CategoryName = @categoryname;
select @brandid = BrandID from ProductBrands where BrandName = @brandname;
insert into Products(ProductCategoryID,ProductBrandID,ProductName, ProductDescription, Quantity, Price,ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) values (@categoryid,@brandid , @name, @description, @quantity, @price, @artid, @serialnumber, @storagelocation,getdate(),getdate());
go

go
create or alter procedure AddProductImage(@productid int, @image_name varchar(max), @productimage varbinary(max))
as
insert into ProductImages(TargetProductID,ImageName,Picture) values (@productid,@image_name,@productimage);
go

go
create or alter procedure AddProductImageExtended(@name varchar(50), @image_name varchar(max), @image varbinary(max))
as
declare @productid as int;
select @productid = ProductID from Products where ProductName = @name;
insert into ProductImages(TargetProductID,ImageName,Picture) values(@productid, @image_name, @image);
go

go
create or alter procedure AddOrder(@productid int, @ordertypeid int,@replacementproductid int,@diagnostictypeid int, @desiredquantity int, @desiredprice money, @clientid int, @userid int, @settotalprice bit)
as
declare @price as money;
declare @diagnostic_price as money;
declare @totalprice as money;
select @price = Price from Products where ProductID = @productid;
select @diagnostic_price = TypePrice from DiagnosticTypes where TypeID = @diagnostictypeid;
if @desiredprice is not null and @settotalprice is not null
begin
if @settotalprice = 1 and @ordertypeid = 1
begin
set @totalprice = @desiredprice;
end
else if @settotalprice = 0 and @ordertypeid = 1
begin
set @totalprice = @desiredprice * @desiredquantity;
end
else if @settotalprice = 1 and @ordertypeid = 2
begin
set @totalprice = @desiredprice;
end
else if @ordertypeid = 1
begin
set @totalprice = @price * @desiredquantity;
end
else if @ordertypeid = 2
begin
set @totalprice = @diagnostic_price;
end
else if @settotalprice = 1
begin
set @totalprice = @desiredprice;
end
else
begin
set @totalprice = @price * @desiredquantity;
end
end
insert into ProductOrders(ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) values( @productid,@ordertypeid,@replacementproductid,@diagnostictypeid,@desiredquantity, @totalprice, @clientid, @userid, getdate(), getdate(),0);
go

go
create or alter procedure AddOrderExtended(@productname varchar(50),@typename varchar(50), @replacementproductname varchar(50),@diagnostictypename varchar(50), @desiredquantity int,@desiredprice money, @clientname varchar(100), @userdisplayname varchar(100), @settotalprice bit)
as
declare @productid as int;
declare @ordertypeid as int;
declare @replacementproductid as int;
declare @diagnostictypeid as int;
declare @clientid as int;
declare @userid as int;
declare @price as money;
declare @diagnostic_price as money;
declare @totalprice as money;
select @productid = ProductID from Products where ProductName = @productname;
select @ordertypeid = OrderTypeID from OrderTypes where TypeName = @typename;
select @replacementproductid = ProductID from Products where ProductName = @replacementproductname;
select @diagnostictypeid = TypeID from DiagnosticTypes where TypeName = @diagnostictypename;
select @clientid = ClientID from Clients where ClientName = @clientname;
select @userid = UserID from Users where UserDisplayName like @userdisplayname;
select @price = Price from Products where ProductID = @productid;
select @diagnostic_price = TypePrice from DiagnosticTypes where TypeID = @diagnostictypeid;
if @desiredprice is not null and @settotalprice is not null
begin
if @settotalprice = 1 and @ordertypeid = 1
begin
set @totalprice = @desiredprice;
end
else if @settotalprice = 0 and @ordertypeid = 1
begin
set @totalprice = @desiredprice * @desiredquantity;
end
else if @settotalprice = 1 and @ordertypeid = 2
begin
set @totalprice = @desiredprice;
end
else if @ordertypeid = 1
begin
set @totalprice = @price * @desiredquantity;
end
else if @ordertypeid = 2
begin
set @totalprice = @diagnostic_price;
end
else if @settotalprice = 1
begin
set @totalprice = @desiredprice;
end
else
begin
set @totalprice = @price * @desiredquantity;
end
end
insert into ProductOrders(ProductID, OrderTypeID, ReplacementProductID, DiagnosticTypeID, DesiredQuantity, OrderPrice, ClientID, UserID, DateAdded, DateModified, OrderStatus) values(@productid,@ordertypeid,@replacementproductid, @diagnostictypeid,@desiredquantity, @totalprice, @clientid, @userid, getdate(),getdate(), 0);
go

go
create or alter procedure AddOrderDelivery(@orderid int, @serviceid int, @deliverycargoid varchar(max), @paymentmethodid int)
as
declare @totalpayment as money;
declare @orderpayment as money;
declare @deliverypayment as money;
select @orderpayment = OrderPrice from ProductOrders where OrderID = @orderid;
select @deliverypayment = ServicePrice from DeliveryServices where ServiceID = @serviceid;
set @totalpayment = @orderpayment + @deliverypayment;
insert into OrderDeliveries(OrderID, ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) values(@orderid,@serviceid, @deliverycargoid, @totalpayment, @paymentmethodid, getdate(), getdate(),  0);
go

go
create or alter procedure AddOrderDeliveryExtended(@productname varchar(50), @servicename varchar(50), @deliverycargoid varchar(max), @paymentmethodname varchar(50))
as
declare @productid as int;
declare @orderid as int;
declare @serviceid as int;
declare @paymentmethodid as int;
declare @orderprice as money;
declare @deliveryprice as money;
declare @totalprice as money;
select @productid = ProductID from Products where ProductName = @productname;
select @orderid = OrderID from ProductOrders where ProductID = @productid;
select @serviceid = ServiceID from DeliveryServices where ServiceName = @servicename;
select @paymentmethodid = PaymentMethodID from PaymentMethods where PaymentMethodName = @paymentmethodname;
select @orderprice = OrderPrice from ProductOrders where OrderID = @orderid;
select @deliveryprice = ServicePrice from DeliveryServices where ServiceID = @serviceid;
set @totalprice = @orderprice + @deliveryprice;
insert into OrderDeliveries(OrderID, ServiceID, DeliveryCargoID, TotalPayment, PaymentMethodID, DateAdded, DateModified, DeliveryStatus) values (@orderid, @serviceid, @deliverycargoid, @totalprice,@paymentmethodid,getdate(),getdate(),0);
go


create or alter procedure GetUserByID(@id int)
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserID = @id;
select * from @result;
go

create or alter procedure GetUserByUserName(@username varchar(100))
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserName like '%' + @username + '%';
select * from @result;
go

go
create or alter procedure GetUserByDisplayName(@displayname varchar(100))
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserDisplayName like '%' + @displayname + '%';
select * from @result;
go

go
create or alter procedure GetUserByEmail(@email varchar(200))
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserEmail like '%' + @email + '%';
select * from @result;
go

go
create or alter procedure GetUserByPhone(@phone varchar(50))
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserPhone like '%' + @phone + '%';
select * from @result;
go

go
create or alter procedure GetUserByBalance(@balance money, @look_below bit)
as
declare @result UserTable;
if @look_below = 1
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserBalance <= @balance;
end
else
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserBalance >= @balance;
end
select * from @result;
go

go
create or alter procedure GetUserByDate(@date date, @look_before bit)
as
declare @result UserTable;
if @look_before = 1
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where DateOfRegister <= @date;
end
else
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where DateOfRegister >= @date;
end
select * from @result;
go

go
create or alter  procedure GetUserStrict(@id int,@username varchar(100), @displayname varchar(100), @email varchar(200), @phone varchar(50), @date date, @balance money, @look_before_date bit, @look_below_balance bit)
as
declare @result UserTable;
if @look_before_date = 1 and @look_below_balance = 1
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserID = @id and UserName like '%' + @username + '%' and UserDisplayName like '%' + @displayname + '%' and UserEmail like '%' + @email + '%' and UserPhone like '%' + @phone + '%' and UserBalance <= @balance and DateOfRegister <= @date;
end
else if @look_before_date = 0 and @look_below_balance = 1
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserID = @id and UserName like '%' + @username + '%' and UserDisplayName like '%' + @displayname + '%' and UserEmail like '%' + @email + '%' and UserPhone like '%' + @phone + '%' and UserBalance <= @balance and DateOfRegister >= @date;
end
else if @look_before_date = 1 and @look_below_balance = 0
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserID = @id and UserName like '%' + @username + '%' and UserDisplayName like '%' + @displayname + '%' and UserEmail like '%' + @email + '%' and UserPhone like '%' + @phone + '%' and UserBalance >= @balance and DateOfRegister <= @date;
end
else
begin
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users where UserID = @id and UserName like '%' + @username + '%' and UserDisplayName like '%' + @displayname + '%' and UserEmail like '%' + @email + '%' and UserPhone like '%' + @phone + '%' and UserBalance >= @balance and DateOfRegister >= @date;
end
select * from @result
go

go
create or alter procedure GetAllUsers
as
declare @result UserTable;
insert into @result(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) select * from Users;
select * from @result;
go

go
create or alter procedure GetClientByID(@id int)
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientID = @id;
select * from @result;
go

go
create or alter procedure GetClientByName(@clientname varchar(100))
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientName like '%' + @clientname + '%';
select * from @result;
go

go 
create or alter procedure GetClientByEmail(@clientemail varchar(100))
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientEmail like '%' + @clientemail + '%';
select * from @result;
go

go 
create or alter procedure GetClientByAddress(@clientaddress varchar(100))
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,CLientBalance,ClientProfilePic) select * from Clients where ClientAddress like '%' + @clientaddress + '%';
select * from @result;
go

go
create or alter procedure GetClientByPhone(@clientphone varchar(50))
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientPhone like '%' + @clientphone + '%';
select * from @result;
go

go
create or alter procedure GetClientByBalance(@balance money, @look_below bit)
as
declare @result ClientTable;
if @look_below = 1
begin
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientBalance <= @balance;
end
else
begin
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientBalance >= @balance;
end
select * from @result;
go

go
create or alter procedure GetClientStrict(@id int, @name varchar(100), @phone varchar(50), @email varchar(100), @address varchar(100), @balance money, @look_below_balance bit)
as
declare @result ClientTable;
if @look_below_balance = 1
begin
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientID = @id and ClientName like '%' + @name + '%' and ClientPhone like '%'+ @phone + '%' and ClientEmail like '%' + @email + '%' and ClientAddress like '%' + @address + '%' and ClientBalance <= @balance;
end
else
begin
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients where ClientID = @id and ClientName like '%' + @name + '%' and ClientPhone like '%'+ @phone + '%' and ClientEmail like '%' + @email + '%' and ClientAddress like '%' + @address + '%' and ClientBalance >= @balance;
end
select * from @result;
go

go
create or alter procedure GetAllClients
as
declare @result as ClientTable
insert into @result(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) select * from Clients;
select * from @result;
go

go
create or alter procedure GetDeliveryServiceByID(@id int)
as
declare @result as DeliveryServiceTable
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where ServiceID = @id;
select * from @result;
go

go
create or alter procedure GetDeliveryServiceByName(@servicename varchar(50))
as
declare @result as DeliveryServiceTable
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where ServiceName like '%' + @servicename + '%';
select * from @result;
go

go
create or alter procedure GetDeliveryServiceByPrice(@price money, @look_below bit)
as
declare @result as DeliveryServiceTable
if @look_below = 1
begin
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where ServicePrice <= @price;
end
else
begin
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where ServicePrice >= @price;
end
select * from @result;
go

go
create or alter procedure GetDeliveryServiceStrict(@id int, @name varchar(50), @price money, @look_below_price bit)
as
declare @result as DeliveryServiceTable
if @look_below_price = 1
begin
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where ServiceID = @id and ServiceName like '%' + @name + '%' and ServicePrice <= @price;
end
else
begin
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices where  ServiceID = @id and ServiceName like '%' + @name + '%' and ServicePrice >= @price;
end
select * from @result;
go

go
create or alter procedure GetAllDeliveryServices
as
declare @result as DeliveryServiceTable
insert into @result(ServiceID,ServiceName,ServicePrice) select * from DeliveryServices;
select * from @result;
go

go
create or alter procedure GetDiagnosticTypeByID(@id int)
as
declare @result as DiagnosticTypeTable
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where TypeID = @id;
select * from @result;
go

go
create or alter procedure GetDiagnosticTypeByName(@typename varchar(50))
as
declare @result as DiagnosticTypeTable
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where TypeName like '%' + @typename + '%';
select * from @result;
go

go
create or alter procedure GetDiagnosticTypeByPrice(@price money, @look_below bit)
as
declare @result as DiagnosticTypeTable
if @look_below = 1
begin
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where TypePrice <= @price;
end
else
begin
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where TypePrice >= @price;
end
select * from @result;
go

go
create or alter procedure GetDiagnosticTypeStrict(@id int, @name varchar(50), @price money, @look_below_price bit)
as
declare @result as DiagnosticTypeTable
if @look_below_price = 1
begin
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where TypeID = @id and TypeName like '%' + @name + '%' and TypePrice <= @price;
end
else
begin
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes where  TypeID = @id and TypeName like '%' + @name + '%' and TypePrice >= @price;
end
select * from @result;
go

go
create or alter procedure GetAllDiagnosticTypes
as
declare @result as DiagnosticTypeTable
insert into @result(TypeID,TypeName,TypePrice) select * from DiagnosticTypes;
select * from @result;
go

go
create or alter procedure GetPaymentMethodByID(@id int)
as
declare @result as PaymentMethodTable;
insert into @result(PaymentMethodID,PaymentMethodName) select * from PaymentMethods where PaymentMethodID like @id;
select * from @result;
go

go
create or alter procedure GetPaymentMethodByName(@name varchar(50))
as
declare @result as PaymentMethodTable;
insert into @result(PaymentMethodID,PaymentMethodName) select * from PaymentMethods where PaymentMethodName like '%' + @name + '%';
select * from @result;
go

go
create or alter procedure GetAllPaymentMethods
as
declare @result as PaymentMethodTable;
insert into @result(PaymentMethodID,PaymentMethodName) select * from PaymentMethods;
select * from @result;
go

go
create or alter procedure GetProductCategoryByID(@id int)
as
declare @result as ProductCategoryTable;
insert into @result(CategoryID, CategoryName) select * from ProductCategories where CategoryID = @id;
select * from @result;
go

go
create or alter procedure GetProductCategoryByName(@name varchar(200))
as
declare @result as ProductCategoryTable;
insert into @result(CategoryID, CategoryName) select * from ProductCategories where CategoryName like '%' + @name + '%';
select * from @result;
go

go
create or alter procedure GetAllProductCategories
as
declare @result as ProductCategoryTable;
insert into @result(CategoryID, CategoryName) select * from ProductCategories;
select * from @result;
go

go
create or alter procedure GetOrderTypeByID(@id int)
as
declare @result as OrderTypeTable;
insert into @result(OrderTypeID, TypeName) select * from OrderTypes where OrderTypeID = @id;
select * from @result;
go

go
create or alter procedure GetOrderTypeByName(@name varchar(200))
as
declare @result as OrderTypeTable;
insert into @result(OrderTypeID, TypeName) select * from OrderTypes where TypeName like '%' + @name + '%';
select * from @result;
go

go
create or alter procedure GetAllOrderTypes
as
declare @result as OrderTypeTable;
insert into @result(OrderTypeID, TypeName) select * from OrderTypes;
select * from @result;
go

go
create or alter procedure GetProductBrandByID(@id int)
as
declare @result as ProductBrandTable;
insert into @result(BrandID, BrandName) select * from ProductBrands where BrandID = @id;
select * from @result;
go

go
create or alter procedure GetProductBrandByName(@name varchar(100))
as
declare @result as ProductBrandTable;
insert into @result(BrandID, BrandName) select * from ProductBrands where BrandName like '%' + @name + '%';
select * from @result;
go

go
create or alter procedure GetAllProductBrands
as
declare @result as ProductBrandTable;
insert into @result(BrandID, BrandName) select * from ProductBrands;
select * from @result;
go

go
create or alter procedure GetProductByID(@id int)
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id;
select * from @result;
go

go
create or alter procedure GetProductByName(@name varchar(50))
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductName like '%' + @name + '%';
select * from @result;
go

go
create or alter procedure GetProductByDescription(@description varchar(max))
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductDescription like '%' + @description + '%';
select * from @result;
go

go
create or alter procedure GetProductByCategory(@category varchar(200))
as
declare @categoryid as int;
declare @result as ProductTable;
select @categoryid = CategoryID from ProductCategories where CategoryName like '%' + @category + '%';
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductCategoryID = @categoryid;
select * from @result;
go

go
create or alter procedure GetProductByBrand(@brand varchar(100))
as
declare @brandid as int;
declare @result as ProductTable;
select @brandid = BrandID from ProductBrands where BrandName like '%' + @brand + '%';
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductBrandID = @brandid;
select * from @result;
go

go
create or alter procedure GetProductByDate(@date date, @look_before bit)
as
declare @result as ProductTable;
if @look_before = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where DateAdded <= @date or DateModified <= @date;
end
else
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where DateAdded >= @date or DateModified >= @date;
end
select * from @result;
go

go
create or alter procedure GetProductByQuantity(@quantity int, @look_below bit)
as
declare @result as ProductTable;
if @look_below = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where Quantity <= @quantity;
end
else
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where Quantity >= @quantity;
end
select * from @result;
go

go
create or alter procedure GetProductByPrice(@price money, @look_below bit)
as
declare @result as ProductTable;
if @look_below = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where Price <= @price;
end
else
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where Price >= @price;
end
select * from @result;
go

go
create or alter procedure GetProductByArtID(@artid varchar(max))
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductArtID like '%' + @artid + '%';
select * from @result;
go

go
create or alter procedure GetProductBySerialNumber(@serialnumber varchar(max))
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductSerialNumber like '%' + @serialnumber + '%';
select * from @result;
go

go
create or alter procedure GetProductByStorageLocation(@location varchar(max))
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductStorageLocation like '%' + @location + '%';
select * from @result;
go

go
create or alter procedure GetProductStrict(@id int, @category varchar(200), @brand varchar(100), @name varchar(50), @description varchar(max), @quantity int, @price money, @artid varchar(max), @serialnumber varchar(max), @storagelocation varchar(max), @date date, @look_below_quantity bit, @look_below_price bit, @look_before_date bit)
as
declare @result as ProductTable;
declare @categoryid as int;
declare @brandid as int;
select @categoryid = CategoryID from ProductCategories where CategoryName like '%' + @category + '%';
select @brandid = BrandID from ProductBrands where BrandName like '%' + @brand + '%';
/* 111 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price <= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 110 */
else if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 0
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price <= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 101 */
else  if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price >= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 011 */
else if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price <= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 100 */
else if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 0
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price >= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 010 */
else if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 0
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price <= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 001 */
else if @look_below_quantity = 0 and @look_below_price = 0 and @look_before_date = 1
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price >= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 000 */
else
begin
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price >= @price and (DateAdded >= @date or DateModified >= @date);
end
select * from @result;
go

go
create or alter procedure GetAllProducts
as
declare @result as ProductTable;
insert into @result(ProductID, ProductCategoryID, ProductBrandID, ProductName, ProductDescription, Quantity, Price, ProductArtID, ProductSerialNumber, ProductStorageLocation, DateAdded, DateModified) select * from Products;
select * from @result;
go

go
create or alter procedure GetProductImagesByID(@id int)
as
declare @result as ProductImagesTable;
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @id;
select * from @result;
go

go
create or alter procedure GetProductImagesByName(@name varchar(50))
as
declare @productid as int;
declare @result as ProductImagesTable;
select @productid = ProductID from Products where ProductName like '%' + @name + '%';
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByDescription(@description varchar(max))
as
declare @productid as int;
declare @result as ProductImagesTable;
select @productid = ProductID from Products where ProductDescription like '%' + @description + '%';
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByArtID(@artid varchar(max))
as
declare @productid as int;
declare @result as ProductImagesTable;
select @productid = ProductID from Products where ProductArtID like '%' + @artid + '%';
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesBySerialNumber(@serialnumber varchar(max))
as
declare @productid as int;
declare @result as ProductImagesTable;
select @productid = ProductID from Products where ProductSerialNumber like '%' + @serialnumber + '%';
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByLocation(@location varchar(max))
as
declare @productid as int;
declare @result as ProductImagesTable;
select @productid = ProductID from Products where ProductStorageLocation like '%' + @location + '%';
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByQuantity(@quantity int, @look_below bit)
as
declare @productid as int;
declare @result as ProductImagesTable;
if @look_below = 1
begin
select @productid = ProductID from Products where Quantity <= @quantity;
end
else
begin
select @productid = ProductID from Products where Quantity >= @quantity;
end
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByPrice(@price money, @look_below bit)
as
declare @productid as int;
declare @result as ProductImagesTable;
if @look_below = 1
begin
select @productid = ProductID from Products where Price <= @price;
end
else
begin
select @productid = ProductID from Products where Price >= @price;
end
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesByDate(@date date, @look_before bit)
as
declare @productid as int;
declare @result as ProductImagesTable;
if @look_before = 1
begin
select @productid = ProductID from Products where DateAdded <= @date or DateModified <= @date;
end
else
begin
select @productid = ProductID from Products where DateAdded >= @date or DateModified >= @date;
end
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetProductImagesStrict(@id int, @category varchar(200), @brand varchar(100), @name varchar(50), @description varchar(max), @quantity int, @price money, @artid varchar(max), @serialnumber varchar(max), @storagelocation varchar(max), @date date, @look_below_quantity bit, @look_below_price bit, @look_before_date bit)
as
declare @categoryid as int;
declare @brandid as int;
declare @productid as int;
declare @result as ProductImagesTable;
/* 111 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 1
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price <= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 110 */
else if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 0
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%' and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price <= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 101 */
else  if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 1
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price >= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 011 */
else if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 1
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price <= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 100 */
else if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 0
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity <= @quantity and Price >= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 010 */
else if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 0
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price <= @price and (DateAdded >= @date or DateModified >= @date);
end
/* 001 */
else if @look_below_quantity = 0 and @look_below_price = 0 and @look_before_date = 1
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price >= @price and (DateAdded <= @date or DateModified <= @date);
end
/* 000 */
else
begin
select @productid = ProductID from Products where ProductID = @id and ProductCategoryID = @categoryid and ProductBrandID = @brandid and ProductName like '%' + @name + '%' and ProductDescription like '%' + @description + '%' and ProductArtID like '%' + @artid + '%' and ProductSerialNumber like '%' + @serialnumber + '%'  and ProductStorageLocation like '%' + @storagelocation + '%' and Quantity >= @quantity and Price >= @price and (DateAdded >= @date or DateModified >= @date);
end
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages where TargetProductID = @productid;
select * from @result;
go

go
create or alter procedure GetAllProductImages
as
declare @result as ProductImagesTable;
insert into @result(TargetProductID, ImageName, Picture) select * from ProductImages;
select * from @result;
go

go
create or alter procedure GetOrderByID(@id int)
as
declare @result as ProductOrdersTable;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where OrderID = @id;
select * from @result;
go

go
create or alter procedure GetOrderByIDExtended(@id int)
as
declare @result as ProductOrdersExtendedTable;
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname as varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
select @orderid  = OrderID from ProductOrders where OrderID = @id;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByProductName(@productname varchar(50))
as
declare @productid as int;
declare @result as ProductOrdersTable;
select @productid = ProductID from Products where ProductName like '%' + @productname + '%';
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid;
select * from @result;
go


go
create or alter procedure GetOrderByProductNameExtended(@productname varchar(50))
as
declare @result as ProductOrdersExtendedTable;
declare @productid int;
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid int;
declare @diagnostictypename varchar(50);
select @productid = ProductID from Products where ProductName like '%' +  @productname + '%';
select @orderid  = OrderID from ProductOrders where ProductID = @productid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByTypeID(@typeid int)
as
declare @result as ProductOrdersTable;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where OrderTypeID = @typeid;
select * from @result;
go

go
create or alter procedure GetOrderByTypeIDExtended(@typeid int)
as
declare @result as ProductOrdersExtendedTable;
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid int;
declare @diagnostictypename varchar(50);
select @orderid  = OrderID from ProductOrders where OrderTypeID = @typeid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByDiagnosticTypeID(@diagnostictypeid int)
as
declare @result as ProductOrdersTable;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where DiagnosticTypeID = @diagnostictypeid;
select * from @result;
go

go
create or alter procedure GetOrderByDiagnosticTypeIDExtended(@diagnostictypeid int)
as
declare @result as ProductOrdersExtendedTable;
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypename varchar(50);
select @orderid  = OrderID from ProductOrders where DiagnosticTypeID = @diagnostictypeid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByReplacementProductName(@replacementproductname varchar(50))
as
declare @replacementproductid as int;
declare @result as ProductOrdersTable;
select @replacementproductid = ProductID from Products where ProductName like '%' + @replacementproductname + '%';
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ReplacementProductID = @replacementproductid;
select * from @result;
go


go
create or alter procedure GetOrderByReplacementProductNameExtended(@targetreplacementproductname varchar(50))
as
declare @result ProductOrdersExtendedTable;
declare @targetreplacementproductid as int; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname as varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
select @targetreplacementproductid = ProductID from Products where ProductName like '%' + @targetreplacementproductname + '%';
select @orderid  = OrderID from ProductOrders where ReplacementProductID = @targetreplacementproductid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go


go
create or alter procedure GetOrderByClientName(@clientname varchar(100))
as
declare @clientid as int;
declare @result as ProductOrdersTable;
select @clientid = ClientID from Clients where ClientName like '%' + @clientname + '%' ;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ClientID = @clientid;
select * from @result;
go

go
create or alter procedure GetOrderByClientNameExtended(@clientname varchar(100))
as
declare @result ProductOrdersExtendedTable;
declare @clientid as int; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
select @clientid = ClientID from Clients where ClientName like '%' + @clientname + '%';
select @orderid  = OrderID from ProductOrders where ClientID = @clientid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByUserDisplayName(@userdisplayname varchar(100))
as
declare @userid as int;
declare @result as ProductOrdersTable;
select @userid = UserID from Users where UserDisplayName like '%' + @userdisplayname + '%';
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where UserID = @userid;
select * from @result;
go

go
create or alter procedure GetOrderByUserDisplayNameExtended(@userdisplayname varchar(100))
as
declare @result ProductOrdersExtendedTable;
declare @userid as int; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
select @userid = UserID from Users where UserDisplayName like '%' + userdisplayname + '%';
select @orderid  = OrderID from ProductOrders where ProductID = @userid;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByPrice(@price money, @look_below bit)
as
declare @result as ProductOrdersTable;
if @look_below = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where OrderPrice <= @price;
end
else
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where OrderPrice >= @price;
end
select * from @result;
go

go
create or alter procedure GetOrderByPriceExtended(@price money, @look_below bit)
as
declare @result as ProductOrdersExtendedTable; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
if @look_below = 1
begin
select @orderid  = OrderID from ProductOrders where OrderPrice <= @price;
end
else
begin
select @orderid  = OrderID from ProductOrders where OrderPrice >= @price;
end
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByQuantity(@quantity int, @look_below bit)
as
declare @result as ProductOrdersTable;
if @look_below = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where DesiredQuantity <= @quantity;
end
else
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where DesiredQuantity >= @quantity;
end
select * from @result;
go

go
create or alter procedure GetOrderByQuantityExtended(@quantity int, @look_below bit)
as
declare @result as ProductOrdersExtendedTable; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
if @look_below = 1
begin
select @orderid  = OrderID from ProductOrders where DesiredQuantity <= @quantity;
end
else
begin
select @orderid  = OrderID from ProductOrders where DesiredQuantity >= @quantity;
end
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByDate(@date date, @look_before bit)
as
declare @result as ProductOrdersTable;
if @look_before = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where DateAdded <= @date or DateModified <= @date;
end
else
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where DateAdded >= @date or DateModified >= @date;
end
select * from @result;
go

go
create or alter procedure GetOrderByDateExtended(@date date, @look_before bit)
as
declare @result as ProductOrdersExtendedTable; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
if @look_before = 1
begin
select @orderid  = OrderID from ProductOrders where DateAdded <= @date or DateModified <= @date;
end
else
begin
select @orderid  = OrderID from ProductOrders where DateAdded >= @date or DateModified >= @date;
end
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderByStatus(@status int)
as
declare @result as ProductOrdersTable;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where OrderStatus = @status;
select * from @result;
go

go
create or alter procedure GetOrderByStatusExtended(@status int)
as
declare @result as ProductOrdersExtendedTable; 
declare @orderid as int;
declare @replacementproductid as int;
declare @replacementproductname varchar(50);
declare @diagnostictypeid as int;
declare @diagnostictypename as varchar(50);
select @orderid  = OrderID from ProductOrders where OrderStatus = @status;
select @replacementproductid = ReplacementProductID from ProductOrders where OrderID = @orderid;
select @replacementproductname = ProductName from Products where ProductID = @replacementproductid;
select @diagnostictypeid = DiagnosticTypeID from ProductOrders where OrderID = @orderid;
select @diagnostictypename = TypeName from DiagnosticTypes where TypeID = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetOrderStrict(@productname varchar(50), @typeid int, @replacementproductname varchar(50),  @diagnostictypeid int, @clientname varchar(100), @userdisplayname varchar(100),@quantity int, @price money, @date date, @status int,@look_below_quantity bit, @look_below_price bit, @look_before_date bit)
as
declare @productid as int;
declare @clientid as int; 
declare @userid as int; 
declare @replacementproductid as int;
declare @result as ProductOrdersTable;
select @productid = ProductID from Products where ProductName like '%' + @productname + '%';
select @clientid = ClientID from Clients where ClientName like '%' + @clientname + '%';
select @userid = UserID from Users where UserDisplayName like '%' + @userdisplayname + '%';
select @replacementproductid = ProductID from Products where ProductName like '%' + @replacementproductname + '%';
/* 111 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity <= @quantity and OrderPrice <= @price and (DateAdded <= @date or DateModified <= @date) and OrderStatus = @status;
end
/* 110 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 0
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid  and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity <= @quantity and OrderPrice <= @price and (DateAdded >= @date or DateModified >= @date) and OrderStatus = @status;
end
/* 101 */
if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity <= @quantity and OrderPrice >= @price and (DateAdded <= @date or DateModified <= @date) and OrderStatus = @status;
end
/* 011 */
if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity >= @quantity and OrderPrice <= @price and (DateAdded <= @date or DateModified <= @date) and OrderStatus = @status;
end
/* 100 */
if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 0
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity <= @quantity and OrderPrice >= @price and (DateAdded >= @date or DateModified >= @date) and OrderStatus = @status;
end
/* 010 */
if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 0
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity >= @quantity and OrderPrice <= @price and (DateAdded >= @date or DateModified >= @date) and OrderStatus = @status;
end
/* 001 */
if @look_below_quantity = 0 and @look_below_price = 0 and @look_before_date = 1
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity >= @quantity and OrderPrice >= @price and (DateAdded <= @date or DateModified <= @date) and OrderStatus = @status;
end
/* 000 */
else
begin
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders where ProductID = @productid and OrderTypeID = @typeid and ReplacementProductID = @replacementproductid and DiagnosticTypeID = @diagnostictypeid and ClientID = @clientid and UserID = @userid and DesiredQuantity >= @quantity and OrderPrice >= @price and (DateAdded >= @date or DateModified >= @date) and OrderStatus = @status;
end
select * from @result;
go

go
create or alter procedure GetOrderStrictExtended(@productname varchar(50), @typeid int, @replacementproductname varchar(50), @diagnostictypeid int, @clientname varchar(100), @userdisplayname varchar(100),@quantity int, @price money, @date date, @status int,@look_below_quantity bit, @look_below_price bit, @look_before_date bit)
as
declare @productid as int;
declare @clientid as int; 
declare @userid as int; 
declare @orderid as int;
declare @replacementproductid as int;
declare @diagnostictypename as varchar(50);
declare @result as ProductOrdersExtendedTable;
select @productid = ProductID from Products where ProductName like '%' + @productname + '%';
select @clientid = ClientID from Clients where ClientName like '%' +  @clientname + '%';
select @userid = UserID from Users where UserDisplayName like '%' + @userdisplayname + '%';
select @orderid  = OrderID from ProductOrders where OrderStatus = @status;
select @replacementproductid = ProductID from Products where ProductName = @replacementproductname;
/* 111 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 1
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity <= @quantity and ProductOrders.OrderPrice <= @price and  (ProductOrders.DateAdded <= @date or ProductOrders.DateModified <= @date) and ProductOrders.OrderStatus = @status;
end
/* 110 */
if @look_below_quantity = 1 and @look_below_price = 1 and @look_before_date = 0
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity <= @quantity and ProductOrders.OrderPrice <= @price and  (ProductOrders.DateAdded >= @date or ProductOrders.DateModified >= @date) and ProductOrders.OrderStatus = @status;
end
/* 101 */
if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 1
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity <= @quantity and ProductOrders.OrderPrice >= @price and  (ProductOrders.DateAdded <= @date or ProductOrders.DateModified <= @date) and ProductOrders.OrderStatus = @status;
end
/* 011 */
if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 1
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity >= @quantity and ProductOrders.OrderPrice <= @price and  (ProductOrders.DateAdded <= @date or ProductOrders.DateModified <= @date) and ProductOrders.OrderStatus = @status;
end
/* 100 */
if @look_below_quantity = 1 and @look_below_price = 0 and @look_before_date = 0
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity <= @quantity and ProductOrders.OrderPrice >= @price and  (ProductOrders.DateAdded >= @date or ProductOrders.DateModified >= @date) and ProductOrders.OrderStatus = @status;
end
/* 010 */
if @look_below_quantity = 0 and @look_below_price = 1 and @look_before_date = 0
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity >= @quantity and ProductOrders.OrderPrice <= @price and  (ProductOrders.DateAdded >= @date or ProductOrders.DateModified >= @date) and ProductOrders.OrderStatus = @status;
end
/* 001 */
if @look_below_quantity = 0 and @look_below_price = 0 and @look_before_date = 1
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity >= @quantity and ProductOrders.OrderPrice >= @price and  (ProductOrders.DateAdded <= @date or ProductOrders.DateModified <= @date) and ProductOrders.OrderStatus = @status;
end
/* 000 */
else
begin
select @orderid = ProductOrders.OrderID from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where ProductOrders.ReplacementProductID = Products.ProductID and ProductOrders.OrderTypeID = @typeid and DiagnosticTypeID = @diagnostictypeid and  ProductOrders.ProductID = @productid and ProductOrders.ClientID = @clientid and ProductOrders.UserID = @userid and ProductOrders.DesiredQuantity >= @quantity and ProductOrders.OrderPrice >= @price and  (ProductOrders.DateAdded >= @date or ProductOrders.DateModified >= @date) and ProductOrders.OrderStatus = @status;
end
select @diagnostictypename = TypeName from DiagnosticTypes where @typeid = @diagnostictypeid;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,@diagnostictypename,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID where OrderID = @orderid;
select * from @result;
go

go
create or alter procedure GetAllOrders
as
declare @result as ProductOrdersTable;
insert into @result (OrderID,ProductID,OrderTypeID,ReplacementProductID,DiagnosticTypeID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) select * from ProductOrders;
select * from @result;
go

go
create or alter procedure GetAllOrdersExtended
as
declare @target_replacement_product as int;
declare @replacementproductname as varchar(50);
declare @row_count_replacement as int;
declare @last_row_replacement as int;
declare @replacementproductlist as table(ReplacementCounter int identity primary key not null, ReplacementID int unique not null, ReplacementName varchar(50) not null);
declare @result as ProductOrdersExtendedTable;
insert into @replacementproductlist (ReplacementID,ReplacementName) select ReplacementProductID,ProductName from ProductOrders inner join Products on Products.ProductID = ProductOrders.ReplacementProductID;
select top 1 @row_count_replacement = ReplacementCounter from @replacementproductlist order by ReplacementCounter asc;
select top 1 @last_row_replacement = ReplacementCounter from @replacementproductlist order by ReplacementCounter desc;
while @row_count_replacement <= @last_row_replacement
begin
select @target_replacement_product = ReplacementID from @replacementproductlist where ReplacementCounter = @row_count_replacement order by ReplacementCounter asc;
select @replacementproductname = ReplacementName from @replacementproductlist where ReplacementID = @target_replacement_product;
insert into @result(OrderID,ProductName,ProductDescription,OrderTypeName,ReplacementProductName,DiagnosticTypeName,DesiredQuantity,OrderPrice,ClientName,ClientEmail,ClientPhone,ClientAddress,UserDisplayName,DateAdded,DateModified,OrderStatus) select ProductOrders.OrderID,Products.ProductName,Products.ProductDescription,OrderTypes.TypeName,@replacementproductname,DiagnosticTypes.TypeName,ProductOrders.DesiredQuantity,ProductOrders.OrderPrice,Clients.ClientName,Clients.ClientEmail,Clients.ClientPhone,Clients.ClientAddress,Users.UserDisplayName,ProductOrders.DateAdded,ProductOrders.DateModified,ProductOrders.OrderStatus from ProductOrders inner join Products on ProductOrders.ProductID = Products.ProductID inner join Clients on ProductOrders.ClientID = Clients.ClientID inner join Users on ProductOrders.UserID = Users.UserID inner join OrderTypes on ProductOrders.OrderTypeID = OrderTypes.OrderTypeID inner join DiagnosticTypes on ProductOrders.DiagnosticTypeID = DiagnosticTypes.TypeID;
set @row_count_replacement = @row_count_replacement + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByID(@id int)
as
declare @result as OrderDeliveryTable;
insert into @result(OrderDeliveryID, OrderID, ServiceID, DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select * from OrderDeliveries where OrderDeliveryID = @id;
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByIDExtended(@id int)
as
declare @rowcountdeliveryorder as int;
declare @lastrowdeliveryorder as int;
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @order_delivery_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @deliveryorderid as table(DeliveryOrderCounter int identity primary key not null,OrderDeliveryID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @deliveryorderid(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderDeliveryID = @id;
select top 1 @rowcountdeliveryorder = DeliveryOrderCounter from @deliveryorderid order by DeliveryOrderCounter asc;
select top 1 @lastrowdeliveryorder = DeliveryOrderCounter from @deliveryorderid order by DeliveryOrderCounter desc;
while @rowcountdeliveryorder <= @lastrowdeliveryorder
begin
	select @order_delivery_id = OrderDeliveryID from @deliveryorderid where DeliveryOrderCounter = @rowcountdeliveryorder order by DeliveryOrderCounter asc;
	if @order_delivery_id is not null
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
	end
	set @rowcountdeliveryorder = @rowcountdeliveryorder + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
	end
	set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByProductName(@productname varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
insert into @productid(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
while @rowcountproduct <= @lastrowproduct
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	if @product_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id;
	end
set @rowcountproduct = @rowcountproduct + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByProductNameExtended(@productname varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @productid(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
while @rowcountproduct <= @lastrowproduct
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	if @product_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id;
	end
set @rowcountproduct = @rowcountproduct + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByClientName(@clientname varchar(100))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
insert into @clientid(ClientID) select distinct ClientID from Clients where ClientName like '%' + @clientname + '%';
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
while @rowcountclient <= @lastrowclient
begin
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	if @client_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ClientID = @client_id;
	end
set @rowcountclient = @rowcountclient + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByClientNameExtended(@clientname varchar(100))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @clientid(ClientID) select distinct ClientID from Clients where ClientName like '%' + @clientname + '%';
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
while @rowcountclient <= @lastrowclient
begin
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	if @client_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ClientID = @client_id;
	end
set @rowcountclient = @rowcountclient + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByServiceName(@servicename varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
insert into @serviceid(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName like '%' + @servicename + '%';
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
while @rowcountservice <= @lastrowservice
begin
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	if @service_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where ServiceID = @service_id;
	end
set @rowcountservice = @rowcountservice + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByServiceNameExtended(@servicename varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @serviceid(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName like '%' + @servicename + '%';
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
while @rowcountservice <= @lastrowservice
begin
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	if @service_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where ServiceID = @service_id;
	end
set @rowcountservice = @rowcountservice + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByPaymentMethodName(@paymentmethodname varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName like '%' + @paymentmethodname + '%';
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @payment_method_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where PaymentMethodID = @payment_method_id;
	end
set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByPaymentMethodNameExtended(@paymentmethodname varchar(50))
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName like '%' + @paymentmethodname + '%';
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @payment_method_id is not NULL
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where PaymentMethodID = @payment_method_id;
	end
set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByOrderDate(@date date, @look_before_date bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
if @look_before_date = 1
begin
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where DateAdded <= @date or DateModified <= @date;
end
else
begin
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where DateAdded >= @date or DateModified >= @date;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByDate(@date date, @look_before_date bit)
as
declare @result as OrderDeliveryTable;
if @look_before_date = 1
begin
insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries where DateAdded <= @date or DateModified <= @date;
end
else
begin
insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries where DateAdded >= @date or DateModified >= @date;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByPrice(@price money, @look_below_price bit)
as
declare @result as OrderDeliveryTable;
if @look_below_price = 1
begin
insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries where TotalPayment <= @price;
end
else
begin
insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries where TotalPayment >= @price;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByStatus(@status int)
as
declare @result as OrderDeliveryTable;
insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries where DeliveryStatus = @status;
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByOrderDateExtended(@date date, @look_before_date bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
if @look_before_date = 1
begin
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where DateAdded <= @date or DateModified <= @date;
end
else
begin
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where DateAdded >= @date or DateModified >= @date;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByDateExtended(@date date, @look_before_date bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		if @look_before_date = 1
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and (OrderDeliveries.DateAdded <= @date or OrderDeliveries.DateModified <= @date);
		end
		else
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and (OrderDeliveries.DateAdded >= @date or OrderDeliveries.DateModified >= @date);
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByPriceExtended(@price money, @look_below_price bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		if @look_below_price = 1
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and TotalPayment <= @price;
		end
		else
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and TotalPayment >= @price;
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryByStatusExtended(@status int)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderID = @order_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderID = @order_id;
	end
set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCounter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.DeliveryStatus = @status;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryStrict(@productname varchar(50), @clientname varchar(100), @servicename varchar(50), @paymentmethodname varchar(50),@price money, @order_date date, @delivery_date date,@status int,@look_below_price bit, @look_before_order_date bit, @look_before_delivery_date bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryTable;
insert into @productid(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
insert into @clientid(Clientid) select distinct ClientID from Clients where ClientName like '%' + @clientname + '%';
insert into @serviceid(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName like '%' + @servicename + '%';
insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName like '%' + @paymentmethodname + '%';
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientID asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientID desc;
while @rowcountproduct <= @lastrowproduct or @rowcountclient <= @lastrowclient
begin
	select @product_id = ProductID from @productid where ProductCounter = @lastrowproduct order by ProductCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @lastrowclient order by ClientCounter asc;
	if @product_id is not null and @client_id is not null
	begin
		if @look_before_order_date = 1
		begin
			insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id and ClientID = @client_id and (DateAdded <= @order_date or DateModified <= @order_date);
		end
		else
		begin
			insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id and ClientID = @client_id and (DateAdded >= @order_date or DateModified >= @order_date);
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountclient = @rowcountclient + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		if @look_before_delivery_date = 1 and @look_below_price = 1
		begin
			insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and  OrderDeliveries.TotalPayment <= @price and (OrderDeliveries.DateAdded <= @delivery_date or OrderDeliveries.DateModified <= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else if @look_before_delivery_date = 1 and @look_below_price = 0
		begin
			insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and  OrderDeliveries.TotalPayment >= @price and (OrderDeliveries.DateAdded <= @delivery_date or OrderDeliveries.DateModified <= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else if @look_before_delivery_date = 0 and @look_below_price = 1
		begin
			insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and  OrderDeliveries.TotalPayment <= @price and (OrderDeliveries.DateAdded >= @delivery_date or OrderDeliveries.DateModified >= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else
		begin
			insert into @result(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, OrderDeliveries.OrderID, OrderDeliveries.ServiceID, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, OrderDeliveries.PaymentMethodID, OrderDeliveries.DateAdded, OrderDeliveries.DateModified, OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and  OrderDeliveries.TotalPayment >= @price and (OrderDeliveries.DateAdded >= @delivery_date or OrderDeliveries.DateModified >= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetOrderDeliveryStrictExtended(@productname varchar(50), @clientname varchar(100), @servicename varchar(50), @paymentmethodname varchar(50),@price money, @order_date date, @delivery_date date,@status int,@look_below_price bit, @look_before_order_date bit, @look_before_delivery_date bit)
as
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @productid(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
insert into @clientid(Clientid) select distinct ClientID from Clients where ClientName like '%' + @clientname + '%';
insert into @serviceid(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName like '%' + @servicename + '%';
insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName  like '%' + @paymentmethodname + '%';
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientID asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientID desc;
while @rowcountproduct <= @lastrowproduct or @rowcountclient <= @lastrowclient
begin
	select @product_id = ProductID from @productid where ProductCounter = @lastrowproduct order by ProductCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @lastrowclient order by ClientCounter asc;
	if @product_id is not null and @client_id is not null
	begin
		if @look_before_order_date = 1
		begin
			insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id and ClientID = @client_id and (DateAdded <= @order_date or DateModified <= @order_date);
		end
		else
		begin
			insert into @orderid(OrderID) select distinct OrderID from ProductOrders where ProductID = @product_id and ClientID = @client_id and (DateAdded >= @order_date or DateModified >= @order_date);
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountclient = @rowcountclient + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		if @look_before_delivery_date = 1 and @look_below_price = 1
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and OrderDeliveries.TotalPayment <= @price and (OrderDeliveries.DateAdded <= @delivery_date or OrderDeliveries.DateModified <= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else if @look_before_delivery_date = 1 and @look_below_price = 0
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and OrderDeliveries.TotalPayment >= @price and (OrderDeliveries.DateAdded <= @delivery_date or OrderDeliveries.DateModified <= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else if @look_before_delivery_date = 0 and @look_below_price = 1
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and OrderDeliveries.TotalPayment <= @price and (OrderDeliveries.DateAdded >= @delivery_date or OrderDeliveries.DateModified >= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
		else
		begin
			insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID where OrderDeliveries.OrderID = @order_id and OrderDeliveries.ServiceID = @service_id and OrderDeliveries.PaymentMethodID = @payment_method_id and OrderDeliveries.TotalPayment >= @price and (OrderDeliveries.DateAdded >= @delivery_date or OrderDeliveries.DateModified >= @delivery_date) and OrderDeliveries.DeliveryStatus = @status;
		end
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure GetAllOrderDeliveries
as
declare @result as OrderDeliveryTable;
insert into @result(OrderDeliveryID, OrderID, ServiceID, DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) select * from OrderDeliveries;
select * from @result;
go

go
create or alter procedure GetAllOrderDeliveriesExtended
as
declare @rowcountdeliveryorder as int;
declare @lastrowdeliveryorder as int;
declare @rowcountproduct as int;
declare @lastrowproduct as int;
declare @rowcountorder as int;
declare @lastroworder as int;
declare @rowcountclient as int;
declare @lastrowclient as int;
declare @rowcountservice as int;
declare @lastrowservice as int;
declare @rowcountpaymentmethod as int;
declare @lastrowpaymentmethod as int;
declare @product_id as int;
declare @order_id as int;
declare @client_id as int;
declare @service_id as int;
declare @payment_method_id as int;
declare @order_delivery_id as int;
declare @productid as table(ProductCounter int identity primary key not null, ProductID int unique);
declare @orderid as table(OrderCounter int identity primary key not null,OrderID int unique);
declare @clientid as table(ClientCounter int identity primary key not null, ClientID int unique);
declare @serviceid as table(ServiceCounter int identity primary key not null,ServiceID int unique);
declare @paymentmethodid as table(PaymentMethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @deliveryorderid as table(DeliveryOrderCounter int identity primary key not null,OrderDeliveryID int unique);
declare @result as OrderDeliveryExtendedTable;
insert into @deliveryorderid(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries;
select top 1 @rowcountdeliveryorder = DeliveryOrderCounter from @deliveryorderid order by DeliveryOrderCounter asc;
select top 1 @lastrowdeliveryorder = DeliveryOrderCounter from @deliveryorderid order by DeliveryOrderCounter desc;
while @rowcountdeliveryorder <= @lastrowdeliveryorder
begin
	select @order_delivery_id = OrderDeliveryID from @deliveryorderid where DeliveryOrderCounter = @rowcountdeliveryorder order by DeliveryOrderCounter asc;
	if @order_delivery_id is not null
	begin
		insert into @orderid(OrderID) select distinct OrderID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
		insert into @serviceid(ServiceID) select distinct ServiceID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
		insert into @paymentmethodid(PaymentMethodID) select distinct PaymentMethodID from OrderDeliveries where OrderDeliveryID = @order_delivery_id;
	end
	set @rowcountdeliveryorder = @rowcountdeliveryorder + 1;
end
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
while @rowcountorder <= @lastroworder
begin
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	if @order_id is not null
	begin
		insert into @productid(ProductID) select distinct ProductID from ProductOrders where OrderID = @order_id;
		insert into @clientid(ClientID) select distinct ClientID from ProductOrders where OrderID = @order_id;
	end
	set @rowcountorder = @rowcountorder + 1;
end
select top 1 @rowcountproduct = ProductCounter from @productid order by ProductCounter asc;
select top 1 @lastrowproduct = ProductCounter from @productid order by ProductCounter desc;
select top 1 @rowcountorder = OrderCounter from @orderid order by OrderCounter asc;
select top 1 @lastroworder = OrderCounter from @orderid order by OrderCounter desc;
select top 1 @rowcountclient = ClientCounter from @clientid order by ClientCounter asc;
select top 1 @lastrowclient = ClientCounter from @clientid order by ClientCOunter desc;
select top 1 @rowcountservice = ServiceCounter from @serviceid order by ServiceCounter asc;
select top 1 @lastrowservice = ServiceCounter from @serviceid order by ServiceCounter desc;
select top 1 @rowcountpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter asc;
select top 1 @lastrowpaymentmethod = PaymentMethodCounter from @paymentmethodid order by PaymentMethodCounter desc;
while @rowcountproduct <= @lastrowproduct or @rowcountorder <= @lastroworder or @rowcountclient <= @lastrowclient or @rowcountservice <= @lastrowservice or @rowcountpaymentmethod <= @lastrowpaymentmethod
begin
	select @product_id = ProductID from @productid where ProductCounter = @rowcountproduct order by ProductCounter asc;
	select @order_id = OrderID from @orderid where OrderCounter = @rowcountorder order by OrderCounter asc;
	select @client_id = ClientID from @clientid where ClientCounter = @rowcountclient order by ClientCounter asc;
	select @service_id = ServiceID from @serviceid where ServiceCounter = @rowcountservice order by ServiceCounter asc;
	select @payment_method_id = PaymentMethodID from @paymentmethodid where PaymentMethodCounter = @rowcountpaymentmethod order by PaymentMethodCounter asc;
	if @product_id is not null and @order_id is not null and @client_id is not null and @service_id is not null and @payment_method_id is not null
	begin
		insert into @result(OrderDeliveryID,ProductName,ProductDescription,DesiredQuantity,OrderPrice,ClientName,ServiceName,ServicePrice,DeliveryCargoID,TotalPayment,PaymentMethodName,DateAdded,DateModified,DeliveryStatus) select OrderDeliveries.OrderDeliveryID, Products.ProductName, Products.ProductDescription, ProductOrders.DesiredQuantity, ProductOrders.OrderPrice, Clients.ClientName, DeliveryServices.ServiceName, DeliveryServices.ServicePrice, OrderDeliveries.DeliveryCargoID, OrderDeliveries.TotalPayment, PaymentMethods.PaymentMethodName,OrderDeliveries.DateAdded,OrderDeliveries.DateModified,OrderDeliveries.DeliveryStatus from OrderDeliveries inner join Products on Products.ProductID = @product_id inner join ProductOrders on ProductOrders.OrderID = OrderDeliveries.OrderID inner join Clients on Clients.ClientID = @client_id inner join DeliveryServices on OrderDeliveries.ServiceID = DeliveryServices.ServiceID inner join PaymentMethods on PaymentMethods.PaymentMethodID = OrderDeliveries.PaymentMethodID;
	end
	set @rowcountproduct = @rowcountproduct + 1;
	set @rowcountorder = @rowcountorder + 1;
	set @rowcountclient = @rowcountclient + 1;
	set @rowcountservice = @rowcountservice + 1;
	set @rowcountpaymentmethod = @rowcountpaymentmethod + 1;
end
select * from @result;
go

go
create or alter procedure UpdateUserByID(@id int, @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50),@new_balance money, @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = try_convert(varbinary(max), '');
select @target_user = UserID from Users where UserID = @id;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
print @new_phone;
print @old_phone;
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone,UserBalance = @new_balance, UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByUserName(@username varchar(100), @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50),@new_balance money, @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = try_convert(varbinary(max), '');
select @target_user = UserID from Users where UserName = @username;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone, UserBalance = @new_balance ,UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByDisplayName(@userdisplayname varchar(100), @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50), @new_balance money, @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max), '');
select @target_user = UserID from Users where UserDisplayName = @userdisplayname;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone, UserBalance = @new_balance ,UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByEmail(@email varchar(200), @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50), @new_balance money, @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max), '');
select @target_user = UserID from Users where UserEmail = @email;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone, UserBalance = @new_balance , UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByPhone(@phone varchar(50), @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50), @new_balance money , @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max), '');
select @target_user = UserID from Users where UserPhone = @phone;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone,UserBalance = @new_balance, UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByBalance(@balance money, @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50), @new_balance money , @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max), '');
select @target_user = UserID from Users where UserBalance = @balance;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone,UserBalance = @new_balance, UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go

go
create or alter procedure UpdateUserByDate(@date date, @new_user_name varchar(100), @new_display_name varchar(100), @new_email varchar(200), @new_password varchar(500), @new_phone varchar(50), @new_balance money, @new_profile_pic varbinary(max), @new_is_admin bit, @new_is_worker bit, @new_is_client bit)
as
declare @target_user as int;
declare @old_user_name as varchar(100);
declare @old_display_name as varchar(100);
declare @old_email as varchar(200);
declare @old_password as varchar(500);
declare @old_phone as varchar(50);
declare @old_balance as money;
declare @old_profile_pic as varbinary(max);
declare @old_is_admin as bit;
declare @old_is_worker as bit;
declare @old_is_client as bit;
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max), '');
select @target_user = UserID from Users where DateOfRegister = @date;
select @old_user_name = UserName from Users where UserID = @target_user;
select @old_display_name = UserDisplayName from Users where UserID = @target_user;
select @old_email = UserEmail from Users where UserID = @target_user;
select @old_password = UserPassword from Users where UserID = @target_user;
select @old_phone = UserPhone from Users where UserID = @target_user;
select @old_balance = UserBalance from Users where UserID = @target_user;
select @old_profile_pic = UserProfilePic from Users where UserID = @target_user;
select @old_is_admin = isAdmin from Users where UserID = @target_user;
select @old_is_worker = isWorker from Users where UserID = @target_user;
select @old_is_client = isClient from Users where UserID = @target_user;
if  @new_user_name = @old_user_name or @new_user_name = '' or @new_user_name is null
begin
set @new_user_name = @old_user_name;
end
if @new_display_name = @old_display_name or @new_display_name = '' or @new_display_name is null
begin
set @new_display_name = @old_display_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_password = @old_password or @new_password = '' or @new_password is null
begin
set @new_password = @old_password;
end
if @new_phone = @old_phone or @new_phone = '' or @new_password is null
begin
set @new_phone = @old_phone;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
if @new_is_admin = @old_is_admin or @new_is_admin is null
begin
set @new_is_admin = @old_is_admin;
end
if @new_is_worker = @old_is_worker or @new_is_worker is null
begin
set @new_is_worker = @old_is_worker;
end
if @new_is_client = @old_is_client or @new_is_client is null
begin
set @new_is_client = @old_is_client;
end
update Users set UserName = @new_user_name, UserDisplayName = @new_display_name, UserEmail = @new_email, UserPassword = @new_password, UserPhone = @new_phone, UserBalance = @new_balance, UserProfilePic = @new_profile_pic, isAdmin = @new_is_admin, isWorker = @new_is_worker, isClient = @new_is_client where UserID = @target_user;
go


go
create or alter procedure UpdateClientByID(@id int, @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientID = @id;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance, ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go

go
create or alter procedure UpdateClientByName(@name varchar(100), @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientName = @name;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance ,ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go

go
create or alter procedure UpdateClientByEmail(@email varchar(100), @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientEmail = @email;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance, ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go


go
create or alter procedure UpdateClientByPhone(@phone varchar(50), @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientPhone = @phone;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance, ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go

go
create or alter procedure UpdateClientByAddress(@address varchar(100), @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientAddress = @address;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance, ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go

go
create or alter procedure UpdateClientByBalance(@balance money, @new_name varchar(100), @new_email varchar(100), @new_phone varchar(50), @new_address varchar(100), @new_balance money, @new_profile_pic varbinary(max))
as
declare @target_client as int;
declare @old_name varchar(100);
declare @old_email varchar(100);
declare @old_phone varchar(50);
declare @old_address varchar(100);
declare @old_balance money;
declare @old_profile_pic varbinary(max);
declare @blankvalue as varbinary(max);
select @blankvalue = convert(varbinary(max),'');
select @target_client = ClientID from Clients where ClientBalance = @balance;
select @old_name = ClientName from Clients where ClientID = @target_client;
select @old_email = ClientEmail from Clients where ClientID = @target_client;
select @old_phone = ClientPhone from Clients where ClientID = @target_client;
select @old_address = ClientAddress from Clients where ClientID = @target_client;
select @old_balance = ClientBalance from Clients where ClientID = @target_client;
select @old_profile_pic = ClientProfilePic from CLients where ClientID = @target_client;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_email = @old_email or @new_email = '' or @new_email is null
begin
set @new_email = @old_email;
end
if @new_phone = @old_phone or @new_phone = '' or @new_phone is null
begin
set @new_phone = @old_phone;
end
if @new_address = @old_address or @new_address = '' or @new_address is null
begin
set @new_address = @old_address;
end
if @new_balance = @old_balance or @new_balance is null
begin
set @new_balance = @old_balance;
end
if @new_profile_pic = @old_profile_pic or @new_profile_pic = @blankvalue or @new_profile_pic is null
begin
set @new_profile_pic = @old_profile_pic;
end
update Clients set ClientName = @new_name, ClientEmail = @new_email, ClientPhone = @new_phone, ClientAddress = @new_address, ClientBalance = @new_balance, ClientProfilePic = @new_profile_pic where ClientID = @target_client;
go

go
create or alter procedure UpdateProductCategoryByID(@id int, @new_category_name varchar(200))
as
declare @target_category as int;
declare @old_category_name as varchar(200);
select @target_category = CategoryID from ProductCategories where CategoryID = @id;
select @old_category_name = CategoryName from ProductCategories where CategoryID = @target_category;
if @new_category_name = @old_category_name or @new_category_name = '' or @new_category_name is null
begin
set @new_category_name = @old_category_name;
end
update ProductCategories set CategoryName = @new_category_name where CategoryID = @target_category;
go

go
create or alter procedure UpdateProductCategoryByName(@name varchar(200), @new_category_name varchar(200))
as
declare @target_category as int;
declare @old_category_name as varchar(200);
select @target_category = CategoryID from ProductCategories where CategoryName = @name;
select @old_category_name = CategoryName from ProductCategories where CategoryID = @target_category;
if @new_category_name = @old_category_name or @new_category_name = '' or @new_category_name is null
begin
set @new_category_name = @old_category_name;
end
update ProductCategories set CategoryName = @new_category_name where CategoryID = @target_category;
go

go
create or alter procedure UpdateProductBrandByID(@id int, @new_brand_name varchar(200))
as
declare @target_brand as int;
declare @old_brand_name as varchar(200);
select @target_brand = BrandID from ProductBrands where BrandID = @id;
select @old_brand_name = BrandName from ProductBrands where BrandID = @target_brand;
if @new_brand_name = @old_brand_name or @new_brand_name = '' or @new_brand_name is null
begin
set @new_brand_name = @old_brand_name;
end
update ProductBrands set BrandName = @new_brand_name where BrandID = @target_brand;
go

go
create or alter procedure UpdateProductBrandByName(@name varchar(200), @new_brand_name varchar(200))
as
declare @target_brand as int;
declare @old_brand_name as varchar(200);
select @target_brand = BrandID from ProductBrands where BrandName = @name;
select @old_brand_name = BrandName from ProductBrands where BrandID = @target_brand;
if @new_brand_name = @old_brand_name or @new_brand_name = '' or @new_brand_name is null
begin
set @new_brand_name = @old_brand_name;
end
update ProductBrands set BrandName = @new_brand_name where BrandID = @target_brand;
go

go
create or alter procedure UpdateOrderTypeByID(@id int, @new_type_name varchar(200))
as
declare @target_type as int;
declare @old_type_name as varchar(200);
select @target_type = OrderTypeID from OrderTypes where OrderTypeID = @id;
select @old_type_name = TypeName from OrderTypes where OrderTypeID = @target_type;
if @new_type_name = @old_type_name or @new_type_name = '' or @new_type_name is null
begin
set @new_type_name = @old_type_name;
end
update OrderTypes set TypeName = @new_type_name where OrderTypeID = @target_type;
go

go
create or alter procedure UpdateOrderTypeByName(@name varchar(200), @new_type_name varchar(200))
as
declare @target_type as int;
declare @old_type_name as varchar(200);
select @target_type = OrderTypeID from OrderTypes where TypeName = @name;
select @old_type_name = TypeName from OrderTypes where OrderTypeID = @target_type;
if @new_type_name = @old_type_name or @new_type_name = '' or @new_type_name is null
begin
set @new_type_name = @old_type_name;
end
update OrderTypes set TypeName = @new_type_name where OrderTypeID = @target_type;
go


go
create or alter procedure UpdatePaymentMethodByID(@id int, @new_method_name varchar(50))
as
declare @target_method as int;
declare @old_method_name as varchar(50);
select @target_method = PaymentMethodID from PaymentMethods where PaymentMethodID = @id;
select @old_method_name = PaymentMethodName from PaymentMethods where PaymentMethodID = @target_method;
if @new_method_name = @old_method_name or @new_method_name = '' or @new_method_name is null
begin
set @new_method_name = @old_method_name;
end
update PaymentMethods set PaymentMethodName = @new_method_name where PaymentMethodID = @target_method;
go

go
create or alter procedure UpdatePaymentMethodByName(@name varchar(50), @new_method_name varchar(50))
as
declare @target_method as int;
declare @old_method_name as varchar(50);
select @target_method = PaymentMethodID from PaymentMethods where PaymentMethodName = @name;
select @old_method_name = PaymentMethodName from PaymentMethods where PaymentMethodID = @target_method;
if @new_method_name = @old_method_name or @new_method_name = '' or @new_method_name is null
begin
set @new_method_name = @old_method_name;
end
update PaymentMethods set PaymentMethodName = @new_method_name where PaymentMethodID = @target_method;
go

go
create or alter procedure UpdateDeliveryServiceByID(@id int, @new_name varchar(50), @new_price money)
as
declare @target_service as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_service = ServiceID from DeliveryServices where ServiceID = @id;
select @old_name = ServiceName from DeliveryServices where ServiceID = @target_service;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_service;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DeliveryServices set ServiceName = @new_name, ServicePrice = @new_price where ServiceID = @target_service;
go

go
create or alter procedure UpdateDeliveryServiceByName(@name varchar(50), @new_name varchar(50), @new_price money)
as
declare @target_service as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_service = ServiceID from DeliveryServices where ServiceName = @name;
select @old_name = ServiceName from DeliveryServices where ServiceID = @target_service;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_service;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DeliveryServices set ServiceName = @new_name, ServicePrice = @new_price where ServiceID = @target_service;
go

go
create or alter procedure UpdateDeliveryServiceByPrice(@price money, @new_name varchar(50), @new_price money)
as
declare @target_service as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_service = ServiceID from DeliveryServices where ServicePrice = @price;
select @old_name = ServiceName from DeliveryServices where ServiceID = @target_service;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_service;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DeliveryServices set ServiceName = @new_name, ServicePrice = @new_price where ServiceID = @target_service;
go

go
create or alter procedure UpdateDiagnosticTypeByID(@id int, @new_name varchar(50), @new_price money)
as
declare @target_type as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_type = TypeID from DiagnosticTypes where TypeID = @id;
select @old_name = TypeName from DiagnosticTypes where TypeID = @target_type;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_type;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DiagnosticTypes set TypeName = @new_name, TypePrice = @new_price where TypeID = @target_type;
go

go
create or alter procedure UpdateDiagnosticTypeByName(@name varchar(50), @new_name varchar(50), @new_price money)
as
declare @target_type as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_type = TypeID from DiagnosticTypes where TypeName = @name;
select @old_name = TypeName from DiagnosticTypes where TypeID = @target_type;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_type;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DiagnosticTypes set TypeName = @new_name, TypePrice = @new_price where TypeID = @target_type;
go

go
create or alter procedure UpdateDiagnosticTypeByPrice(@price money, @new_name varchar(50), @new_price money)
as
declare @target_type as int;
declare @old_name as varchar(50);
declare @old_price as money;
select @target_type = TypeID from DiagnosticTypes where TypePrice = @price;
select @old_name = TypeName from DiagnosticTypes where TypeID = @target_type;
select @old_price = ServicePrice from DeliveryServices where ServiceID = @target_type;
if @new_name = @old_name or @new_name = '' or @new_name is null
begin
set @new_name = @old_name;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
update DiagnosticTypes set TypeName = @new_name, TypePrice = @new_price where TypeID = @target_type;
go

go
create or alter procedure UpdateProductByID(@id int, @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductID = @id;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByCategory(@categoryname varchar(200), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_categories table(CategoryCounter int identity primary key not null, CategoryID int unique not null);
declare @row_counter_category int;
declare @last_row_category int;
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @row_counter int;
declare @last_row int;
declare @target_product int;
declare @target_category int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
insert into @target_categories(CategoryID) select distinct CategoryID from ProductCategories where CategoryName = @categoryname;
select top 1 @row_counter_category = CategoryCounter from @target_categories order by CategoryCounter asc;
select top 1 @last_row_category = CategoryCounter from @target_categories order by CategoryCounter desc;
while @row_counter_category <= @last_row_category
begin
select @target_category = CategoryID from @target_categories where CategoryCounter = @row_counter_category order by CategoryCounter asc;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductCategoryID = @target_category;
set @row_counter_category = @row_counter_category + 1;
end
select top 1 @row_counter = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_counter <= @last_row
begin
select @target_product = ProductID from @target_products where ProductCounter = @row_counter order by ProductCounter asc;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateProductByBrand(@brandname varchar(100), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_brands table(BrandCounter int identity primary key not null, BrandID int unique not null);
declare @row_counter_brand int;
declare @last_row_brand int;
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @row_counter int;
declare @last_row int;
declare @target_product int;
declare @target_brand int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
insert into @target_brands(BrandID) select distinct BrandID from ProductBrands where BrandName = @brandname;
select top 1 @row_counter_brand = BrandCounter from @target_brands order by BrandCounter asc;
select top 1 @last_row_brand = BrandCounter from @target_brands order by BrandCounter desc;
while @row_counter_brand <= @last_row_brand
begin
select @target_brand = BrandID from @target_brands where BrandCounter = @row_counter_brand order by BrandCounter asc;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductBrandID = @target_brand;
set @row_counter_brand = @row_counter_brand + 1;
end
select top 1 @row_counter = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_counter <= @last_row
begin
select @target_product = ProductID from @target_products where ProductCounter = @row_counter order by ProductCounter asc;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateProductByName(@name varchar(50), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductName = @name;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByDescription(@description varchar(max), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductDescription = @description;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByArtID(@artid varchar(max), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductArtID = @artid;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductBySerialNumber(@serialnumber varchar(max), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductSerialNumber = @serialnumber;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByStorageLocation(@location varchar(max), @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where ProductStorageLocation = @location;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByQuantity(@quantity int, @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where Quantity = @quantity;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductByPrice(@price money, @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where Price = @price;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go


go
create or alter procedure UpdateProductByDate(@date date, @new_category int, @new_brand int, @new_name varchar(50), @new_description varchar(max), @new_quantity int, @new_price money, @new_art_id varchar(max), @new_serial_number varchar(max), @new_storage_location varchar(max))
as
declare @target_product int;
declare @old_category int;
declare @old_brand int;
declare @old_name varchar(50);
declare @old_description varchar(max);
declare @old_quantity int;
declare @old_price int;
declare @old_art_id varchar(max);
declare @old_serial_number varchar(max);
declare @old_storage_location varchar(max);
select @target_product = ProductID from Products where DateAdded = @date or DateModified = @date;
select @old_category = ProductCategoryID from Products where ProductID = @target_product;
select @old_brand = ProductBrandID from Products where ProductID = @target_product;
select @old_name = ProductName from Products where ProductID = @target_product;
select @old_description = ProductDescription from Products where ProductID = @target_product;
select @old_quantity = Quantity from Products where ProductID = @target_product;
select @old_price = Price from Products where ProductID = @target_product;
select @old_art_id = ProductArtID from Products where ProductID = @target_product;
select @old_serial_number = ProductSerialNumber from Products where ProductID = @target_product;
select @old_storage_location = ProductStorageLocation from Products where ProductID = @target_product;
if not exists(select CategoryID from ProductCategories where CategoryID = @new_category) or @new_category is null
begin
set @new_category = @old_category;
end
if not exists(select BrandID from ProductBrands where BrandID = @new_brand) or @new_brand is null
begin
set @new_brand = @old_brand;
end
if @new_name = @old_name or @new_name = '' or @old_name is null
begin
set @new_name = @old_name;
end
if @new_description = @old_description or @new_description = '' or @new_description is null
begin
set @new_description = @old_description;
end
if @new_quantity = @old_quantity or @new_quantity is null
begin
set @new_quantity = @old_quantity;
end
if @new_price = @old_price or @new_price is null
begin
set @new_price = @old_price;
end
if @new_art_id = @old_art_id or @new_art_id = '' or @new_art_id is null
begin
set @new_art_id = @old_art_id;
end
if @new_serial_number = @old_serial_number or @new_serial_number = '' or @new_art_id is null
begin
set @new_serial_number = @old_serial_number;
end
if @new_storage_location = @old_storage_location or @new_storage_location = '' or @new_storage_location is null
begin
set @new_storage_location = @old_storage_location;
end
update Products set ProductCategoryID = @new_category, ProductBrandID = @new_brand, ProductName = @new_name, ProductDescription = @new_description, Quantity = @new_quantity, Price = @new_price, ProductArtID = @new_art_id, ProductSerialNumber = @new_serial_number, ProductStorageLocation = @new_storage_location, DateModified = getdate() where ProductID = @target_product;
go

go
create or alter procedure UpdateProductImage(@image_name varchar(max), @new_image_name varchar(max), @updated_image varbinary(max))
as
declare @old_image_name varchar(max);
declare @current_image varbinary(max);
select @old_image_name = ImageName from ProductImages where ImageName = @image_name;
select @current_image = Picture from ProductImages where ImageName = @image_name;
if @new_image_name = @old_image_name or  @new_image_name = '' or @new_image_name is null
begin
set @new_image_name = @old_image_name;
end
if @updated_image = @current_image or @updated_image is null
begin
set @updated_image = @current_image;
end
update ProductImages set ImageName = @new_image_name,Picture = @updated_image where ImageName = @image_name;
go

go
create or alter procedure UpdateOrderByID(@id int, @new_product_id int,@new_order_type int,@new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where OrderID = @id;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByTypeID(@typeid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where OrderTypeID = @typeid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByDiagnosticTypeID(@diagnostictypeid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where DiagnosticTypeID = @diagnostictypeid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderByProductID(@productid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_desired_quantity int, @new_diagnostic_type int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ProductID = @productid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderByProductName(@productname varchar(100), @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_product int;
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
select @target_product = ProductID from Products where ProductName = @productname;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ProductID = @target_product;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByUserID(@userid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where UserID = @userid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderByUserDisplayName(@userdisplayname varchar(100), @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_user int;
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
select @target_user = UserID from Users where UserDisplayName = @userdisplayname;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where UserID = @target_user;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByReplacementProductID(@productid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ReplacementProductID = @productid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderByReplacementProductName(@productname varchar(100), @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_product int;
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
select @target_product = ProductID from Products where ProductName = @productname;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ReplacementProductID = @target_product;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderByClientID(@clientid int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_desired_quantity int,@new_diagnostic_type int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ClientID = @clientid;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByClientName(@clientname varchar(100), @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_client int;
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
select @target_client = ClientID from Clients where ClientName = @clientname;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where ClientID = @target_client;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByDesiredQuantity(@quantity int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where DesiredQuantity = @quantity;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByPrice(@price money, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where OrderPrice = @price;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByDate(@date date, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int, @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where DateAdded = @date or  DateModified = @date;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderByStatus(@status int, @new_product_id int, @new_order_type int, @new_replacement_product_id int, @new_diagnostic_type int , @new_desired_quantity int, @new_order_price money, @new_client_id int, @new_user_id int, @new_order_status int, @new_price_is_total bit)
as
declare @target_orders table(OrderCounter int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_counter int;
declare @last_row int;
declare @old_product_id int;
declare @old_order_type int;
declare @old_replacement_product_id int;
declare @old_diagnostic_type int;
declare @old_desired_quantity int;
declare @old_order_price int;
declare @old_client_id int;
declare @old_user_id int;
declare @old_order_status int;
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where OrderStatus = @status;
select top 1 @row_counter = OrderCounter from @target_orders order by OrderCounter asc;
select top 1 @last_row = OrderCounter from @target_orders order by OrderCounter desc;
while @row_counter <= @last_row
begin
select @target_order = OrderID from @target_orders where OrderCounter = @row_counter order by OrderCounter asc;
select @old_product_id = ProductID from ProductOrders where OrderID = @target_order;
select @old_order_type = OrderTypeID from ProductOrders where OrderID = @target_order;
select @old_replacement_product_id = ReplacementProductID from ProductOrders where OrderID = @target_order;
select @old_diagnostic_type = DiagnosticTypeID from ProductOrders where OrderID = @target_order;
select @old_desired_quantity = DesiredQuantity from ProductOrders where OrderID = @target_order;
select @old_order_price = OrderPrice from ProductOrders where OrderID = @target_order;
select @old_client_id = ClientID from ProductOrders where OrderID = @target_order;
select @old_user_id = UserID from ProductOrders where OrderID = @target_order;
select @old_order_status = OrderStatus from ProductOrders where OrderID = @target_order;
if @new_product_id  = @old_product_id or @new_product_id is null
begin
set @new_product_id = @old_product_id;
end
if @new_order_type  = @old_order_type or @new_order_type is null
begin
set @new_order_type = @old_order_type;
end
if @new_replacement_product_id  = @old_replacement_product_id or @new_replacement_product_id is null
begin
set @new_replacement_product_id = @old_replacement_product_id;
end
if @new_diagnostic_type = @old_diagnostic_type or @new_diagnostic_type is null
begin
set @new_diagnostic_type = @old_diagnostic_type;
end
if @new_desired_quantity = @old_desired_quantity or @new_product_id is null
begin
set @new_desired_quantity = @old_desired_quantity;
end
if @new_order_price = @old_order_price or @new_order_price is null
begin
set @new_order_price = @old_order_price;
end
else
begin
if @new_price_is_total = 1
begin
set @new_order_price = @new_order_price;
end
else
begin
set @new_order_price = @new_order_price * @new_desired_quantity ;
end
end
if @new_client_id = @old_client_id or @new_client_id = null
begin
set @new_client_id = @old_client_id;
end
if @new_user_id = @old_user_id or @new_user_id is null
begin
set @new_user_id = @old_user_id;
end
if @new_order_status = @old_order_status or @new_order_status is null
begin
set @new_order_status = @old_order_status;
end
update ProductOrders set ProductID = @new_product_id, OrderTypeID = @new_order_type, ReplacementProductID = @new_replacement_product_id,DiagnosticTypeID = @new_diagnostic_type, DesiredQuantity = @new_desired_quantity, OrderPrice = @new_order_price, ClientID = @new_client_id, UserID = @new_user_id, DateModified = getdate(), OrderStatus = @new_order_status where OrderID = @target_order;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByID(@id int, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderDeliveryID = @id;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByOrderID(@orderid int, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderID = @orderid;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByDeliveryServiceID(@deliveryserviceid int, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where ServiceID = @deliveryserviceid;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByDeliveryServiceName(@deliveryservicename varchar(50), @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_delivery_services table(ServiceCounter int identity primary key not null, ServiceID int unique not null);
declare @row_counter_service int;
declare @last_row_service int;
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_delivery_service int;
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_delivery_services(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName = @deliveryservicename;
select top 1 @row_counter_service = ServiceCounter from @target_delivery_services order by ServiceCounter asc;
select top 1 @last_row_service = ServiceCounter from @target_delivery_services order by ServiceCounter desc;
while @row_counter_service <= @last_row_service
begin
select @target_delivery_service = ServiceID from @target_delivery_services where ServiceCounter = @row_counter_service order by ServiceCounter asc;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where ServiceID = @target_delivery_service;
set @row_counter_service = @row_counter_service + 1
end
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByDeliveryCargoID(@deliverycargoid varchar(max), @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DeliveryCargoID = @deliverycargoid;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByPrice(@price money, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where TotalPayment = @price;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go


go
create or alter procedure UpdateOrderDeliveryByPaymentMethodID(@paymentmethodid int, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where PaymentMethodID = @paymentmethodid;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByPaymentMethodName(@paymentmethodname varchar(50), @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_payment_methods table(MethodCounter int identity primary key not null, PaymentMethodID int unique);
declare @row_counter_method int;
declare @last_row_method int;
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_payment_method int;
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_payment_methods(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName =  @paymentmethodname;
select top 1 @row_counter_method = MethodCounter from @target_payment_methods order by MethodCounter asc;
select top 1 @last_row_method = MethodCounter from @target_payment_methods order by MethodCounter desc;
while @row_counter_method <= @last_row_method
begin
select @target_payment_method = PaymentMethodID from @target_payment_methods where MethodCounter = @row_counter_method order by MethodCounter asc;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where PaymentMethodID = @target_payment_method;
set @row_counter_method = @row_counter_method + 1;
end
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByStatus(@status int, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DeliveryStatus = @status;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure UpdateOrderDeliveryByDate(@date date, @new_order int, @new_service int, @new_delivery_cargo_id varchar(max), @new_payment_method int, @new_status int)
as
declare @target_order_deliveries table(OrderDeliveryCounter int identity primary key not null, OrderDeliveryID int unique not null);
declare @target_order_delivery int;
declare @row_counter int;
declare @last_row int;
declare @old_order int;
declare @old_service int;
declare @old_delivery_cargo_id varchar(max);
declare @old_payment_method int;
declare @old_status int;
declare @new_order_price money;
declare @new_delivery_price money;
declare @new_total_payment money;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DateAdded = @date or DateModified = @date;
select top 1 @row_counter = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter asc;
select top 1 @last_row = OrderDeliveryCounter from @target_order_deliveries order by OrderDeliveryCounter desc;
while @row_counter <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where OrderDeliveryCounter = @row_counter order by OrderDeliveryCounter asc;
select @old_order = OrderID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_service = ServiceID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_delivery_cargo_id = DeliveryCargoID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_payment_method = PaymentMethodID from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
select @old_status = DeliveryStatus from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
if @new_order = @old_order or @new_order is null
begin
set @new_order = @old_order;
end
if @new_service = @old_service or @new_service is null
begin
set @new_service = @old_service;
end
if @new_delivery_cargo_id = @old_delivery_cargo_id or @new_delivery_cargo_id = '' or @new_delivery_cargo_id is null
begin
set @new_delivery_cargo_id = @old_delivery_cargo_id;
end
if @new_payment_method = @old_payment_method or @new_payment_method is null
begin
set @new_payment_method = @old_payment_method;
end
if @new_status = @old_status or @new_status is null
begin
set @new_status = @old_status;
end
select @new_order_price = OrderPrice from ProductOrders where OrderID = @new_order;
select @new_delivery_price = ServicePrice from DeliveryServices where ServiceID = @new_service;
set @new_total_payment = @new_order_price + @new_delivery_price;
update OrderDeliveries set OrderID = @new_order, ServiceID = @new_service, DeliveryCargoID = @new_delivery_cargo_id, TotalPayment = @new_total_payment, DateModified = getdate(), DeliveryStatus = @new_status where OrderDeliveryID = @target_order_delivery;
set @row_counter = @row_counter + 1;
end
go

go
create or alter procedure DeleteUserByID(@id int)
as
delete from Users where UserID = @id;
go

go
create or alter procedure DeleteUserByUserName(@name varchar(100))
as
delete from Users where UserName like '%' + @name + '%';
go

go
create or alter procedure DeleteUserByDisplayName(@displayname varchar(100))
as
delete from Users where UserDisplayName like '%' + @displayname + '%';
go

go
create or alter procedure DeleteUserByEmail(@email varchar(200))
as
delete from Users where UserEmail like '%' + @email + '%';
go

go
create or alter procedure DeleteUserByPhone(@phone varchar(50))
as
delete from Users where UserPhone like '%' + @phone + '%';
go

go 
create or alter procedure DeleteUserByBalance(@balance money, @also_delete_below_that_balance bit, @also_delete_above_that_balance bit)
as
if @also_delete_above_that_balance = 1
begin
delete from Users where UserBalance >= @balance;
end
else if @also_delete_below_that_balance = 1
begin
delete from Users where UserBalance <= @balance;
end
else
begin
delete from Users where UserBalance = @balance;
end
go

go 
create or alter procedure DeleteUserByDate(@date date, @also_delete_before_that_date bit, @also_delete_after_that_date bit)
as
if @also_delete_before_that_date = 1
begin
delete from Users where DateOfRegister >= @date;
end
else if @also_delete_after_that_date = 1
begin
delete from Users where DateOfRegister <= @date;
end
else
begin
delete from Users where DateOfRegister = @date;
end
go

go
create or alter procedure DeleteAllUsers
as
delete from Users;
go


go
create or alter procedure DeleteClientByID(@id int)
as
delete from Clients where ClientID = @id;
go

go 
create or alter procedure DeleteClientByUserID(@userid int)
as
delete from Clients where UserID = @userid;
go

go
create or alter procedure DeleteClientByUserName(@username varchar(100), @delete_user_as_well bit)
as
declare @target_user_id int;
select @target_user_id = UserID from Users where UserName like '%' + @username + '%';
if @delete_user_as_well = 1
begin
delete from Users where UserID = @target_user_id;
delete from Clients where UserID = @target_user_id;
end
else
begin
delete from Clients where UserID = @target_user_id;
end
go

go
create or alter procedure DeleteClientByName(@name varchar(100))
as
delete from Clients where ClientName like '%' + @name + '%';
go

go
create or alter procedure DeleteClientByEmail(@email varchar(100))
as
delete from Clients where ClientEmail like '%' + @email + '%';
go

go
create or alter procedure DeleteClientByPhone(@phone varchar(50))
as
delete from Clients where ClientPhone like '%' + @phone + '%';
go

go
create or alter procedure DeleteClientByAddress(@address varchar(100))
as
delete from Clients where ClientAddress like '%' + @address + '%';
go

go
create or alter procedure DeleteClientByBalance(@balance money, @also_delete_below_balance bit, @also_delete_above_balance bit)
as
if @also_delete_below_balance = 1
begin
delete from Clients where ClientBalance <= @balance;
end
else if @also_delete_above_balance = 1
begin
delete from Clients where ClientBalance >= @balance;
end
else
begin
delete from Clients where ClientBalance = @balance;
end
go

go
create or alter procedure DeleteAllClients
as
delete from Clients;
go


go
create or alter procedure DeleteProductCategoryByID(@id int)
as
delete from ProductCategories where CategoryID = @id;
go

go
create or alter procedure DeleteProductCategoryByName(@name varchar(200))
as
delete from ProductCategories where CategoryName like '%' + @name + '%';
go

go
create or alter procedure DeleteAllProductCategories
as
delete from ProductCategories;
go

go
create or alter procedure DeleteOrderTypeByID(@id int)
as
delete from OrderTypes where OrderTypeID = @id;
go

go
create or alter procedure DeleteOrderTypeByName(@name varchar(200))
as
delete from OrderTypes where TypeName like '%' + @name + '%';
go

go
create or alter procedure DeleteAllOrderTypes
as
delete from OrderTypes;
go

go
create or alter procedure DeleteProductBrandByID(@id int)
as
delete from ProductBrands where BrandID = @id;
go

go
create or alter procedure DeleteProductBrandByName(@name varchar(100))
as
delete from ProductBrands where BrandName like '%' + @name + '%';
go

go
create or alter procedure DeleteAllProductBrands
as
delete from ProductBrands;
go

go
create or alter procedure DeleteDeliveryServiceByID(@id int)
as
delete from DeliveryServices where ServiceID = @id;
go

go
create or alter procedure DeleteDeliveryServiceByName(@name varchar(50))
as
delete from DeliveryServices where ServiceName like '%' + @name + '%'; 
go

go
create or alter procedure DeleteDeliveryServiceByPrice(@price money, @also_delete_below_price bit, @also_delete_above_price bit)
as
if @also_delete_below_price = 1
begin
delete from DeliveryServices where ServicePrice <= @price;
end
else if @also_delete_above_price = 1
begin
delete from DeliveryServices where ServicePrice >= @price;
end
else
begin
delete from DeliveryServices where ServicePrice = @price;
end
go

go
create or alter procedure DeleteAllDeliveryServices
as
delete from DeliveryServices;
go

go
create or alter procedure DeleteDiagnosticTypeByID(@id int)
as
delete from DiagnosticTypes where TypeID = @id;
go

go
create or alter procedure DeleteDiagnosticTypeByName(@name varchar(50))
as
delete from DiagnosticTypes where TypeName like '%' + @name + '%'; 
go

go
create or alter procedure DeleteDiagnosticTypeByPrice(@price money, @also_delete_below_price bit, @also_delete_above_price bit)
as
if @also_delete_below_price = 1
begin
delete from DiagnosticTypes where TypePrice <= @price;
end
else if @also_delete_above_price = 1
begin
delete from DiagnosticTypes where TypePrice >= @price;
end
else
begin
delete from DiagnosticTypes where TypePrice = @price;
end
go

go
create or alter procedure DeleteAllDiagnosticTypes
as
delete from DiagnosticTypes;
go

go
create or alter procedure DeletePaymentMethodByID(@id int)
as
delete from PaymentMethods where PaymentMethodID = @id;
go

go
create or alter procedure DeletePaymentMethodByName(@name varchar(50))
as
delete from PaymentMethods where PaymentMethodName like '%' + @name + '%';
go

go
create or alter procedure DeleteAllPaymentMethods
as
delete from PaymentMethods;
go

go
create or alter procedure DeleteProductByID(@id int)
as
delete from Products where ProductID = @id;
go

go
create or alter procedure DeleteProductByCategoryID(@categoryid int)
as
delete from Products where ProductCategoryID = @categoryid;
go

go
create or alter procedure DeleteProductByCategoryName(@categoryname varchar(200))
as
declare @target_category int;
select @target_category = CategoryID from ProductCategories where CategoryName like '%' + @categoryname + '%';
delete from Products where ProductCategoryID = @target_category;
go

go
create or alter procedure DeleteProductByBrandID(@brandid int)
as
delete from Products where ProductBrandID = @brandid;
go

go
create or alter procedure DeleteProductByBrandName(@brandname varchar(100))
as
declare @target_brand int;
select @target_brand = BrandID from ProductBrands where BrandName like '%' + @brandname + '%';
delete from Products where ProductBrandID = @target_brand;
go



go
create or alter procedure DeleteProductByName(@name varchar(50))
as
delete from Products where ProductName like '%' + @name + '%';
go

go
create or alter procedure DeleteProductByDescription(@description varchar(max))
as
delete from Products where ProductDescription like '%' + @description + '%';
go

go
create or alter procedure DeleteProductByQuantity(@quantity int, @also_delete_below_amount bit, @also_delete_above_amount bit)
as
if @also_delete_below_amount = 1
begin
delete from Products where Quantity <= @quantity;
end
else if @also_delete_above_amount = 1
begin
delete from Products where Quantity >= @quantity;
end
else
begin
delete from Products where Quantity = @quantity;
end
go

go
create or alter procedure DeleteProductByPrice(@price money, @also_delete_below_amount bit, @also_delete_above_amount bit)
as
if @also_delete_below_amount = 1
begin
delete from Products where Price <= @price;
end
else if @also_delete_above_amount = 1
begin
delete from Products where Price >= @price;
end
else
begin
delete from Products where Price = @price;
end
go

go
create or alter procedure DeleteProductByArtID(@artid varchar(max))
as
delete from Products where ProductArtID like '%' + @artid + '%';
go

go
create or alter procedure DeleteProductBySerialNumber(@serialnumber varchar(max))
as
delete from Products where ProductSerialNumber like '%' + @serialnumber + '%';
go

go
create or alter procedure DeleteProductByStorageLocation(@location varchar(max))
as
delete from Products where ProductStorageLocation like '%' + @location + '%';
go

go
create or alter procedure DeleteProductByDate(@date date, @also_delete_before_date bit, @also_delete_after_date bit)
as
if @also_delete_before_date = 1
begin
delete from Products where DateAdded <= @date or DateModified <= @date;
end
else if @also_delete_after_date = 1
begin
delete from Products where DateAdded >= @date or DateModified >= @date;
end
else
begin
delete from Products where DateAdded = @date or DateModified = @date;
end
go

go
create or alter procedure DeleteAllProducts
as
delete from Products;
go

go
create or alter procedure DeleteProductImage(@image_name varchar(max))
as
delete from ProductImages where ImageName = @image_name;
go

go
create or alter procedure DeleteProductImageByProductID(@productid int)
as
delete from ProductImages where TargetProductID = @productid;
go

go
create or alter procedure DeleteProductImageByProductName(@name varchar(50))
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductName like '%' + @name + '%';
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductDescription(@description varchar(max))
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductDescription like '%' + @description + '%';
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductArtID(@artid varchar(max))
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductArtID like '%' + @artid + '%';
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageBySerialNumber(@serialnumber varchar(max))
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductSerialNumber like '%' + @serialnumber + '%';
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductStorageLocation(@location varchar(max))
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductStorageLocation like '%' + @location + '%';
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductQuantity(@quantity int, @also_delete_if_below_amount bit, @also_delete_if_above_amount bit)
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
if @also_delete_if_below_amount = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Quantity <= @quantity;
end
else if @also_delete_if_above_amount = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Quantity >= @quantity;
end
else
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Quantity = @quantity;
end
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductPrice(@price money, @also_delete_if_below_amount bit, @also_delete_if_above_amount bit)
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
if @also_delete_if_below_amount = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Price <= @price;
end
else if @also_delete_if_above_amount = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Price >= @price;
end
else
begin
insert into @target_products(ProductID) select distinct ProductID from Products where Price = @price;
end
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductImageByProductDate(@date date, @also_delete_if_before_date bit, @also_delete_if_after_date bit)
as
declare @target_products table(ProductCounter int identity primary key not null, ProductID int unique not null);
declare @target_product int;
declare @row_count int;
declare @last_row int;
if @also_delete_if_before_date = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where DateAdded <= @date or DateModified <= @date;
end
else if @also_delete_if_after_date = 1
begin
insert into @target_products(ProductID) select distinct ProductID from Products where DateAdded >= @date or DateModified >= @date;
end
else
begin
insert into @target_products(ProductID) select distinct ProductID from Products where DateAdded = @date or DateModified = @date;
end
select top 1 @row_count = ProductCounter from @target_products order by ProductCounter asc;
select top 1 @last_row = ProductCounter from @target_products order by ProductCounter desc;
while @row_count <= @last_row
begin
select @target_product = ProductID from @target_products order by ProductCounter asc;
delete from ProductImages where TargetProductID = @target_product;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteAllProductImages
as
delete from ProductImages;
go

go
create or alter procedure DeleteProductOrderByID(@id int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderID = @id;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByTypeID(@typeid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderTypeID = @typeid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByDiagnosticTypeID(@typeid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DiagnosticTypeID = @typeid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go


go
create or alter procedure DeleteProductOrderByProductID(@productid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ProductID = @productid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByProductName(@productname varchar(50))
as
declare @target_products table(ProductCount int identity primary key not null, ProductID int unique not null);
declare @row_count_product int;
declare @last_row_product int;
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_product int;
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
select top 1 @row_count_product = ProductCount from @target_products order by ProductCount asc;
select top 1 @last_row_product = ProductCount from @target_products order by ProductCount desc;
while @row_count_product <= @last_row_product
begin
select @target_product = ProductID from @target_products where ProductCount = @row_count_product order by ProductCount asc;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ProductID = @target_product;
set @row_count_product = @row_count_product + 1;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByReplacementProductID(@productid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ReplacementProductID = @productid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByReplacementProductName(@productname varchar(50))
as
declare @target_products table(ProductCount int identity primary key not null, ProductID int unique not null);
declare @row_count_product int;
declare @last_row_product int;
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_product int;
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_products(ProductID) select distinct ProductID from Products where ProductName like '%' + @productname + '%';
select top 1 @row_count_product = ProductCount from @target_products order by ProductCount asc;
select top 1 @last_row_product = ProductCount from @target_products order by ProductCount desc;
while @row_count_product <= @last_row_product
begin
select @target_product = ProductID from @target_products where ProductCount = @row_count_product order by ProductCount asc;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ReplacementProductID = @target_product;
set @row_count_product = @row_count_product + 1;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByQuantity(@quantity int, @also_delete_below_amount bit, @also_delete_above_amount bit)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
if @also_delete_below_amount = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DesiredQuantity <= @quantity;
end
else if @also_delete_above_amount = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DesiredQuantity >= @quantity;
end
else
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DesiredQuantity = @quantity;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByPrice(@price money, @also_delete_below_amount bit, @also_delete_above_amount bit)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
if @also_delete_below_amount = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderPrice <= @price;
end
else if @also_delete_above_amount = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderPrice >= @price;
end
else
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderPrice = @price;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByClientID(@clientid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ClientID = @clientid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByClientName(@clientname varchar(100))
as
declare @target_clients table(ClientCount int identity primary key not null, ClientID int unique not null);
declare @row_count_client int;
declare @last_row_client int;
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_client int;
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_clients(ClientID) select distinct ClientID from Clients where ClientName like '%' + @clientname + '%';
select top 1 @row_count_client = ClientCount from @target_clients order by ClientCount asc;
select top 1 @last_row_client = ClientCount from @target_clients order by ClientCount desc;
while @row_count_client <= @last_row_client
begin
select @target_client = ClientID from @target_clients where ClientCount = @row_count_client order by ClientCount asc;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where ClientID = @target_client;
set @row_count_client = @row_count_client + 1;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByUserID(@userid int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where UserID = @userid;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByUserDisplayName(@userdisplayname varchar(100))
as
declare @target_users table(UserCount int identity primary key not null, UserID int unique not null);
declare @row_count_user int;
declare @last_row_user int;
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_user int;
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_users(UserID) select distinct UserID from Users where UserDisplayName like '%' + @userdisplayname + '%';
select top 1 @row_count_user = UserCount from @target_users order by UserCount asc;
select top 1 @last_row_user = UserCount from @target_users order by UserCount desc;
while @row_count_user <= @last_row_user
begin
select @target_user = UserID from @target_users where UserCount = @row_count_user order by UserCount asc;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where UserID = @target_user;
set @row_count_user = @row_count_user + 1;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByDate(@date date, @also_delete_before_date bit, @also_delete_after_date bit)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
if @also_delete_before_date = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DateAdded <= @date or DateModified <= @date;
end
else if @also_delete_after_date = 1
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DateAdded >= @date or DateModified >= @date;
end
else
begin
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where DateAdded = @date or DateModified = @date;
end
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteProductOrderByStatus(@status int)
as
declare @target_product_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @target_order int;
declare @row_count int;
declare @last_row int;
insert into @target_product_orders(OrderID) select distinct OrderID from ProductOrders where OrderStatus = @status;
select top 1 @row_count = OrderCount from @target_product_orders order by OrderCount asc;
select top 1 @last_row = OrderCount from @target_product_orders order by OrderCount desc;
while @row_count <= @last_row
begin
select @target_order = OrderID from @target_product_orders where OrderCount = @row_count order by OrderCount asc;
delete from ProductOrders where OrderID = @target_order;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteAllProductOrders
as
delete from ProductOrders;
go

go
create or alter procedure DeleteOrderDeliveryByID(@id int)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderDeliveryID = @id;
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByOrderID(@orderid int)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderID = @orderid;
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByDeliveryServiceID(@serviceid int)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where ServiceID = @serviceid;
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByDeliveryServiceName(@servicename varchar(50))
as
declare @target_services table(ServiceCount int identity primary key not null, ServiceID int unique not null);
declare @target_service int;
declare @row_count_service int;
declare @last_row_service int;
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_services(ServiceID) select distinct ServiceID from DeliveryServices where ServiceName like '%' + @servicename + '%';
select top 1 @row_count_service = ServiceCount from @target_services order by ServiceCount asc;
select top 1 @last_row_service = ServiceCount from @target_services order by ServiceCount desc;
while @row_count_service <= @last_row_service
begin
select @target_service = ServiceID from @target_services where ServiceCount = @row_count order by ServiceCount asc;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where ServiceID = @target_service;
set @row_count_service = @row_count_service + 1;
end
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByCargoID(@cargoid varchar(max))
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DeliveryCargoID like '%' + @cargoid + '%';
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByPrice(@price money, @also_delete_below_amount bit, @also_delete_above_amount bit)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
if @also_delete_below_amount = 1
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where TotalPayment <= @price;
end
else if @also_delete_above_amount = 1
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where TotalPayment >= @price;
end
else
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where TotalPayment = @price;
end
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByPaymentMethodID(@paymentmethodid int)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where PaymentMethodID = @paymentmethodid;
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByPaymentMethodName(@methodname varchar(50))
as
declare @target_payment_methods table(MethodCount int identity primary key not null, PaymentMethodID int unique not null);
declare @target_payment_method int;
declare @row_count_method int;
declare @last_row_method int;
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_payment_methods(PaymentMethodID) select distinct PaymentMethodID from PaymentMethods where PaymentMethodName like '%' + @methodname + '%';
select top 1 @row_count_method = MethodCount from @target_payment_methods order by MethodCount asc;
select top 1 @last_row_method = MethodCount from @target_payment_methods order by MethodCount desc;
while @row_count_method <= @last_row_method
begin
select @target_payment_method = PaymentMethodID from @target_payment_methods where MethodCount = @row_count order by MethodCount asc;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where ServiceID = @target_payment_method;
set @row_count_method = @row_count_method + 1;
end
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByDate(@date date, @also_delete_before_date bit, @also_delete_after_date bit)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
if @also_delete_before_date = 1
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DateAdded <= @date or DateModified <= @date;
end
else if @also_delete_after_date = 1
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DateAdded >= @date or DateModified >= @date;
end
else
begin
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DateAdded = @date or DateModified = @date;
end
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteOrderDeliveryByStatus(@status int)
as
declare @target_order_deliveries table(DeliveryCount int identity primary key not null, OrderDeliveryID int unique not null);
declare @row_count int;
declare @last_row int;
declare @target_order_delivery int;
insert into @target_order_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where DeliveryStatus = @status;
select top 1 @row_count = DeliveryCount from @target_order_deliveries order by DeliveryCount asc;
select top 1 @last_row = DeliveryCount from @target_order_deliveries order by DeliveryCount desc;
while @row_count <= @last_row
begin
select @target_order_delivery = OrderDeliveryID from @target_order_deliveries where DeliveryCount = @row_count order by DeliveryCount asc;
delete from OrderDeliveries where OrderDeliveryID = @target_order_delivery;
set @row_count = @row_count + 1;
end
go

go
create or alter procedure DeleteAllOrderDeliveries
as
delete from OrderDeliveries;
go

go
create or alter trigger User_OnInsert on Users for insert as
begin
set nocount on;
declare @inserteduid int;
declare @insertedname varchar(100);
declare @inserteddisplayname varchar(100);
declare @insertedemail varchar(200);
declare @insertedpassword varchar(500);
declare @insertedphone varchar(50);
declare @insertedbalance money;
declare @insertedregisterdate date;
declare @insertedprofilepic varbinary(max);
declare @insertedisadmin bit;
declare @insertedisworker bit;
declare @insertedisclient bit;
declare @message varchar(max);
declare @blank_address varchar(max);
declare @existingclientid int;
declare @insertdata table(InsertedUID int,InsertedName varchar(100),InsertedDisplayName varchar(100),InsertedEmail varchar(200),InsertedPassword varchar(500),InsertedPhone varchar(50),InsertedBalance money,InsertedRegisterDate date,InsertedProfilePic varbinary(max),InsertedIsAdmin bit,InsertedIsWorker bit,InsertedIsClient bit);
select @inserteduid = UserID from inserted;
select @insertedname = UserName from inserted;
select @inserteddisplayname = UserDisplayName from inserted;
select @insertedemail = UserEmail from inserted;
select @insertedpassword = UserPassword from inserted;
select @insertedphone = UserPhone from inserted;
select @insertedbalance = UserBalance from inserted;
select @insertedregisterdate = DateOfRegister from inserted;
select @insertedprofilepic = UserProfilePic from inserted;
select @insertedisadmin = IsAdmin from inserted;
select @insertedisworker = IsWorker from inserted;
select @insertedisclient = IsClient from inserted;
if @insertedisclient = 1
begin
set @blank_address = 'Insert address here';
select @existingclientid = ClientID from Clients where UserID = @inserteduid;
if @existingclientid is null or @existingclientid = 0
begin
insert into Clients(UserID, ClientName,ClientEmail, ClientPhone, ClientAddress,ClientProfilePic) values(@inserteduid,@inserteddisplayname,@insertedemail,@insertedphone,@blank_address,@insertedprofilepic);
end
end
set @message = '[INSERT]User was added with the following ID: ' + try_convert(varchar(max), @inserteduid) + ' ';
insert into @insertdata(InsertedUID,InsertedName,InsertedDisplayName,InsertedEmail,InsertedPassword,InsertedPhone,InsertedBalance,InsertedRegisterDate,InsertedProfilePic,InsertedIsAdmin,InsertedIsWorker,InsertedIsClient) values(@inserteduid,@insertedname,@inserteddisplayname,@insertedemail,@insertedpassword,@insertedphone,@insertedbalance,@insertedregisterdate,@insertedprofilepic,@insertedisadmin,@insertedisworker,@insertedisclient);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @inserteduid) + ' , ';
set @data_string = @data_string + 'Username: ' + try_convert(varchar(max), @insertedname) + ' , ';
set @data_string = @data_string + 'Display Name: ' + try_convert(varchar(max), @inserteddisplayname) + ' , ';
set @data_string = @data_string + 'Email: ' + try_convert(varchar(max), @insertedemail) + ' , ';
set @data_string = @data_string + 'Password: ' + try_convert(varchar(max), @insertedpassword) + ' , ';
set @data_string = @data_string + 'Phone: ' + try_convert(varchar(max), @insertedphone) + ' , ';
set @data_string = @data_string + 'Balance: ' + try_convert(varchar(max), @insertedbalance) + ' , ';
set @data_string = @data_string + 'Register Date: ' + try_convert(varchar(max), @insertedregisterdate) + ' , ';
set @data_string = @data_string + 'Profile Picture: ' + try_convert(varchar(max), @insertedprofilepic) + ' , ';
set @data_string = @data_string + 'Is Admin: ' + try_convert(varchar(max), @insertedisadmin) + ' , ';
set @data_string = @data_string + 'Is Worker: ' + try_convert(varchar(max), @insertedisworker) + ' , ';
set @data_string = @data_string + 'Is Client: ' + try_convert(varchar(max), @insertedisclient) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger User_OnUpdate on Users for update as
begin
set nocount on;
declare @olduid int;
declare @newuid int;
declare @oldname varchar(100);
declare @newname varchar(100);
declare @olddisplayname varchar(100);
declare @newdisplayname varchar(100);
declare @oldemail varchar(200);
declare @newemail varchar(200);
declare @oldpassword varchar(500);
declare @newpassword varchar(500);
declare @oldphone varchar(50);
declare @newphone varchar(50);
declare @oldbalance money;
declare @newbalance money;
declare @oldregisterdate date;
declare @newregisterdate date;
declare @oldprofilepic varbinary(max);
declare @newprofilepic varbinary(max);
declare @oldisadmin bit;
declare @newisadmin bit;
declare @oldisworker bit;
declare @newisworker bit;
declare @oldisclient bit;
declare @newisclient bit;
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldUID int,NewUID int,OldUName varchar(100),NewUName varchar(100),OldDisplayName varchar(100),NewDisplayName varchar(100),OldEmail varchar(200),NewEmail varchar(200),OldPassword varchar(500),NewPassword varchar(500),OldPhone varchar(50),NewPhone varchar(50),OldBalance money,NewBalance money,OldRegisterDate date,NewRegisterDate date,OldProfilePic varbinary(max),NewProfilePic varbinary(max),OldIsAdmin bit,NewIsAdmin bit,OldIsWorker bit,NewIsWorker bit,OldIsClient bit,NewIsClient bit);
declare @target_clients table(ClientCounter int identity primary key not null, ClientID int unique not null);
declare @row_count_client int;
declare @last_row_client int;
declare @target_client int;
select @olduid = deleted.UserID from deleted inner join inserted on deleted.UserID = inserted.UserID;
select @newuid = inserted.UserID from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldname = deleted.UserName from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newname = inserted.UserName from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @olddisplayname = deleted.UserDisplayName from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newdisplayname = inserted.UserDisplayName from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldemail = deleted.UserEmail from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newemail = inserted.UserEmail from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldpassword = deleted.UserPassword from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newpassword = inserted.UserPassword from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldphone = deleted.UserPhone from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newphone = inserted.UserPhone from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldbalance = deleted.UserBalance from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newbalance = inserted.UserBalance from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldregisterdate = deleted.DateOfRegister from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newregisterdate = inserted.DateOfRegister from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldprofilepic = deleted.UserProfilePic from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newprofilepic = inserted.UserProfilePic from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid
select @oldisadmin = deleted.isAdmin from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newisadmin = inserted.isAdmin from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldisworker = deleted.isWorker from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newisworker = inserted.isWorker from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @oldisclient = deleted.isClient from deleted inner join inserted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
select @newisclient = inserted.isClient from inserted inner join deleted on deleted.UserID = inserted.UserID where deleted.UserID = @olduid;
if @oldisclient = 1 or @newisclient = 1
begin
if @newuid != @olduid or @newdisplayname != @olddisplayname or @newemail != @oldemail or @newphone != @oldphone or @newbalance != @oldbalance or @newprofilepic != @oldprofilepic
begin
insert into @target_clients(ClientID) select distinct ClientID from Clients where UserID = @olduid or UserID = @newuid;
select top 1 @row_count_client = ClientCounter from @target_clients order by ClientCounter asc;
select top 1 @last_row_client = ClientCounter from @target_clients order by ClientCounter desc;
while @row_count_client <= @last_row_client
begin
select @target_client = ClientID from @target_clients where ClientCounter = @row_count_client order by ClientCounter asc;
update Clients set UserID = @newuid, ClientName = @newdisplayname, ClientEmail = @newemail, ClientPhone = @newphone, ClientBalance = @newbalance, ClientProfilePic = @newprofilepic where ClientID = @target_client;
set @row_count_client = @row_count_client + 1;
end
end
end
set @message = '[UPDATE]User was updated with the following ID: ' + try_convert(varchar(max), @olduid);
insert into @updatedata(LogDate,LogMessage,OldUID,NewUID,OldUName,NewUName,OldDisplayName,NewDisplayName,OldEmail,NewEmail,OldPassword,NewPassword,OldPhone,NewPhone,OldBalance,NewBalance,OldRegisterDate,NewRegisterDate,OldProfilePic,NewProfilePic,OldIsAdmin,NewIsAdmin,OldIsWorker,NewIsWorker,OldIsClient,NewIsClient) values(getdate(), @message,@olduid,@newuid,@oldname,@newname,@olddisplayname,@newdisplayname,@oldemail,@newemail,@oldpassword,@newpassword, @oldphone,@newphone,@oldbalance,@newbalance,@oldregisterdate,@newregisterdate,@oldprofilepic,@newprofilepic,@oldisadmin,@newisadmin,@oldisworker,@newisworker,@oldisclient,@newisclient);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @olduid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @olduid) + ' , ';
set @data_string = @data_string + 'Old Username: ' + try_convert(varchar(max), @oldname) + ' , ';
set @data_string = @data_string + 'New Username: ' + try_convert(varchar(max), @newname) + ' , ';
set @data_string = @data_string + 'Old Display Name: ' + try_convert(varchar(max), @olddisplayname) + ' , ';
set @data_string = @data_string + 'New Display Name: ' + try_convert(varchar(max), @newdisplayname) + ' , ';
set @data_string = @data_string + 'Old Email: ' + try_convert(varchar(max), @oldemail) + ' , ';
set @data_string = @data_string + 'New Email: ' + try_convert(varchar(max), @newemail) + ' , ';
set @data_string = @data_string + 'Old Password: ' + try_convert(varchar(max), @oldpassword) + ' , ';
set @data_string = @data_string + 'New Password: ' + try_convert(varchar(max), @newpassword) + ' , ';
set @data_string = @data_string + 'Old Phone: ' + try_convert(varchar(max), @oldphone) + ' , ';
set @data_string = @data_string + 'New Phone: ' + try_convert(varchar(max), @newphone) + ' , ';
set @data_string = @data_string + 'Old Balance: ' + try_convert(varchar(max), @oldbalance) + ' , ';
set @data_string = @data_string + 'New Balance: ' + try_convert(varchar(max), @newbalance) + ' , ';
set @data_string = @data_string + 'Old Register Date: ' + try_convert(varchar(max), @oldregisterdate) + ' , ';
set @data_string = @data_string + 'New Register Date: ' + try_convert(varchar(max), @newregisterdate) + ' , ';
set @data_string = @data_string + 'Old Profile Picture: ' + try_convert(varchar(max), @oldprofilepic) + ' , ';
set @data_string = @data_string + 'New Profile Picture: ' + try_convert(varchar(max), @newprofilepic) + ' , ';
set @data_string = @data_string + 'Old Is Admin: ' + try_convert(varchar(max), @oldisadmin) + ' , ';
set @data_string = @data_string + 'New Is Admin: ' + try_convert(varchar(max), @newisadmin) + ' , ';
set @data_string = @data_string + 'Old Is Worker: ' + try_convert(varchar(max), @oldisworker) + ' , ';
set @data_string = @data_string + 'New Is Worker: ' + try_convert(varchar(max), @newisworker) + ' , ';
set @data_string = @data_string + 'Old Is Client: ' + try_convert(varchar(max), @oldisclient) + ' , ';
set @data_string = @data_string + 'New Is Client: ' + try_convert(varchar(max), @newisclient) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger User_OnDelete on Users for delete as
begin
set nocount on;
declare @deleteduid int;
declare @deletedname varchar(100);
declare @deleteddisplayname varchar(100);
declare @deletedemail varchar(200);
declare @deletedpassword varchar(500);
declare @deletedphone varchar(50);
declare @deletedbalance money;
declare @deletedregisterdate date;
declare @deletedprofilepic varbinary(max);
declare @deletedisadmin bit;
declare @deletedisworker bit;
declare @deletedisclient bit;
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedUID int,DeletedName varchar(100),DeletedDisplayName varchar(100),DeletedEmail varchar(200),DeletedPassword varchar(500),DeletedPhone varchar(50),DeletedBalance money,DeletedRegisterDate date,DeletedProfilePic varbinary(max),DeletedIsAdmin bit,DeletedIsWorker bit,DeletedIsClient bit);
select @deleteduid = UserID from deleted;
select @deletedname = UserName from deleted;
select @deleteddisplayname = UserDisplayName from deleted;
select @deletedemail = UserEmail from deleted;
select @deletedpassword = UserPassword from deleted;
select @deletedphone = UserPhone from deleted;
select @deletedbalance = UserBalance from deleted;
select @deletedregisterdate = DateOfRegister from deleted;
select @deletedprofilepic = UserProfilePic from deleted;
select @deletedisadmin = IsAdmin from deleted;
select @deletedisworker = IsWorker from deleted;
select @deletedisclient = IsClient from deleted;
set @message = '[DELETE]User was removed with the following ID: ' + try_convert(varchar(max), @deleteduid);
insert into @deletedata(LogDate,LogMessage,DeletedUID,DeletedName,DeletedDisplayName,DeletedEmail,DeletedPassword,DeletedPhone,DeletedBalance,DeletedRegisterDate,DeletedProfilePic,DeletedIsAdmin,DeletedIsWorker,DeletedIsClient) values(getdate(),@message,@deleteduid,@deletedname,@deleteddisplayname,@deletedemail,@deletedpassword,@deletedphone,@deletedbalance,@deletedregisterdate,@deletedprofilepic,@deletedisadmin,@deletedisworker,@deletedisclient);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deleteduid) + ' , ';
set @data_string = @data_string + 'Username: ' + try_convert(varchar(max), @deletedname) + ' , ';
set @data_string = @data_string + 'Display Name: ' + try_convert(varchar(max), @deleteddisplayname) + ' , ';
set @data_string = @data_string + 'Email: ' + try_convert(varchar(max), @deletedemail) + ' , ';
set @data_string = @data_string + 'Password: ' + try_convert(varchar(max), @deletedpassword) + ' , ';
set @data_string = @data_string + 'Phone: ' + try_convert(varchar(max), @deletedphone) + ' , ';
set @data_string = @data_string + 'Balance: ' + try_convert(varchar(max), @deletedbalance) + ' , ';
set @data_string = @data_string + 'Register Date: ' + try_convert(varchar(max), @deletedregisterdate) + ' , ';
set @data_string = @data_string + 'Profile Picture: ' + try_convert(varchar(max), @deletedprofilepic) + ' , ';
set @data_string = @data_string + 'Is Admin: ' + try_convert(varchar(max), @deletedisadmin) + ' , ';
set @data_string = @data_string + 'Is Worker: ' + try_convert(varchar(max), @deletedisworker) + ' , ';
set @data_string = @data_string + 'Is Client: ' + try_convert(varchar(max), @deletedisclient) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger Client_OnInsert on Clients for insert as
begin
set nocount on;
declare @insertedcid int;
declare @inserteduid int;
declare @insertedname varchar(100);
declare @insertedemail varchar(100);
declare @insertedphone varchar(50);
declare @insertedaddress varchar(100);
declare @insertedbalance money;
declare @insertedprofilepic varbinary(max);
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedCID int,InsertedUID int,InsertedName varchar(100),InsertedEmail varchar(100),InsertedPhone varchar(50),InsertedAddress varchar(100),InsertedBalance money,InsertedProfilePic varbinary(max));
select @insertedcid = ClientID from inserted;
select @inserteduid = UserID from inserted;
select @insertedname = ClientName from inserted;
select @insertedemail = ClientEmail from inserted;
select @insertedphone = ClientPhone from inserted;
select @insertedaddress = ClientAddress from inserted;
select @insertedbalance = ClientBalance from inserted;
select @insertedprofilepic = ClientProfilePic from inserted;
set @message = '[INSERT]Client was added with the following ID: ' + try_convert(varchar(max), @insertedcid);
insert into @insertdata(LogDate,LogMessage,InsertedCID,InsertedUID,InsertedName,InsertedEmail,InsertedPhone,InsertedAddress,InsertedBalance,InsertedProfilePic) values(getdate(),@message,@insertedcid,@insertedcid,@insertedname,@insertedemail,@insertedphone,@insertedaddress,@insertedbalance,@insertedprofilepic);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedcid) + ' , ';
set @data_string = @data_string + 'User ID: ' + try_convert(varchar(max), @inserteduid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedname) + ' , ';
set @data_string = @data_string + 'Email: ' + try_convert(varchar(max), @insertedemail) + ' , ';
set @data_string = @data_string + 'Phone: ' + try_convert(varchar(max), @insertedphone) + ' , ';
set @data_string = @data_string + 'Address: ' + try_convert(varchar(max), @insertedaddress) + ' , ';
set @data_string = @data_string + 'Balance: ' + try_convert(varchar(max), @insertedbalance) + ' , ';
set @data_string = @data_string + 'Profile Picture: ' + try_convert(varchar(max), @insertedprofilepic) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger Client_OnUpdate on Clients for update as
begin
set nocount on;
declare @oldcid int;
declare @newcid int;
declare @olduid int;
declare @newuid int;
declare @oldcname varchar(100);
declare @newcname varchar(100);
declare @oldemail varchar(100);
declare @newemail varchar(100);
declare @oldphone varchar(50);
declare @newphone varchar(50);
declare @oldaddress varchar(100);
declare @newaddress varchar(100);
declare @oldbalance money;
declare @newbalance money;
declare @oldprofilepic varbinary(max);
declare @newprofilepic varbinary(max);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldCID int,NewCID int,OldUID int,NewUID int,OldCName varchar(100),NewCName varchar(100),OldEmail varchar(100),NewEmail varchar(100),OldPhone varchar(50),NewPhone varchar(50),OldAddress varchar(100),NewAddress varchar(100),OldBalance money,NewBalance money,OldProfilePic varbinary(max),NewProfilePic varbinary(max));
declare @target_users table(UserCount int identity primary key not null, UserID int unique not null);
declare @target_user_id int;
declare @row_count_user int;
declare @last_row_user int;
select @oldcid = deleted.ClientID from deleted inner join inserted on deleted.ClientID = inserted.ClientID;
select @newcid = inserted.ClientID from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @olduid = deleted.UserID from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newuid = inserted.UserID from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldcname = deleted.ClientName from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newcname = inserted.ClientName from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldemail = deleted.ClientEmail from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newemail = inserted.ClientEmail from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldphone = deleted.ClientPhone from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newphone = inserted.ClientPhone from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldaddress = deleted.ClientAddress from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newaddress = inserted.ClientAddress from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldbalance = deleted.ClientBalance from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newbalance = inserted.ClientBalance from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @oldprofilepic = deleted.ClientProfilePic from deleted inner join inserted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
select @newprofilepic = inserted.ClientProfilePic from inserted inner join deleted on deleted.ClientID = inserted.ClientID where deleted.ClientID = @oldcid;
if (@olduid is not null and @olduid > 0) or (@newuid is not null and @newuid > 0)
begin
if @newcname != @oldcname or @newemail != @oldemail or @newphone != @oldphone or @newbalance != @oldbalance or @newprofilepic != @oldprofilepic
begin
insert into @target_users(UserID) select distinct UserID from Users where UserID = @olduid or UserID = @newuid;
select top 1 @row_count_user = UserCount from @target_users order by UserCount asc;
select top 1 @last_row_user = UserCount from @target_users order by UserCount desc;
while @row_count_user <= @last_row_user
begin
select @target_user_id = UserID from @target_users where UserCount = @row_count_user order by UserCount asc;
update Users set UserDisplayName = @newcname, UserEmail = @newemail, UserPhone = @newphone, UserBalance = @newbalance, UserProfilePic = @newprofilepic where UserID = @target_user_id;
set @row_count_user = @row_count_user + 1;
end
end
end
set @message = '[UPDATE]Client was updated with the following ID: ' + try_convert(varchar(max), @oldcid);
insert into @updatedata(LogDate,LogMessage,OldCID,NewCID,OldUID,NewUID,OldCName,NewCName,OldEmail,NewEmail,OldPhone,NewPhone,OldAddress,NewAddress,OldBalance,NewBalance,OldProfilePic,NewProfilePic) values(getdate(),@message,@oldcid,@newcid,@olduid,@newuid,@oldcname,@newcname,@oldemail,@newemail,@oldphone,@newphone,@oldaddress,@newaddress,@oldbalance,@newbalance,@oldprofilepic,@newprofilepic);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldcid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newcid) + ' , ';
set @data_string = @data_string + 'Old User ID: ' + try_convert(varchar(max), @olduid) + ' , ';
set @data_string = @data_string + 'New User ID: ' + try_convert(varchar(max), @newuid) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @oldcname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newcname) + ' , ';
set @data_string = @data_string + 'Old Email: ' + try_convert(varchar(max), @oldemail) + ' , ';
set @data_string = @data_string + 'New Email: ' + try_convert(varchar(max), @newemail) + ' , ';
set @data_string = @data_string + 'Old Phone: ' + try_convert(varchar(max), @oldphone) + ' , ';
set @data_string = @data_string + 'New Phone: ' + try_convert(varchar(max), @newphone) + ' , ';
set @data_string = @data_string + 'Old Address: ' + try_convert(varchar(max), @oldaddress) + ' , ';
set @data_string = @data_string + 'New Address: ' + try_convert(varchar(max), @newaddress) + ' , ';
set @data_string = @data_string + 'Old Balance: ' + try_convert(varchar(max), @oldbalance) + ' , ';
set @data_string = @data_string + 'New Balance: ' + try_convert(varchar(max), @newbalance) + ' , ';
set @data_string = @data_string + 'Old Profile Picture: ' + try_convert(varchar(max), @oldprofilepic) + ' , ';
set @data_string = @data_string + 'New Profile Picture: ' + try_convert(varchar(max), @newprofilepic) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);

end
go

go
create or alter trigger Client_OnDelete on Clients for delete as
begin
set nocount on;
declare @deletedcid int;
declare @deleteduid int;
declare @deletedname varchar(100);
declare @deletedemail varchar(100);
declare @deletedphone varchar(50);
declare @deletedaddress varchar(100);
declare @deletedbalance money;
declare @deletedprofilepic varbinary(max);
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedCID int,DeletedUID int,DeletedName varchar(100),DeletedEmail varchar(100),DeletedPhone varchar(50),DeletedAddress varchar(100),DeletedBalance money,DeletedProfilePic varbinary(max));
select @deletedcid = ClientID from deleted;
select @deleteduid = UserID from deleted;
select @deletedname = ClientName from deleted;
select @deletedemail = ClientEmail from deleted;
select @deletedphone = ClientPhone from deleted;
select @deletedaddress = ClientAddress from deleted;
select @deletedbalance = ClientBalance from deleted;
select @deletedprofilepic = ClientProfilePic from deleted;
set @message = '[DELETE]Client was removed with the following ID: ' + try_convert(varchar(max), @deletedcid);
insert into @deletedata(LogDate,LogMessage,DeletedCID,DeletedUID,DeletedName,DeletedEmail,DeletedPhone,DeletedAddress,DeletedBalance,DeletedProfilePic) values(getdate(),@message,@deletedcid,@deletedcid,@deletedname,@deletedemail,@deletedphone,@deletedaddress,@deletedbalance,@deletedprofilepic);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedcid) + ' , ';
set @data_string = @data_string + 'User ID: ' + try_convert(varchar(max), @deleteduid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedname) + ' , ';
set @data_string = @data_string + 'Email: ' + try_convert(varchar(max), @deletedemail) + ' , ';
set @data_string = @data_string + 'Phone: ' + try_convert(varchar(max), @deletedphone) + ' , ';
set @data_string = @data_string + 'Address: ' + try_convert(varchar(max), @deletedaddress) + ' , ';
set @data_string = @data_string + 'Balance: ' + try_convert(varchar(max), @deletedbalance) + ' , ';
set @data_string = @data_string + 'Profile Picture: ' + try_convert(varchar(max), @deletedprofilepic) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductCategory_OnInsert on ProductCategories for insert
as
begin
 /* PC here is short for Product Category */
set nocount on;
declare @insertedpcid int;
declare @insertedpcname varchar(200);
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedPCID int,InsertedPCName varchar(200));
select @insertedpcid = CategoryID from inserted;
select @insertedpcname = CategoryName from inserted;
set @message = '[INSERT]Product category was added with the following ID: ' + try_convert(varchar(max), @insertedpcid);
insert into @insertdata(LogDate,LogMessage,InsertedPCID,InsertedPCName) values(getdate(), @message, @insertedpcid,@insertedpcname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedpcid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedpcname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductCategory_OnUpdate on ProductCategories for update
as
begin
/* PC here is short for Product Category */
set nocount on;
declare @oldpcid int;
declare @newpcid int;
declare @oldpcname varchar(200);
declare @newpcname varchar(200);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldPCID int,NewPCID int,OldPCName varchar(200),NewPCName varchar(200));
select @oldpcid = deleted.CategoryID from deleted inner join inserted on deleted.CategoryID = inserted.CategoryID;
select @newpcid = inserted.CategoryID from inserted inner join deleted on deleted.CategoryID = inserted.CategoryID where deleted.CategoryID = @oldpcid;
select @oldpcname = deleted.CategoryName from deleted inner join inserted on deleted.CategoryID = inserted.CategoryID where deleted.CategoryID = @oldpcid;
select @newpcname = inserted.CategoryName from inserted inner join deleted on deleted.CategoryID = inserted.CategoryID where deleted.CategoryID = @oldpcid;
set @message = '[UPDATE]Product Category was updated with the following ID: ' + try_convert(varchar(max), @oldpcid);
insert into @updatedata(LogDate,LogMessage,OldPCID,NewPCID,OldPCName,NewPCName) values(getdate(),@message,@oldpcid,@newpcid,@oldpcname,@newpcname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldpcid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newpcid) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @oldpcname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newpcname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductCategory_OnDelete on ProductCategories for delete
as
begin
 /* PC here is short for Product Category */
set nocount on;
declare @deletedpcid int;
declare @deletedpcname varchar(200);
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedPCID int,DeletedPCName varchar(200));
select @deletedpcid = CategoryID from deleted;
select @deletedpcname = CategoryName from deleted;
set @message = '[DELETE]Product category was removed with the following ID: ' + try_convert(varchar(max), @deletedpcid);
insert into @deletedata(LogDate,LogMessage,DeletedPCID,DeletedPCName) values(getdate(), @message, @deletedpcid,@deletedpcname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedpcid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedpcname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger OrderType_OnInsert on OrderTypes for insert
as
begin
 /* T here is short for Type */
set nocount on;
declare @insertedtid int;
declare @insertedtname varchar(200);
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedTID int,InsertedTName varchar(200));
select @insertedtid = OrderTypeID from inserted;
select @insertedtname = TypeName from inserted;
set @message = '[INSERT]Order Type was added with the following ID: ' + try_convert(varchar(max), @insertedtid);
insert into @insertdata(LogDate,LogMessage,InsertedTID,InsertedTName) values(getdate(), @message, @insertedtid,@insertedtname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedtid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedtname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);

end
go

go
create or alter trigger OrderType_OnUpdate on OrderTypes for update
as
begin
/* T here is short for Type * */
set nocount on;
declare @oldtid int;
declare @newtid int;
declare @oldtname varchar(200);
declare @newtname varchar(200);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldTID int,NewTID int,OldTName varchar(200),NewTName varchar(200));
select @oldtid = deleted.OrderTypeID from deleted inner join inserted on deleted.OrderTypeID = inserted.OrderTypeID;
select @newtid = inserted.OrderTypeID from inserted inner join deleted on deleted.OrderTypeID = inserted.OrderTypeID where deleted.OrderTypeID = @oldtid;
select @oldtname = deleted.TypeName from deleted inner join inserted on deleted.OrderTypeID = inserted.OrderTypeID where deleted.OrderTypeID = @oldtid;
select @newtname = inserted.TypeName from inserted inner join deleted on deleted.OrderTypeID = inserted.OrderTypeID where deleted.OrderTypeID = @oldtid;
set @message = '[UPDATE]Order Type was updated with the following ID: ' + try_convert(varchar(max), @oldtid);
insert into @updatedata(LogDate,LogMessage,OldTID,NewTID,OldTName,NewTName) values(getdate(),@message,@oldtid,@newtid,@oldtname,@newtname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldtid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newtid) + ' , ';
set @data_string = @data_string +  'New Name: ' + try_convert(varchar(max), @oldtname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newtname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger OrderType_OnDelete on OrderTypes for delete
as
begin
/* T here is short for Type */
set nocount on;
declare @deletedtid int;
declare @deletedtname varchar(200);
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedTID int,DeletedTName varchar(200));
select @deletedtid = OrderTypeID from deleted;
select @deletedtname = TypeName from deleted;
set @message = '[DELETE]Order Type was removed with the following ID: ' + try_convert(varchar(max), @deletedtid);
insert into @deletedata(LogDate,LogMessage,DeletedTID,DeletedTName) values(getdate(), @message, @deletedtid,@deletedtname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedtid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedtname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go


go
create or alter trigger ProductBrand_OnInsert on ProductBrands for insert
as
begin
/* PB here is short for Product Brand */
set nocount on;
declare @insertedpbid int;
declare @insertedpbname varchar(100);
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max), InsertedPBID int,InsertedPBName varchar(100));
select @insertedpbid = BrandID from inserted;
select @insertedpbname = BrandName from inserted;
set @message = '[INSERT]Product brand was added with the following ID: ' + try_convert(varchar(max), @insertedpbid);
insert into @insertdata(LogDate,LogMessage,InsertedPBID,InsertedPBName) values(getdate(),@message,@insertedpbid,@insertedpbname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedpbid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedpbname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductBrand_OnUpdate on ProductBrands for update
as
begin
set nocount on;
declare @oldpbid int;
declare @newpbid int;
declare @oldpbname varchar(100);
declare @newpbname varchar(100);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldPBID int,NewPBID int,OldPBName varchar(100),NewPBName varchar(100));
select @oldpbid = deleted.BrandID from deleted inner join inserted on deleted.BrandID = inserted.BrandID;
select @newpbid = inserted.BrandID from inserted inner join deleted on deleted.BrandID = inserted.BrandID where deleted.BrandID = @oldpbid;;
select @oldpbname = deleted.BrandName from deleted inner join inserted on deleted.BrandID = inserted.BrandID where deleted.BrandID = @oldpbid;;
select @newpbname = inserted.BrandName from inserted inner join deleted on deleted.BrandID = inserted.BrandID where deleted.BrandID = @oldpbid;;
set @message = '[UPDATE]Product brand was updated with the following ID: ' + try_convert(varchar(max), @oldpbid);
insert into @updatedata(LogDate,LogMessage,OldPBID,NewPBID,OldPBName,NewPBName) values(getdate(), @message, @oldpbid, @newpbid, @oldpbname, @newpbname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldpbid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newpbid) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @oldpbname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newpbname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductBrand_OnDelete on ProductBrands for delete
as
begin
/* PB here is short for Product Brand */
set nocount on;
declare @deletedpbid int;
declare @deletedpbname varchar(100);
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max), DeletedPBID int,DeletedPBName varchar(100));
select @deletedpbid = BrandID from deleted;
select @deletedpbname = BrandName from deleted;
set @message = '[DELETE]Product brand was removed with the following ID: ' + try_convert(varchar(max), @deletedpbid);
insert into @deletedata(LogDate,LogMessage,DeletedPBID,DeletedPBName) values(getdate(),@message,@deletedpbid,@deletedpbname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedpbid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedpbname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DeliveryService_OnInsert on DeliveryServices for insert
as
begin
/* DS here is short for DeliveryService */
set nocount on;
declare @inserteddsid int;
declare @inserteddsname varchar(50);
declare @inserteddsprice money;
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedDSID int,InsertedDSName varchar(50),InsertedDSPrice money);
select @inserteddsid = ServiceID from inserted;
select @inserteddsname = ServiceName from inserted;
select @inserteddsprice = ServicePrice from inserted;
set @message = '[INSERT]Delivery service was added with the following ID: ' + try_convert(varchar(max), @inserteddsid);
insert into @insertdata(LogDate,LogMessage,InsertedDSID,InsertedDSName,InsertedDSPrice) values(getdate(), @message, @inserteddsid,@inserteddsname,@inserteddsprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @inserteddsid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @inserteddsname) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @inserteddsprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DeliveryService_OnUpdate on DeliveryServices for update
as
begin
/* DS here is short for DeliveryService */
set nocount on;
declare @olddsid int;
declare @newdsid int;
declare @olddsname varchar(100);
declare @newdsname varchar(100);
declare @olddsprice money;
declare @newdsprice money;
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldDSID int,NewDSID int,OldDSName varchar(100),NewDSName varchar(100),OldDSPrice money,NewDSPrice money);
select @olddsid = deleted.ServiceID from deleted inner join inserted on deleted.ServiceID = inserted.ServiceID;
select @newdsid = inserted.ServiceID from inserted inner join deleted on deleted.ServiceID = inserted.ServiceID where deleted.ServiceID = @olddsid;
select @olddsname = deleted.ServiceName from deleted inner join inserted on deleted.ServiceID = inserted.ServiceID where deleted.ServiceID = @olddsid;
select @newdsname = inserted.ServiceName from inserted inner join deleted on deleted.ServiceID = inserted.ServiceID where deleted.ServiceID = @olddsid;
select @olddsprice = deleted.ServicePrice from deleted inner join inserted on deleted.ServiceID = inserted.ServiceID where deleted.ServiceID = @olddsid;
select @newdsprice = inserted.ServicePrice from inserted inner join deleted on deleted.ServiceID = inserted.ServiceID where deleted.ServiceID = @olddsid;
set @message = '[UPDATE]Delivery service was updated with the following ID: ' + try_convert(varchar(max), @olddsid);
insert into @updatedata(LogDate,LogMessage,OldDSID,NewDSID,OldDSName,NewDSName,OldDSPrice,NewDSPrice) values(getdate(), @message, @olddsid, @newdsid, @olddsname, @newdsname, @olddsprice, @newdsprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @olddsid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newdsid) + ' , ';
set @data_string = @data_string + 'Old Name: ' +try_convert(varchar(max), @olddsname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newdsname) + ' , ';
set @data_string = @data_string + 'Old Price: ' + try_convert(varchar(max), @olddsprice) + ' , ';
set @data_string = @data_string + 'New Price: ' + try_convert(varchar(max), @newdsprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DeliveryService_OnDelete on DeliveryServices for delete
as
begin
/* DS here is short for DeliveryService */
set nocount on;
declare @deleteddsid int;
declare @deleteddsname varchar(50);
declare @deleteddsprice money;
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedDSID int,DeletedDSName varchar(50),DeletedDSPrice money);
select @deleteddsid = ServiceID from deleted;
select @deleteddsname = ServiceName from deleted;
select @deleteddsprice = ServicePrice from deleted;
set @message = '[DELETE]Delivery service was removed with the following ID: ' + try_convert(varchar(max), @deleteddsid);
insert into @deletedata(LogDate,LogMessage,DeletedDSID,DeletedDSName,DeletedDSPrice) values(getdate(), @message, @deleteddsid,@deleteddsname,@deleteddsprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deleteddsid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deleteddsname) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @deleteddsprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DiagnosticType_OnInsert on DiagnosticTypes for insert
as
begin
/* DT here is short for Diagnostic Type */
set nocount on;
declare @inserteddtid int;
declare @inserteddtname varchar(50);
declare @inserteddtprice money;
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedDTID int,InsertedDTName varchar(50),InsertedDTPrice money);
select @inserteddtid = TypeID from inserted;
select @inserteddtname = TypeName from inserted;
select @inserteddtprice = TypePrice from inserted;
set @message = '[INSERT]Diagnostic Type was added with the following ID: ' + try_convert(varchar(max), @inserteddtid);
insert into @insertdata(LogDate,LogMessage,InsertedDTID,InsertedDTName,InsertedDTPrice) values(getdate(), @message, @inserteddtid,@inserteddtname,@inserteddtprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @inserteddtid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @inserteddtname) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @inserteddtprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DiagnosticType_OnUpdate on DiagnosticTypes for update
as
begin
/* DT here is short for Diagnostic Type */
set nocount on;
declare @olddtid int;
declare @newdtid int;
declare @olddtname varchar(100);
declare @newdtname varchar(100);
declare @olddtprice money;
declare @newdtprice money;
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldDTID int,NewDTID int,OldDTName varchar(100),NewDTName varchar(100),OldDTPrice money,NewDTPrice money);
select @olddtid = deleted.TypeID from deleted inner join inserted on deleted.TypeID = inserted.TypeID;
select @newdtid = inserted.TypeID from inserted inner join deleted on deleted.TypeID = inserted.TypeID where deleted.TypeID = @olddtid;
select @olddtname = deleted.TypeName from deleted inner join inserted on deleted.TypeID = inserted.TypeID where deleted.TypeID = @olddtid;
select @newdtname = inserted.TypeName from inserted inner join deleted on deleted.TypeID = inserted.TypeID where deleted.TypeID = @olddtid;
select @olddtprice = deleted.TypePrice from deleted inner join inserted on deleted.TypeID = inserted.TypeID where deleted.TypeID = @olddtid;
select @newdtprice = inserted.TypePrice from inserted inner join deleted on deleted.TypeID = inserted.TypeID where deleted.TypeID = @olddtid;
set @message = '[UPDATE]Diagnostic Type was updated with the following ID: ' + try_convert(varchar(max), @olddtid);
insert into @updatedata(LogDate,LogMessage,OldDTID,NewDTID,OldDTName,NewDTName,OldDTPrice,NewDTPrice) values(getdate(), @message, @olddtid, @newdtid, @olddtname, @newdtname, @olddtprice, @newdtprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @olddtid) + ' , '
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newdtid) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @olddtname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newdtname) + ' , ';
set @data_string = @data_string + 'Old Price: ' + try_convert(varchar(max), @olddtprice) + ' , ';
set @data_string = @data_string + 'New Price: ' + try_convert(varchar(max), @newdtprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger DiagnosticType_OnDelete on DiagnosticTypes for delete
as
begin
/* DT here is short for Diagnostic Type */
set nocount on;
declare @deleteddtid int;
declare @deleteddtname varchar(50);
declare @deleteddtprice money;
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedDTID int,DeletedDTName varchar(50),DeletedDTPrice money);
select @deleteddtid = TypeID from deleted;
select @deleteddtname = TypeName from deleted;
select @deleteddtprice = TypePrice from deleted;
set @message = '[DELETE]Diagnostic Type was removed with the following ID: ' + try_convert(varchar(max), @deleteddtid);
insert into @deletedata(LogDate,LogMessage,DeletedDTID,DeletedDTName,DeletedDTPrice) values(getdate(), @message, @deleteddtid,@deleteddtname,@deleteddtprice);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deleteddtid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deleteddtname) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @deleteddtprice) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger PaymentMethod_OnInsert on PaymentMethods for insert
as
begin
/* Here PM is short for Payment Method */
set nocount on;
declare @insertedpmid as int;
declare @insertedpmname varchar(50);
declare @message varchar(max); 
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedPMID int,InsertedPMName varchar(50));
select @insertedpmid = PaymentMethodID from inserted;
select @insertedpmname = PaymentMethodName from inserted;
set @message = '[INSERT]Payment method was added with the following ID: ' + try_convert(varchar(max), @insertedpmid);
insert into @insertdata(LogDate,LogMessage,InsertedPMID,InsertedPMName) values (getdate(), @message, @insertedpmid, @insertedpmname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedpmid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedpmname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger PaymentMethod_OnUpdate on PaymentMethods for update
as
begin
/* Here PM is short for Payment Method */
set nocount on;
declare @oldpmid int;
declare @newpmid int;
declare @oldpmname varchar(50);
declare @newpmname varchar(50);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldPMID int,NewPMID int,OldPMName varchar(50),NewPMName varchar(50));
select @oldpmid = deleted.PaymentMethodID from deleted inner join inserted on deleted.PaymentMethodID = inserted.PaymentMethodID;
select @newpmid = inserted.PaymentMethodID from inserted inner join deleted on deleted.PaymentMethodID = inserted.PaymentMethodID where deleted.PaymentMethodID = @oldpmid;;
select @oldpmname = deleted.PaymentMethodName from deleted inner join inserted on deleted.PaymentMethodID = inserted.PaymentMethodID where deleted.PaymentMethodID = @oldpmid;;
select @newpmname = inserted.PaymentMethodName from inserted inner join deleted on deleted.PaymentMethodID = inserted.PaymentMethodID where deleted.PaymentMethodID = @oldpmid;;
set @message = '[UPDATE]Payment method was updated with the following ID: '+ try_convert(varchar(max), @oldpmid);
insert into @updatedata(LogDate,LogMessage,OldPMID,NewPMID,OldPMName,NewPMName) values(getdate(),@message,@oldpmid,@newpmid,@oldpmname,@newpmname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string  + 'Old ID: ' + try_convert(varchar(max),@oldpmid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newpmid) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @oldpmname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newpmname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger PaymentMethod_OnDelete on PaymentMethods for delete
as
begin
/* Here PM is short for Payment Method */
set nocount on;
declare @deletedpmid as int;
declare @deletedpmname varchar(50);
declare @message varchar(max); 
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedPMID int,DeletedPMName varchar(50));
select @deletedpmid = PaymentMethodID from deleted;
select @deletedpmname = PaymentMethodName from deleted;
set @message = '[DELETE]Payment method was removed with the following ID: ' + try_convert(varchar(max), @deletedpmid);
insert into @deletedata(LogDate,LogMessage,DeletedPMID,DeletedPMName) values (getdate(), @message, @deletedpmid, @deletedpmname);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedpmid) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedpmname) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go




go
create or alter trigger Product_OnInsert on Products for insert as
begin
set nocount on;
declare @insertedpid int;
declare @insertedcategory int;
declare @insertedbrand int;
declare @insertedname varchar(50);
declare @inserteddescription varchar(max);
declare @insertedquantity int;
declare @insertedprice money;
declare @insertedartid varchar(max);
declare @insertedserialnumber varchar(max);
declare @insertedlocation varchar(max);
declare @inserteddateadded date;
declare @inserteddatemodified date;
declare @message varchar(max);
declare @insertdata table(logdate date, logmessage varchar(max), InsertedPID int,InsertedCategory int,InsertedBrand int,InsertedName varchar(50),InsertedDescription varchar(max),InsertedQuantity int,InsertedPrice money,InsertedArtID varchar(max),InsertedSerialNumber varchar(max),InsertedLocation varchar(max),InsertedDateAdded date, InsertedDateModified date);
select @insertedpid = ProductID from inserted;
select @insertedcategory = ProductCategoryID from inserted;
select @insertedbrand = ProductBrandID from inserted;
select @insertedname = ProductName from inserted;
select @inserteddescription = ProductDescription from inserted;
select @insertedquantity = Quantity from inserted;
select @insertedprice = Price from inserted;
select @insertedartid = ProductArtID from inserted;
select @insertedserialnumber = ProductSerialNumber from inserted;
select @insertedlocation = ProductStorageLocation from inserted;
select @inserteddateadded = DateAdded from inserted;
select @inserteddatemodified = DateModified from inserted;
set @message = '[INSERT]Product was added with the following ID: ' + try_convert(varchar(max), @insertedpid);
insert into @insertdata(logdate,logmessage,InsertedPID,InsertedCategory,InsertedBrand,InsertedName,InsertedDescription,InsertedQuantity,InsertedPrice,InsertedArtID,InsertedSerialNumber,InsertedLocation,InsertedDateAdded,InsertedDateModified) values(getdate(), @message,@insertedpid,@insertedcategory,@insertedbrand,@insertedname,@inserteddescription,@insertedquantity, @insertedprice, @insertedartid,@insertedserialnumber, @insertedlocation, @inserteddateadded, @inserteddatemodified);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedpid) + ' , ';
set @data_string = @data_string + 'Category ID: ' + try_convert(varchar(max), @insertedcategory) + ' , ';
set @data_string = @data_string + 'Brand ID: ' + try_convert(varchar(max), @insertedbrand) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @insertedname) + ' , ';
set @data_string = @data_string + 'Desription: ' + try_convert(varchar(max), @inserteddescription) + ' , ';
set @data_string = @data_string + 'Quantity: ' + try_convert(varchar(max), @insertedquantity) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @insertedprice) + ' , ';
set @data_string = @data_string + 'Articule Number: ' + try_convert(varchar(max), @insertedartid) + ' , ';
set @data_string = @data_string + 'Serial Number: ' + try_convert(varchar(max), @insertedserialnumber) + ' , ';
set @data_string = @data_string + 'Storage Location: ' + try_convert(varchar(max), @insertedlocation) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @inserteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @inserteddatemodified) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger Product_OnUpdate on Products for update as
begin
set nocount on;
declare @oldpid int;
declare @newpid int;
declare @oldcategory int;
declare @newcategory int;
declare @oldbrand int;
declare @newbrand int;
declare @oldname varchar(50);
declare @newname varchar(50);
declare @olddescription varchar(max);
declare @newdescription varchar(max);
declare @oldquantity int;
declare @newquantity int;
declare @oldprice money;
declare @newprice money;
declare @oldartid varchar(max);
declare @newartid varchar(max);
declare @oldserialnumber varchar(max);
declare @newserialnumber varchar(max);
declare @oldlocation varchar(max);
declare @newlocation varchar(max);
declare @olddateadded date;
declare @newdateadded date;
declare @olddatemodified date;
declare @newdatemodified date;
declare @message varchar(max);
declare @row_count_order int;
declare @last_row_order int;
declare @target_order_id int;
declare @target_order_type int;
declare @target_diagnostic_type int;
declare @target_order_quantity int;
declare @new_order_price int;
declare @product_orders table(OrderCount int identity primary key not null, OrderID int unique not null, OrderType int not null,DiagnosticType int not null, OrderQuantity int not null);
declare @updatedata table(logdate date, logmessage varchar(max), OldPID int, NewPID int, OldCategory int, NewCategory int, OldBrand int, NewBrand int, OldProductName varchar(50), NewProductName varchar(50), OldDescription varchar(max), NewDescription varchar(max), OldQuantity int, NewQuantity int, OldPrice money, NewPrice money, OldArtID varchar(max), NewArtID varchar(max), OldSerialNumber varchar(max), NewSerialNumber varchar(max), OldLocation varchar(max), NewLocation varchar(max), OldDateAdded date, NewDateAdded date, OldDateModified date, NewDateModified date);
select @oldpid = deleted.ProductID from deleted inner join inserted on deleted.ProductID = inserted.ProductID; 
select @newpid = inserted.ProductID from inserted inner join deleted on inserted.ProductID = deleted.ProductID where deleted.ProductID = @oldpid;
select @oldcategory = deleted. ProductCategoryID from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newcategory = inserted.ProductCategoryID from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldbrand = deleted.ProductBrandID from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newbrand = inserted.ProductBrandID from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldname = deleted.ProductName from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newname = inserted.ProductName from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @olddescription = deleted.ProductDescription from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newdescription = inserted.ProductDescription from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldquantity = deleted.Quantity from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newquantity = inserted.Quantity from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldprice = deleted.Price from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newprice = inserted.Price from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldartid = deleted.ProductArtID from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newartid = inserted.ProductArtID from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldserialnumber = deleted.ProductSerialNumber from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newserialnumber = inserted.ProductSerialNumber from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @oldlocation = deleted.ProductStorageLocation from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newlocation = inserted.ProductStorageLocation from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @olddateadded = deleted.DateAdded from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newdateadded = inserted.DateAdded from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @olddatemodified = deleted.DateModified from deleted inner join inserted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
select @newdatemodified = inserted.DateModified from inserted inner join deleted on deleted.ProductID = inserted.ProductID where deleted.ProductID = @oldpid;
if @newprice is not null and @newprice != @oldprice
begin
insert into @product_orders(OrderID,OrderType,DiagnosticType,OrderQuantity) select distinct ProductOrders.OrderID,ProductOrders.OrderTypeID,ProductOrders.DiagnosticTypeID, ProductOrders.DesiredQuantity from ProductOrders where ProductID = @oldpid or ProductID = @newpid; 
select top 1 @row_count_order = OrderCount from @product_orders order by OrderCount asc;
select top 1 @last_row_order = OrderCount from @product_orders order by OrderCount desc;
while @row_count_order <= @last_row_order
begin
select @target_order_id = OrderID, @target_order_type = OrderType, @target_diagnostic_type = DiagnosticType, @target_order_quantity = OrderQuantity from @product_orders where OrderCount = @row_count_order order by OrderCount asc;
if @target_order_type = 2 and @target_diagnostic_type = 2
begin
set @new_order_price = @newprice;
end
else
begin
set @new_order_price = @newprice * @target_order_quantity;
update ProductOrders set OrderPrice = @new_order_price, DateModified = getdate() where OrderID = @target_order_id;
end
set @row_count_order = @row_count_order + 1;
end
end
set @new_order_price = 0;
select @message = '[UPDATE]Product was updated with the following ID: ' + try_convert(varchar(max), @oldpid) + '. The orders related to this product were updated as well.';
insert into @updatedata(logdate,logmessage,OldPID,NewPID,OldCategory,NewCategory,OldBrand,NewBrand,OldProductName,NewProductName,OldDescription,NewDescription,OldQuantity,NewQuantity,OldPrice,NewPrice,OldArtID,NewArtID,OldSerialNumber,NewSerialNumber,OldLocation,NewLocation,OldDateAdded,NewDateAdded,OldDateModified,NewDateModified) values(getdate(), @message, @oldpid, @newpid, @oldcategory, @newcategory, @oldbrand, @newbrand,@oldname, @newname, @olddescription, @newdescription, @oldquantity, @newquantity, @oldprice, @newprice, @oldartid, @newartid, @oldserialnumber, @newserialnumber, @oldlocation, @newlocation, @olddateadded, @newdateadded, @olddatemodified, @newdatemodified);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldpid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newpid) + ' , ';
set @data_string = @data_string + 'Old Category ID: ' + try_convert(varchar(max), @oldcategory) + ' , ';
set @data_string = @data_string + 'New Category ID: ' + try_convert(varchar(max), @newcategory) + ' , ';
set @data_string = @data_string + 'Old Brand ID: ' + try_convert(varchar(max), @oldbrand) + ' , ';
set @data_string = @data_string + 'New Brand ID: ' + try_convert(varchar(max), @newbrand) + ' , ';
set @data_string = @data_string + 'Old Name: ' + try_convert(varchar(max), @oldname) + ' , ';
set @data_string = @data_string + 'New Name: ' + try_convert(varchar(max), @newname) + ' , ';
set @data_string = @data_string + 'Old Desription: ' + try_convert(varchar(max), @olddescription) + ' , ';
set @data_string = @data_string + 'New Desription: ' + try_convert(varchar(max), @newdescription) + ' , ';
set @data_string = @data_string + 'Old Quantity: ' + try_convert(varchar(max), @oldquantity) + ' , ';
set @data_string = @data_string + 'New Quantity: ' + try_convert(varchar(max), @newquantity) + ' , ';
set @data_string = @data_string + 'Old Price: ' + try_convert(varchar(max), @oldprice) + ' , ';
set @data_string = @data_string + 'New Price: ' + try_convert(varchar(max), @newprice) + ' , ';
set @data_string = @data_string + 'Old Articule Number: ' + try_convert(varchar(max), @oldartid) + ' , ';
set @data_string = @data_string + 'New Articule Number: ' + try_convert(varchar(max), @newartid) + ' , ';
set @data_string = @data_string + 'Old Serial Number: ' + try_convert(varchar(max), @oldserialnumber) + ' , ';
set @data_string = @data_string + 'New Serial Number: ' + try_convert(varchar(max), @newserialnumber) + ' , ';
set @data_string = @data_string + 'Old Storage Location: ' + try_convert(varchar(max), @oldlocation) + ' , ';
set @data_string = @data_string + 'New Storage Location: ' + try_convert(varchar(max), @newlocation) + ' , ';
set @data_string = @data_string + 'Old Date Added: ' + try_convert(varchar(max), @olddateadded) + ' , ';
set @data_string = @data_string + 'New Date Added: ' + try_convert(varchar(max), @newdateadded) + ' , ';
set @data_string = @data_string + 'Old Date Modified: ' + try_convert(varchar(max), @olddatemodified) + ' , ';
set @data_string = @data_string + 'New Date Modified: ' + try_convert(varchar(max), @newdatemodified) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger Product_OnDelete on Products for delete as
begin
set nocount on;
declare @deletedpid int;
declare @deletedcategory int;
declare @deletedbrand int;
declare @deletedname varchar(50);
declare @deleteddescription varchar(max);
declare @deletedquantity int;
declare @deletedprice money;
declare @deletedartid varchar(max);
declare @deletedserialnumber varchar(max);
declare @deletedlocation varchar(max);
declare @deleteddateadded date;
declare @deleteddatemodified date;
declare @message varchar(max);
declare @deletedata table(logdate date, logmessage varchar(max), DeletedPID int,DeletedCategory int,DeletedBrand int,DeletedName varchar(50),DeletedDescription varchar(max),DeletedQuantity int,DeletedPrice money,DeletedArtID varchar(max),DeletedSerialNumber varchar(max),DeletedLocation varchar(max),DeletedDateAdded date, DeletedDateModified date);
select @deletedpid = ProductID from deleted;
select @deletedcategory = ProductCategoryID from deleted;
select @deletedbrand = ProductBrandID from deleted;
select @deletedname = ProductName from deleted;
select @deleteddescription = ProductDescription from deleted;
select @deletedquantity = Quantity from deleted;
select @deletedprice = Price from deleted;
select @deletedartid = ProductArtID from deleted;
select @deletedserialnumber = ProductSerialNumber from deleted;
select @deletedlocation = ProductStorageLocation from deleted;
select @deleteddateadded = DateAdded from deleted;
select @deleteddatemodified = DateModified from deleted;
set @message = '[DELETE]Product was removed with the following ID: ' + try_convert(varchar(max), @deletedpid);
insert into @deletedata(logdate,logmessage,DeletedPID,DeletedCategory,DeletedBrand,DeletedName,DeletedDescription,DeletedQuantity,DeletedPrice,DeletedArtID,DeletedSerialNumber,DeletedLocation,DeletedDateAdded,DeletedDateModified) values(getdate(), @message,@deletedpid,@deletedcategory,@deletedbrand,@deletedname,@deleteddescription,@deletedquantity, @deletedprice, @deletedartid, @deletedserialnumber, @deletedlocation, @deleteddateadded, @deleteddatemodified);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedpid) + ' , ';
set @data_string = @data_string + 'Category ID: ' + try_convert(varchar(max), @deletedcategory) + ' , ';
set @data_string = @data_string + 'Brand ID: ' + try_convert(varchar(max), @deletedbrand) + ' , ';
set @data_string = @data_string + 'Name: ' + try_convert(varchar(max), @deletedname) + ' , ';
set @data_string = @data_string + 'Desription: ' + try_convert(varchar(max), @deleteddescription) + ' , ';
set @data_string = @data_string + 'Quantity: ' + try_convert(varchar(max), @deletedquantity) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @deletedprice) + ' , ';
set @data_string = @data_string + 'Articule Number: ' + try_convert(varchar(max), @deletedartid) + ' , ';
set @data_string = @data_string + 'Serial Number: ' + try_convert(varchar(max), @deletedserialnumber) + ' , ';
set @data_string = @data_string + 'Storage Location: ' + try_convert(varchar(max), @deletedlocation) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @deleteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @deleteddatemodified) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductImage_OnInsert on ProductImages for insert
as
begin
/* Here and in the triggers for the Product table P is short for Product */
set nocount on;
declare @insertedpid int;
declare @insertedimagename varchar(max);
declare @insertedpimage varbinary(max);
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedPID int,InsertedImageName varchar(max),InsertedPImage varchar(max));
select @insertedpid = TargetProductID from inserted;
select @insertedimagename = ImageName from inserted;
select @insertedpimage = Picture from inserted;
set @message = '[INSERT]Product image was added for the following Product ID: ' + try_convert(varchar(max), @insertedpid);
insert into @insertdata(LogDate,LogMessage,InsertedPID,InsertedImageName,InsertedPImage) values(getdate(),@message,@insertedpid,@insertedimagename,@insertedpimage);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedpid) + ' , ';
set @data_string = @data_string + 'Image Name: ' + try_convert(varchar(max), @insertedimagename) + ' , ';
set @data_string = @data_string + 'Image in string format: ' + try_convert(varchar(max), @insertedpimage) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductImage_OnUpdate on ProductImages for delete
as
begin
/* Here and in the triggers for the Product table P is short for Product */
set nocount on;
declare @oldpid int;
declare @newpid int;
declare @oldimagename varchar(max);
declare @newimagename varchar(max);
declare @oldimage varbinary(max);
declare @newimage varbinary(max);
declare @message varchar(max);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldPID int,NewPID int,OldImageName varchar(max), NewImageName varchar(max),OldImage varbinary(max),NewImage varbinary(max));
select @oldpid = deleted.TargetProductID from deleted inner join inserted on deleted.TargetProductID = inserted.TargetProductID;
select @newpid = inserted.TargetProductID from inserted inner join deleted on inserted.TargetProductID = deleted.TargetProductID where deleted.TargetProductID = @oldpid;
select @oldimagename = deleted.ImageName from deleted inner join inserted on deleted.TargetProductID = inserted.TargetProductID;
select @newimagename = inserted.ImageName from inserted inner join deleted on inserted.TargetProductID = deleted.TargetProductID where deleted.TargetProductID = @oldpid;
select @oldimage = deleted.Picture from deleted inner join inserted on inserted.TargetProductID = deleted.TargetProductID where deleted.TargetProductID = @oldpid;
select @newimage = inserted.Picture from inserted inner join deleted on inserted.TargetProductID = deleted.TargetProductID where deleted.TargetProductID = @oldpid;
set @message = '[UPDATE]Product image was updated for the following Product ID: ' + try_convert(varchar(max), @oldpid);
insert into @updatedata(LogDate,LogMessage,OldPID,NewPID,OldImageName,NewImageName,OldImage,NewImage) values(getdate(), @message, @oldpid, @newpid, @oldimagename,@newimagename, @oldimage, @newimage);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldpid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newpid) + ' , ';
set @data_string = @data_string + 'Old Image Name: ' + try_convert(varchar(max), @oldimagename) + ' , ';
set @data_string = @data_string + 'New Image Name: ' + try_convert(varchar(max), @newimagename) + ' , ';
set @data_string = @data_string + 'Old Image in string format: ' + try_convert(varchar(max), @oldimage) + ' , ';
set @data_string = @data_string + 'New Image in string format: ' + try_convert(varchar(max), @newimage) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductImage_OnDelete on ProductImages for delete
as
begin
/* Here and in the triggers for the Product table P is short for Product */
set nocount on;
declare @deletedpid int;
declare @deletedimagename varchar(max);
declare @deletedpimage varbinary(max);
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedPID int,DeletedImageName varchar(max),DeletedPImage varchar(max));
select @deletedpid = TargetProductID from deleted;
select @deletedimagename = ImageName from deleted;
select @deletedpimage = Picture from deleted;
set @message = '[DELETE]Product image was removed for the following Product ID: ' + try_convert(varchar(max), @deletedpid);
insert into @deletedata(LogDate,LogMessage,DeletedPID,DeletedImageName,DeletedPImage) values(getdate(),@message,@deletedpid,@deletedimagename,@deletedpimage);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedpid) + ' , ';
set @data_string = @data_string + 'Image Name: ' + try_convert(varchar(max), @deletedimagename) + ' , ';
set @data_string = @data_string + 'Image in string format: ' + try_convert(varchar(max), @deletedpimage) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductOrder_OnInsert on ProductOrders for insert
as
begin
/* here O is for Order, P is for Product, C is for Client and U is for User */
set nocount on;
declare @insertedoid int;
declare @insertedpid int;
declare @insertedtid int;
declare @insertedrpid int;
declare @inserteddtid int;
declare @insertedquantity int;
declare @insertedprice money;
declare @insertedcid int;
declare @inserteduid int;
declare @inserteddateadded date;
declare @inserteddatemodified date;
declare @insertedstatus int;
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedOID int,InsertedPID int,InsertedTID int,InsertedRPID int,InsertedDTID int,InsertedQuantity int,InsertedPrice money,InsertedCID int,InsertedUID int,InsertedDateAdded date,InsertedDateModified date,InsertedStatus int);
select @insertedoid = OrderID from inserted;
select @insertedpid = ProductID from inserted;
select @insertedtid = OrderTypeID from inserted;
select @insertedrpid = ReplacementProductID from inserted;
select @inserteddtid = DiagnosticTypeID from inserted;
select @insertedquantity = DesiredQuantity from inserted;
select @insertedprice = OrderPrice from inserted;
select @insertedcid = ClientID from inserted;
select @inserteduid = UserID from inserted;
select @inserteddateadded = DateAdded from inserted;
select @inserteddatemodified = DateModified from inserted;
select @insertedstatus = OrderStatus from inserted;
if @insertedtid = 0
begin
set @insertedtid = 1;
update ProductOrders set OrderTypeID = @insertedtid where OrderID = @insertedoid;
end
set @message = '[INSERT]An order was added with the following ID: ' + try_convert(varchar(max), @insertedoid);
insert into @insertdata(LogDate,LogMessage,InsertedOID,InsertedPID,InsertedTID,InsertedRPID,InsertedDTID,InsertedQuantity,InsertedPrice,InsertedCID,InsertedUID,InsertedDateAdded,InsertedDateModified,InsertedStatus) values(getdate(),@message,@insertedoid,@insertedpid,@insertedtid,@insertedrpid,@inserteddtid,@insertedquantity,@insertedprice,@insertedcid,@inserteduid,@inserteddateadded,@inserteddatemodified,@insertedstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedoid) + ' , ';
set @data_string = @data_string + 'Product ID: ' + try_convert(varchar(max), @insertedpid) + ' , ';
set @data_string = @data_string + 'Order Type ID: ' + try_convert(varchar(max), @insertedtid) + ' , ';
set @data_string = @data_string + 'Replacement Product ID: ' + try_convert(varchar(max), @insertedrpid) + ' , ';
set @data_string = @data_string + 'Diagnostic Type ID: ' + try_convert(varchar(max), @inserteddtid) + ' , ';
set @data_string = @data_string + 'Desired quantity: ' + try_convert(varchar(max), @insertedquantity) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @insertedprice) + ' , ';
set @data_string = @data_string + 'Client ID: ' + try_convert(varchar(max), @insertedcid) + ' , ';
set @data_string = @data_string + 'User ID: ' + try_convert(varchar(max), @inserteduid) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @inserteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @inserteddatemodified) + ' , ';
set @data_string = @data_string + 'Status: ' + try_convert(varchar(max), @insertedstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductOrder_OnUpdate on ProductOrders for update
as
begin
set nocount on;
/* here O is for Order, P is for Product, C is for Client and U is for User */
declare @oldoid int;
declare @newoid int;
declare @oldpid int;
declare @newpid int;
declare @oldtid int;
declare @newtid int;
declare @oldrpid int;
declare @newrpid int;
declare @olddtid int;
declare @newdtid int;
declare @oldquantity int;
declare @newquantity int;
declare @oldprice money;
declare @newprice money;
declare @oldcid int;
declare @newcid int;
declare @olduid int;
declare @newuid int;
declare @olddateadded date;
declare @newdateadded date;
declare @olddatemodified date;
declare @newdatemodified date;
declare @oldstatus int;
declare @newstatus int;
declare @message varchar(max);
declare @row_count_deliveries int;
declare @last_row_deliveries int;
declare @row_count_users int;
declare @last_row_users int;
declare @row_count_delivery_services int;
declare @last_row_delivery_services int;
declare @target_delivery int;
declare @target_user int;
declare @target_delivery_service int;
declare @target_delivery_price money;
declare @target_user_balance money;
declare @new_total_payment money;
declare @new_user_balance money;
declare @new_product_quantity int;
declare @updatedata table(LogDate date,LogMessage varchar(max),OldOID int,NewOID int,OldPID int,NewPID int,OldTID int, NewTID int,OldRPID int,NewRPID int,OldDTID int,NewDTID int,OldQuantity int,NewQuantity int,OldPrice money,NewPrice money,OldCID int,NewCID int,OldUID int,NewUID int,OldDateAdded date,NewDateAdded date,OldDateModified date,NewDateModified date,OldStatus int,NewStatus int);
declare @target_deliveries table(DeliveryCounter int identity primary key not null,OrderDeliveryID int unique not null);
declare @target_users table(UserCounter int identity primary key not null, UserID int unique not null,UserBalance money);
declare @target_delivery_services table(ServiceCounter int identity primary key not null, ServiceID int unique not null, ServicePrice money);
select @oldoid = deleted.OrderID from deleted inner join inserted on deleted.OrderID = inserted.OrderID;
select @newoid = inserted.OrderID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldpid = deleted.ProductID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newpid = inserted.ProductID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldtid = deleted.OrderTypeID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newtid = inserted.OrderTypeID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldrpid = deleted.ReplacementProductID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newrpid = inserted.ReplacementProductID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @olddtid = deleted.DiagnosticTypeID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newdtid = inserted.DiagnosticTypeID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldquantity = deleted.DesiredQuantity from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newquantity = inserted.DesiredQuantity from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldprice = deleted.OrderPrice from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newprice = inserted.OrderPrice from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldcid = deleted.ClientID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newcid = inserted.ClientID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @olduid = deleted.UserID from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newuid = inserted.UserID from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @olddateadded = deleted.DateAdded from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newdateadded = inserted.DateAdded from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @olddatemodified = deleted.DateModified from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newdatemodified = inserted.DateModified from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
select @oldstatus = deleted.OrderStatus from deleted inner join inserted on deleted.OrderID = inserted.OrderID where deleted.OrderID = @oldoid;
select @newstatus = inserted.OrderStatus from inserted inner join deleted on inserted.OrderID = deleted.OrderID where deleted.OrderID = @oldoid;
insert into @target_deliveries(OrderDeliveryID) select distinct OrderDeliveryID from OrderDeliveries where OrderID = @oldoid or OrderID = @newoid;
insert into @target_users(UserID,UserBalance) select distinct UserID,UserBalance from Users where UserID = @olduid or UserID = @newuid;
/* Make that so the deliveries have updated their prices if the price is updated */
if @newprice is not null and @newprice != @oldprice
begin
select top 1 @row_count_deliveries = DeliveryCounter from @target_deliveries order by DeliveryCounter asc;
select top 1 @last_row_deliveries = DeliveryCounter from @target_deliveries order by DeliveryCounter desc;
while @row_count_deliveries <= @last_row_deliveries
begin
select @target_delivery = OrderDeliveryID from @target_deliveries where DeliveryCounter = @row_count_deliveries order by DeliveryCounter asc;
insert into @target_delivery_services(ServiceID) select distinct ServiceID from OrderDeliveries where OrderDeliveryID = @target_delivery;
set @row_count_deliveries = @row_count_deliveries + 1;
end
select top 1 @row_count_delivery_services = ServiceCounter from @target_delivery_services order by ServiceCounter asc;
select top 1 @last_row_delivery_services = ServiceCounter from @target_delivery_services order by ServiceCounter desc;
while @row_count_delivery_services <= @last_row_delivery_services
begin
select @target_delivery_service = ServiceID from @target_delivery_services where ServiceCounter = @row_count_delivery_services order by ServiceCounter asc;
select @target_delivery_price = ServicePrice from DeliveryServices where ServiceID = @target_delivery_service;
update @target_delivery_services set ServicePrice = @target_delivery_price;
set @row_count_delivery_services = @row_count_delivery_services + 1;
end
select top 1 @row_count_deliveries = DeliveryCounter from @target_deliveries order by DeliveryCounter asc;
select top 1 @last_row_deliveries = DeliveryCounter from @target_deliveries order by DeliveryCounter desc;
select top 1 @row_count_delivery_services = ServiceCounter from @target_delivery_services order by ServiceCounter asc;
select top 1 @last_row_delivery_services = ServiceCounter from @target_delivery_services order by ServiceCounter desc;
while @row_count_deliveries <= @last_row_deliveries or @row_count_delivery_services <= @row_count_delivery_services
begin
select @target_delivery_service = ServiceID, @target_delivery_price = ServicePrice from @target_delivery_services where ServiceCounter = @row_count_delivery_services order by ServiceCounter asc;
select @target_delivery = OrderDeliveryID from @target_deliveries where DeliveryCounter = @row_count_deliveries order by DeliveryCounter asc;
set @new_total_payment = @newprice + @target_delivery_price;
update OrderDeliveries set TotalPayment = @new_total_payment, DateModified = getdate() where OrderDeliveryID = @target_delivery;
set @row_count_deliveries = @row_count_deliveries + 1;
set @row_count_delivery_services = @row_count_delivery_services + 1;
end
end
if (@oldtid = 0 or @newtid = 0) and @newtid != @oldtid
begin
set @newtid = 1;
update ProductOrders set OrderTypeID = @newtid where OrderID = @newoid or OrderID = @oldoid;
end
if @newstatus is not null and @newstatus != @oldstatus
begin
/* From here on is statuses and users */
if (@newstatus = 1 or @newstatus = 2 or @newstatus = 3 or @newstatus = 9) and (@oldstatus != 1 or @oldstatus != 2 or @oldstatus != 3 or @oldstatus != 7 or @oldstatus != 8 or @oldstatus != 9 or @oldstatus != 11)
begin
select top 1 @row_count_users = UserCounter from @target_users order by UserCounter asc;
select top 1 @last_row_users = UserCounter from @target_users order by UserCounter desc;
while @row_count_users <= @last_row_users
begin
select @target_user = UserID, @target_user_balance = UserBalance from @target_users where UserCounter = @row_count_users order by UserCounter asc;
set @new_user_balance = @target_user_balance + @newprice;
update Users set UserBalance = @new_user_balance where UserID = @target_user;
select @new_product_quantity = Quantity from Products where ProductID = @newpid;
set @new_product_quantity = @new_product_quantity - @newquantity;
update Products set Quantity = @new_product_quantity where ProductID = @newpid;
set @row_count_users = @row_count_users + 1;
end
end
else if (@newstatus = 7 or @newstatus = 8) and (@oldstatus = 1 or @oldstatus = 2 or @oldstatus = 3 or @oldstatus = 9 or @oldstatus != 7 or @oldstatus != 8)
begin
select top 1 @row_count_users = UserCounter from @target_users order by UserCounter asc;
select top 1 @last_row_users = UserCounter from @target_users order by UserCounter desc;
while @row_count_users <= @last_row_users
begin
select @target_user = UserID, @target_user_balance = UserBalance from @target_users where UserCounter = @row_count_users order by UserCounter asc;
set @new_user_balance = @target_user_balance - @newprice;
update Users set UserBalance = @new_user_balance where UserID = @target_user;
select @new_product_quantity = Quantity from Products where ProductID = @newpid;
set @new_product_quantity = @new_product_quantity + @newquantity;
update Products set Quantity = @new_product_quantity where ProductID = @newpid;
set @row_count_users = @row_count_users + 1;
end
end
else if (@newstatus = 10) and (@oldstatus = 1 or @oldstatus = 2 or @oldstatus = 3 or @oldstatus = 9 or @oldstatus != 10) and (@newrpid != 0 or @oldrpid = 0)
begin
select top 1 @row_count_users = UserCounter from @target_users order by UserCounter asc;
select top 1 @last_row_users = UserCounter from @target_users order by UserCounter desc;
while @row_count_users <= @last_row_users
begin
select @target_user = UserID, @target_user_balance = UserBalance from @target_users where UserCounter = @row_count_users order by UserCounter asc;
declare @new_target_price int;
select @new_target_price = Price from Products where ProductID = @newrpid or ProductID = @oldrpid;
set @new_user_balance = @target_user_balance - @newprice;
update Users set UserBalance = @new_user_balance where UserID = @target_user;
set @newprice = @new_target_price * @newquantity;
select @new_product_quantity = Quantity from Products where ProductID = @newpid;
set @new_product_quantity = @new_product_quantity + @newquantity;
update Products set Quantity = @new_product_quantity where ProductID = @newpid;
select @new_product_quantity = Quantity from Products where ProductID = @newrpid;
set @new_product_quantity = @new_product_quantity - @newquantity;
update Products set Quantity = @new_product_quantity where ProductID = @newrpid;
update ProductOrders set OrderPrice = @newprice where OrderID = @newoid or OrderID = @oldoid;
set @row_count_users = @row_count_users + 1;
end
end
else if (@newstatus = 10) and (@oldstatus != 10) and (@newrpid != 0 or @oldrpid = 0)
begin
set @newprice = @new_target_price * @newquantity;
update ProductOrders set OrderPrice = @newprice where OrderID = @newoid or OrderID = @oldoid;
end
else if (@newstatus = 11) and (@oldstatus != 11) and (@newtid = 2 or @oldtid = 2) and (@newdtid = 2 or @olddtid = 2)
begin
select @new_target_price = Price from Products where ProductID = @newrpid;
set @newprice = @new_target_price;
update ProductOrders set OrderPrice = @newprice where OrderID = @newoid or OrderID = @oldoid;
end
else if @newstatus = 4 or @newstatus = 5
begin
update ProductOrders set OrderStatus = 6, DateModified = getdate() where OrderID = @newoid;
end
end
set @message = '[UPDATE]An order was updated with the following ID: ' + try_convert(varchar(max), @oldoid) + '. The users and deliveries associated with that order were updated as well. ';
insert into @updatedata(LogDate,LogMessage,OldOID,NewOID,OldPID,NewPID,OldTID,NewTID,OldRPID,NewRPID,OldDTID,NewDTID,OldQuantity,NewQuantity,OldPrice,NewPrice,OldCID,NewCID,OldUID,NewUID,OldDateAdded,NewDateAdded,OldDateModified,NewDateModified,OldStatus,NewStatus) values(getdate(),@message,@oldoid,@newoid,@oldpid,@newpid,@oldtid,@newtid,@oldrpid,@newrpid,@oldtid,@newdtid,@oldquantity,@newquantity,@oldprice,@newprice,@oldcid,@newcid,@olduid,@newuid,@olddateadded,@newdateadded,@olddatemodified,@newdatemodified,@oldstatus,@newstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldoid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newoid) + ' , ';
set @data_string = @data_string + 'Old Product ID: ' + try_convert(varchar(max), @oldpid) + ' , ';
set @data_string = @data_string + 'New Product ID: ' + try_convert(varchar(max), @newpid) + ' , ';
set @data_string = @data_string + 'Old Order Type ID: ' + try_convert(varchar(max), @oldtid) + ' , ';
set @data_string = @data_string + 'New Order Type ID: ' + try_convert(varchar(max), @newtid) + ' , ';
set @data_string = @data_string + 'Old Replacement Product ID: ' + try_convert(varchar(max), @oldrpid) + ' , ';
set @data_string = @data_string + 'New Replacement Product ID: ' + try_convert(varchar(max), @newrpid) + ' , ';
set @data_string = @data_string + 'Old Diagnostic Type ID: ' + try_convert(varchar(max), @olddtid) + ' , ';
set @data_string = @data_string + 'New Diagnostic Type ID: ' + try_convert(varchar(max), @newdtid) + ' , ';
set @data_string = @data_string + 'Old Desired quantity: ' + try_convert(varchar(max), @oldquantity) + ' , ';
set @data_string = @data_string + 'New Desired quantity: ' + try_convert(varchar(max), @newquantity) + ' , ';
set @data_string = @data_string + 'Old Price: ' + try_convert(varchar(max), @oldprice) + ' , ';
set @data_string = @data_string + 'New Price: ' + try_convert(varchar(max), @newprice) + ' , ';
set @data_string = @data_string + 'Old Client ID: ' + try_convert(varchar(max), @oldcid) + ' , ';
set @data_string = @data_string + 'New Client ID: ' + try_convert(varchar(max), @newcid) + ' , ';
set @data_string = @data_string + 'Old User ID: ' + try_convert(varchar(max), @olduid) + ' , ';
set @data_string = @data_string + 'New User ID: ' + try_convert(varchar(max), @newuid) + ' , ';
set @data_string = @data_string + 'Old Date Added: ' + try_convert(varchar(max), @olddateadded) + ' , ';
set @data_string = @data_string + 'New Date Added: ' + try_convert(varchar(max), @newdateadded) + ' , ';
set @data_string = @data_string + 'Old Date Modified: ' + try_convert(varchar(max), @olddatemodified) + ' , ';
set @data_string = @data_string + 'New Date Modified: ' + try_convert(varchar(max), @newdatemodified) + ' , ';
set @data_string = @data_string + 'Old Status: ' + try_convert(varchar(max), @oldstatus) + '';
set @data_string = @data_string + 'New Status: ' + try_convert(varchar(max), @newstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger ProductOrder_OnDelete on ProductOrders for delete
as
begin
/* here O is for Order, P is for Product, C is for Client and U is for User */
set nocount on;
declare @deletedoid int;
declare @deletedpid int;
declare @deletedtid int;
declare @deletedrpid int;
declare @deleteddtid int;
declare @deletedquantity int;
declare @deletedprice money;
declare @deletedcid int;
declare @deleteduid int;
declare @deleteddateadded date;
declare @deleteddatemodified date;
declare @deletedstatus int;
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedOID int,DeletedPID int,DeletedTID int,DeletedRPID int,DeletedDTID int,DeletedQuantity int,DeletedPrice money,DeletedCID int,DeletedUID int,DeletedDateAdded date,DeletedDateModified date,DeletedStatus int);
select @deletedoid = OrderID from deleted;
select @deletedpid = ProductID from deleted;
select @deletedtid = OrderTypeID from deleted;
select @deletedrpid = ReplacementProductID from deleted;
select @deleteddtid = DiagnosticTypeID from deleted;
select @deletedquantity = DesiredQuantity from deleted;
select @deletedprice = OrderPrice from deleted;
select @deletedcid = ClientID from deleted;
select @deleteduid = UserID from deleted;
select @deleteddateadded = DateAdded from deleted;
select @deleteddatemodified = DateModified from deleted;
select @deletedstatus = OrderStatus from deleted;
set @message = '[DELETE]An order was removed with the following ID: ' + try_convert(varchar(max), @deletedoid);
insert into @deletedata(LogDate,LogMessage,DeletedOID,DeletedPID,DeletedTID,DeletedRPID,DeletedDTID,DeletedQuantity,DeletedPrice,DeletedCID,DeletedUID,DeletedDateAdded,DeletedDateModified,DeletedStatus) values(getdate(),@message,@deletedoid,@deletedpid,@deletedtid,@deletedrpid,@deleteddtid,@deletedquantity,@deletedprice,@deletedcid,@deleteduid,@deleteddateadded,@deleteddatemodified,@deletedstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedoid) + ' , ';
set @data_string = @data_string + 'Product ID: ' + try_convert(varchar(max), @deletedpid) + ' , ';
set @data_string = @data_string + 'Order Type ID: ' + try_convert(varchar(max), @deletedtid) + ' , ';
set @data_string = @data_string + 'Replacement Product ID: ' + try_convert(varchar(max), @deletedrpid) + ' , ';
set @data_string = @data_string + 'Diagnostic Type ID: ' + try_convert(varchar(max), @deleteddtid) + ' , ';
set @data_string = @data_string + 'Desired quantity: ' + try_convert(varchar(max), @deletedquantity) + ' , ';
set @data_string = @data_string + 'Price: ' + try_convert(varchar(max), @deletedprice) + ' , ';
set @data_string = @data_string + 'Client ID: ' + try_convert(varchar(max), @deletedcid) + ' , ';
set @data_string = @data_string + 'User ID: ' + try_convert(varchar(max), @deleteduid) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @deleteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @deleteddatemodified) + ' , ';
set @data_string = @data_string + 'Status: ' + try_convert(varchar(max), @deletedstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger OrderDelivery_OnInsert on OrderDeliveries for insert
as
begin
/* Here OD is for Order Delivery, O is for Order, DS is for Delivery Service and PM is for Payment Method */
set nocount on;
declare @insertedodid int;
declare @insertedoid int;
declare @inserteddsid int;
declare @insertedcargoid varchar(max);
declare @insertedtotalpayment money;
declare @insertedpmid int;
declare @inserteddateadded date;
declare @inserteddatemodified date;
declare @insertedstatus int;
declare @message varchar(max);
declare @insertdata table(LogDate date,LogMessage varchar(max),InsertedODID int,InsertedOID int,InsertedDSID int,InsertedCargoID varchar(max),InsertedTotalPayment money,InsertedPMID int,InsertedDateAdded date,InsertedDateModified date,InsertedStatus int);
select @insertedodid = OrderDeliveryID from inserted;
select @insertedoid = OrderID from inserted;
select @inserteddsid = ServiceID from inserted;
select @insertedcargoid = DeliveryCargoID from inserted;
select @insertedtotalpayment = TotalPayment from inserted;
select @insertedpmid = PaymentMethodID from inserted;
select @inserteddateadded = DateAdded from inserted;
select @inserteddatemodified = DateModified from inserted;
select @insertedstatus = DeliveryStatus from inserted;
set @message = '[INSERT]A delivery was added with the following ID: ' + try_convert(varchar(max), @insertedodid);
insert into @insertdata(LogDate,LogMessage,InsertedODID,InsertedOID,InsertedDSID,InsertedCargoID,InsertedTotalPayment,InsertedPMID,InsertedDateAdded,InsertedDateModified,InsertedStatus) values(getdate(), @message, @insertedodid, @insertedoid, @inserteddsid, @insertedcargoid, @insertedtotalpayment, @insertedpmid, @inserteddateadded, @inserteddatemodified, @insertedstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @insertedodid) + ' , ';
set @data_string = @data_string + 'Order ID: ' + try_convert(varchar(max), @insertedoid) + ' , ';
set @data_string = @data_string + 'Delivery Service ID: ' + try_convert(varchar(max), @inserteddsid) + ' , ';
set @data_string = @data_string + 'Payment Method ID: ' + try_convert(varchar(max), @insertedpmid) + ' , ';
set @data_string = @data_string + 'Delivery Cargo ID: ' + try_convert(varchar(max),@insertedcargoid) + ' , ';
set @data_string = @data_string + 'Total Price: ' + try_convert(varchar(max), @insertedtotalpayment) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @inserteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @inserteddatemodified) + ' , ';
set @data_string = @data_string + 'Status: ' + try_convert(varchar(max), @insertedstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger OrderDelivery_OnUpdate on OrderDeliveries for update
as
begin
/* Here OD is for Order Delivery, O is for Order, DS is for Delivery Service and PM is for Payment Method */
set nocount on;
declare @oldodid int;
declare @newodid int;
declare @oldoid int;
declare @newoid int;
declare @olddsid int;
declare @newdsid int;
declare @oldcargoid varchar(max);
declare @newcargoid varchar(max);
declare @oldtotalpayment money;
declare @newtotalpayment money;
declare @oldpmid int;
declare @newpmid int;
declare @olddateadded date;
declare @newdateadded date;
declare @olddatemodified date;
declare @newdatemodified date;
declare @oldstatus int;
declare @newstatus int;
declare @target_order int;
declare @row_count_order int;
declare @last_row_order int;
declare @message varchar(max);
declare @target_orders table(OrderCount int identity primary key not null, OrderID int unique not null);
declare @updatedata table(LogDate date,LogMessage varchar(max),OldODID int,NewODID int,OldOID int,NewOID int,OldDSID int,NewDSID int,OldCargoID varchar(max),NewCargoID varchar(max),OldTotalPayment money,NewTotalPayment money,OldPMID int,NewPMID int,OldDateAdded date,NewDateAdded date,OldDateModified date,NewDateModified date,OldStatus int,NewStatus int);
select @oldodid = deleted.OrderDeliveryID from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID;
select @newodid = inserted.OrderDeliveryID from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @oldoid = deleted.OrderID from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newoid = inserted.OrderID from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @olddsid = deleted.ServiceID from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newdsid = inserted.ServiceID from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @oldcargoid = deleted.DeliveryCargoID from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newcargoid = inserted.DeliveryCargoID from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @oldtotalpayment = deleted.TotalPayment from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newtotalpayment = inserted.TotalPayment from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @oldpmid = deleted.PaymentMethodID from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newpmid = inserted.PaymentMethodID from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @olddateadded = deleted.DateAdded from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newdateadded = inserted.DateAdded from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @olddatemodified = deleted.DateModified from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newdatemodified = inserted.DateModified from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @oldstatus = deleted.DeliveryStatus from deleted inner join inserted on deleted.OrderDeliveryID = inserted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
select @newstatus = inserted.DeliveryStatus from inserted inner join deleted on inserted.OrderDeliveryID = deleted.OrderDeliveryID where deleted.OrderDeliveryID = @oldodid;
if @newstatus is not null and @newstatus != @oldstatus
begin
/* here the statuses will be managed */
insert into @target_orders(OrderID) select distinct OrderID from ProductOrders where OrderID = @oldoid or OrderID = @newoid;
select top 1 @row_count_order = OrderCount from @target_orders order by OrderCount asc;
select top 1 @last_row_order = OrderCount from @target_orders order by OrderCount desc;
while @row_count_order <= @last_row_order
begin
select @target_order = OrderID from @target_orders where OrderCount = @row_count_order order by OrderCount asc;
if @newstatus = 1
begin
update ProductOrders set OrderStatus = 1, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 2
begin
update ProductOrders set OrderStatus = 3, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 3
begin
update ProductOrders set OrderStatus = 2, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 6
begin
update ProductOrders set OrderStatus = 6, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 7
begin
update ProductOrders set OrderStatus = 7, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 8
begin
update ProductOrders set OrderStatus = 8, DateModified = getdate() where OrderID = @target_order;
end
else if @newstatus = 9
begin
update ProductOrders set OrderStatus = 9, DateModified = getdate() where OrderID = @target_order;
end
set @row_count_order = @row_count_order + 1;
end
if @newstatus = 4 or @newstatus = 5
begin
update OrderDeliveries set DeliveryStatus = 6, DateModified = getdate() where OrderDeliveryID = @newodid;
end
end
set @message = '[UPDATE]A delivery was updated with the following ID: ' + try_convert(varchar(max), @oldodid);
insert into @updatedata(LogDate,LogMessage,OldODID,NewODID,OldOID,NewOID,OldDSID,NewDSID,OldCargoID,NewCargoID,OldTotalPayment,NewTotalPayment,OldPMID,NewPMID,OldDateAdded,NewDateAdded,OldDateModified,NewDateModified,OldStatus,NewStatus) values(getdate(), @message, @oldodid,@newodid,@oldoid,@newoid,@olddsid,@newdsid,@oldcargoid,@newcargoid,@oldtotalpayment,@newtotalpayment,@oldpmid,@newpmid,@olddateadded,@newdateadded,@olddatemodified,@newdatemodified,@oldstatus,@newstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'Old ID: ' + try_convert(varchar(max), @oldodid) + ' , ';
set @data_string = @data_string + 'New ID: ' + try_convert(varchar(max), @newodid) + ' , ';
set @data_string = @data_string + 'Old Order ID: ' + try_convert(varchar(max), @oldoid) + ' , ';
set @data_string = @data_string + 'New Order ID: ' + try_convert(varchar(max), @newoid) + ' , ';
set @data_string = @data_string + 'Old Delivery Service ID: ' + try_convert(varchar(max), @olddsid) + ' , ';
set @data_string = @data_string + 'New Delivery Service ID: ' + try_convert(varchar(max), @newdsid) + ' , ';
set @data_string = @data_string + 'Old Payment Method ID: ' + try_convert(varchar(max), @oldpmid) + ' , ';
set @data_string = @data_string + 'New Payment Method ID: ' + try_convert(varchar(max), @newpmid) + ' , ';
set @data_string = @data_string + 'Old Delivery Cargo ID: ' + try_convert(varchar(max),@oldcargoid) + ' , ';
set @data_string = @data_string + 'New Delivery Cargo ID: ' + try_convert(varchar(max),@newcargoid) + ' , ';
set @data_string = @data_string + 'Old Total Price: ' + try_convert(varchar(max), @oldtotalpayment) + ' , ';
set @data_string = @data_string + 'New Total Price: ' + try_convert(varchar(max), @newtotalpayment) + ' , ';
set @data_string = @data_string + 'Old Date Added: ' + try_convert(varchar(max), @olddateadded) + ' , ';
set @data_string = @data_string + 'New Date Added: ' + try_convert(varchar(max), @newdateadded) + ' , ';
set @data_string = @data_string + 'Old Date Modified: ' + try_convert(varchar(max), @olddatemodified) + ' , ';
set @data_string = @data_string + 'New Date Modified: ' + try_convert(varchar(max), @newdatemodified) + ' , ';
set @data_string = @data_string + 'Old Status: ' + try_convert(varchar(max), @oldstatus) + ' , ';
set @data_string = @data_string + 'New Status: ' + try_convert(varchar(max), @newstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

go
create or alter trigger OrderDelivery_OnDelete on OrderDeliveries for delete
as
begin
/* Here OD is for Order Delivery, O is for Order, DS is for Delivery Service and PM is for Payment Method */
set nocount on;
declare @deletedodid int;
declare @deletedoid int;
declare @deleteddsid int;
declare @deletedcargoid varchar(max);
declare @deletedtotalpayment money;
declare @deletedpmid int;
declare @deleteddateadded date;
declare @deleteddatemodified date;
declare @deletedstatus int;
declare @message varchar(max);
declare @deletedata table(LogDate date,LogMessage varchar(max),DeletedODID int,DeletedOID int,DeletedDSID int,DeletedCargoID varchar(max),DeletedTotalPayment money,DeletedPMID int,DeletedDateAdded date,DeletedDateModified date,DeletedStatus int);
select @deletedodid = OrderDeliveryID from deleted;
select @deletedoid = OrderID from deleted;
select @deleteddsid = ServiceID from deleted;
select @deletedcargoid = DeliveryCargoID from deleted;
select @deletedtotalpayment = TotalPayment from deleted;
select @deletedpmid = PaymentMethodID from deleted;
select @deleteddateadded = DateAdded from deleted;
select @deleteddatemodified = DateModified from deleted;
select @deletedstatus = DeliveryStatus from deleted;
set @message = '[DELETE]A delivery was removed with the following ID: ' + try_convert(varchar(max), @deletedodid);
insert into @deletedata(LogDate,LogMessage,DeletedODID,DeletedOID,DeletedDSID,DeletedCargoID,DeletedTotalPayment,DeletedPMID,DeletedDateAdded,DeletedDateModified,DeletedStatus) values(getdate(), @message, @deletedodid, @deletedoid, @deleteddsid, @deletedcargoid, @deletedtotalpayment, @deletedpmid, @deleteddateadded, @deleteddatemodified, @deletedstatus);
declare @data_string as varchar(max);
set @data_string = 'Additional details: ';
set @data_string = @data_string + 'ID: ' + try_convert(varchar(max), @deletedodid) + ' , ';
set @data_string = @data_string + 'Order ID: ' + try_convert(varchar(max), @deletedoid) + ' , ';
set @data_string = @data_string + 'Delivery Service ID: ' + try_convert(varchar(max), @deleteddsid) + ' , ';
set @data_string = @data_string + 'Payment Method ID: ' + try_convert(varchar(max), @deletedpmid) + ' , ';
set @data_string = @data_string + 'Delivery Cargo ID: ' + try_convert(varchar(max),@deletedcargoid) + ' , ';
set @data_string = @data_string + 'Total Price: ' + try_convert(varchar(max), @deletedtotalpayment) + ' , ';
set @data_string = @data_string + 'Date Added: ' + try_convert(varchar(max), @deleteddateadded) + ' , ';
set @data_string = @data_string + 'Date Modified: ' + try_convert(varchar(max), @deleteddatemodified) + ' , ';
set @data_string = @data_string + 'Status: ' + try_convert(varchar(max), @deletedstatus) + '';
set @message = @message + @data_string;
insert into Logs(LogDate,LogMessage) values (getdate(), @message);
end
go

DBCC checkident(ProductCategories,reseed,0);
DBCC checkident(ProductBrands,reseed,0);
DBCC checkident(DeliveryServices,reseed, 0);
DBCC checkident(PaymentMethods,reseed,0);
DBCC checkident(Users,reseed, 0);
DBCC checkident(Clients,reseed,0);
DBCC checkident(Products,reseed,0);
DBCC checkident(ProductImages,reseed,0);
DBCC checkident(ProductOrders, reseed, 0);
DBCC checkident(OrderDeliveries,reseed, 0);

/* NEVER DELETE THE DUMMY VALUES */
set identity_insert ProductBrands on;
insert into ProductBrands(BrandID,BrandName) values(0,'dummy');
set identity_insert ProductBrands off;
set identity_insert ProductCategories on;
insert into ProductCategories(CategoryID,CategoryName) values(0,'dummy');
set identity_insert ProductCategories off;
set identity_insert DeliveryServices on;
insert into DeliveryServices(ServiceID,ServiceName,ServicePrice) values(0,'dummy',0.0);
set identity_insert DeliveryServices off;
set identity_insert OrderTypes on;
insert into OrderTypes(OrderTypeID,TypeName) values( 0, 'dummy');
insert into OrderTypes(OrderTypeID,TypeName) values(1,'shop');
insert into OrderTypes(OrderTypeID,TypeName) values(2,'service_center');
set identity_insert OrderTypes off;
set identity_insert DiagnosticTypes on;
insert into DiagnosticTypes(TypeID,TypeName,TypePrice) values(0,'dummy',0);
insert into DiagnosticTypes(TypeID,TypeName,TypePrice) values(1,'repair',120);
insert into DiagnosticTypes(TypeID,TypeName,TypePrice) values(2, 'product_replacement',0);
set identity_insert DiagnosticTypes off;
set identity_insert PaymentMethods on;
insert into PaymentMethods(PaymentMethodID,PaymentMethodName) values (0, 'dummy');
set identity_insert PaymentMethods off;
set identity_insert Users on;
insert into Users(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,UserBalance,DateOfRegister,UserProfilePic,isAdmin,isWorker,isClient) values(0,'dummy','','','','',0,'',0x,0,0,0);
set identity_insert Users off;
set identity_insert Clients on;
insert into Clients(ClientID,UserID,ClientName,ClientEmail,ClientPhone,ClientAddress,ClientBalance,ClientProfilePic) values(0,0,'dummy','','','',0,0x);
set identity_insert Clients off;
set identity_insert Products on;
insert into Products(ProductID,ProductCategoryID,ProductBrandID,ProductName,ProductDescription,Quantity,Price,ProductArtID,ProductStorageLocation,DateAdded,DateModified) values(0,0,0,'dummy','',0,0,'','','','');
set identity_insert Products off;
insert into ProductImages(TargetProductID,ImageName,Picture) values(0,'PRODUCT_0_IMAGE_0',0x);
set identity_insert ProductOrders on;
insert into ProductOrders(OrderID,ProductID,DesiredQuantity,OrderPrice,ClientID,UserID,DateAdded,DateModified,OrderStatus) values(0,0,0,0,0,0,'','',0);
set identity_insert ProductOrders off;
set identity_insert OrderDeliveries on;
insert into OrderDeliveries(OrderDeliveryID,OrderID,ServiceID,DeliveryCargoID,TotalPayment,PaymentMethodID,DateAdded,DateModified,DeliveryStatus) values(0,0,0,'',0,0,'','',0);
set identity_insert OrderDeliveries off;


/* These are for testing purposes 
declare @blankpfp as varbinary(max);
select @blankpfp = convert(varbinary(max),'');
exec RegisterClient @username='radoslav_dimitrov99',@displayname='Radoslav Dimitrov',@email='radoslavdimitrov9999@gmail.com',@password='1234567',@phone='0898308347',@address='8000 Burgas, j.k. Slaveykov, bl. 25, vh. B, et.12, ap.35',@registerdate='2023-10-1', @profilepic=@blankpfp;
exec RegisterClient @username='stavrev98',@displayname='Stoyko Stavrev',@email='stavrev98@gmail.com',@password='0000',@phone='089999',@address='Karnobat, ul. Subi Dimitrow 25',@registerdate='2023-10-7', @profilepic=0x;
exec RegisterUser @username='v.tekeliew101',@displayname='Weselin Tekeliew',@email='v.tekeliev@gmail.com',@password='123456',@phone='098 897 5017',@registerdate='2023-10-1', @profilepic = @blankpfp, @isadmin = 1, @isclient = 0, @isworker = 1;
exec AddClientWithoutRegistering @clientname = 'Grigor Grigorov', @email = 'grisha@mail.bg', @phone='0857474',@address='Sofia, kv. Nadezhda 4',@profilepic=0x;
select * from Users where UserName = 'v.tekeliew101';

exec GetUserByUserName @username = 'v.tekeliew101';
exec GetUserByDisplayName @displayname = 'Weselin Tekeliew';
exec GetUserByEmail @email='v.tekeliev@gmail.com';
exec GetUserByPhone @phone = '098 897 5017';
exec GetAllUsers;
exec GetClientByName @clientname = 'Radoslav Dimitrov';
exec GetClientByEmail @clientemail = 'radoslavdimitrov9999@gmail.com';
exec GetClientByPhone @clientphone = '0898308347';
exec GetAllClients;
exec GetAllProducts;
exec GetAllOrders;
exec GetAllordersExtended;
exec GetOrderByIDExtended @id = 0;
exec GetOrderByProductName @productname = 'dummy';
update Products set ProductArtID = 'PS504939' where ProductID = 7;

exec AddPaymentMethod @method_name='Payment On Delivery';
exec AddPaymentMethod @method_name='Credit Card';
exec AddPaymentMethod @method_name='Direct Payment';
exec AddProductCategory @categoryname = 'Home';
exec AddProductBrand @brandname = 'Sony';
exec AddDeliveryService @name='Econt',@price=7.32;
exec AddProduct 2,0,'Sofa','A comfortable sofa',40,150.0,'','';
exec AddProductExtended 'Home','dummy','Armchair','A very comfortable armchair', 24,120.0,'','';
insert into Products(ProductCategoryID,ProductBrandID,ProductName,ProductDescription,Quantity,Price,DateAdded,DateModified) values(2,1,'Playstation 5 Pro', 'Playstation 5 Pro', 140, 1000, getdate(), getdate());
exec AddOrder 2, 2, 25.0, 4,8,0;
exec AddOrder 2, 2, 25.0, 4,8,1;
exec AddOrderExtended 'Armchair', 2, 240.0, 'Radoslav Dimitrov', 'Weselin Tekeliew',0;
exec AddOrderExtended 'Armchair', 2, 240.0, 'Radoslav Dimitrov', 'Weselin Tekeliew',1;
exec AddOrderDelivery 5, 2, '',6;
exec AddOrderDeliveryExtended 'Armchair','Econt','','Direct Payment';
exec AddOrderDeliveryExtended 'Armchair','Econt','','Payment On Delivery';
declare @targetuser as UserTable;
insert into @targetuser(UserID,UserName,UserDisplayName,UserEmail,UserPassword,UserPhone,DateOfRegister,UserProfilePic,isAdmin,isClient,isWorker) exec GetUserByUserName @username = 'v.tekeliew101';
exec UpdateUserByID @id=2, @new_user_name='radoslav_dimitrov',@new_display_name='',@new_email='',@new_password='',@new_phone='0898308347',@new_balance=1500,@new_profile_pic=0x2345,@new_is_admin=0,@new_is_worker=0,@new_is_client=1;
update Users set UserBalance = 1500 where UserID = 2;
select * from @targetuser;
select * from Users;
select * from DeliveryServices;
select * from OrderTypes;
select * from DiagnosticTypes;
select * from PaymentMethods;
select * from ProductCategories;
select * from ProductBrands;
select * from Clients;
select * from Products;
select * from ProductImages;
select * from ProductOrders;
select * from OrderDeliveries;


delete from PaymentMethods where PaymentMethodName = 'dummy';
delete from DeliveryServices where ServiceName = 'dummy' and  ServiceID = 1;
delete from Users where UserID = 3;
delete from Clients where UserID = 6;
delete from Users where UserID = 6;
delete from OrderDeliveries where OrderID = 3;
 */

/* uncomment and execute this if you mess up the constraints on the tables

alter table OrderDeliveries add constraint FK_OrderID_OrderDelivery foreign key(OrderID) references ProductOrders(OrderID) on update cascade on delete cascade ;
alter table Orderdeliveries add constraint FK_ServiceID_OrderDelivery foreign key (ServiceID) references DeliveryServices(ServiceID) on update cascade;
alter table OrderDeliveries add constraint FK_PaymentMethodID_OrderDelivery foreign key(PaymentMethodID) references PaymentMethods(PaymentMethodID) on update cascade;

alter table ProductOrders add constraint FK_ProductID_ProductOrder foreign key(ProductID) references Products(ProductID);
alter table ProductOrders add constraint FK_ClientID_ProductOrder foreign key(ClientID) references Clients(ClientID);
alter table ProductOrders add constraint FK_UserID_ProductOrder foreign key(UserID) references Users(UserID);


select * from Products inner join ProductBrands on Products.ProductBrandID = ProductBrands.BrandID;

alter table Products add constraint FK_ProductCategoryID_Product foreign key(ProductCategoryID) references ProductCategories(CategoryID) on update cascade;
alter table Products add constraint FK_ProductBrandID_Product foreign key(ProductBrandID) references ProductBrands(BrandID) on update cascade;

alter table ProductImages add constraint FK_TargetProductID_ProductImage foreign key(TargetProductID) references Products(ProductID) on update cascade on delete cascade;
 */

/* This was for testing purposes as well
delete from Clients;
delete from Users;
delete from PaymentMethods;
delete from ProductCategories;
delete from Products;
delete from ProductImages;
delete from ProductOrders;
delete from DeliveryServices;
delete from OrderDeliveries;
*/