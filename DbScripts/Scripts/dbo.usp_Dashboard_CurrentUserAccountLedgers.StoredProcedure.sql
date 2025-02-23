USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CurrentUserAccountLedgers]    Script Date: 7/8/2020 4:10:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 02-03-2020
-- Last modified: NA
-- Description:	Display current user AccountLedgers List.
-- Usage: EXEC usp_Dashboard_CurrentUserAccountLedgers @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-02-25',@ConstantsClosingBalance='Closing Balance',@ConstantsOpeningBalance='Opening Balance'
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CurrentUserAccountLedgers]
    @Action INT,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
	@StartDate DATE,
    @EndDate DATE,
	@ConstantsClosingBalance NVARCHAR(100),
	@ConstantsOpeningBalance NVARCHAR(100)
AS
BEGIN
   
    IF (@Action = 1)
    BEGIN

	 -- Fetch account ledgers for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN

       select
	   WorkshopId,Debit,Credit 
	   from dbo.AccountLedger where Particulars !=@ConstantsClosingBalance
	   and Particulars != @ConstantsOpeningBalance and Date between @StartDate and @EndDate
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	Declare @DistributorId int=0
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

       select
	   WorkshopId,Debit,Credit 
	   from dbo.AccountLedger where Particulars !=@ConstantsClosingBalance
	   and Particulars != @ConstantsOpeningBalance and Date between @StartDate and @EndDate
	   and DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get account ledgers based on workshopId using UserId

        select
	    a.WorkshopId,a.Debit,a.Credit 
	    from dbo.AccountLedger a
		inner join dbo.WorkShops w on w.WorkShopId=a.WorkShopId
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId
        where a.Particulars !=@ConstantsClosingBalance and a.Particulars != @ConstantsOpeningBalance 
		and a.Date between @StartDate and @EndDate and do.UserId=@UserId
		
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get account ledgers based on workshopId using UserId

        select
	    a.WorkshopId,a.Debit,a.Credit 
	    from dbo.AccountLedger a
       inner join dbo.SalesExecutiveWorkshop sew on sew.WorkShopId=a.WorkShopId
       where a.Particulars !=@ConstantsClosingBalance and a.Particulars != @ConstantsOpeningBalance 
		and a.Date between @StartDate and @EndDate and sew.UserId=@UserId
	  
    END;
       
    END;

   
END;



