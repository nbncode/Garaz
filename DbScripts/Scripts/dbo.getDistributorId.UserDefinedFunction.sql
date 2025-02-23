USE [Garaaz]
GO
/****** Object:  UserDefinedFunction [dbo].[getDistributorId]    Script Date: 16-3-2020 11:13:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[getDistributorId]  
(  
    @userID nvarchar(500),  
    @role nvarchar(500)  
)  
RETURNS nvarchar(500)  
AS  
BEGIN  
    Declare @logid nvarchar(500);  
  
 if(@role='Distributor' or @role='DistributorUsers')  
 begin  
   select top 1 @logid=DistributorId from DistributorUser  
   where Userid=@Userid  
 end  
 else if(@role='Workshop' or @role='WorkshopUsers')  
 begin  
   declare @workshopId nvarchar(500)  
     
   set @workshopId = dbo.getWorkshopId(@userID,@role)  
  
   select top 1 @logid=d.DistributorId from Distributors d  
   inner join DistributorWorkShops  w  
   on d.DistributorId= w.DistributorId  
   where w.WorkShopId=@workshopId 
   --and w.UserId=@userID  
 end  
 else if(@role='SuperAdmin')  
 begin  
   select top 1 @logid=DistributorId from Distributors  
 end
 else if(exists(select *from AspNetUserRoles r
	inner join AspNetRoles ar
	on r.RoleId=ar.Id
	where ar.Name='RoIncharge' and UserId=@userID))
	begin
		select top 1 @logid=DistributorId from DistributorsOutlets  
		where UserId=@userID		
	end
  else if(exists(select *from AspNetUserRoles r
	inner join AspNetRoles ar
	on r.RoleId=ar.Id
	where ar.Name='SalesExecutive' and UserId=@userID))
	begin
		select top 1 @logid=DistributorId from DistributorUser  
		where UserId=@userID		
	end
  
    RETURN  @logid  
  
END  
GO
