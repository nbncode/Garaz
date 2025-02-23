USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CurrentUserStocksPrice]    Script Date: 11-11-2020 12:10:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 11-11-2020 replace StockQuantity to PartQty column to calculate stock price
-- Last modified: NA
-- Description:	Display current user Stocks price.
-- Usage: EXEC usp_Dashboard_CurrentUserStocksPrice @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA'
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CurrentUserStocksPrice]
    @Action INT,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128)
AS
BEGIN
   
    IF (@Action = 1)
    BEGIN

	 -- Fetch daily stocks for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN

       select
	  sum(isnull(o.PartQty,0)*isnull(p.Price,0))as StockPrice
	   from dbo.DailyStock o
	   inner join dbo.Product p on  p.PartNo=o.PartNum and p.DistributorId=o.DistributorId and p.RootPartNum=o.RootPartNum
	    where o.OutletId Is Not Null
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	Declare @DistributorId int=0
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

      select
	  sum(isnull(o.PartQty,0)*isnull(p.Price,0))as StockPrice
	   from dbo.DailyStock o
	   inner join dbo.Product p on  p.PartNo=o.PartNum and p.DistributorId=o.DistributorId and p.RootPartNum=o.RootPartNum	   
	   where o.OutletId Is Not Null and o.DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get daily stocks based on outletId using UserId

        select
	  sum(isnull(o.PartQty,0)*isnull(p.Price,0))as StockPrice
	   from dbo.DailyStock o
	   inner join dbo.Product p on  p.PartNo=o.PartNum and p.DistributorId=o.DistributorId and p.RootPartNum=o.RootPartNum
        inner join dbo.DistributorsOutlets do on do.OutletId=o.OutletId
        where o.OutletId Is Not Null and do.UserId=@UserId
		
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get daily stocks based on outletId using UserId

        select
	  sum(isnull(o.PartQty,0)*isnull(p.Price,0))as StockPrice
	   from dbo.DailyStock o
	   inner join dbo.Product p on  p.PartNo=o.PartNum and p.DistributorId=o.DistributorId and p.RootPartNum=o.RootPartNum
		inner join dbo.OutletsUsers u on u.OutletId=o.OutletId
        inner join dbo.RoSalesExecutive rs on rs.RoUserId=u.UserId
       where o.OutletId Is Not Null and rs.SeUserId=@UserId
	  
    END;
       
    END;

   
END;



