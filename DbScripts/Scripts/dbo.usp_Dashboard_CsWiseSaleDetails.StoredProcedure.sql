USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CsWiseSaleDetails]    Script Date: 9/24/2020 4:13:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 12-03-2020
-- Last modified: 24-09-2020
-- Description:	Display customer Sale Details Filter by Customer Segment Wise.
-- Usage: EXEC usp_Dashboard_CsWiseSaleDetails @Action=9,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@GroupName='All',@CustomerType=Null,@FirstDayOfMonth=Null,@LastDayOfPrevMonth=Null,@Skip=0,@Take=10,@SearchTxt=Null
-- Usage: EXEC usp_Dashboard_CsWiseSaleDetails @Action=10,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@GroupName='All',@CustomerType='INDEPENDENT WORKSHOP',@FirstDayOfMonth=Null,@LastDayOfPrevMonth=Null,@Skip=0,@Take=10,@SearchTxt=Null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CsWiseSaleDetails]
    @Action int,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
    @StartDate DATE,
    @EndDate DATE,
	@GroupName nvarchar(200),
	@CustomerType nvarchar(300)=NULL,
	@FirstDayOfMonth DATE,
	@LastDayOfPrevMonth DATE,
	@Skip INT,
    @Take INT,
	@SearchTxt nvarchar(200)
AS
BEGIN

   -- Declare needed parameters
	 Declare	 
	 @prevYrStartDate DATE,@prvYrEndDate DATE,@DistributorId int=0
	 
	 -- Get distributorId using UserId and role
    set @DistributorId=dbo.getDistributorId(@UserId,@Role)

	-- Set previous year startDate and endDate
	set @prevYrStartDate=DATEADD(year, -1, @StartDate)
	set @prvYrEndDate=DATEADD(year, -1, @EndDate)

	 DECLARE @sqlCurrentSale NVARCHAR(MAX);
	 DECLARE @AvgSale NVARCHAR(MAX);
	 DECLARE @sqlPreviousSale NVARCHAR(MAX);

    DECLARE @ParameterDef NVARCHAR(max);
    SET @ParameterDef
        = '@Role NVARCHAR(100),
							@UserId NVARCHAR(200),
                            @StartDate DATE,
                            @EndDate DATE,
							@prevYrStartDate DATE,
                            @prvYrEndDate DATE,
	                        @GroupName nvarchar(200),
	                        @CustomerType nvarchar(300),
	                        @FirstDayOfMonth DATE,
	                        @LastDayOfPrevMonth DATE,
	                        @Skip INT,
                            @Take INT,
	                        @SearchTxt nvarchar(200)';
