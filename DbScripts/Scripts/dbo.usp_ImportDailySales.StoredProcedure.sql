USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportDailySales]    Script Date: 5-11-2020 3:29:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_ImportDailySales]
    @Action INT = NULL,
    @DistributorId INT = NULL,
    @LocCode NVARCHAR(50) = NULL,
    @LocDesc NVARCHAR(MAX) = NULL,
    @PartNum NVARCHAR(500) = NULL,
    @PartDesc NVARCHAR(MAX) = NULL,
    @RootPartNum NVARCHAR(MAX) = NULL,
    @PartCategory NVARCHAR(MAX) = NULL,
    @Day NVARCHAR(50) = NULL,
    @PartGroup NVARCHAR(500) = NULL,
    @CalMonthYear NVARCHAR(500) = NULL,
    @ConsPartyCode NVARCHAR(50) = NULL,
    @ConsPartyName NVARCHAR(500) = NULL,
    @ConsPartyTypeDesc NVARCHAR(MAX) = NULL,
    @DocumentNum NVARCHAR(50) = NULL,
    @Remarks NVARCHAR(500) = NULL,
    @RetailQty Decimal(18,4) = 0,
    @ReturnQty Decimal(18,4) = 0,
    @NetRetailQty Decimal(18,4) = 0,
    @RetailSelling Decimal(18,4) = 0,
    @ReturnSelling Decimal(18,4) = 0,
    @NetRetailSelling Decimal(18,4) = 0,
    @DiscountAmount Decimal(18,4) = 0,
    @CoNo NVARCHAR(500) = NULL,
    @CreatedDate DATETIME = NULL,
	@ResponseCode INT OUTPUT
