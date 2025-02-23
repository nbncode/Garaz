USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CurrentUserWorkshops]    Script Date: 9/23/2020 5:24:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 02-03-2020
-- Last modified: NA
-- Description:	Display current user Workshops List.
-- Usage: EXEC usp_Dashboard_TotalSaleDetail @Action=1, @Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA'
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CurrentUserWorkshops]
    @Action INT,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128)
AS
BEGIN
   
    IF (@Action = 1)
    BEGIN
	 -- Fetch workshops for role SuperAdmin
	 IF (@Role = 'SuperAdmin')
    BEGIN

       select * from dbo.WorkShops
	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	Declare @DistributorId int=0
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

       select w.*  from dbo.WorkShops w
       inner join dbo.DistributorWorkShops dw on dw.WorkshopId=w.WorkShopid
       where dw.DistributorId=@DistributorId
	   
    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	-- Get workshops based on workshopId using UserId

        select w.* from dbo.WorkShops w
        inner join dbo.DistributorsOutlets do on do.OutletId=w.OutletId
        where do.UserId=@UserId
		
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	-- Get workshops based on workshopId using UserId

       select w.* from dbo.WorkShops w
       inner join dbo.SalesExecutiveWorkshop sew on sew.WorkShopId=w.WorkShopId
       where sew.UserId=@UserId
	  
    END;
       
    END;

   
END;



