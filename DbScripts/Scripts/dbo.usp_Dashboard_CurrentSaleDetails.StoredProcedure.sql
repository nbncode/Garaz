USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CurrentSaleDetails]    Script Date: 7/8/2020 4:09:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 28-03-2020
-- Last modified: NA
-- Description:	Display current user current sale details.
-- Usage: EXEC usp_Dashboard_CurrentSaleDetails @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA',@StartDate='2020-03-01',@EndDate='2020-03-28',@BranchCode=Null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CurrentSaleDetails]
    @Action INT,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
	@StartDate date,
	@EndDate date,
	@BranchCode nvarchar(50)
AS
BEGIN
   
     -- Declare Tables for current user workshops
	DECLARE @Workshops TABLE (WorkshopId int)
   
	 -- Fetch workshops for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN
	 -- Set workshops in temp table @Workshops
	insert into @Workshops (WorkshopId)select WorkshopId from dbo.WorkShops 
     	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	Declare @DistributorId int=0
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

       insert into @Workshops (WorkshopId)select WorkshopId  from dbo.DistributorWorkShops 
       where DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get workshops based on workshopId using UserId

        insert into @Workshops (WorkshopId)select w.WorkshopId from dbo.WorkShops w
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId
        where do.UserId=@UserId
		
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get workshops based on workshopId using UserId

       insert into @Workshops (WorkshopId)select WorkshopId from dbo.SalesExecutiveWorkshop
       where UserId=@UserId
	  
    END;

	If(@Action=1)
	Begin

	select d.* from dbo.DailySalesTrackerWithInvoiceData d
				inner join @Workshops w on d.WorkshopId=w.WorkshopId
				where d.CreatedDate between @StartDate and @EndDate
				and d.LocCode=Case when @BranchCode Is Not Null Then @BranchCode Else d.LocCode End
	End
   
END;



