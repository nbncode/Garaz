USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[Sp_DailySalesData]    Script Date: 9/15/2020 4:02:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Sp_DailySalesData]            
 @Role nvarchar(500)=null,            
 @UserId nvarchar(500)=null,          
 @Index int=null,            
 @Number int=10,          
 @StartDate datetime = null,          
 @EndDate datetime = null          
AS            
BEGIN            
--exec Sp_DailySalesData @UserId='30f16e5c-9b6d-4419-8ada-61b1f4f92119', @Role='SuperAdmin',@Index=1,@StartDate='2019-01-01', @EndDate='2019-10-30'          
 declare @From int            
 declare @To int            
           
          
 if(@Index!=-1)          
 begin          
  if(@Index=1)            
  begin            
   set @From= 1            
   set @To = @From + @Number            
  end            
  else             
  begin            
   set @From= (@Number * (@Index-1))            
   set @To = @From + @Number            
   set @From = @From + 1            
  end          
 end          
          
 declare @DistributorId int            
 declare @WorkShopId int            
        
 set @DistributorId=dbo.getDistributorId(@UserId, @Role)           
 set @WorkShopId=dbo.getWorkshopId(@UserId, @Role)         
        
 declare @sql nvarchar(max)            
 set @sql=';WITH search            
			 AS            
				(select Num = ROW_NUMBER() OVER (order by p.GroupName,nt.CreatedDate),Total=Count(*) OVER(),          
						 p.GroupName,p.GroupId,nt.Year,nt.MonthName,nt.TotalQty ,nt.CreatedDate
						 from ProductGroup p     
						 inner join Bestseller bs on bs.groupId=p.groupid          
						 left join (SELECT max(CreatedDate)CreatedDate,
						 YEAR(CreatedDate) [Year],           
										 Convert(char(3), DATENAME(month,CreatedDate), 0) [MonthName],GroupId,          
										 SUM(Cast(NetRetailQty As decimal))TotalQty          
									 FROM DailySalesTrackerWithInvoiceData          
									 where GroupId is not null and NetRetailQty IS NOT NULL and Cast(NetRetailQty As decimal) > 0'          
           
 if(@StartDate is not null and @EndDate is not null)            
 begin            
	set @sql=@sql + ' and CreatedDate between cast('''+cast(@StartDate as nvarchar) +''' As datetime) and cast('''+cast(@EndDate as nvarchar) +''' As datetime)'          
 end     
       
 /* Role wise sale data filter*/   
	  
  if(@Role<>'SuperAdmin')            
  begin            
	set @sql=@sql + ' and DistributorId= '+cast(@DistributorId as nvarchar)
	
	if(@Role= 'RoIncharge')        
	   begin  
	   set @sql=@sql + ' and WorkShopId in(select w.WorkshopId from dbo.WorkShops w
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId
        where do.UserId='+N''''+cast(@UserId as nvarchar(500))+N''')'        
	   end  
	   
	   ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	  set @sql=@sql + ' and WorkShopId in(select WorkshopId from dbo.SalesExecutiveWorkshop
       where UserId='+N''''+cast(@UserId as nvarchar(500))+N''')'  
    END;             
     Else if(@WorkShopId is not null)        
	   begin        
	   set @sql=@sql + ' and WorkShopId= '+cast(@WorkShopId as nvarchar)        
	   end        
  end           
          
          
 set @sql=@sql + ' GROUP BY YEAR(CreatedDate), MONTH(CreatedDate),           
					DATENAME(MONTH, CreatedDate),GroupId)nt          
					on p.GroupId=nt.GroupId'           
   set @sql=@sql + ' where 1=1'            
          
 /* Role wise best part filter*/            
 if(@Role<>'SuperAdmin')            
 begin            
  set @sql=@sql + ' and bs.DistributorId= '+cast(@DistributorId as nvarchar)   
 end           
           
             
 set @sql=@sql + ')'            
 if(@Index!=-1)          
 begin            
 set @sql=@sql+' select top('+cast(@Number as nvarchar)+') *from search'           
 set @sql=@sql+' where Num between '+cast(@From as nvarchar)+' and '+cast(@To as nvarchar)  
 set @sql=@sql+' order by GroupName,CreatedDate '          
 end          
 else          
 begin          
 set @sql=@sql+' select *from search order by GroupName,CreatedDate'           
 end          
 print @sql              
 exec(@sql)             
            
END