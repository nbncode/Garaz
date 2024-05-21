USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CollectionDetail]    Script Date: 10/8/2020 10:56:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====================================================================================================
-- Author: Vikram Singh Saini & Lokesh Choudhary
-- Create date: 26-03-2020
-- Last modified: 08-10-2020 (Add sorting)
-- Description:	Display collection (or account ledger) detail.
-- Usage: EXEC usp_Dashboard_CollectionDetail @Action=6, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-02-25', @CustomerType='',@BranchCode='', @Skip=0, @Take=0, @SearchTxt='', @CustomerCode='',@ConstantsClosingBalance='Closing Balance',@ConstantsOpeningBalance='Opening Balance',@SortBy='',@SortOrder='desc'
-- =====================================================================================================

ALTER PROCEDURE [dbo].[usp_Dashboard_CollectionDetail]
    @Action INT,
    @Role NVARCHAR(500),
    @UserId NVARCHAR(500),
    @StartDate DATE,
    @EndDate DATE,
    @CustomerType NVARCHAR(200),
    @BranchCode NVARCHAR(30),
    @Skip INT,
    @Take INT,
    @SearchTxt NVARCHAR(200),
    @CustomerCode NVARCHAR(100),
	@ConstantsClosingBalance NVARCHAR(100),
	@ConstantsOpeningBalance NVARCHAR(100),
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    DECLARE @sql NVARCHAR(MAX);

	DECLARE @OutletId INT=NULL;

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
							@OutletId INT,
							@ConstantsClosingBalance NVARCHAR(100),
	                        @ConstantsOpeningBalance NVARCHAR(100),
							@SortBy NVARCHAR(200),
							@SortOrder NVARCHAR(10)';

    -- GET COLLECTION BY RO WISE
    IF (@Action = 0)
    BEGIN
        SET @sql
            = N'SELECT (isnull(sum(case when a.VchType!=''Payment'' then a.Credit else 0 end),0)-isnull(sum(case when a.VchType=''Payment'' then a.Debit else 0 end),0)) AS TotalCollection FROM dbo.AccountLedger a 
			  INNER JOIN dbo.Workshops w ON a.WorkshopId=w.WorkShopId
			  WHERE a.WorkshopId IS NOT NULL AND w.OutletId IS NOT NULL ';
    END;

    -- GET COLLECTION BY SE WISE
    ELSE IF (@Action = 1)
    BEGIN
        SET @sql
            = N'SELECT (isnull(sum(case when a.VchType!=''Payment'' then a.Credit else 0 end),0)-isnull(sum(case when a.VchType=''Payment'' then a.Debit else 0 end),0)) AS TotalCollection FROM dbo.AccountLedger a	
			  INNER JOIN dbo.Workshops w ON a.WorkshopId=w.WorkShopId		   
			  WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) ';
    END;

    -- GET COLLECTION BY CS WISE
    ELSE IF (@Action = 2)
    BEGIN
        SET @sql
            = N'SELECT (isnull(sum(case when a.VchType!=''Payment'' then a.Credit else 0 end),0)-isnull(sum(case when a.VchType=''Payment'' then a.Debit else 0 end),0)) AS TotalCollection FROM dbo.AccountLedger a 
			  INNER JOIN dbo.Workshops w ON a.WorkshopId=w.WorkShopId 
			  WHERE a.WorkshopId IS NOT NULL ';
    END;

    /* ~~~~~~~~~ RO WISE DETAIL ~~~~~~~~~ */

    -- RO WISE COLLECTION DETAIL
    ELSE IF (@Action = 3)
    BEGIN
        SET @sql
            = N'SELECT w.Type AS CustomerType, COUNT(DISTINCT a.Code) AS NoOfCustomers FROM dbo.AccountLedger a 
			  INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
			  WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL) ';
    END;

    -- RO WISE BRANCH COLLECTION DETAIL
    ELSE IF (@Action = 4)
    BEGIN
        SET @sql
            = N'SELECT t.BranchCode,
					   t.BranchName,
					   SUM(t.NumberOfCustomers) AS NumberOfCustomers						
				FROM
					(SELECT w.Type AS CustomerType, 
						o.OutletCode AS BranchCode,
						o.OutletName AS BranchName,
						COUNT(DISTINCT a.Code) AS NumberOfCustomers FROM dbo.AccountLedger a 
				INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
				INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
			    WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL) ';
    END;

    -- RO WISE BRANCH CUSTOMERS BY TYPE
    ELSE IF (@Action = 5)
    BEGIN
        SET @sql
            = N'SELECT w.Type AS CustomerType, COUNT(DISTINCT a.Code) AS NoOfCustomers FROM dbo.AccountLedger a 
			  INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
			  INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
			  WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId IS NOT NULL) 
			  AND o.OutletCode=@BranchCode ';
    END;

    -- RO WISE CUSTOMER COLLECTION DETAIL
    ELSE IF (@Action = 6)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerCode,
					   t.CustomerName,
					   t.CustomerType,
					   t.TotalRows						
				FROM
					(SELECT a.Code AS CustomerCode, max(w.WorkShopName) AS CustomerName, w.Type AS CustomerType, 				
				TotalRows =  COUNT(*) OVER()
				FROM dbo.AccountLedger a
					 INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId					 
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE outletId IS NOT NULL) 
				AND o.OutletCode=@BranchCode ';

        IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND a.Code LIKE @SearchTxt OR a.PartyName LIKE @SearchTxt ';
        END;

    END;

    /* ~~~~~~~~~ SE WISE DETAIL ~~~~~~~~~ */

    -- SE WISE COLLECTION DETAIL
    ELSE IF (@Action = 7)
    BEGIN
        SET @sql
            = N'SELECT w.Type AS CustomerType, COUNT(DISTINCT a.Code) AS NoOfCustomers FROM dbo.AccountLedger a 
			  INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
			  WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) ';
    END;

    -- SE WISE BRANCH COLLECTION DETAIL
    ELSE IF (@Action = 8)
    BEGIN
        SET @sql
            = N'SELECT t.BranchCode,
					   t.SalesExecName,
					   SUM(t.NumberOfCustomers) AS NumberOfCustomers						
				FROM
					(SELECT w.Type AS CustomerType, 
						o.OutletCode AS BranchCode,
						CONCAT(u.FirstName,'' '', u.LastName) AS SalesExecName,
						COUNT(DISTINCT a.Code) AS NumberOfCustomers FROM dbo.AccountLedger a 
				INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
				INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				INNER JOIN dbo.SalesExecutiveWorkshop s ON a.WorkshopId=s.WorkshopId
				INNER JOIN dbo.UserDetails u ON s.UserId=u.UserId
			    WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) ';
    END;

    -- SE WISE BRANCH CUSTOMERS BY TYPE
    ELSE IF (@Action = 9)
    BEGIN
        SET @sql
            = N'SELECT w.Type AS CustomerType, COUNT(DISTINCT a.Code) AS NoOfCustomers FROM dbo.AccountLedger a 
			  INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
			  INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
			  WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) 
			  AND o.OutletCode=@BranchCode ';
    END;

    -- SE WISE CUSTOMER COLLECTION DETAIL
    ELSE IF (@Action = 10)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerCode,
					   t.CustomerName,
					   t.CustomerType,
					   t.TotalRows						
				FROM
					(SELECT a.Code AS CustomerCode, max(w.WorkShopName) AS CustomerName, w.Type AS CustomerType, 				
				TotalRows =  COUNT(*) OVER()
				FROM dbo.AccountLedger a
					 INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId					 
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE a.WorkshopId IS NOT NULL AND a.WorkshopId IN (SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop) 
				AND o.OutletCode=@BranchCode ';

		IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND a.Code LIKE @SearchTxt OR a.PartyName LIKE @SearchTxt';
        END;
    END;

	 /* ~~~~~~~~~ CS WISE DETAIL ~~~~~~~~~ */

    -- CS WISE COLLECTION DETAIL
    ELSE IF (@Action = 11)
    BEGIN
        SET @sql
            = N'SELECT w.Type AS CustomerType, COUNT(DISTINCT a.Code) AS NoOfCustomers FROM dbo.AccountLedger a 
			  INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
			  WHERE a.WorkshopId IS NOT NULL ';
    END;

	 -- CS WISE CUSTOMER COLLECTION DETAIL
    ELSE IF (@Action = 12)
    BEGIN
        SET @sql
            = N'SELECT t.CustomerCode,
					   t.CustomerName,
					   t.CustomerType,
					   t.TotalRows						
				FROM
					(SELECT a.Code AS CustomerCode, max(w.WorkShopName) AS CustomerName, w.Type AS CustomerType, 				
				TotalRows =  COUNT(*) OVER()
				FROM dbo.AccountLedger a
					 INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId					 
					 INNER JOIN dbo.Outlets o ON w.outletId=o.OutletId
				WHERE a.WorkshopId IS NOT NULL ';        

		IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND a.Code LIKE @SearchTxt OR a.PartyName LIKE @SearchTxt';
        END;
    END;

    -- *** SPECIFIC CUSTOMER DETAIL (COMMON FOR RO, SE and CS) ***
    ELSE IF (@Action = 13)
    BEGIN
        SET @sql
            = 'SELECT a.Code AS CustomerCode, max(w.WorkShopName) AS CustomerName, w.Type AS CustomerType, a.Date AS PaymentDate, a.Particulars, isnull(sum(case when a.VchType=''Payment'' then a.Debit else a.Credit end),0) AS PaymentAmount,
			a.VchType ,TotalRows =  COUNT(*) OVER()
				    FROM dbo.AccountLedger a
					INNER JOIN dbo.WorkShops w ON a.WorkshopId=w.WorkShopId
					WHERE a.Code=@CustomerCode ';
    END;

    /* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */
	SET @sql = @sql + ' AND a.VchType in(''Receipt'',''Cash Receipt'',''Payment'') '; 
    -- FILTER COLLECTION BY USER ROLE AND DATES
    IF (@Role = 'SuperAdmin')
    BEGIN
        SET @sql = @sql + ' AND a.Date BETWEEN @StartDate AND @EndDate';
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN
        DECLARE @distId NVARCHAR(50);
        SET @distId = dbo.getDistributorId(@UserId, @Role);
        SET @sql
            = @sql + ' AND a.DistributorId=' + ISNULL(@distId, 0)
              + ' AND CAST(a.Date AS DATE) BETWEEN @StartDate AND @EndDate';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        --DECLARE @outletId INT=NULL;
        SET @OutletId =
        (
            SELECT TOP 1
                OutletId
            FROM dbo.DistributorsOutlets do
            WHERE do.UserId = @UserId
        );
        SET @sql
            = @sql + ' AND CAST(a.Date AS DATE) BETWEEN @StartDate AND @EndDate'
              + ' AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.WorkShops WHERE OutletId=Isnull(@OutletId,0))';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @sql
            = @sql + ' AND CAST(a.Date AS DATE) BETWEEN @StartDate AND @EndDate'
              + ' AND a.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)';
    END;

    /* ~~~~~~~~~ FURTHER GROUPING ~~~~~~~~~ */

    -- GROUP RO WISE COLLECTION DETAIL 
    -- GROUP RO WISE BRANCH CUSTOMERS BY TYPE
    -- GROUP SE WISE COLLECTION DETAIL
    -- GROUP SE WISE BRANCH CUSTOMERS BY TYPE
	-- GROUP CS WISE COLLECTION DETAIL
    IF (@Action = 3 OR @Action = 5 OR @Action = 7 OR @Action = 9 OR @Action=11)
    BEGIN
        SET @sql = @sql + ' GROUP BY w.Type';
    END;

    -- GROUP RO WISE BRANCH COLLECTION DETAIL    
    ELSE IF (@Action = 4)
    BEGIN
        SET @sql
            = @sql
              + ' GROUP BY w.Type, o.OutletCode, o.OutletName) AS t GROUP BY t.BranchCode, t.BranchName ORDER BY t.BranchCode';
    END;

    -- GROUP RO WISE CUSTOMER COLLECTION DETAIL
    -- GROUP SE WISE CUSTOMER COLLECTION DETAIL
	-- GROUP CS WISE CUSTOMER COLLECTION DETAIL
    ELSE IF (@Action = 6 OR @Action = 10 OR @Action=12)
    BEGIN
	  -- COMMON FILTER BY CUSTOMERTYPE
	    IF (@CustomerType <> '' and @CustomerType IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND w.Type=@CustomerType';
        END;

        SET @sql
            = @sql
              + ' GROUP BY a.Code, a.PartyName, w.Type ) AS t';
		IF (@SortBy <> '' and @SortBy IS NOT NULL)
        BEGIN
			SET @sql = @sql + ' ORDER BY '+CASE WHEN @SortBy='CustomerCode' THEN 'CustomerCode '  when @SortBy='CustomerName' THEN 'CustomerName '
			when @SortBy='CustomerType' THEN 'CustomerType ' ELSE 'TotalRows ' END + cast(@SortOrder as varchar);
        END;
		else begin SET @sql = @sql + ' ORDER BY TotalRows' end
		SET @sql = @sql + ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
    END;

    -- GROUP SE WISE BRANCH COLLECTION DETAIL    
    ELSE IF (@Action = 8)
    BEGIN
        SET @sql
            = @sql
              + ' GROUP BY w.Type, o.OutletCode, u.FirstName, u.LastName) AS t GROUP BY t.BranchCode, t.SalesExecName ORDER BY t.BranchCode';
    END;

    -- GROUP SPECIFIC CUSTOMER DETAIL (COMMON FOR RO, SE and CS)
    ELSE IF (@Action = 13)
    BEGIN
        SET @sql= @sql + ' GROUP BY a.Code, a.PartyName, w.Type, a.Date, a.Particulars,a.VchType ';

			  IF (@SortBy <> '' and @SortBy IS NOT NULL)
              BEGIN
		     	SET @sql = @sql + ' ORDER BY '+CASE WHEN @SortBy='CustomerCode' THEN 'CustomerCode '  when @SortBy='CustomerName' THEN 'CustomerName '
			    when @SortBy='CustomerType' THEN 'CustomerType ' 
			    when @SortBy='PaymentDate' THEN 'PaymentDate '
			    when @SortBy='Particulars' THEN 'Particulars '
			    when @SortBy='PaymentAmount' THEN 'PaymentAmount '
			    when @SortBy='VchType' THEN 'VchType '
			    ELSE 'TotalRows ' END + cast(@SortOrder as varchar);
              END;
			  Else Begin SET @sql = @sql + ' ORDER BY TotalRows '; END

			  SET @sql = @sql + ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';
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
					   @OutletId=@OutletId,
					   @ConstantsClosingBalance=@ConstantsClosingBalance,
	                   @ConstantsOpeningBalance=@ConstantsOpeningBalance,
					   @SortBy=@SortBy,
	                   @SortOrder=@SortOrder;
END;
