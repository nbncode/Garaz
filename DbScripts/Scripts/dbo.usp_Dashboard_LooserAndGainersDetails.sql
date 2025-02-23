USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_LooserAndGainersDetails]    Script Date: 9-11-2020 12:53:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================  
-- Author: Lokesh Choudhary  
-- Create date: 14-10-2020  
-- Last modified: 
-- Description: Display looser and gainer customers data
-- Usage: EXEC usp_Dashboard_LooserAndGainersDetails @Action=0, @Role='SuperAdmin', @UserId='24539543-60d2-49d7-b73b-b14c24e6b987', @StartDate='2020-10-01', @EndDate='2020-10-13', @CustomerType='',@BranchCode='CMU', @Skip=0, @Take=100, @SearchTxt='',@SalesExecUserId='7da7d5d2-89bf-45b5-b1ce-d2c318bc9434',@SortBy='',@SortOrder='asc'  
-- ===================================================================================  
ALTER PROCEDURE [dbo].[usp_Dashboard_LooserAndGainersDetails]  
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
    @SalesExecUserId NVARCHAR(200),
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10)
AS  
BEGIN       
   -- Declare needed parameters  
    Declare @PrvYrStartDate DATE,@PrvYrEndDate DATE, @DistributorId int=0

   -- Get distributorId using UserId and role
    set @DistributorId=dbo.getDistributorId(@UserId,@Role)

 -- Set previous year startDate and endDate  
 set @PrvYrStartDate=DATEADD(year, -1, @StartDate)  
 set @PrvYrEndDate=DATEADD(year, -1, @EndDate)  

 DECLARE @sql NVARCHAR(MAX);

    DECLARE @ParameterDef NVARCHAR(max);
    SET @ParameterDef
        = '@Role NVARCHAR(500),
							@UserId NVARCHAR(500),
							@StartDate DATE,
							@EndDate DATE,
							@CustomerType NVARCHAR(200),
							@BranchCode NVARCHAR(50),
							@Skip INT,
							@Take INT,
							@SearchTxt NVARCHAR(200),
							@PrvYrStartDate DATE,
							@PrvYrEndDate DATE,
							@SalesExecUserId NVARCHAR(200),
							@SortBy NVARCHAR(200),
	                        @SortOrder NVARCHAR(10)';

 IF (@Action = 0)  
    BEGIN 
          SET @sql
            = 'SELECT  max(w.WorkshopId) AS WorkShopId,
               isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

                isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale

			   from dbo.WorkShops w where 1=1';
    END;  

	Else IF (@Action = 1)  
    BEGIN 
          SET @sql
            = 'SELECT  max(w.WorkshopId) AS WorkShopId,
               isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

                isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale

			   from dbo.WorkShops w where w.OutletId <> '''' and w.OutletId Is Not NULL';
    END;

	Else IF (@Action = 2)  
    BEGIN 
          SET @sql
            = 'SELECT  max(w.WorkshopId) AS WorkShopId,
               isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

                isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale

			   from dbo.WorkShops w
			   inner join dbo.SalesExecutiveWorkshop se on w.WorkshopId=se.WorkshopId 
			   where 1=1';
    END;

	Else IF (@Action = 3)  
    BEGIN 
          SET @sql
            = 'SELECT  max(w.WorkshopId) AS WorkShopId,
               isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

                isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale

			   from dbo.WorkShops w where w.Type <> '''' and w.Type Is Not NULL';
    END;

	Else IF (@Action = 4 or @Action = 8 or @Action =12)  
    BEGIN 
          SET @sql
            = ';with cte as (
			   SELECT max(w.WorkshopId) AS WorkShopId,
			   max(w.Type) AS CustomerType,
			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS CurrentOrderDays,

			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS PreviousOrderDays,

			   isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

			  isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale
               
			   from dbo.WorkShops w';

