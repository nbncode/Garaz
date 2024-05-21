USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_WalletDetail]    Script Date: 10/13/2020 10:17:45 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====================================================================================================  
-- Author: Vikram Singh Saini  
-- Create date: 28-03-2020  
-- Last modified: 13-10-2020  (Add server side sorting)
-- Description: Display wallets detail RO, SE and CS wise.  
-- Usage: EXEC usp_Dashboard_WalletDetail @Action=14, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2020-01-01', @EndDate='2020-10-13', @CustomerType='',@BranchCode='TRS', @Skip=0, @Take=10, @SearchTxt='', @CustomerCode='2978',@FirstDayOfMonth='2020-10-01',@LastDayOfPrevMonth='2020-09-30',@SortBy='averagesale , walletbalance',@SortOrder='desc'  
-- =====================================================================================================  

ALTER PROCEDURE [dbo].[usp_Dashboard_WalletDetail]  
    @Action INT,  
    @Role NVARCHAR(500)=null,  
    @UserId NVARCHAR(500)=null,  
    @StartDate DATE=null,  
    @EndDate DATE=null,  
    @CustomerType NVARCHAR(200)=null,  
    @BranchCode NVARCHAR(50)=null,  
    @Skip INT=null,  
    @Take INT=null,  
    @SearchTxt NVARCHAR(200)=null,  
    @CustomerCode NVARCHAR(100)=null,  
    @FirstDayOfMonth DATE=null,  
    @LastDayOfPrevMonth DATE=null,
	@SortBy NVARCHAR(200)=null,
	@SortOrder NVARCHAR(10)=null
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
              + ' WHERE MAX(wt.WorkShopId)=s.WorkShopId AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth)';  
    END;  
    ELSE IF (@Role = 'Distributor')  
    BEGIN  
        SET @distId = dbo.getDistributorId(@UserId, @Role);  
        SET @AvgSaleByRole  
            = @AvgSaleByRole + ' WHERE MAX(wt.WorkShopId)=s.WorkShopId AND s.DistributorId=' + ISNULL(@distId, '0')  
              + ' AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth)';  
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
              + ' WHERE MAX(wt.WorkShopId)=s.WorkShopId AND MAX(wt.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId='  
              + ISNULL(@outletId, '0')  
              + ') AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth)';  
    END;  
    ELSE IF (@Role = 'SalesExecutive')  
    BEGIN  
        SET @AvgSaleByRole  
            = @AvgSaleByRole  
              + ' WHERE MAX(wt.WorkShopId)=s.WorkShopId AND MAX(wt.WorkShopId) IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)  
     AND s.CreatedDate BETWEEN DATEADD(MONTH, -3, @FirstDayOfMonth) AND @LastDayOfPrevMonth)';  
    END;  
  
  
    /* ~~~~~~~~~ MAIN BOXES ~~~~~~~~~ */  
  
    -- ALL WALLETS TOTAL BALANCE  
    IF (@Action = 0)  
    BEGIN  
        SET @sql  
            = 'SELECT ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0)   
       FROM dbo.WalletTransaction wt  
    INNER JOIN dbo.Workshops w ON w.WorkShopId=wt.WorkshopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql = @sql + ' WHERE w.Type IS NOT NULL';  
		
    END;  
  
    -- TOTAL WALLET BALANCE BY RO WISE  
    ELSE IF (@Action = 1)  
    BEGIN  
        SET @sql  
            = N'SELECT ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0)  
       FROM dbo.WalletTransaction wt  
    INNER JOIN dbo.Workshops w ON w.WorkShopId=wt.WorkshopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' WHERE wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL)';  
    END;  
  
    -- TOTAL WALLET BALANCE BY SE WISE  
    ELSE IF (@Action = 2)  
    BEGIN  
        SET @sql  
            = 'SELECT ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0)  
       FROM dbo.WalletTransaction wt  
    INNER JOIN dbo.Workshops w ON w.WorkShopId=wt.WorkshopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' WHERE wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';  
    END;  
  
    -- TOTAL WALLET BALANCE BY CS WISE  
    ELSE IF (@Action = 3)  
    BEGIN  
        SET @sql  
            = 'SELECT ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0)  
       FROM dbo.WalletTransaction wt  
    INNER JOIN dbo.Workshops w ON w.WorkShopId=wt.WorkshopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END; 
    END;  
  
  
    /* ~~~~~~~~~ RO WISE DETAIL ~~~~~~~~~ */  
  
    -- RO WISE WALLET DETAIL  
    ELSE IF (@Action = 4)  
    BEGIN  
  
        SET @sql  
            = N'SELECT t.CustomerType,  
     SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
     SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
     SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance  
    FROM  
     (SELECT w.Type AS CustomerType,   
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,   
      ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance  
      FROM dbo.WalletTransaction wt           
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' WHERE wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL)';  
    END;  
  
    -- RO WISE BRANCH WALLET DETAIL  
    ELSE IF (@Action = 5)  
    BEGIN  
        SET @sql  
            = N'SELECT t.BranchCode,  
      t.BranchName,        
      SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
      SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
      SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance  
       FROM  
     (SELECT w.Type AS CustomerType,   
     o.OutletCode AS BranchCode,  
     o.OutletName AS BranchName,  
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,    
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance  
      FROM dbo.WalletTransaction wt          
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
      WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL)';  
  
        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND w.Type=@CustomerType';  
        END;  
    END;  
  
    -- RO WISE BRANCH CUSTOMERS BY TYPE  
    ELSE IF (@Action = 6)  
    BEGIN  
        SET @sql  
            = N'SELECT t.CustomerType,  
     SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
     SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
     SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance       
    FROM  
     (SELECT w.Type AS CustomerType,   
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,   
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance       
      FROM dbo.WalletTransaction wt           
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
      WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL) AND o.OutletCode=@BranchCode';  
    END;  
  
    -- RO WISE CUSTOMER WALLET DETAIL FILTER BY CUSTOMER TYPE  
    ELSE IF (@Action = 7)  
    BEGIN  
        SET @sql  
            = N'SELECT u.ConsPartyCode AS CustomerCode,  
                    w.WorkShopName AS CustomerName,  
     w.Type AS CustomerType,          
     ' + @AvgSaleByRole  
              + ' AS AverageSale,   
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance,  
     TotalRows =  COUNT(*) OVER()  
      FROM dbo.WalletTransaction wt  
      INNER JOIN dbo.UserDetails u ON wt.UserId=u.UserId             
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
      WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL) AND o.OutletCode=@BranchCode';  
  
        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND w.Type=@CustomerType';  
        END;  
  
        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND u.ConsPartyCode LIKE @SearchTxt OR w.WorkShopName LIKE @SearchTxt';  
        END;  
    END;  
  
    /* ~~~~~~~~~ SE WISE DETAIL ~~~~~~~~~ */  
  
    -- SE WISE WALLET DETAIL  
    ELSE IF (@Action = 8)  
    BEGIN  
  
        SET @sql  
            = N'SELECT t.CustomerType,  
     SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
     SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
     SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance  
    FROM  
     (SELECT w.Type AS CustomerType,   
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,   
      ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance  
      FROM dbo.WalletTransaction wt           
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';  
    END;  
  
    -- SE WISE BRANCH WALLET DETAIL  
    ELSE IF (@Action = 9)  
    BEGIN  
        SET @sql  
            = N'SELECT t.BranchCode,  
      t.SalesExecName,        
      SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
      SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
      SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance  
       FROM  
     (SELECT w.Type AS CustomerType,   
     o.OutletCode AS BranchCode,  
     CONCAT(u.FirstName,'' '', u.LastName) AS SalesExecName,  
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,    
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance  
      FROM dbo.WalletTransaction wt          
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
         INNER JOIN dbo.SalesExecutiveWorkshop s ON wt.WorkshopId=s.WorkshopId  
      INNER JOIN dbo.UserDetails u ON s.UserId=u.UserId  
         WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop)';  
  
        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND w.Type=@CustomerType';  
        END;  
    END;  
  
    -- SE WISE BRANCH CUSTOMERS BY TYPE  
    ELSE IF (@Action = 10)  
    BEGIN  
        SET @sql  
            = N'SELECT t.CustomerType,  
     SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
     SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
     SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance       
    FROM  
     (SELECT w.Type AS CustomerType,   
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,   
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance       
      FROM dbo.WalletTransaction wt           
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
      SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
      WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) AND o.OutletCode=@BranchCode';  
    END;  
  
    -- SE WISE CUSTOMER WALLET DETAIL FILTER BY CUSTOMER TYPE  
    ELSE IF (@Action = 11)  
    BEGIN  
        SET @sql  
            = N'SELECT u.ConsPartyCode AS CustomerCode,  
                    w.WorkShopName AS CustomerName,  
     w.Type AS CustomerType,          
     ' + @AvgSaleByRole  
              + ' AS AverageSale,   
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance,  
     TotalRows =  COUNT(*) OVER()  
      FROM dbo.WalletTransaction wt  
      INNER JOIN dbo.UserDetails u ON wt.UserId=u.UserId             
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql  
            = @sql  
              + ' INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId  
      WHERE wt.WorkshopId IS NOT NULL AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) AND o.OutletCode=@BranchCode';  
  
        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND w.Type=@CustomerType';  
        END;  
  
        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND u.ConsPartyCode LIKE @SearchTxt OR w.WorkShopName LIKE @SearchTxt';  
        END;  
    END;  
  
    /* ~~~~~~~~~ CS WISE DETAIL ~~~~~~~~~ */  
  
    -- CS WISE WALLET DETAIL  
    ELSE IF (@Action = 12)  
    BEGIN  
        SET @sql  
            = N'SELECT t.CustomerType,  
     SUM(t.NumberOfCustomers) AS NumberOfCustomers,       
     SUM(ISNULL(t.AvgSale,0)) AS AvgSale,  
     SUM(ISNULL(t.WalletBalance,0)) AS WalletBalance  
    FROM  
     (SELECT w.Type AS CustomerType,   
     COUNT(DISTINCT wt.WorkShopId) AS NumberOfCustomers,    
     ' + @AvgSaleByRole  
              + ' AS AvgSale,   
      ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance  
      FROM dbo.WalletTransaction wt           
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
        
    END;  
  
    -- CS WISE CUSTOMER WALLET DETAIL  
    ELSE IF (@Action = 13)  
    BEGIN  
        SET @sql  
            = N'SELECT u.ConsPartyCode AS CustomerCode,  
                    w.WorkShopName AS CustomerName,  
     w.Type AS CustomerType,          
     ' + @AvgSaleByRole  
              + ' AS AverageSale,   
     ISNULL(SUM(CASE WHEN wt.Type = ''Cr'' THEN wt.Amount END),0)-ISNULL(SUM( CASE WHEN wt.Type = ''Dr'' THEN wt.Amount END),0) AS WalletBalance,  
     TotalRows =  COUNT(*) OVER()  
      FROM dbo.WalletTransaction wt  
      INNER JOIN dbo.UserDetails u ON wt.UserId=u.UserId             
      INNER JOIN dbo.Workshops w ON wt.WorkShopId=w.WorkShopId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql = @sql + ' WHERE wt.WorkshopId IS NOT NULL'; 
		 
        IF (@CustomerType <> '' and @CustomerType IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND w.Type=@CustomerType';  
        END; 

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)  
        BEGIN  
            SET @sql = @sql + ' AND u.ConsPartyCode LIKE @SearchTxt OR w.WorkShopName LIKE @SearchTxt';  
        END;  
    END;  
  
    -- COMMON FOR RO, SE, CS  
    ELSE IF (@Action = 14)  
    BEGIN  
        SET @sql  
            = 'SELECT wt.CreatedDate as DateOfTransaction, wt.Description as TransactionDetails, (CASE WHEN wt.Type=''Cr'' THEN wt.Amount ELSE -wt.Amount END) AS Amount,  
       TotalRows =  COUNT(*) OVER()  
             FROM WalletTransaction wt  
    INNER JOIN dbo.UserDetails u ON wt.UserId=u.UserId';  
  
        IF (@Role = 'Distributor')  
        BEGIN  
            SET @sql = @sql + ' INNER JOIN dbo.DistributorWorkshops dw ON wt.WorkShopId=dw.WorkShopId';  
        END;  
  
        SET @sql = @sql + ' WHERE u.ConsPartyCode=@CustomerCode';  
    END;  
  
    /* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */  
  
    IF (@Role = 'SuperAdmin')  
    BEGIN  
        SET @sql = @sql + ' AND CAST(wt.CreatedDate AS DATE) BETWEEN @StartDate AND @EndDate';  
    END;  
    ELSE IF (@Role = 'Distributor')  
    BEGIN  
        SET @sql  
            = @sql + ' AND dw.DistributorId=' + ISNULL(@distId, '0')  
              + ' AND CAST(wt.CreatedDate AS DATE) BETWEEN @StartDate AND @EndDate';  
    END;  
    ELSE IF (@Role = 'RoIncharge')  
    BEGIN  
        SET @sql  
            = @sql + ' AND CAST(wt.CreatedDate AS DATE) BETWEEN @StartDate AND @EndDate'  
              + ' AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId='  
              + ISNULL(@outletId, '0') + ')';  
    END;  
    ELSE IF (@Role = 'SalesExecutive')  
    BEGIN  
        SET @sql  
            = @sql + ' AND CAST(wt.CreatedDate AS DATE) BETWEEN @StartDate AND @EndDate'  
              + ' AND wt.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)';  
    END;  
  
    /* ~~~~~~~~~ FURTHER GROUPING ~~~~~~~~~ */  
  
    -- GROUP RO, SE, CS WISE WALLET DETAIL   
    IF (@Action = 4 OR @Action = 8 OR @Action = 12)  
    BEGIN  
        SET @sql = @sql + ' GROUP BY w.Type) AS t GROUP BY t.CustomerType';  
    END;  
  
    -- GROUP RO WISE BRANCH WALLET DETAIL       
    ELSE IF (@Action = 5)  
    BEGIN  
        SET @sql  
            = @sql  
              + ' GROUP BY w.Type, o.OutletCode, o.OutletName) AS t GROUP BY t.BranchCode, t.BranchName ORDER BY t.BranchCode';  
    END;  
  
    -- GROUP SE WISE BRANCH WALLET DETAIL      
    ELSE IF (@Action = 9)  
    BEGIN  
        SET @sql  
            = @sql  
              + ' GROUP BY w.Type, o.OutletCode, u.FirstName, u.LastName) AS t GROUP BY t.BranchCode, t.SalesExecName ORDER BY t.BranchCode';  
    END;  
  
    -- GROUP RO, SE WISE BRANCH CUSTOMER WALLET DETAIL      
    ELSE IF (@Action = 6 OR @Action = 10)  
    BEGIN  
        SET @sql = @sql + ' GROUP BY w.Type) AS t GROUP BY t.CustomerType';  
    END;  
  
    -- GROUP RO, SE, CS WISE CUSTOMER WALLET DETAIL FILTER BY CUSTOMER TYPE, ORDER BY AND SKIP-TAKE   
    ELSE IF (@Action = 7 OR @Action = 11 OR @Action = 13)  
    BEGIN            
		SET @sql= @sql+ ' GROUP BY w.Type, u.ConsPartyCode, w.WorkShopName ORDER BY ';

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
  
    ELSE IF (@Action = 14)  
    BEGIN  

       if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' ORDER BY '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql= @sql+ ' ORDER BY DateOfTransaction';
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
                       @FirstDayOfMonth = @FirstDayOfMonth,  
                       @LastDayOfPrevMonth = @LastDayOfPrevMonth;  
END;  