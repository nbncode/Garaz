USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportCBOrders]    Script Date: 5-11-2020 4:42:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_ImportCBOrders]
    @Action INT = NULL,
    @distributorId INT = NULL,
    @OrderNumber VARCHAR(500) = NULL,
    @CODate DATETIME = NULL,
    @LocCode NVARCHAR(50) = NULL,
    @PartyCode NVARCHAR(50) = NULL,
    @PartyName NVARCHAR(500) = NULL,
    @PartNum NVARCHAR(100) = NULL,
    @PartDesc NVARCHAR(MAX) = NULL,
    @Order NVARCHAR(50) = NULL,
    @CBO NVARCHAR(50) = NULL,
    @StkMW NVARCHAR(50) = NULL,
    @ETA NVARCHAR(50) = NULL,
    @Inv NVARCHAR(500) = NULL,
    @Pick NVARCHAR(50) = NULL,
    @Alloc NVARCHAR(50) = NULL,
    @BO NVARCHAR(50) = NULL,
    @AO NVARCHAR(50) = NULL,
    @ActionStatus NVARCHAR(50) = NULL,
    @Remark NVARCHAR(50) = NULL,
    @PD NVARCHAR(50) = NULL,
	@PartStatus NVARCHAR(100)=NULL
AS
BEGIN
    IF (@Action = 1)
    BEGIN

        -- Update row if exists
        UPDATE dbo.CustomerBackOrder
        SET PartyName = @PartyName,           
            PartDesc = @PartDesc,
            CONo = @OrderNumber,
            [Order] = @Order,
            CBO = @CBO,
            StkMW = @StkMW,
            ETA = @ETA,
            Inv = @Inv,
            Pick = @Pick,
            Alloc = @Alloc,
            BO = @BO,
            AO = @AO,
            [Action] = @ActionStatus,
            Remark = @Remark,
            PD = @PD,
            DistributorId = @distributorId
        WHERE LocCode = @LocCode
              AND PartyCode = @PartyCode
              AND PartNum = @PartNum
              AND CoDate = @CoDate;

        -- Insert the row if the UPDATE statement failed.  
        IF (@@ROWCOUNT = 0)
        BEGIN
            INSERT INTO dbo.CustomerBackOrder
            (
                CODate,
                LocCode,
                PartyCode,
                PartyName,
                PartNum,
                PartDesc,
                [Order],
                CBO,
                StkMW,
                ETA,
                Inv,
                Pick,
                Alloc,
                BO,
                AO,
                Action,
                Remark,
                PD,
                DistributorId,
                CONo,
				PartStatus
            )
            VALUES
            (@CODate, @LocCode, @PartyCode, @PartyName, @PartNum, @PartDesc, @Order, @CBO, @StkMW, @ETA, @Inv, @Pick,
             @Alloc, @BO, @AO, @ActionStatus, @Remark, @PD, @distributorId, @OrderNumber,@PartStatus);
        END;
    END;

    ELSE IF (@Action = 2)
    BEGIN
        UPDATE c
        SET c.WorkshopId = (CASE
                                WHEN ws.WorkshopId IS NOT NULL THEN
                                    ws.WorkshopId
                                ELSE
                                    dw.WorkShopId
                            END
                           )
        FROM dbo.CustomerBackOrder c
            CROSS APPLY
        (
            SELECT TOP 1
                u.*
            FROM dbo.UserDetails u
			inner join dbo.DistributorWorkShops w on u.UserId=w.UserId
            WHERE u.ConsPartyCode = c.PartyCode and w.DistributorId=c.DistributorId
        ) u
            LEFT OUTER JOIN dbo.WorkshopsUsers ws
                ON ws.UserId = u.UserId
            LEFT OUTER JOIN dbo.DistributorWorkShops dw
                ON dw.UserId = u.UserId
        WHERE c.DistributorId = @distributorId;
    END;

END;

