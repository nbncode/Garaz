USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportOutlets]    Script Date: 7/8/2020 4:31:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_ImportOutlets]
    @distributorId INT = NULL,
    @OutletCode NVARCHAR(500),
    @OutletName NVARCHAR(500),
    @Address NVARCHAR(500) = NULL,
	@ResponseCode INT OUTPUT
AS
BEGIN

    DECLARE @OutletId INT = NULL;
	DECLARE @OutletDistrId INT = NULL;

    -- Check if any distributor for the passed outlet code already exists
	-- so that we will not allow creating same outlet with same code for other distributor
	SELECT @OutletDistrId = d.DistributorId, @OutletId=o.OutletId FROM dbo.Outlets o
	INNER JOIN dbo.DistributorsOutlets d ON o.OutletId=d.OutletId
	where o.OutletCode=@OutletCode

	--- Show error Outlet code matches for other distributor
	IF (@OutletDistrId IS NOT NULL AND @OutletDistrId<>@distributorId)
	BEGIN
	set @ResponseCode=0
	END
	-- Outlet code matches for same distributor, then update
	IF (@OutletDistrId IS NOT NULL AND @OutletDistrId=@distributorId)
	BEGIN
	 UPDATE dbo.Outlets
        SET OutletName = @OutletName,
            Address = @Address
        WHERE OutletId = @OutletId;
		set @ResponseCode=1
	END
	ELSE
	BEGIN
	-- If no distributor exists for the outlet code, then we can create outlet
	IF (@OutletDistrId IS NULL)
	BEGIN
	INSERT INTO dbo.Outlets
        (
            OutletCode,
            OutletName,
            Address
        )
        VALUES
        (@OutletCode, @OutletName, @Address);
        SET @OutletId = SCOPE_IDENTITY();

		--- Create relation with distributor
		 INSERT INTO dbo.DistributorsOutlets
        (
            DistributorId,
            OutletId
        )
        VALUES
        (@distributorId, @OutletId);
		set @ResponseCode=1
	END

	
	End
	END	  