AS
BEGIN

    SET NOCOUNT ON;

    IF (@Action = 1)
    BEGIN

        SET NOCOUNT ON;

        DECLARE @UserId NVARCHAR(128) = NULL,
                @WorkShopId INT = NULL,
                @GroupId INT = NULL,
                @ProductId INT = NULL;

        -- Show error if row exists.

		If EXISTS (SELECT distributorId FROM dbo.DailySalesTrackerWithInvoiceData WHERE LocCode = @LocCode AND PartNum = @PartNum AND ConsPartyCode=@ConsPartyCode AND CreatedDate=@CreatedDate AND DocumentNum=@DocumentNum AND DistributorId = @DistributorId)
       Begin
        set @ResponseCode=0
       End
	    -- Insert sale row if not exists.
   else
      Begin
       INSERT INTO DailySalesTrackerWithInvoiceData
            (
                LocCode,
                LocDesc,
                PartNum,
                PartDesc,
                RootPartNum,
                PartCategory,
                [Day],
                PartGroup,
                CalMonthYear,
                ConsPartyCode,
                ConsPartyName,
                ConsPartyTypeDesc,
                DocumentNum,
                Remarks,
                RetailQty,
                ReturnQty,
                NetRetailQty,
                RetailSelling,
                ReturnSelling,
                NetRetailSelling,
                DiscountAmount,
                CoNo,
                DistributorId,
                UserId,
                WorkShopId,
                GroupId,
                ProductId,
                CreatedDate
            )
            VALUES
            (@LocCode, @LocDesc, @PartNum, @PartDesc, @RootPartNum, @PartCategory, @Day, @PartGroup, @CalMonthYear,
             @ConsPartyCode, @ConsPartyName, @ConsPartyTypeDesc, @DocumentNum, @Remarks, @RetailQty, @ReturnQty,
             @NetRetailQty, @RetailSelling, @ReturnSelling, @NetRetailSelling, @DiscountAmount, @CoNo, @DistributorId,
             @UserId, @WorkShopId, @GroupId, @ProductId, @CreatedDate);

			 set @ResponseCode=1
      End

	  -- --- comment update on 03/07/2020 as per called Raj sir
   --     UPDATE DailySalesTrackerWithInvoiceData
   --     SET LocCode = @LocCode,
   --         LocDesc = @LocDesc,
   --         PartDesc = @PartDesc,
   --         RootPartNum = @RootPartNum,
   --         PartCategory = @PartCategory,
   --         PartGroup = @PartGroup,
   --         ConsPartyName = @ConsPartyName,
   --         ConsPartyTypeDesc = @ConsPartyTypeDesc,
   --         DocumentNum = @DocumentNum,
   --         Remarks = @Remarks,
   --         RetailQty = @RetailQty,
   --         ReturnQty = @ReturnQty,
   --         NetRetailQty = @NetRetailQty,
   --         RetailSelling = @RetailSelling,
   --         NetRetailSelling = @NetRetailSelling,
   --         DiscountAmount = @DiscountAmount,
   --         CoNo = @DocumentNum,
   --         DistributorId = @DistributorId,
   --         UserId = @UserId,
   --         WorkShopId = @WorkShopId,
   --         GroupId = @GroupId,
   --         ProductId = @ProductId,
   --         CreatedDate = @CreatedDate,
			--Day = @Day,
		 --   ConsPartyCode = @ConsPartyCode,
			--CalMonthYear = @CalMonthYear
   --     WHERE DocumentNum = @DocumentNum
   --           AND PartNum = @PartNum

			--  --- change update on 06/06/2020 as per client requirement
			-- -- AND LocCode = @LocCode
   --          -- AND Day = @Day
   --         --  AND CalMonthYear = @CalMonthYear
			-- -- AND ConsPartyCode = @ConsPartyCode
   --           AND DistributorId = @DistributorId;

   --     -- Insert the row if the UPDATE statement failed.  
   --     IF (@@ROWCOUNT = 0)
   --     BEGIN
   --      Add insert query here
   --     END;
    END;

    IF (@Action = 2)
    BEGIN
        -- Now update ProductId, GroupId, UserId, WorkshopId in DailySales
        UPDATE d
        SET d.ProductId = p.ProductId,
            d.GroupId = p.GroupId
        FROM DailySalesTrackerWithInvoiceData d
            INNER JOIN Product p
                ON d.PartNum = p.PartNo
                   AND p.DistributorId = d.DistributorId
        WHERE d.DistributorId = @DistributorId;

		UPDATE d
		SET d.UserId = w.UserId,
		    d.WorkShopId=w.WorkShopId
        FROM DailySalesTrackerWithInvoiceData d
		 OUTER APPLY (select top 1 u.UserId,dw.WorkShopId from UserDetails u
		   inner join DistributorWorkShops dw  ON u.UserId = dw.UserId 
		   where u.ConsPartyCode=d.ConsPartyCode and dw.DistributorId=d.DistributorId) as w
        WHERE d.DistributorId = @DistributorId



        --UPDATE d
        --SET d.UserId = u.UserId
        --FROM DailySalesTrackerWithInvoiceData d
        --    INNER JOIN UserDetails u
        --        ON d.ConsPartyCode = u.ConsPartyCode
        --WHERE d.DistributorId = @DistributorId;

        --UPDATE d
        --SET d.WorkShopId = (CASE
        --                        WHEN ws.WorkshopId IS NOT NULL THEN
        --                            ws.WorkshopId
        --                        ELSE
        --                            dw.WorkShopId
        --                    END
        --                   )
        --FROM DailySalesTrackerWithInvoiceData d
        --    LEFT OUTER JOIN WorkshopsUsers ws
        --        ON d.UserId = ws.UserId
        --    LEFT OUTER JOIN DistributorWorkShops dw
        --        ON d.UserId = dw.UserId
        --WHERE d.DistributorId = @DistributorId;


		 UPDATE d
        SET d.SalesUserId =sw.UserId
        FROM dbo.DailySalesTrackerWithInvoiceData d
            INNER JOIN dbo.SalesExecutiveWorkshop sw
                ON d.WorkShopId = sw.WorkshopId
        WHERE d.SalesUserId Is NULL and d.DistributorId = @DistributorId;


		 set @ResponseCode=1
    END;
END;