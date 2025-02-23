USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_SeWiseSaleDetails]    Script Date: 9/24/2020 4:13:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 12-03-2020
-- Last modified: 24-09-2020
-- Description:	Display customer Sale Details Filter by Sales Executive Wise.
-- Usage: EXEC usp_Dashboard_SeWiseSaleDetails @Action=4,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@GroupName='All',@BranchCode=Null,@CustomerType=Null,@Skip=0,@Take=10,@SearchTxt=Null
-- Usage: EXEC usp_Dashboard_SeWiseSaleDetails @Action=5,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@GroupName='All',@BranchCode=Null,@CustomerType=Null,@Skip=0,@Take=10,@SearchTxt=Null
-- Usage: EXEC usp_Dashboard_SeWiseSaleDetails @Action=6,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@GroupName='All',@BranchCode=Null,@CustomerType='INDEPENDENT WORKSHOP',@Skip=0,@Take=10,@SearchTxt=Null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_SeWiseSaleDetails]
    @Action int,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
    @StartDate DATE,
    @EndDate DATE,
	@GroupName nvarchar(200),
	@BranchCode nvarchar(50),
	@CustomerType nvarchar(300),
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
	                        @BranchCode nvarchar(50),
	                        @CustomerType nvarchar(300),
	                        @FirstDayOfMonth DATE,
	                        @LastDayOfPrevMonth DATE,
	                        @Skip INT,
                            @Take INT,
	                        @SearchTxt nvarchar(200)';
if(@Action=4 or @Action=5)
	BEGIN
		  /* ~~~~~~~~~ COMMON FILTERING AVGSale BY ROLE AND DATE ~~~~~~~~~ */
	
	 SET @AvgSale='SELECT (SUM(cast(sd.NetRetailSelling as decimal(18,2)))/3) FROM dbo.DailySalesTrackerWithInvoiceData sd
		  WHERE sd.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth';

	if(@CustomerType <> '' and @CustomerType IS NOT NULL) 
	   Begin
	   SET @AvgSale = @AvgSale + ' and sd.ConsPartyTypeDesc=@CustomerType';
	   End

	if(@GroupName <> '' and @GroupName IS NOT NULL and @GroupName!='All' and @GroupName!='all') 
	Begin
	   SET @AvgSale = @AvgSale + ' and sd.PartCategory=@GroupName';
	   End

    if(@BranchCode <> '' and @BranchCode IS NOT NULL) 
	Begin
	    SET @AvgSale = @AvgSale + ' and sd.LocCode=@BranchCode';
	End
	
	 IF (@Role = 'SuperAdmin')
    BEGIN
       
    SET @AvgSale = @AvgSale + ' And sd.WorkshopId in(select WorkShopId from dbo.SalesExecutiveWorkshop group by WorkShopId)';

    END;
       
    ELSE IF (@Role = 'Distributor')
    BEGIN
       
    SET @AvgSale = @AvgSale + ' and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' And sd.WorkshopId in(select w.WorkShopId from dbo.SalesExecutiveWorkshop w inner join dbo.DistributorWorkshops dw on w.WorkShopId=dw.WorkShopId where dw.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' group by w.WorkShopId)';

    END;
	ELSE IF (@Role = 'RoIncharge')
    BEGIN

	    SET @AvgSale = @AvgSale + ' and sd.WorkshopId in(select w.WorkshopId from dbo.SalesExecutiveWorkshop w
	  inner join dbo.RoSalesExecutive e on w.UserId=e.SeUserId where e.RoUserId=@UserId group by w.WorkShopId) and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN

	   SET @AvgSale = @AvgSale + ' and sd.WorkshopId IN(SELECT WorkShopId FROM dbo.SalesExecutiveWorkshop WHERE UserId=@UserId  group by WorkShopId) and sd.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);
			  
    END;
