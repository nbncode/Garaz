USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CboDetail]    Script Date: 30-10-2020 3:19:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====================================================================================================
-- Author: Vikram Singh Saini & Lokesh Choudhary
-- Create date: 25-02-2020
-- Last modified: 12-10-2020 --Use CustomerType dynamic and add order by
-- Description:	Display customer back orders detail RO and SE wise.
-- Usage: EXEC usp_Dashboard_CboDetail @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2020-10-01', @EndDate='2020-10-30', @CustomerType='',@BranchCode='TRS', @Skip=0, @Take=0,@SearchTxt='', @CustomerCode='',@CoNumber='',@FirstDayOfMonth='2020-10-01',@LastDayOfPrevMonth='2020-09-30',@SortBy='',@SortOrder='asc'
-- =====================================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CboDetail]
    @Action INT,
    @Role NVARCHAR(500),
    @UserId NVARCHAR(500),
    @StartDate DATE,
    @EndDate DATE,
    @CustomerType NVARCHAR(200),
    @BranchCode NVARCHAR(10),
    @Skip INT,
    @Take INT,
    @SearchTxt NVARCHAR(200),
    @CustomerCode NVARCHAR(100),
    @CoNumber NVARCHAR(200),
    @FirstDayOfMonth DATE,
    @LastDayOfPrevMonth DATE,
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

    DECLARE @ParameterDef NVARCHAR(500);
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
							@CustomerCode NVARCHAR(100),
							@CoNumber NVARCHAR(200),
							@FirstDayOfMonth DATE,
							@LastDayOfPrevMonth DATE';

    /* ~~~~~~~~~ SET QUERY FOR AVERAGE SALE BY ROLE ~~~~~~~~~ */
    DECLARE @AvgSaleByRole NVARCHAR(MAX);
    DECLARE @distId NVARCHAR(50);
    DECLARE @outletId NVARCHAR(50);

    SET @AvgSaleByRole
        = '(SELECT ISNULL(SUM(CONVERT(DECIMAL(18, 2), s.NetRetailSelling))/3,0) FROM dbo.DailySalesTrackerWithInvoiceData s';

    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @AvgSaleByRole
            = @AvgSaleByRole
              + ' WHERE c.PartyCode=s.ConsPartyCode AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        SET @distId = dbo.getDistributorId(@UserId, @Role);
        SET @AvgSaleByRole
            = @AvgSaleByRole + ' WHERE c.PartyCode=s.ConsPartyCode AND s.DistributorId=' + ISNULL(@distId, '0')
              + ' AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
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
            = @AvgSaleByRole
              + ' WHERE c.PartyCode=s.ConsPartyCode AND MAX(c.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId='
              + ISNULL(@outletId, '0')
              + ') AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @AvgSaleByRole
            = @AvgSaleByRole
              + ' WHERE c.PartyCode=s.ConsPartyCode AND MAX(c.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)
			  AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth) AS AvgSale';
    END;


    /* ~~~~~~~~~ MAIN BOXES ~~~~~~~~~ */

    -- FILTER CBO BY RO WISE
    IF (@Action = 0)
    BEGIN
        SET @sql
            = N'SELECT c.PartNum,c.PartStatus,(isnull(p.Price,0)*isnull(c.[order],0))as Price FROM dbo.CustomerBackOrder c
			   INNER JOIN dbo.Workshops w ON c.WorkshopId=w.WorkShopId
               INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId WHERE w.OutletId IS NOT NULL';
    END;

    -- FILTER CBO BY SE WISE
    ELSE IF (@Action = 1)
    BEGIN
        SET @sql
            = N'SELECT c.PartNum,c.PartStatus,(isnull(p.Price,0)*isnull(c.[order],0))as Price FROM dbo.CustomerBackOrder c 
			  INNER JOIN dbo.Workshops w ON c.WorkshopId=w.WorkShopId
			  INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId WHERE c.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';
    END;

    -- GET CBO SUM
    ELSE IF (@Action = 2)
    BEGIN
        SET @sql
            = N'SELECT SUM(isnull(p.Price,0)*isnull(c.[order],0)) FROM dbo.CustomerBackOrder c 
			  INNER JOIN dbo.Workshops w ON c.WorkshopId=w.WorkShopId
			  INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId WHERE (c.PartStatus <> ''Cancel'' or c.PartStatus is null)AND p.Price IS NOT NULL';
    END;

    -- GET CANCELLED CBO SUM
    ELSE IF (@Action = 3)
    BEGIN
        SET @sql
            = N'SELECT SUM(isnull(p.Price,0)*isnull(c.[order],0)) FROM dbo.CustomerBackOrder c 
			  INNER JOIN dbo.Workshops w ON c.WorkshopId=w.WorkShopId
			  INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId WHERE c.PartStatus = ''Cancel'' AND p.Price IS NOT NULL';
    END;


    /* ~~~~~~~~~ RO WISE DETAIL ~~~~~~~~~ */


    -- RO WISE CBO DETAIL
    ELSE IF (@Action = 4)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerType,
					SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
					SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
					SUM(t.CboPrice) AS CboPrice,
					SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
					SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
					SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
					SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND w.outletId IS NOT NULL';
    END;

    -- RO WISE BRANCH CBO DETAIL
    ELSE IF (@Action = 5)
    BEGIN
        SET @sql
            = N'SELECT t.BranchCode,
						t.BranchName,
						SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
						SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
						SUM(t.CboPrice) AS CboPrice,
						SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
						SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
						SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
						SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
						o.OutletCode AS BranchCode,
						o.OutletName AS BranchName,
						COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
						COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
						SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
						' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
							 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
							 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
							 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
						WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND w.outletId IS NOT NULL';

        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType';
        END;
    END;

    -- RO WISE BRANCH CUSTOMER CBO DETAIL	
    ELSE IF (@Action = 6)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerType,
					SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
					SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
					SUM(t.CboPrice) AS CboPrice,
					SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
					SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
					SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
					SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND w.outletId IS NOT NULL AND o.OutletCode=@BranchCode';
    END;

    -- RO WISE CUSTOMER CBO DETAIL FILTER BY CUSTOMER TYPE	
    ELSE IF (@Action = 7)
    BEGIN
        SET @sql
            = N'SELECT c.PartyCode AS CustomerCode,
                max(c.PartyName) AS CustomerName,
			    max(w.Type) AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ',	
				TotalRows =  COUNT(*) OVER()
				FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND w.outletId IS NOT NULL AND o.OutletCode=@BranchCode';

        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType';
        END;

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND c.PartyCode LIKE @SearchTxt OR c.PartyName LIKE @SearchTxt';
        END;
    END;

    -- RO WISE PARTICULAR CUSTOMER DETAIL
    ELSE IF (@Action = 8)
    BEGIN
        SET @sql
            = N'SELECT c.PartyCode AS CustomerCode,
                c.PartyName AS CustomerName,
			    c.CONo AS CoNumber,
				SUM(CONVERT(int, cast(c.[Order]as decimal))) AS NumberOfParts,
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice,				
				DATEDIFF(DAY, c.CODate, GETDATE()) AS NumberOfDaysSinceOrder,
				TotalRows =  COUNT(*) OVER()				
				FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId	
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo	AND c.DistributorId=p.DistributorId			 
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <>' + N'''' + N''''
              + ') AND w.outletId IS NOT NULL AND o.OutletCode=@BranchCode';

        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType AND c.PartyCode=@CustomerCode';
        END;

		IF (@CustomerCode <> '' AND @CustomerCode IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND c.PartyCode=@CustomerCode';
        END;

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND c.PartyCode LIKE @SearchTxt OR c.PartyName LIKE @SearchTxt';
        END;
    END;

    -- RO WISE CUSTOMER PARTS DETAIL
    ELSE IF (@Action = 9 OR @Action = 15)
    BEGIN
        SET @sql
            = N'SELECT c.CODate,
                c.LocCode,
				c.PartNum,
				c.PartDesc,
				c.[Order],
				p.Price AS UnitPrice,
				CONVERT(int, cast(c.[Order]as decimal)) * isnull(p.Price,0) AS OrderValue,
				c.CBO,
				c.StkMW,
				c.ETA,
				c.Inv,
				c.Pick,
				c.Alloc,
				c.Bo, c.AO, c.Action, c.PD							
				FROM dbo.CustomerBackOrder c
				INNER JOIN dbo.Product p ON c.PartNum=p.PartNo AND c.DistributorId=p.DistributorId
				WHERE CONo=@CoNumber';
    END;

    /* ~~~~~~~~~ SALES EXECUTIVE WISE DETAIL ~~~~~~~~~ */

    -- SE WISE CBO DETAIL
    ELSE IF (@Action = 10)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerType,
					SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
					SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
					SUM(t.CboPrice) AS CboPrice,
					SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
					SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
					SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
					SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND c.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';
    END;

    -- SE WISE BRANCH CBO DETAIL
    ELSE IF (@Action = 11)
    BEGIN
        SET @sql
            = N'SELECT t.BranchCode,
						t.BranchName,
						SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
						SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
						SUM(t.CboPrice) AS CboPrice,
						SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
						SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
						SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
						SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
						o.OutletCode AS BranchCode,
						o.OutletName AS BranchName,
						COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
						COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
						SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
						SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
						' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
							 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
							 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
							 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
						WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND c.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';

        IF (@CustomerType <> '' AND @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType';
        END;
    END;

    -- SE WISE BRANCH CUSTOMER CBO DETAIL	
    ELSE IF (@Action = 12)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerType,
					SUM(t.NumberOfCboCustomers) AS NumberOfCboCustomers,
					SUM(t.NumberOfCboOrders) AS NumberOfCboOrders,
					SUM(t.CboPrice) AS CboPrice,
					SUM(t.CboPrice0To7Days) AS CboPrice0To7Days,
					SUM(t.CboPrice7To15Days) AS CboPrice7To15Days,
					SUM(t.CboPriceMoreThan15Days) AS CboPriceMoreThan15Days,
					SUM(t.AvgSale) AS AvgSale
				FROM
					(SELECT w.Type AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ' FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND c.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) AND o.OutletCode=@BranchCode';
    END;

    -- SE WISE CUSTOMER CBO DETAIL FILTER BY CUSTOMER TYPE	
    ELSE IF (@Action = 13)
    BEGIN
        SET @sql
            = N'SELECT c.PartyCode AS CustomerCode,
                max(c.PartyName) AS CustomerName,
			    max(w.Type) AS CustomerType, 
				COUNT(DISTINCT w.WorkShopId) AS NumberOfCboCustomers, 
				COUNT(DISTINCT c.CONo) AS NumberOfCboOrders, 
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())<7 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice0To7Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE()) BETWEEN 7 AND 15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPrice7To15Days, 
				SUM(CASE WHEN DATEDIFF(DAY, c.CODate, GETDATE())>15 THEN (isnull(p.Price,0)*isnull(c.[order],0)) ELSE 0 END) AS CboPriceMoreThan15Days, 
				' + @AvgSaleByRole
              + ',
				TotalRows =  COUNT(*) OVER()
				FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo and c.distributorId=p.distributorId
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND c.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) AND o.OutletCode=@BranchCode';

        IF (@CustomerType <> '' AND @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType';
        END;

        IF (@SearchTxt <> '' AND @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND c.PartyCode LIKE @SearchTxt OR c.PartyName LIKE @SearchTxt';
        END;
    END;

    -- SE WISE PARTICULAR CUSTOMER DETAIL
    ELSE IF (@Action = 14)
    BEGIN
        SET @sql
            = N'SELECT c.PartyCode AS CustomerCode,
                c.PartyName AS CustomerName,	
				c.CONo AS CoNumber,		    	
				SUM(CONVERT(int, cast(c.[Order]as decimal))) AS NumberOfParts,	
				SUM(isnull(p.Price,0)*isnull(c.[order],0)) AS CboPrice,			
				DATEDIFF(DAY, c.CODate, GETDATE()) AS NumberOfDaysSinceOrder,
				TotalRows =  COUNT(*) OVER()				
				FROM dbo.CustomerBackOrder c
					 INNER JOIN dbo.WorkShops w ON c.WorkshopId=w.WorkShopId	
					 INNER JOIN dbo.Product p ON c.PartNum=p.PartNo	AND c.DistributorId=p.DistributorId				 
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE c.WorkshopId IS NOT NULL AND (c.CONo IS NOT NULL OR c.CONo <> '''') AND c.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) AND o.OutletCode=@BranchCode';

        IF (@CustomerType <> '' AND @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType AND c.PartyCode=@CustomerCode';
        END;
		IF (@CustomerType <> '' AND @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType AND c.PartyCode=@CustomerCode';
        END;

        IF (@CustomerCode <> '' AND @CustomerCode IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND c.PartyCode=@CustomerCode';
        END;
    END;

    /* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */

    -- FILTER CBO BY USER ROLE AND DATES
    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @sql = @sql + ' AND CAST(c.CODate AS DATE) BETWEEN @StartDate AND @EndDate';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        SET @sql
            = @sql + ' AND c.DistributorId=' + ISNULL(@distId, '0')
              + ' AND CAST(c.CODate AS DATE) BETWEEN @StartDate AND @EndDate';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @sql
            = @sql + ' AND CAST(c.CODate AS DATE) BETWEEN @StartDate AND @EndDate'
              + ' AND c.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId='
              + ISNULL(@outletId, '0') + ')';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @sql
            = @sql + ' AND CAST(c.CODate AS DATE) BETWEEN @StartDate AND @EndDate'
              + ' AND c.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)';
    END;

    /* ~~~~~~~~~ FURTHER GROUPING ~~~~~~~~~ */

    -- GROUP RO WISE CBO DETAIL 
    -- GROUP RO WISE BRANCH CUSTOMER CBO DETAIL
    -- GROUP SE WISE CBO DETAIL
    -- GROUP SE WISE BRANCH CUSTOMER CBO DETAIL
    IF (@Action = 4 OR @Action = 6 OR @Action = 10 OR @Action = 12)
    BEGIN
        SET @sql = @sql + ' GROUP BY w.Type, c.PartyCode) AS t GROUP BY t.CustomerType';
    END;

    -- GROUP RO WISE BRANCH CBO DETAIL
    -- GROUP SE WISE BRANCH CBO DETAIL
    ELSE IF (@Action = 5 OR @Action = 11)
    BEGIN
        SET @sql
            = @sql
              + ' GROUP BY w.Type, c.PartyCode, o.OutletCode, o.OutletName) AS t GROUP BY t.BranchCode, t.BranchName';
    END;

    -- GROUP RO WISE CUSTOMER CBO DETAIL FILTER BY CUSTOMER TYPE, ORDER BY AND SKIP-TAKE	
    -- GROUP SE WISE CUSTOMER CBO DETAIL FILTER BY CUSTOMER TYPE, ORDER BY AND SKIP-TAKE
    ELSE IF (@Action = 7 OR @Action = 13)
    BEGIN
        SET @sql= @sql+ ' GROUP BY c.PartyCode ORDER BY ';
       if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql= @sql+ ' c.PartyCode';
	   END
	    SET @sql= @sql+ ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
    END;

    -- GROUP RO WISE PARTICULAR CUSTOMER DETAIL
    -- GROUP SE WISE PARTICULAR CUSTOMER DETAIL
    ELSE IF (@Action = 8 OR @Action = 14)
    BEGIN
        SET @sql= @sql + ' GROUP BY c.PartyCode, c.PartyName, c.CODate, c.CoNo ORDER BY ';
		if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql= @sql+ ' c.CoNo';
	   END
	    SET @sql= @sql+ ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
    END;

    PRINT @sql;

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
                       @CustomerCode = @CustomerCode,
                       @CoNumber = @CoNumber,
                       @FirstDayOfMonth = @FirstDayOfMonth,
                       @LastDayOfPrevMonth = @LastDayOfPrevMonth;
END;