-----Filter by RoWise
		IF (@Action = 4)
        BEGIN
            SET @sql = @sql + ' where w.OutletId <> '''' and w.OutletId Is Not NULL';
        END;
		 ---Filter by salesExecutive wise
		Else IF (@Action = 8)  
        BEGIN 
		SET @sql = @sql + ' inner join dbo.SalesExecutiveWorkshop se 
		              on w.WorkshopId=se.WorkshopId where 1=1';
        END;
		---Filter by CustomerType wise
		Else IF (@Action = 12)  
        BEGIN 
		SET @sql = @sql + ' where w.Type<>'''' And w.Type IS NOT NULL';
        END;
	END;

	Else IF (@Action = 5)  
    BEGIN 
          SET @sql
            = ';with cte as (
			   SELECT w.WorkshopId AS WorkShopId,
			   max(o.OutletCode) AS BranchCode,
			   max(o.OutletName) AS BranchOrSalesName,
			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS CurrentOrderDays,

			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS PreviousOrderDays,

			   isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

			  isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale
               
			   from dbo.WorkShops w
			   inner join dbo.Outlets o on w.outletId=o.outletId
			    where 1=1';
    END;

	Else IF (@Action = 6 or @Action =7 or @Action = 10 or @Action =11 or @Action = 13 or @Action =14)  
    BEGIN 
          SET @sql
            = ';with cte as (
			   SELECT w.WorkshopId AS WorkShopId,
			   max(u.ConsPartyCode) AS CustomerCode,
			   max(w.WorkShopName) AS CustomerName,
			   max(w.Type) AS CustomerType,
			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS CurrentOrderDays,

			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS PreviousOrderDays,

			   isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentOrderValue,

			  isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousOrderValue

			   from dbo.WorkShops w
			   inner join dbo.DistributorWorkShops dw on w.WorkShopId=dw.WorkShopId
			   inner join dbo.UserDetails u on dw.UserId=u.UserId';

		--- Filter by RoWise
		IF (@Action = 6 or @Action = 7)
        BEGIN
            SET @sql = @sql + ' where w.OutletId=(select top 1 outletId from dbo.Outlets where OutletCode=@BranchCode)';
        END;
		 ---Filter by salesExecutive wise
		Else IF (@Action = 10 or @Action = 11)  
        BEGIN 
		SET @sql = @sql + ' where w.WorkShopId in(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@SalesExecUserId)';
        END;
		---Filter by CustomerType wise
		Else IF (@Action = 13 or @Action =14)  
        BEGIN 
		SET @sql = @sql + ' where w.Type=@CustomerType';
        END;

		IF (@SearchTxt <> '' and @SearchTxt IS NOT NULL)
        BEGIN
            SET @sql = @sql + ' AND (u.ConsPartyCode LIKE ''%'+@SearchTxt+'%'' OR w.WorkshopName LIKE ''%'+@SearchTxt+'%'') ';
        END;
    END;

	Else IF (@Action = 9)  
    BEGIN 
          SET @sql
            = ';with cte as (
			   SELECT w.WorkshopId AS WorkShopId,
			   max(se.UserId) AS SeUserId,
			   max(o.OutletCode) AS BranchCode,
			   max(u.FirstName+'' ''+ u.LastName) AS BranchOrSalesName,
			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS CurrentOrderDays,

			   (select count(Distinct cast(CreatedDate as date)) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End) AS PreviousOrderDays,

			   isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @StartDate AND @EndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS CurrentSale,

			  isnull((select sum(cast(NetRetailSelling as decimal(18,2))) from
			   dbo.DailySalesTrackerWithInvoiceData where WorkshopId=w.WorkshopId and CreatedDate BETWEEN @PrvYrStartDate AND @PrvYrEndDate and DistributorId= Case when @Role!=''SuperAdmin'' Then ' + ISNULL(cast(@DistributorId as varchar),0)+' Else DistributorId End),0) AS PreviousSale
               
			   from dbo.WorkShops w
			   inner join dbo.Outlets o on w.outletId=o.outletId
			   inner join dbo.SalesExecutiveWorkshop se on w.WorkshopId=se.WorkshopId
			   inner join dbo.UserDetails u on se.UserId=u.UserId
			    where 1=1';
    END;
	

	/* ~~~~~~~~~ COMMON FILTERING BY ROLE AND DATE ~~~~~~~~~ */
    -- No case for 'SuperAdmin' as we are retrieving all results. 

    IF (@Role = 'Distributor')
    BEGIN
	
        SET @sql = @sql + ' and w.WorkShopId in(select WorkShopId from dbo.DistributorWorkshops where DistributorId=' + ISNULL(cast(@DistributorId as varchar),0)+')';
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
        SET @sql = @sql + ' and w.OutletId=(SELECT TOP 1 OutletId FROM dbo.DistributorsOutlets 
            WHERE UserId = @UserId)';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
        SET @sql
            = @sql
              + ' and w.WorkshopId IN(SELECT DISTINCT WorkShopId FROM dbo.SalesExecutiveWorkshop se WHERE se.UserId=@UserId)';
			  
    END;
	
	IF (@Action = 0 or @Action = 1 or @Action = 2 or @Action = 3)  
    BEGIN 
	 SET @sql= @sql+ ' Group By w.WorkShopId';
	 END;

	 Else IF (@Action = 4or @Action = 5 or @Action = 8or @Action = 9 or @Action =12)  
    BEGIN 
	 SET @sql= @sql+ ' Group By w.WorkShopId)select *,cast(Case When CurrentSale>PreviousSale  Then 1 Else 0 End as bit) as IsGainer from cte';
	 END;

	  Else IF (@Action = 6 or @Action = 7 or @Action = 10 or @Action = 11 or @Action = 13 or @Action = 14)  
    BEGIN 
	 SET @sql= @sql+ ' Group By w.WorkShopId)
	 select *,
	 cast(Case When PreviousOrderDays>0 Then (CurrentOrderDays-PreviousOrderDays)*100/cast(PreviousOrderDays as decimal(18,2)) Else cast(CurrentOrderDays as decimal(18,2))End as decimal(18,2)) as GrowthDays,
	 (Case When PreviousOrderValue>0 Then(CurrentOrderValue-PreviousOrderValue)*100/PreviousOrderValue Else CurrentOrderValue End) as GrowthValue,
	 TotalRows = COUNT(*) OVER()
	  from cte';
	  --- filter Gainers Customer
	IF (@Action = 6 or @Action = 10 or @Action = 13)  
    BEGIN 
	 SET @sql= @sql+ ' Where CurrentOrderValue>PreviousOrderValue Order By';
	END;
	--- filter Loosers Customer
	Else
	BEGIN 
	 SET @sql= @sql+ ' Where CurrentOrderValue<=PreviousOrderValue Order By';
	END;

	if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql= @sql+ ' CustomerCode';
	   END
	    SET @sql= @sql+ ' OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY';



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
                       @PrvYrStartDate = @PrvYrStartDate,
                       @PrvYrEndDate = @PrvYrEndDate,
                       @SalesExecUserId = @SalesExecUserId,
					   @SortBy =@SortBy,
	                   @SortOrder=@SortOrder;
END;  
  
  
  