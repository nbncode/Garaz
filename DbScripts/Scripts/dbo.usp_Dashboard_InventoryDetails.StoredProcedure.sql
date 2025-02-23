USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_InventoryDetails]    Script Date: 11-11-2020 12:35:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 28-03-2020
-- Last modified: 11-11-2020 replace StockQuantity to PartQty column to calculate stock price
-- Description:	Display Inventory details.
-- Usage: EXEC usp_Dashboard_InventoryDetails @Action=0, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA',@BranchCode='TRS',@Skip=0,@Take=10,@SearchTxt='',@SortBy='NumberOfStock',@SortOrder='desc',@AvgThreeMonthStartDate ='2020-07-01',@AvgThreeMonthEndDate='2020-09-30',@DaysDiff=8
-- Usage: EXEC usp_Dashboard_InventoryDetails @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA',@BranchCode='TRS',@Skip=0,@Take=10,@SearchTxt='',@SortBy='NumberOfStock',@SortOrder='desc',@AvgThreeMonthStartDate ='2020-07-01',@AvgThreeMonthEndDate='2020-09-30',@DaysDiff=8
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_InventoryDetails]
    @Action int,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
	@BranchCode nvarchar(50),	
	@Skip INT,
    @Take INT,
	@SearchTxt NVARCHAR(200),
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10),
	@AvgThreeMonthStartDate DATE,
    @AvgThreeMonthEndDate DATE,
	@DaysDiff int
AS
BEGIN
DECLARE @sql NVARCHAR(MAX);
   IF(@Action=0)
	Begin
    SET @sql= 'select 	 
	max(o.OutletCode) as BranchCode,max(o.OutletName)as BranchName,sum(isnull(s.PartQty,0))as PartLinesInStock,
    sum(isnull(s.PartQty,0)*isnull(p.Price,0))as StockPrice,
	AverageSales=(select isnull(sum(isnull(cast(NetRetailSelling as decimal(18,2)),0))/3,0) from dbo.DailySalesTrackerWithInvoiceData where DistributorId=max(s.DistributorId) and LocCode=max(o.OutletCode)and CreatedDate between '''+cast(@AvgThreeMonthStartDate as varchar)+''' and '''+cast(@AvgThreeMonthEndDate as varchar)+'''),
	StockDays=Case when (select sum(isnull(cast(NetRetailSelling as decimal(18,2)),0))/90 from dbo.DailySalesTrackerWithInvoiceData where DistributorId=max(s.DistributorId) and LocCode=max(o.OutletCode)and CreatedDate between '''+cast(@AvgThreeMonthStartDate as varchar)+''' and '''+cast(@AvgThreeMonthEndDate as varchar)+''')>0 then sum(isnull(s.PartQty,0)*isnull(p.Price,0))/(select sum(isnull(cast(NetRetailSelling as decimal(18,2)),0))/90 from dbo.DailySalesTrackerWithInvoiceData where DistributorId=max(s.DistributorId) and LocCode=max(o.OutletCode)and CreatedDate between '''+cast(@AvgThreeMonthStartDate as varchar)+''' and '''+cast(@AvgThreeMonthEndDate as varchar)+''') Else 0 End
    from dbo.product p
    inner join dbo.dailyStock s on p.PartNo=s.PartNum
	inner join dbo.Outlets o on s.OutletId=o.OutletId where 1=1 ';


    IF (@Role = 'Distributor')
    BEGIN

	SET @sql=@sql+' And s.OutletId in (select OutletId from dbo.DistributorsOutlets where DistributorId=(select top 1 DistributorId from dbo.DistributorUser where UserId='''+cast(@UserId as varchar(200))+''')) ';
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN

	SET @sql=@sql+' And s.OutletId in (select OutletId from dbo.DistributorsOutlets where UserId='''+cast(@UserId as varchar(200))+''') ';
		
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	
	SET @sql=@sql+' And s.OutletId in (select o.OutletId from dbo.DistributorsOutlets o
        inner join dbo.RoSalesExecutive se on o.UserId=se.RoUserId
       where se.SeUserId='''+cast(@UserId as varchar(200))+''') ';
    END;  

   SET @sql=@sql+' group by s.LocationCode order by s.LocationCode ';
	
	   
	     PRINT @sql;
         EXEC (@sql)

	End
   
   Else If(@Action=1)
   Begin

	  SET @sql
            = ';WITH search          
   AS  
   (
  select 	      
	 max(g.GroupName)as PartGroup,
	 max(Case when p.PartCategoryCode=''Maruti Suzuki Genuine Parts'' Then ''M'' 
     When p.PartCategoryCode=''Maruti Suzuki Genuine Accessories'' Then ''AA''
     When p.PartCategoryCode=''Maruti Suzuki Genuine Oil'' Then ''AG'' 
     When p.PartCategoryCode=''Tools'' Then ''T'' Else p.PartCategoryCode End) as PartCategory,
	 max(p.RootPartNum) as RootPartNumber,
	 max(s.PartNum) as PartNumber,
	 max(p.Description)as PartDescription,
	 max(isnull(p.Price,0)) as MRP,
	 sum(isnull(s.PartQty,0))as NumberOfStock,
	 TotalRows =  COUNT(*) OVER(),
	 AvgConsumption=(select Case When '+cast(@DaysDiff as varchar)+'>0 Then COUNT(*)/'+cast(@DaysDiff as varchar)+' Else count(*) end from dbo.DailySalesTrackerWithInvoiceData where DistributorId=max(s.DistributorId) and LocCode='''+cast(@BranchCode as varchar)+''' and PartNum=max(s.PartNum) and CreatedDate between '''+cast(@AvgThreeMonthStartDate as varchar)+''' and '''+cast(@AvgThreeMonthEndDate as varchar)+''')	 
    from dbo.dailyStock s		
    inner join dbo.product p on p.PartNo=s.PartNum
	inner join dbo.productGroup g on g.GroupId=p.GroupId   
	where s.LocationCode='''+cast(@BranchCode as varchar)+'''';
	if(@SearchTxt <> '' and @SearchTxt IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' And s.PartNum like ''%'+cast(@SearchTxt as varchar(200))+'%''';
	   End
	  
    SET @sql=@sql+' group by s.PartNum ORDER BY';
	if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' '+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql=@sql+' TotalRows';
	   End
	   SET @sql=@sql+' OFFSET '+cast(@Skip as varchar)+' ROWS FETCH NEXT '+cast(@Take as varchar)+' ROWS ONLY) select *from search';
	   
	     PRINT @sql;
         EXEC (@sql) 
   End  
END;



