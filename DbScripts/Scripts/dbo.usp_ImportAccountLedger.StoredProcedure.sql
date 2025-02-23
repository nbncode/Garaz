USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportAccountLedger]    Script Date: 10-11-2020 11:51:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[usp_ImportAccountLedger]
    @Action INT = NULL,
    @distributorId INT = NULL,
    @Location NVARCHAR(200) = NULL,
    @ConsPartyCode NVARCHAR(50) = NULL,
    @PartyName NVARCHAR(500) = NULL,
    @Date DATETIME = NULL,
    @Particulars NVARCHAR(MAX) = NULL,
    @VchType NVARCHAR(50) = NULL,
    @VchNo NVARCHAR(50) = NULL,
    @Debit DECIMAL(18, 4) = NULL,
    @Credit DECIMAL(18, 4) = NULL,
	@RunningBalance DECIMAL(18, 4) = NULL,
    @DueDays int = NULL
AS
BEGIN
    DECLARE @IsClosing BIT = 0,
            @WorkShopId INT = NULL,
            @AccountLedgerId INT = NULL,
            @OldParticulars NVARCHAR(MAX) = NULL;

    IF (@Action = 1)
    BEGIN
        ---  Find WorkshopId using ConsPartyCode.Process out if WorkshopId Not found.

		
  select @WorkShopId =(CASE WHEN ws.WorkshopId IS NOT NULL THEN ws.WorkshopId ELSE dw.WorkshopId END)
	 from UserDetails u
  Left outer join WorkshopsUsers ws ON u.UserId = ws.UserId and ws.WorkshopId 
  in(select WorkshopId from DistributorWorkShops where DistributorId=@distributorId ) 

  Left outer join DistributorWorkShops dw ON u.UserId = dw.UserId and dw.DistributorId=@distributorId
  where u.ConsPartyCode=@ConsPartyCode;
       

        IF (@WorkShopId IS NOT NULL)
        BEGIN
		---- Set location using workshopId for Workshops Table
		if(@Location Is Null)
		Begin
		SELECT @Location = o.OutletCode from dbo.workshops w
         inner join dbo.outlets o on w.outletId=o.outletId
		 where w.WorkShopId=@WorkShopId
		End
            ---  Set IsClosing using Particulars equal Closing Balance.
            IF (@Particulars = 'Closing Balance')
            BEGIN
                SET @IsClosing = 1;
            END;

            --- Check record already exist.Update when exist other then insert.

			-- Changed on March 5, 2020
			--IF (@Particulars = 'Opening Balance' OR @Particulars = 'Closing Balance')

            --IF (@Particulars = 'Closing Balance')
			IF (@Particulars = 'Opening Balance' OR @Particulars = 'Closing Balance')
            BEGIN
                SELECT TOP 1
                    @AccountLedgerId = AccountLeaderId,
                    @OldParticulars = Particulars
                FROM AccountLedger
                WHERE WorkshopId = @WorkShopId
                      AND Particulars = @Particulars;
            END;
            ELSE
            BEGIN
                SELECT TOP 1
                    @AccountLedgerId = AccountLeaderId,
                    @OldParticulars = Particulars
                FROM AccountLedger
                WHERE DistributorId = @distributorId
                      AND VchNo = @VchNo
					  And Location=@Location;
            END;

            IF (@AccountLedgerId IS NOT NULL AND @OldParticulars IS NOT NULL)
            BEGIN
			IF (@Particulars != 'Opening Balance')
			BEGIN
                UPDATE AccountLedger
                SET Location = @Location,
                    PartyName = @PartyName,
                    Code = @ConsPartyCode,
                    Date = @Date,
                    Particulars = @Particulars,
                    VchType = @VchType,
                    VchNo = @VchNo,
                    Debit = @Debit,
                    Credit = @Credit,
                    DistributorId = @distributorId,
                    IsClosing = @IsClosing,
					WorkshopId=@WorkShopId,
					RunningBalance=@RunningBalance,	
					DueDays=@DueDays
                WHERE AccountLeaderId = @AccountLedgerId;
				END
            END;

            ELSE
            BEGIN			
                INSERT INTO AccountLedger
                (
                    Location,
                    PartyName,
                    Code,
                    Date,
                    Particulars,
                    VchType,
                    VchNo,
                    Debit,
                    Credit,
                    DistributorId,
                    IsClosing,
                    WorkshopId,
					RunningBalance,	
					DueDays
                )
                VALUES
                (@Location, @PartyName, @ConsPartyCode, @Date, @Particulars, @VchType, @VchNo, @Debit, @Credit,
                 @distributorId, @IsClosing, @WorkShopId,@RunningBalance,@DueDays);				 
            END;
        END;

    END;


END;
