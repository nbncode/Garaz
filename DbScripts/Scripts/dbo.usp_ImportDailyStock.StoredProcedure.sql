USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportDailyStock]    Script Date: 11-11-2020 10:55:28 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_ImportDailyStock]
    @Action INT = NULL,
    @distributorId INT = NULL,
    @LocationCode NVARCHAR(500) = NULL,
    @PartNum NVARCHAR(500) = NULL,
    @partQty int = NULL,
	@RootPartNum NVARCHAR(200) = NULL
AS
BEGIN

    IF (@Action = 1)
    BEGIN
        DELETE FROM dbo.DailyStock
        WHERE DistributorId = @distributorId;
        UPDATE dbo.Product
        SET CurrentStock = 0
        WHERE DistributorId = @distributorId;
    END;
    IF (@Action = 2)
    BEGIN

	 update d 
     set d.StockQuantity=total.SumOfTotal
     from DailyStock d
      inner join (
    	select LocationCode,RootPartNum,sum(isnull(PartQty,0))SumOfTotal from DailyStock d
    	WHERE d.DistributorId = @distributorId
	    group by LocationCode,RootPartNum
        ) total
    on (d.LocationCode=total.LocationCode and d.RootPartNum=total.RootPartNum) 
    or (d.RootPartNum is null and total.RootPartNum is null and d.LocationCode=total.LocationCode )
    WHERE d.DistributorId = @distributorId

        UPDATE p
        SET p.CurrentStock =
            (
                SELECT SUM(   CASE
                                  WHEN ISNUMERIC(s.StockQuantity) = 1 THEN
                                      CONVERT(decimal, s.StockQuantity)
                                  ELSE
                                      0
                              END
                          )
                FROM dbo.DailyStock s
                WHERE s.PartNum = p.PartNo
                      AND s.DistributorId = p.DistributorId
                      AND s.DistributorId = @distributorId
            )
        FROM dbo.Product p
        WHERE p.DistributorId = @distributorId;

        UPDATE d
        SET d.outletId =
            (
			SELECT o.OutletId
                FROM dbo.outlets o		
				JOIN dbo.DistributorsOutlets do ON o.OutletId=do.OutletId		
                WHERE o.OutletCode = d.LocationCode
                      AND do.DistributorId = @distributorId                
            )
        FROM dbo.DailyStock d
        WHERE d.DistributorId = @distributorId;
    END;
    IF (@Action = 3)
    BEGIN
        INSERT INTO dbo.DailyStock
        (
            DistributorId,
            LocationCode,
            PartNum,
            StockQuantity,
			RootPartNum,
			PartQty
        )
        VALUES
        (@distributorId, @LocationCode, @PartNum, null,@RootPartNum,@partQty);
    END;
END;

