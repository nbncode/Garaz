USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_Sale_CategoryWise]    Script Date: 6-11-2020 3:15:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 04-03-2020
-- Last modified: 24-09-2020
-- Description:	Display customer Sale Filter by ProductGroup  and Sub group wise.
-- Usage: EXEC usp_Dashboard_Sale_CategoryWise @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2018-01-01', @EndDate='2019-02-25',@GroupName=Null,@CoDealerDist='CO-DEALER,DISTRIBUTOR,CO-DISTRIBUTOR'
-- Usage: EXEC usp_Dashboard_Sale_CategoryWise @Action=2, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2018-01-01', @EndDate='2019-02-25',@GroupName='All',@CoDealerDist='CO-DEALER,DISTRIBUTOR,CO-DISTRIBUTOR'
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_Sale_CategoryWise]
    @Action INT,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
    @StartDate DATE,
    @EndDate DATE,
	@GroupName NVARCHAR(200),
	@CoDealerDist NVARCHAR(max)=NULL
AS
BEGIN

	-- Declare Tables for current and previous sale
   DECLARE @currentSale TABLE (LocCode nvarchar(50),SalesUserId nvarchar(128),ConsPartyTypeDesc nvarchar(200),PartCategory NVARCHAR(20),NetRetailSelling decimal(18,2))
   DECLARE @DistributorId int=0

   -- Get distributorId using UserId and role
    set @DistributorId=dbo.getDistributorId(@UserId,@Role)

    IF (@Action = 1)
    BEGIN
	
	 Declare @prevYrStartDate DATE,@prvYrEndDate DATE

	-- Set previous year startDate and endDate
	set @prevYrStartDate=DATEADD(year, -1, @StartDate)
	set @prvYrEndDate=DATEADD(year, -1, @EndDate)

	-- Declare Tables for previous sale
	DECLARE @previousSale TABLE (PartCategory NVARCHAR(10),NetRetailSelling decimal(18,2))

	 -- Fetch sale for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN

	-- Set current sale data in temp table @currentSale
	insert into @currentSale (PartCategory,NetRetailSelling)
	select PartCategory,sum(cast(NetRetailSelling as decimal(18,2))) from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))
	   and PartCategory Is NOT NULL  group by PartCategory
	  
    -- Set previous sale data in temp table @previousSale
	insert into @previousSale (PartCategory,NetRetailSelling)
	select PartCategory,sum(cast(NetRetailSelling as decimal(18,2))) from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))
	   and PartCategory Is NOT NULL
	    group by PartCategory
	
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	   insert into @currentSale (PartCategory,NetRetailSelling)
	   select PartCategory,sum(cast(NetRetailSelling as decimal(18,2))) from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 
	   and PartCategory Is NOT NULL and DistributorId=@DistributorId
	     group by PartCategory

	   insert into @previousSale (PartCategory,NetRetailSelling)
	   select PartCategory,sum(cast(NetRetailSelling as decimal(18,2))) from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 
	   and PartCategory Is NOT NULL and DistributorId=@DistributorId 
	   group by PartCategory

    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get current sale based on locCode using UserId
	declare @outletCode nvarchar(50)=(select o.OutletCode from DistributorsOutlets do
		inner join Outlets o on o.OutletId =do.OutletId
		where do.UserId=@UserId)

       insert into @currentSale (PartCategory,NetRetailSelling)
	    select PartCategory,sum(cast(NetRetailSelling as decimal(18,2)))
        from dbo.DailySalesTrackerWithInvoiceData
        where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))
		and PartCategory Is NOT NULL and LocCode Is NOT NULL
	    and LocCode=@outletCode and DistributorId=@DistributorId group by PartCategory

		insert into @previousSale (PartCategory,NetRetailSelling)
	    select PartCategory,sum(cast(NetRetailSelling as decimal(18,2)))
        from dbo.DailySalesTrackerWithInvoiceData
        where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate
	    and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 
		and PartCategory Is NOT NULL and LocCode Is NOT NULL
		and LocCode=@outletCode and DistributorId=@DistributorId group by PartCategory

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get current sale based on workshopId using UserId
       insert into @currentSale (PartCategory,NetRetailSelling)
	   select PartCategory,sum(cast(NetRetailSelling as decimal(18,2)))
       from dbo.DailySalesTrackerWithInvoiceData
       where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
       and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 
	   and PartCategory Is NOT NULL and SalesUserId=@UserId and DistributorId=@DistributorId group by PartCategory

	   insert into @previousSale (PartCategory,NetRetailSelling)
	   select PartCategory,sum(cast(NetRetailSelling as decimal(18,2)))
       from dbo.DailySalesTrackerWithInvoiceData
       where NetRetailSelling Is NOT NULL and CreatedDate between @prevYrStartDate and @prvYrEndDate
       and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) 
	   and PartCategory Is NOT NULL and SalesUserId=@UserId and DistributorId=@DistributorId group by PartCategory

    END;
	
	select PartCategory,isnull(NetRetailSelling,0)as NetRetailSelling from @currentSale
	select PartCategory,isnull(NetRetailSelling,0)as NetRetailSelling from @previousSale
       
    END;

	else IF (@Action = 2)
    BEGIN
	-- Declare needed parameters
	 Declare @RoSale decimal(18,2)=0,
	 @SalesExecutiveSale decimal(18,2)=0,
	 @CustomerSegmentSale decimal(18,2)=0,
	 @PartGroupSale decimal(18,2)=0

	-- Set GroupName
	IF(@GroupName='All' or @GroupName='all' or @GroupName='')
	Begin
	Set @GroupName=NULL
	End

	 -- Fetch sale for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN

	-- Set current sale data in temp table @currentSale
	insert into @currentSale (LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling)
	  select LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,',')) and PartCategory=Case When @GroupName IS NOT NULL THEN @GroupName Else PartCategory END
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	   insert into @currentSale (LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling)
	  select LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling from dbo.DailySalesTrackerWithInvoiceData where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	   and ConsPartyTypeDesc Not in(select * from dbo.split(@CoDealerDist,','))and PartCategory=Case When @GroupName IS NOT NULL THEN @GroupName Else PartCategory END and DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get current sale based on LocCode using UserId
	set @outletCode=(select o.OutletCode from DistributorsOutlets do
		inner join Outlets o on o.OutletId =do.OutletId
		where do.UserId=@UserId)

       insert into @currentSale (LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling)
	    select LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling
        from dbo.DailySalesTrackerWithInvoiceData
        where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
	    and ConsPartyTypeDesc Not in(select * from dbo.Split(@CoDealerDist,','))and PartCategory=Case When @GroupName IS NOT NULL THEN @GroupName Else PartCategory END
	    and LocCode=@outletCode and DistributorId=@DistributorId

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get current sale based on salesUserId using UserId
       insert into @currentSale (LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling)
	   select LocCode,SalesUserId,ConsPartyTypeDesc,PartCategory,NetRetailSelling
       from dbo.DailySalesTrackerWithInvoiceData
       where NetRetailSelling Is NOT NULL and CreatedDate between @StartDate and @EndDate
       and ConsPartyTypeDesc Not in(select * from dbo.Split(@CoDealerDist,',')) and PartCategory=Case When @GroupName IS NOT NULL THEN @GroupName Else PartCategory END
       and SalesUserId=@UserId and DistributorId=@DistributorId

    END;	

	-- Set current Sale value in parameters group by sub group

       select @RoSale=isnull(sum(NetRetailSelling),0) from @currentSale
	   where LocCode IS NOT NULL and LocCode!=''

	    select @SalesExecutiveSale=isnull(sum(NetRetailSelling),0) from @currentSale
	   where SalesUserId IS NOT NULL and SalesUserId!=''

	   select @CustomerSegmentSale=isnull(sum(NetRetailSelling),0) from @currentSale where ConsPartyTypeDesc is not null and ConsPartyTypeDesc!=''

	   select @PartGroupSale=isnull(sum(NetRetailSelling),0) from @currentSale
	   where PartCategory IS NOT NULL and PartCategory!=''

	 --  Set sp response
	select @RoSale as RoWiseCurrentSale,@SalesExecutiveSale as SalesExeWiseCurrentSale,@CustomerSegmentSale as CuSagWiseCurrentSale,
	@PartGroupSale as PartGroupWisePreviousSale
       
    END;
   
END;




