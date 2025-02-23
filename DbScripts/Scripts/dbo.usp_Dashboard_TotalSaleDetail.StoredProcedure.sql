USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_TotalSaleDetail_Temp]    Script Date: 6-11-2020 2:35:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================    
-- Author: Lokesh Choudhary    
-- Create date: 02-03-2020    
-- Last modified: 18-09-2020    
-- Description: Display customer Sale detail for all workshops when @TargetWorkshopIdsStr is null else for target workshops .    
-- Usage: EXEC usp_Dashboard_TotalSaleDetail @Action=1, @Role='Distributor', @UserId='385550b1-b537-4b35-8123-81b28599b3f6', @StartDate='2020-01-01', @EndDate='2020-09-18',@CoDealerDist='CO-DEALER,DISTRIBUTOR,CO-DISTRIBUTOR'    
-- ===================================================================================    

ALTER PROCEDURE [dbo].[usp_Dashboard_TotalSaleDetail]    
    @Action INT,    
    @Role NVARCHAR(100),    
    @UserId NVARCHAR(128),    
    @StartDate DATE,    
    @EndDate DATE,    
    @CoDealerDist NVARCHAR(max)=NULL    
AS    
BEGIN   
   -- Get distributorId using UserId and role
   Declare @DistributorId int=0
   set @DistributorId=dbo.getDistributorId(@UserId,@Role)

    IF (@Action = 1)  
    BEGIN  
   -- Declare needed parameters  
     Declare @CurrentSale decimal(18,2)=0,@PreviousSale decimal(18,2)=0,@CoDealerOrDistSale decimal(18,2)=0

    Declare @prevYrStartDate DATE,@prvYrEndDate DATE,@TotalSales decimal(18,2)=0
  
 -- Set previous year startDate and endDate  
 set @prevYrStartDate=DATEADD(year, -1, @StartDate)  
 set @prvYrEndDate=DATEADD(year, -1, @EndDate)  

  -- Fetch sale for role SuperAdmin  
  IF (@Role = 'SuperAdmin')  
    BEGIN  

	-- Fetch Total Sales without any ConsPartyTypeDesc

	select @TotalSales=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData 
	where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate  

 -- Set currentSale for role SuperAdmin  
    select @CurrentSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate  
    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))  
  
 -- Set previousSale for role SuperAdmin  
    select @PreviousSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate  
    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 

	select @CoDealerOrDistSale = @TotalSales - @CurrentSale
      
    END;  

    ELSE IF (@Role = 'Distributor')  
    BEGIN  

	select @TotalSales=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData 
	where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate and DistributorId=@DistributorId

    select @CurrentSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate  
    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) and DistributorId=@DistributorId  
  
    select @PreviousSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate  
    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) and DistributorId=@DistributorId  
  
   select @CoDealerOrDistSale = @TotalSales - @CurrentSale
      
    END;  

    ELSE IF (@Role = 'RoIncharge')  
    BEGIN  
 -- Get current sale based on workshopId using UserId  
        select @CurrentSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
        from dbo.DailySalesTrackerWithInvoiceData d  
        inner join dbo.WorkShops w on w.WorkShopId=d.WorkShopId  
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId  
        where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate  
       and d.ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))  
       and do.UserId=@UserId and d.DistributorId=@DistributorId   
  
        select @PreviousSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
        from dbo.DailySalesTrackerWithInvoiceData d  
        inner join dbo.WorkShops w on w.WorkShopId=d.WorkShopId  
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId  
        where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @prevYrStartDate and @prvYrEndDate  
        and d.ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))  
        and do.UserId=@UserId and d.DistributorId=@DistributorId 
  
        select @TotalSales=sum(cast (d.NetRetailSelling as decimal(18,2)))  
        from dbo.DailySalesTrackerWithInvoiceData d  
        inner join dbo.WorkShops w on w.WorkShopId=d.WorkShopId  
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId  
        where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate  
        and do.UserId=@UserId and d.DistributorId=@DistributorId   

		select @CoDealerOrDistSale = @TotalSales - @CurrentSale

    END;
	
    ELSE IF (@Role = 'SalesExecutive')  
    BEGIN  
 -- Get current sale based on workshopId using UserId  
       select @CurrentSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
       from dbo.DailySalesTrackerWithInvoiceData d  
       inner join dbo.SalesExecutiveWorkshop sw on sw.WorkShopId=d.WorkShopId  
       where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate  
       and d.ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))  
       and sw.UserId=@UserId and d.DistributorId=@DistributorId   
  
    select @PreviousSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
       from dbo.DailySalesTrackerWithInvoiceData d  
       inner join dbo.SalesExecutiveWorkshop sw on sw.WorkShopId=d.WorkShopId  
       where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @prevYrStartDate and @prvYrEndDate  
       and d.ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))  
       and sw.UserId=@UserId and d.DistributorId=@DistributorId   
  
    select @TotalSales=sum(cast (d.NetRetailSelling as decimal(18,2)))  
       from dbo.DailySalesTrackerWithInvoiceData d  
       inner join dbo.SalesExecutiveWorkshop sw on sw.WorkShopId=d.WorkShopId  
       where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate
       and sw.UserId=@UserId and d.DistributorId=@DistributorId   

	   select @CoDealerOrDistSale = @TotalSales - @CurrentSale

    END;  

   select @CurrentSale as CurrentSale,@PreviousSale as PreviousSale,@CoDealerOrDistSale as CoDealerOrDistSale  
         
    END;  
	-- show only currentSale
	IF (@Action = 2)  
    BEGIN 
	  
  -- Fetch sale for role SuperAdmin  
  IF (@Role = 'SuperAdmin')  
    BEGIN  
	 
    select @CurrentSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate 
      
    END;  

    ELSE IF (@Role = 'Distributor')  
    BEGIN   
	
    select @CurrentSale=sum(cast (NetRetailSelling as decimal(18,2)))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate and DistributorId=@DistributorId  
      
    END;  

    ELSE IF (@Role = 'RoIncharge')  
    BEGIN  
 
        select @CurrentSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
        from dbo.DailySalesTrackerWithInvoiceData d  
        inner join dbo.WorkShops w on w.WorkShopId=d.WorkShopId  
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId  
        where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate 
       and do.UserId=@UserId and d.DistributorId=@DistributorId  

    END;
	
    ELSE IF (@Role = 'SalesExecutive')  
    BEGIN  
 
       select @CurrentSale=sum(cast (d.NetRetailSelling as decimal(18,2)))  
       from dbo.DailySalesTrackerWithInvoiceData d  
       inner join dbo.SalesExecutiveWorkshop sw on sw.WorkShopId=d.WorkShopId  
       where  d.NetRetailSelling Is NOT NULL and d.CreatedDate between @StartDate and @EndDate  
       and sw.UserId=@UserId and d.DistributorId=@DistributorId   

    END;  

   select @CurrentSale as CurrentSale
         
	END
     
END;  
  
  
  