END;


	if(@Action=4)
	BEGIN
	
	   -- Group BY Customer Wise current and previous sale
	   SET @AvgSale=@AvgSale+' and sd.ConsPartyTypeDesc=max(s.ConsPartyTypeDesc)'

	   SET @sqlCurrentSale
            = 'SELECT max(s.LocCode)as BranchCode,
			 max(s.ConsPartyTypeDesc)as CustomerType,
			 count(DISTINCT s.ConsPartyCode) as TotalCustomer,

              isnull(('+@AvgSale+'),0) as AverageSale,

		sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';


		SET @sqlPreviousSale
		               ='select max(s.ConsPartyTypeDesc)as CustomerType ,
	 sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';
	END

	ELSE if(@Action=5)
	BEGIN

	SET @AvgSale=@AvgSale+' and sd.LocCode=max(s.LocCode)'

	SET @sqlCurrentSale
            = 'SELECT DISTINCT MAX(s.LocCode)as BranchCode,
			(select max(u.FirstName)+'' ''+max(u.LastName) from dbo.userdetails u
	   inner join dbo.SalesExecutiveWorkshop sew on u.UserId=sew.UserId
	    where sew.WorkshopId=max(s.WorkshopId) group by sew.WorkshopId)as SalesExecutiveName,
	      
	      count(DISTINCT s.ConsPartyCode) as TotalCustomer,

             isnull(('+@AvgSale+'),0) as AverageSale,

		sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';


		SET @sqlPreviousSale
		               ='select MAX(s.LocCode)as BranchCode ,
	 sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';
	
	END
	ELSE if(@Action=6 or @Action=7)
	BEGIN

	  -- Group BY CustomerCode current and previous sale

	  SET @sqlCurrentSale
            = ';WITH Main_CTE AS(
			SELECT DISTINCT MAX(s.ConsPartyCode)as CustomerCode,
			max(s.ConsPartyName)as CustomerName,
	       max(s.ConsPartyTypeDesc)as CustomerType, 
		sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';


	  SET @sqlPreviousSale
		               ='select MAX(s.ConsPartyCode)as CustomerCode ,
	 sum(cast(s.NetRetailSelling as decimal(18,2)))as NetRetailSelling
               from 
			   dbo.DailySalesTrackerWithInvoiceData s
			   where 1=1';
	 
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

	    if(@BranchCode <> '' and @BranchCode IS NOT NULL) 
	   Begin

	   SET @sqlCurrentSale=@sqlCurrentSale+' and s.LocCode=@BranchCode ';

	   SET @sqlPreviousSale=@sqlPreviousSale+' and s.LocCode=@BranchCode ';
	   End

    IF (@Role = 'SuperAdmin')
    BEGIN
       
    SET @sqlCurrentSale = @sqlCurrentSale + ' And s.WorkshopId in(select WorkShopId from dbo.SalesExecutiveWorkshop group by WorkShopId)';

	SET @sqlPreviousSale = @sqlPreviousSale + ' And s.WorkshopId in(select WorkShopId from dbo.SalesExecutiveWorkshop group by WorkShopId)';
    END;
         ELSE IF (@Role = 'Distributor')
    BEGIN
       
	SET @sqlCurrentSale = @sqlCurrentSale + ' and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' And s.WorkshopId in(select w.WorkShopId from dbo.SalesExecutiveWorkshop w inner join dbo.DistributorWorkshops dw on w.WorkShopId=dw.WorkShopId where dw.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' group by w.WorkShopId)';

    SET @sqlPreviousSale = @sqlPreviousSale + ' and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' And s.WorkshopId in(select w.WorkShopId from dbo.SalesExecutiveWorkshop w inner join dbo.DistributorWorkshops dw on w.WorkShopId=dw.WorkShopId where dw.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+' group by w.WorkShopId)';

    END;
	ELSE IF (@Role = 'RoIncharge')
    BEGIN

	    SET @sqlCurrentSale = @sqlCurrentSale + ' and s.WorkshopId in(select w.WorkshopId from dbo.SalesExecutiveWorkshop w
	  inner join dbo.RoSalesExecutive e on w.UserId=e.SeUserId where e.RoUserId=@UserId group by w.WorkShopId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

	   SET @sqlPreviousSale = @sqlPreviousSale + ' and s.WorkshopId in(select w.WorkshopId from dbo.SalesExecutiveWorkshop w
	  inner join dbo.RoSalesExecutive e on w.UserId=e.SeUserId where e.RoUserId=@UserId group by w.WorkShopId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN

	   SET @sqlCurrentSale = @sqlCurrentSale + ' and s.WorkshopId IN(SELECT WorkShopId FROM dbo.SalesExecutiveWorkshop WHERE UserId=@UserId  group by WorkShopId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);

	    SET @sqlPreviousSale = @sqlPreviousSale + ' and s.WorkshopId IN(SELECT WorkShopId FROM dbo.SalesExecutiveWorkshop WHERE UserId=@UserId  group by WorkShopId) and s.DistributorId=' + ISNULL(cast(@DistributorId as varchar),0);
			  
    END;

	if(@Action=4)
	BEGIN

	SET @sqlCurrentSale = @sqlCurrentSale + ' group by s.ConsPartyTypeDesc';

	SET @sqlPreviousSale = @sqlPreviousSale + ' group by s.ConsPartyTypeDesc';
	   
	END

	ELSE if(@Action=5)
	BEGIN

	SET @sqlCurrentSale = @sqlCurrentSale + ' group by s.LocCode';

	SET @sqlPreviousSale = @sqlPreviousSale + ' group by s.LocCode';
	
	END
	ELSE if(@Action=6 or @Action=7)
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
	                   @BranchCode =@BranchCode,
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
	                   @BranchCode =@BranchCode,
	                   @CustomerType =@CustomerType,
	                   @FirstDayOfMonth =@FirstDayOfMonth,
	                   @LastDayOfPrevMonth =@LastDayOfPrevMonth,
	                   @Skip =@Skip,
                       @Take =@Take,
	                   @SearchTxt =@SearchTxt;
	
END


