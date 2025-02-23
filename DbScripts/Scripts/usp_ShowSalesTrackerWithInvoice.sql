USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ShowSalesTrackerWithInvoice]    Script Date: 7/8/2020 4:40:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 17-03-2020
-- Last modified: NA
-- Description:	Display daily sales data for page ShowSalesTrackerWithInvoice.
-- Usage: EXEC usp_ShowSalesTrackerWithInvoice @OrderBy='', @OrderByDirection='ASC',@Length=10,@From=0,@DistributorId=0,@WorkshopId=0,@StartDate=Null,@EndDate=Null,@FromAmt=Null,@ToAmt=Null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_ShowSalesTrackerWithInvoice]
    @OrderBy NVARCHAR(200),
	@OrderByDirection NVARCHAR(20),
    @Length int=10, 
	@From int, 
    @DistributorId int, 
	@WorkshopId int, 
	@StartDate Date=Null,
	@EndDate Date=Null,
	@FromAmt nvarchar(100)=Null,
	@ToAmt nvarchar(100)=Null
AS
BEGIN

     ------- Set length to
	 Declare @To int=0; 
	 set @To=@From+@Length;
	 set @From=@From+1;
	 

 --- Set Order By Column if Null
 if(@OrderBy='' or @OrderByDirection='')
 Begin
  set @OrderBy='DailySalesTrackerId';
  set @OrderByDirection='Asc';
 End


 declare @sql nvarchar(max)            
 set @sql=';WITH search            
			 AS            
				(select Num = ROW_NUMBER() OVER (order by '+cast(@OrderBy as nvarchar) +' '+cast(@OrderByDirection as nvarchar) +'),TotalRecords=Count(*) OVER(),          
						 * From DailySalesTrackerWithInvoiceData        
									 where 1=1'         
           
 if(@StartDate is not null and @StartDate!='')            
 begin            
	set @sql=@sql + ' and CreatedDate >='''+ cast(@StartDate as nvarchar) +''''
 end       

 if(@EndDate is not null and @EndDate!='')            
 begin            
	set @sql=@sql + ' and CreatedDate <='''+ cast(@EndDate as nvarchar) +''''          
 end         
          
  if(@DistributorId>0)            
  begin            
	set @sql=@sql + ' and DistributorId= '''+ cast(@DistributorId as nvarchar) +''''  
  end   
  
  if(@WorkShopId>0)        
  begin        
  set @sql=@sql + ' and WorkShopId= '''+ cast(@WorkShopId as nvarchar) +''''  
  end          

  if(@FromAmt is not null and @FromAmt!='')        
  begin        
  set @sql=@sql + ' and cast(NetRetailSelling as decimal)>= '''+ cast(@FromAmt as nvarchar) +''''       
  end  

  if(@ToAmt is not null and @ToAmt!='')        
  begin        
  set @sql=@sql + ' and cast(NetRetailSelling as decimal)<= '''+cast(@ToAmt as nvarchar) +''''       
  end  

           
             
 set @sql=@sql + ')'  
          
 set @sql=@sql+' select top('+cast(@Length as nvarchar)+') * from search'           
 set @sql=@sql+' where Num between '+cast(@From as nvarchar)+' and '+cast(@To as nvarchar)            
 
 print @sql              
 exec(@sql)             

   
END;



