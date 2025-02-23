USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_TargetWorkshops]    Script Date: 9/19/2020 12:35:43 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 02-03-2020
-- Last modified: 18-09-2020
-- Description:	Display customer current schemes Target details.
-- Usage: EXEC usp_Dashboard_TargetWorkshops @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-02-25',@CoDealerDist='CO-DEALER,DISTRIBUTOR,CO-DISTRIBUTOR'
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_TargetWorkshops]
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
    @StartDate DATE,
    @EndDate DATE,
	@CoDealerDist NVARCHAR(max)=NULL  
AS
BEGIN
	
	-- Declare needed parameters
    Declare @CurrentSale decimal(18,2)=0,@PreviousSale decimal(18,2)=0,@CoDealerOrDistSale decimal(18,2)=0,
	@NewTarget decimal(18,2)=0,@prevYrStartDate DATE,@prvYrEndDate DATE,@DistributorId int=0

	-- Set previous year startDate and endDate
	set @prevYrStartDate=DATEADD(year, -1, @StartDate)
	set @prvYrEndDate=DATEADD(year, -1, @EndDate)

	-- Declare Temp table
	 Declare @TEMP table (TargetWorkshopId int,NewTarget decimal(18,2))

	 -- Fetch and set target workshopIds and NewTargets for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN
	-- set value in tempTable
	 insert into @temp
     select WorkShopId,cast(NewTarget as decimal) from dbo.TargetWorkShop 
     where SchemeId in (select SchemeId from dbo.Schemes where StartDate>=@StartDate and (StartDate <=@EndDate or EndDate >=@StartDate) and EndDate <= @EndDate)
     and NewTarget<>''and NewTarget<>'0'
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Set distributorId using UserId
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	-- set value in tempTable
	insert into @temp
    select t.WorkShopId,cast(t.NewTarget as decimal) from dbo.TargetWorkShop t
	 inner join dbo.DistributorWorkShops ds on ds.WorkShopId=t.WorkShopId
     where t.SchemeId in (select SchemeId from dbo.Schemes where StartDate>=@StartDate and (StartDate <=@EndDate or EndDate >=@StartDate) and EndDate <= @EndDate and DistributorId=@DistributorId)
     and t.NewTarget<>''and NewTarget<>'0' and ds.DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get target workshops based on workshopId using UserId
       
	-- set value in tempTable 
     insert into @temp
     select t.WorkShopId,cast(t.NewTarget as decimal) from dbo.TargetWorkShop t
	 inner join dbo.WorkShops w on w.WorkShopId=t.WorkShopId
	 inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId
     where t.SchemeId in (select SchemeId from dbo.Schemes where StartDate>=@StartDate and (StartDate <=@EndDate or EndDate >=@StartDate) and EndDate <= @EndDate and DistributorId=do.DistributorId)
     and t.NewTarget<>''and NewTarget<>'0' and do.UserId=@UserId

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get  distributorId using UserId
      select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	-- set value in tempTable
     insert into @temp
     select t.WorkShopId,cast(t.NewTarget as decimal) from dbo.TargetWorkShop t
	 inner join dbo.SalesExecutiveWorkshop sw on sw.WorkShopId=t.WorkShopId
     where t.SchemeId in (select SchemeId from dbo.Schemes where StartDate>=@StartDate and (StartDate <=@EndDate or EndDate >=@StartDate) and EndDate <= @EndDate and DistributorId=@DistributorId)
     and t.NewTarget<>''and NewTarget<>'0' and sw.UserId=@UserId
	  
    END;

	-- Set currentSale for Target Workshops
       select @CurrentSale=sum(cast (NetRetailSelling as decimal))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))
	   and WorkShopId in (SELECT TargetWorkshopId FROM @TEMP group by TargetWorkshopId)

	-- Set previousSale for Target Workshops
	   select @PreviousSale=sum(cast (NetRetailSelling as decimal))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))
	   and WorkShopId in (SELECT TargetWorkshopId FROM @TEMP group by TargetWorkshopId)

	-- Set Co_Dealer and Distributor sale for Target Workshops
	   select @CoDealerOrDistSale=sum(cast (NetRetailSelling as decimal))  from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc in(select * from dbo.split(@CoDealerDist,','))
	   and WorkShopId in (SELECT TargetWorkshopId FROM @TEMP group by TargetWorkshopId)


	   -- Print Sp response 

	   select @CurrentSale as CurrentSale,@PreviousSale as PreviousSale,@CoDealerOrDistSale as CoDealerOrDistSale,
	   (SELECT sum(NewTarget) FROM @TEMP)as NewTarget
   
END;