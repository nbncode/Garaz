USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CustomerDetail]    Script Date: 10/13/2020 12:33:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====================================================================================================
-- Author: Vikram Singh Saini
-- Create date: 14-04-2020
-- Last modified: 13-10-2020 Add sorting and fix issue in showing billed and non billed customer count
-- Description:	Display customers detail RO and SE wise
-- Usage: EXEC usp_Dashboard_CustomerDetail @Action=10, @Role='SuperAdmin', @UserId='24539543-60d2-49d7-b73b-b14c24e6b987', @StartDate='2020-10-01', @EndDate='2020-10-13', @CustomerType='',@BranchCode='CMU', @Skip=0, @Take=100, @SearchTxt='',@CurrentDate='2020-10-13', @FirstDayOfMonth='2020-10-01',@LastDayOfPrevMonth='2020-09-30', @PrvYrStartDate='2019-10-01', @PrvYrEndDate='2019-10-13',@SalesExecUserId='7da7d5d2-89bf-45b5-b1ce-d2c318bc9434',@SortBy='',@SortOrder='asc'
-- =====================================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CustomerDetail]
    @Action INT,
    @Role NVARCHAR(500),
    @UserId NVARCHAR(500),
    @StartDate DATE,
    @EndDate DATE,
    @CustomerType NVARCHAR(200),
    @BranchCode NVARCHAR(50),
    @Skip INT,
    @Take INT,
    @SearchTxt NVARCHAR(200),
    @CurrentDate DATE,
    @FirstDayOfMonth DATE,
    @LastDayOfPrevMonth DATE,
    @PrvYrStartDate DATE,
    @PrvYrEndDate DATE,
    @SalesExecUserId NVARCHAR(200),
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

    DECLARE @ParameterDef NVARCHAR(max);
    SET @ParameterDef
        = '@Role NVARCHAR(500),
							@UserId NVARCHAR(500),
							@StartDate DATE,
							@EndDate DATE,
							@CustomerType NVARCHAR(200),
							@BranchCode NVARCHAR(10),
							@Skip INT,
							@Take INT,
							@SearchTxt NVARCHAR(200),
							@CurrentDate DATE,
							@FirstDayOfMonth DATE,
							@LastDayOfPrevMonth DATE,
							@PrvYrStartDate DATE,
							@PrvYrEndDate DATE,
							@SalesExecUserId NVARCHAR(200),
							@SortBy NVARCHAR(200),
	                        @SortOrder NVARCHAR(10)';

    -- Abbreviation Guide
    -- NBC - Non Billed Customers
    -- BC - Billed Customers

    DECLARE @AvgSaleByRole NVARCHAR(MAX),
            @CurrentMonthSaleByRole NVARCHAR(MAX),
            @PrvYrSaleByRole NVARCHAR(MAX);
    DECLARE @distId NVARCHAR(50);
    DECLARE @outletId NVARCHAR(50);

    /* ~~~~~~~~~ SET QUERY FOR AVERAGE SALE BY ROLE ~~~~~~~~~ */

    SET @AvgSaleByRole
        = '(SELECT ISNULL(SUM(CONVERT(DECIMAL(18, 2), ds.NetRetailSelling))/3,0) FROM dbo.DailySalesTrackerWithInvoiceData ds';

    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @AvgSaleByRole
            = @AvgSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND ds.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        SET @distId = dbo.getDistributorId(@UserId, @Role);
        SET @AvgSaleByRole
            = @AvgSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND ds.DistributorId=' + ISNULL(@distId, '0')
              + ' AND ds.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @outletId =
        (
            SELECT TOP 1
                OutletId
            FROM dbo.DistributorsOutlets do
            WHERE do.UserId = @UserId
        );
        SET @AvgSaleByRole
            = @AvgSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.OutletId)=' + ISNULL(@outletId, '0')
              + ' AND ds.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @AvgSaleByRole
            = @AvgSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)
			  AND ds.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;

    /* ~~~~~~~~~ SET QUERY FOR CURRENT MONTH SALE BY ROLE ~~~~~~~~~ */

    SET @CurrentMonthSaleByRole
        = '(SELECT ISNULL(SUM(CONVERT(DECIMAL(18, 2), ds.NetRetailSelling)),0) FROM dbo.DailySalesTrackerWithInvoiceData ds';

    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @CurrentMonthSaleByRole
            = @CurrentMonthSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MONTH(@CurrentDate)=MONTH(ds.CreatedDate) AND YEAR(@CurrentDate)=YEAR(ds.CreatedDate)) AS CurrentMonthSale';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        SET @CurrentMonthSaleByRole
            = @CurrentMonthSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND ds.DistributorId='
              + ISNULL(@distId, '0')
              + ' AND MONTH(@CurrentDate)=MONTH(ds.CreatedDate) AND YEAR(@CurrentDate)=YEAR(ds.CreatedDate)) AS CurrentMonthSale';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @CurrentMonthSaleByRole
            = @CurrentMonthSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.OutletId) ='
              + ISNULL(@outletId, '0')
              + ' AND MONTH(@CurrentDate)=MONTH(ds.CreatedDate) AND YEAR(@CurrentDate)=YEAR(ds.CreatedDate)) AS CurrentMonthSale';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @CurrentMonthSaleByRole
            = @CurrentMonthSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)
			  AND MONTH(@CurrentDate)=MONTH(ds.CreatedDate) AND YEAR(@CurrentDate)=YEAR(ds.CreatedDate)) AS CurrentMonthSale';
    END;

    /* ~~~~~~~~~ SET QUERY FOR PREVIOUS YEAR SALE BY ROLE ~~~~~~~~~ */

    SET @PrvYrSaleByRole
        = '(SELECT ISNULL(SUM(CONVERT(DECIMAL(18, 2), ds.NetRetailSelling)),0) FROM dbo.DailySalesTrackerWithInvoiceData ds';

    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @PrvYrSaleByRole
            = @PrvYrSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND ds.CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate) AS PrvYrSale';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        SET @PrvYrSaleByRole
            = @PrvYrSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND ds.DistributorId=' + ISNULL(@distId, '0')
              + ' AND ds.CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate) AS PrvYrSale';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @PrvYrSaleByRole
            = @PrvYrSaleByRole + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.OutletId) ='
              + ISNULL(@outletId, '0') + ' AND ds.CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate) AS PrvYrSale';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @PrvYrSaleByRole
            = @PrvYrSaleByRole
              + ' WHERE MAX(w.WorkShopId)=ds.WorkShopId AND MAX(w.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)
			  AND ds.CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate) AS PrvYrSale';
    END;

    /* ~~~~~~~~~ MAIN BOXES ~~~~~~~~~ */

    -- MAIN: NBC & BC
    IF (@Action = 0)
    BEGIN
        SET @sql
            = 'SELECT COUNT(DISTINCT w.WorkshopId) AS TotalCustomers, 
					  COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			          COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers
			 FROM dbo.Workshops w
			     LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId and s.CreatedDate BETWEEN @StartDate AND @EndDate where 1=1';
    END;

    -- CUSTOMERS BY RO WISE
    ELSE IF (@Action = 1)
    BEGIN
        SET @sql
            = 'SELECT COUNT(DISTINCT w.WorkshopId) AS TotalCustomers, 
					  COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			          COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers
			 FROM dbo.Workshops w
			     LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId and s.CreatedDate BETWEEN @StartDate AND @EndDate where 1=1';
        
    END;

    -- CUSTOMERS BY SE WISE
    ELSE IF (@Action = 2)
    BEGIN
        SET @sql
            = 'SELECT COUNT(DISTINCT w.WorkshopId) AS TotalCustomers, 
					  COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			          COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers
			 FROM dbo.Workshops w
			     LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId and s.CreatedDate BETWEEN @StartDate AND @EndDate WHERE w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';
    END;

    /* ~~~~~~~~~ RO WISE DETAIL ~~~~~~~~~ */

    -- RO WISE CUSTOMER DETAIL
    ELSE IF (@Action = 3)
    BEGIN
        SET @sql
            = 'SELECT w.Type AS CustomerType, COUNT(DISTINCT w.WorkshopId) AS TotalCustomers, 
					  COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			          COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers
			 FROM dbo.Workshops w
			     LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate Where w.Type IS NOT NULL ';
    END;

    -- RO WISE BRANCH CUSTOMER DETAIL    
    ELSE IF (@Action = 4)
    BEGIN
        SET @sql
            = N'  SELECT o.OutletCode AS BranchCode,
					o.OutletName AS BranchName,
					COUNT(DISTINCT w.WorkShopId) AS TotalCustomers, 	
					COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			        COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers					
					FROM dbo.Workshops w	
					INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId		     
					  LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate where 1=1';
      
    END;

    -- RO WISE BRANCH BILLED CUSTOMERS
    ELSE IF (@Action = 5)
    BEGIN
        SET @sql
            = N' SELECT u.ConsPartyCode AS CustomerCode, w.WorkshopName AS CustomerName, w.Type AS CustomerType,
			' + @AvgSaleByRole + ',' + @CurrentMonthSaleByRole + ',' + @PrvYrSaleByRole
              + ', TotalRows = COUNT(*) OVER()									
					FROM dbo.Workshops w			     
					  INNER JOIN dbo.DailySalesTrackerWithInvoiceData s ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate
					  INNER JOIN dbo.DistributorWorkshops dw ON w.WorkshopId = dw.WorkshopId
					  INNER JOIN dbo.UserDetails u ON dw.UserId = u.UserId';

        SET @sql
            = @sql
              + ' INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId
			      WHERE w.OutletId IS NOT NULL AND w.Type IS NOT NULL AND o.OutletCode=@BranchCode';

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND (u.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR w.WorkshopName LIKE ''%'+@SearchTxt+'%'')';
        END;

    END;

    -- RO WISE BRANCH NON BILLED CUSTOMERS
    ELSE IF (@Action = 6)
    BEGIN
        SET @sql
            = N' SELECT u.ConsPartyCode AS CustomerCode, w.WorkshopName AS CustomerName, w.Type AS CustomerType,
			' + @AvgSaleByRole
              + ',(SELECT ISNULL(DATEDIFF(day, MAX(ds.CreatedDate), @CurrentDate),0) FROM dbo.DailySalesTrackerWithInvoiceData ds WHERE MAX(w.WorkshopId)=ds.WorkshopId) AS NonBilledFromDays,'
              + @PrvYrSaleByRole
              + ', TotalRows = COUNT(*) OVER()									
					FROM dbo.Workshops w
					  INNER JOIN dbo.DistributorWorkshops dw ON w.WorkshopId = dw.WorkshopId
					  INNER JOIN dbo.UserDetails u ON dw.UserId = u.UserId';

        SET @sql
            = @sql
              + ' INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId
			      WHERE w.WorkshopId NOT IN (SELECT DISTINCT w.WorkshopId FROM dbo.Workshops w
												INNER JOIN dbo.DailySalesTrackerWithInvoiceData s
													ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate	
												WHERE w.type IS NOT NULL AND w.OutletId IS NOT NULL) 
				  AND w.OutletId IS NOT NULL AND w.Type IS NOT NULL AND o.OutletCode=@BranchCode ';

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND (u.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR w.WorkshopName LIKE ''%'+@SearchTxt+'%'') ';
        END;
    END;

    /* ~~~~~~~~~ SE WISE DETAIL ~~~~~~~~~ */

    -- SE WISE CUSTOMER DETAIL
    ELSE IF (@Action = 7)
    BEGIN
        SET @sql
            = 'SELECT w.Type AS CustomerType, COUNT(DISTINCT w.WorkshopId) AS TotalCustomers, 
					  COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			          COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers
			 FROM dbo.Workshops w
			     LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate
				 INNER JOIN dbo.SalesExecutiveWorkshop se ON w.WorkshopId = se.WorkshopId   WHERE w.Type IS NOT NULL ';
    END;

    -- SE WISE BRANCH CUSTOMER DETAIL    
    ELSE IF (@Action = 8)
    BEGIN
        SET @sql
            = N'  SELECT o.OutletCode AS BranchCode,
					CONCAT(u.FirstName,'' '', u.LastName) AS SalesExecName,
					u.UserId AS SalesExecUserId,
					COUNT(DISTINCT w.WorkShopId) AS TotalCustomers, 	
					COUNT(DISTINCT s.WorkshopId) AS BilledCustomers,
			        COUNT(DISTINCT w.WorkshopId) - COUNT(DISTINCT s.WorkshopId) AS NonBilledCustomers					
					FROM dbo.Workshops w			     
					  LEFT JOIN dbo.DailySalesTrackerWithInvoiceData s
			       ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate
				    INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId
			      INNER JOIN dbo.SalesExecutiveWorkshop se ON w.WorkshopId=se.WorkshopId
				  INNER JOIN dbo.UserDetails u ON se.UserId=u.UserId
			      WHERE 1=1';
    END;

    -- SE WISE BRANCH BILLED CUSTOMERS
    ELSE IF (@Action = 9)
    BEGIN
        SET @sql
            = N' SELECT u.ConsPartyCode AS CustomerCode, w.WorkshopName AS CustomerName, w.Type AS CustomerType,
			' + @AvgSaleByRole + ',' + @CurrentMonthSaleByRole + ',' + @PrvYrSaleByRole
              + ', TotalRows = COUNT(*) OVER()									
					FROM dbo.Workshops w			     
					  INNER JOIN dbo.DailySalesTrackerWithInvoiceData s ON w.WorkshopId = s.WorkshopId AND s.CreatedDate BETWEEN @StartDate AND @EndDate
					  INNER JOIN dbo.SalesExecutiveWorkshop se ON w.WorkshopId = se.WorkshopId And se.UserId=@SalesExecUserId
					  INNER JOIN dbo.UserDetails u ON se.UserId = u.UserId
					   INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId
			      WHERE o.OutletCode=@BranchCode ';

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND (u.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR w.WorkshopName LIKE ''%'+@SearchTxt+'%'')';
        END;

    END;

    -- SE WISE BRANCH NON BILLED CUSTOMERS
    ELSE IF (@Action = 10)
    BEGIN
        SET @sql
            = N' SELECT u.ConsPartyCode AS CustomerCode, w.WorkshopName AS CustomerName, w.Type AS CustomerType,
			' + @AvgSaleByRole
              + ',(SELECT ISNULL(DATEDIFF(day, MAX(ds.CreatedDate), @CurrentDate),0) FROM dbo.DailySalesTrackerWithInvoiceData ds WHERE MAX(w.WorkshopId)=ds.WorkshopId) AS NonBilledFromDays,'
              + @PrvYrSaleByRole
              + ', TotalRows = COUNT(*) OVER()									
					FROM dbo.Workshops w
					  INNER JOIN dbo.SalesExecutiveWorkshop se ON w.WorkshopId = se.WorkshopId
					  INNER JOIN dbo.UserDetails u ON se.UserId = u.UserId 
					  INNER JOIN dbo.Outlets o ON w.OutletId=o.OutletId
			      WHERE w.WorkshopId NOT IN (SELECT DISTINCT s.WorkshopId from dbo.DailySalesTrackerWithInvoiceData s
					INNER JOIN dbo.SalesExecutiveWorkshop se ON s.WorkshopId = se.WorkshopId	AND s.CreatedDate BETWEEN @StartDate AND @EndDate
					WHERE se.UserId=@SalesExecUserId) 
					And se.UserId=@SalesExecUserId
				  AND o.OutletCode=@BranchCode';

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND (u.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR w.WorkshopName LIKE ''%'+@SearchTxt+'%'')';
        END;
    END;

    /* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */

    -- No case for 'SuperAdmin' as we are retrieving all results. 
    IF (@Role = 'Distributor')
    BEGIN
        SET @sql = @sql + ' and w.WorkShopId in(select WorkShopId from dbo.DistributorWorkshops where DistributorId=' + ISNULL(@distId, '0')+')';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @sql = @sql + ' and w.OutletId=' + ISNULL(@outletId, '0');
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @sql
            = @sql
              + ' and w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)';
			  
    END;

	 IF (@Action = 1)
    BEGIN
        SET @sql = @sql + ' and w.OutletId IS NOT NULL';
    END;

    /* ~~~~~~~~~ FURTHER GROUPING ~~~~~~~~~ */

    -- GROUP RO & SE CUSTOMER DETAIL 
    ELSE IF (@Action = 3 OR @Action = 7)
    BEGIN
        SET @sql = @sql + ' GROUP BY w.Type';
    END;

    -- GROUP RO WISE BRANCH CUSTOMER DETAIL    	
    ELSE IF (@Action = 4)
    BEGIN
        SET @sql = @sql + ' GROUP BY o.OutletCode, o.OutletName';
    END;

    -- GROUP RO & SE WISE BRANCH BILLED & NON BILLED CUSTOMERS
    ELSE IF (@Action = 5 OR @Action = 6 OR @Action = 9 OR @Action = 10)
    BEGIN
        SET @sql= @sql+ ' GROUP BY w.workshopid,u.ConsPartyCode, w.WorkshopName, w.Type ORDER BY';

	  if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql= @sql+ ' u.ConsPartyCode';
	   END
	    SET @sql= @sql+ ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
			   
    END;

    -- GROUP SE WISE BRANCH CUSTOMER DETAIL    	
    ELSE IF (@Action = 8)
    BEGIN
        SET @sql = @sql + ' GROUP BY o.OutletCode, u.FirstName, u.LastName, u.UserId';
    END;

    PRINT @sql
    EXEC sp_Executesql @sql,
                       @ParameterDef,
                       @Role = @Role,
                       @UserId = @UserId,
                       @StartDate = @StartDate,
                       @EndDate = @EndDate,
                       @CustomerType = @CustomerType,
                       @BranchCode = @BranchCode,
                       @Skip = @Skip,
                       @Take = @Take,
                       @SearchTxt = @SearchTxt,
                       @CurrentDate = @CurrentDate,
                       @FirstDayOfMonth = @FirstDayOfMonth,
                       @LastDayOfPrevMonth = @LastDayOfPrevMonth,
                       @PrvYrStartDate = @PrvYrStartDate,
                       @PrvYrEndDate = @PrvYrEndDate,
                       @SalesExecUserId = @SalesExecUserId,
					   @SortBy =@SortBy,
	                   @SortOrder=@SortOrder;
END;
