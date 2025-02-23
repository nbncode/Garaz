USE [Garaaz]
GO
/****** Object:  UserDefinedFunction [dbo].[getWorkshopId]    Script Date: 16-3-2020 11:13:43 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[getWorkshopId]
(
    @userID nvarchar(500),
	@role nvarchar(500)
)
RETURNS nvarchar(500)
AS
BEGIN
    Declare @logid nvarchar(500);

	if(@role='WorkshopUsers')
	begin
			select top 1 @logid=WorkshopId from WorkshopsUsers
			where Userid=@Userid
	end
	else if(@role='Workshop')
	begin
			select top 1 @logid=WorkshopId from DistributorWorkShops
			where Userid=@Userid
	end

    RETURN  @logid

END

GO