if(@Action=8)
	BEGIN
		  /* ~~~~~~~~~ COMMON FILTERING AVGSale BY ROLE AND DATE ~~~~~~~~~ */
	
	 SET @AvgSale='SELECT (SUM(cast(sd.NetRetailSelling as decimal(18,2)))/3) FROM dbo.DailySalesTrackerWithInvoiceData sd
	      inner join dbo.WorkShops w on sd.WorkshopId=w.WorkshopId 
		  WHERE sd.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth and w.Type <> '''' ';

	if(@CustomerType <> '' and @CustomerType IS NOT NULL) 
	   Begin
	   SET @AvgSale = @AvgSale + ' and sd.ConsPartyTypeDesc=@CustomerType';
	   End

	if(@GroupName <> '' and @GroupName IS NOT NULL and @GroupName!='All' and @GroupName!='all') 
	Begin
	   SET @AvgSale = @AvgSale + ' and sd.PartCategory=@GroupName';
	   End

       
    IF (@Role = 'Distributor')
    BEGIN
       
    SET @AvgSale = @AvgSale + ' and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' And w.WorkshopId in(select WorkShopId from dbo.DistributorWorkshops where DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+')';

    END;
	ELSE IF (@Role = 'RoIncharge')
    BEGIN

	    SET @AvgSale = @AvgSale + ' and w.OutletId=(SELECT TOP 1 OutletId FROM dbo.DistributorsOutlets WHERE UserId = @UserId) and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN

	   SET @AvgSale = @AvgSale + ' and w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId) and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);
			  
    END;
END;


	if(@Action=8)
	BEGIN
	
	   -- Group BY Customer Wise current and previous sale
	   SET @AvgSale=@AvgSale+' and sd.ConsPartyTypeDesc=max(s.ConsPartyTypeDesc)'

	   SET @sqlCurrentSale
            = 'SELECT  max(s.ConsPartyTypeDesc)as CustomerType,
			 count(DISTINCT s.ConsPartyCode) as TotalCustomer,

              isnull(('+@AvgSale+'),0) as AverageSale,

		sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   inner join dbo.WorkShops w on s.WorkshopId=w.WorkshopId
			   where w.Type <> '''' ';


		SET @sqlPreviousSale
		               ='select max(s.ConsPartyTypeDesc)as CustomerType ,
	 sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   inner join dbo.WorkShops w on s.WorkshopId=w.WorkshopId
			   where w.Type <> '''' ';
	END
	
	ELSE if(@Action=9 or @Action=10)
	BEGIN

	  -- Group BY CustomerCode current and previous sale

	  SET @sqlCurrentSale
            = ';WITH Main_CTE AS(
			SELECT MAX(s.ConsPartyCode)as CustomerCode,
			max(s.ConsPartyName)as CustomerName,
	       max(s.ConsPartyTypeDesc)as CustomerType, 
		sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   inner join dbo.WorkShops w on s.WorkshopId=w.WorkshopId
			   where w.Type <> ''''';


	  SET @sqlPreviousSale
		               ='select MAX(s.ConsPartyCode)as CustomerCode ,
	 sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   inner join dbo.WorkShops w on s.WorkshopId=w.WorkshopId
			   where w.Type <> ''''';
	 
	END

	/* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */
	SET @sqlCurrentSale = @sqlCurrentSale + ' and s.NetRetailSelling Is NOT NULL and s.CreatedDate between @StartDate and @EndDate';

	SET @sqlPreviousSale = @sqlPreviousSale + ' and s.NetRetailSelling Is NOT NULL and s.CreatedDate between @prevYrStartDate and @prvYrEndDate';
	
	if(@CustomerType <> '' and @CustomerType IS NOT NULL) 
	   Begin

	   SET @sqlCurrentSale=@sqlCurrentSale+' and s.ConsPartyTypeDesc=@CustomerType';

	   SET @sqlPreviousSale=@sqlPreviousSale+' and s.ConsPartyTypeDesc=@CustomerType';
	   End

	   if(@GroupName <> '' and @GroupName IS NOT NULL and @GroupName!='All' and @GroupName!='all') 
	   Begin

	   SET @sqlCurrentSale=@sqlCurrentSale+' and s.PartCategory=@GroupName';

	   SET @sqlPreviousSale=@sqlPreviousSale+' and s.PartCategory=@GroupName';
	   End

       
    IF (@Role = 'Distributor')
    BEGIN
	
	   SET @sqlCurrentSale = @sqlCurrentSale + ' and w.WorkshopId in(select WorkShopId from dbo.DistributorWorkshops where DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+') and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

	SET @sqlPreviousSale = @sqlPreviousSale + ' and w.WorkshopId in(select WorkShopId from dbo.DistributorWorkshops where DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+') and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

    END;
	ELSE IF (@Role = 'RoIncharge')
    BEGIN

	    SET @sqlCurrentSale = @sqlCurrentSale + ' and w.OutletId=(SELECT TOP 1 OutletId FROM dbo.DistributorsOutlets WHERE UserId = @UserId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

	   SET @sqlPreviousSale = @sqlPreviousSale + ' and w.OutletId=(SELECT TOP 1 OutletId FROM dbo.DistributorsOutlets WHERE UserId = @UserId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN

	   SET @sqlCurrentSale = @sqlCurrentSale + ' and w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

	   SET @sqlPreviousSale = @sqlPreviousSale + ' and w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);
			  
    END;

	if(@Action=8)
	BEGIN

	SET @sqlCurrentSale = @sqlCurrentSale + ' group by s.ConsPartyTypeDesc';

	SET @sqlPreviousSale = @sqlPreviousSale + ' group by s.ConsPartyTypeDesc';
	   
	END
	
	ELSE if(@Action=9 or @Action=10)
	BEGIN

	IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sqlCurrentSale = @sqlCurrentSale + ' AND (s.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR s.ConsPartyName LIKE ''%'+@SearchTxt+'%'') ';

			SET @sqlPreviousSale = @sqlPreviousSale + ' AND (s.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR s.ConsPartyName LIKE ''%'+@SearchTxt+'%'') ';
        END;

	SET @sqlCurrentSale = @sqlCurrentSale + ' group by s.ConsPartyCode)
     , Count_CTE AS (
	 SELECT COUNT(*) AS [TotalRows]
    FROM Main_CTE)
	 SELECT *
     FROM Main_CTE, Count_CTE
    ORDER BY Main_CTE.CustomerCode
    OFFSET @Skip ROWS
    FETCH NEXT @Take ROWS ONLY';

	SET @sqlPreviousSale = @sqlPreviousSale + ' group by s.ConsPartyCode';
	 
	END

	
	 PRINT @sqlCurrentSale

	 PRINT @sqlPreviousSale
    EXEC sp_Executesql @sqlCurrentSale,
                       @ParameterDef,
                       @Role = @Role,
                       @UserId=@UserId,
                       @StartDate=@StartDate,
                       @EndDate =@EndDate,
				       @prevYrStartDate =@prevYrStartDate,
                       @prvYrEndDate =@prvYrEndDate,
	                   @GroupName =@GroupName,
	                   @CustomerType =@CustomerType,
	                   @FirstDayOfMonth =@FirstDayOfMonth,
	                   @LastDayOfPrevMonth =@LastDayOfPrevMonth,
	                   @Skip =@Skip,
                       @Take =@Take,
	                   @SearchTxt =@SearchTxt;

					   EXEC sp_Executesql
	                   @sqlPreviousSale,
                       @ParameterDef,
                       @Role = @Role,
                       @UserId=@UserId,
                       @StartDate=@StartDate,
                       @EndDate =@EndDate,
				       @prevYrStartDate =@prevYrStartDate,
                       @prvYrEndDate =@prvYrEndDate,
	                   @GroupName =@GroupName,
	                   @CustomerType =@CustomerType,
	                   @FirstDayOfMonth =@FirstDayOfMonth,
	                   @LastDayOfPrevMonth =@LastDayOfPrevMonth,
	                   @Skip =@Skip,
                       @Take =@Take,
	                   @SearchTxt =@SearchTxt;
	
END


