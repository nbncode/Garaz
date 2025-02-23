USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportRequestPartFilter]    Script Date: 7/8/2020 4:32:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====================================================================================================
-- Author: Lokesh Choudhary
-- Create date: 11-05-2020
-- Last modified: null
-- Description:	Import RequestPartFilter.
-- Usage: EXEC usp_ImportRequestPartFilter @Action=1, @DistributorId=1009, @CarMake='Maruti', @Model='Ciaz', @Year='2018,2019,2020', @Modification='',@PartGroup='LAMPS-HEADLAMP', @PartDescription='UNIT,HEADLAMP LH', @PartNumber='35321M74L01',@RootPartNumber='35321M74L00'
-- =====================================================================================================

ALTER PROCEDURE [dbo].[usp_ImportRequestPartFilter]
    @Action INT = NULL,
    @DistributorId INT = NULL,
    @CarMake NVARCHAR(500) = NULL,
    @Model NVARCHAR(500) = NULL,
    @Year NVARCHAR(200) = NULL,
    @Modification NVARCHAR(MAX) = NULL,
    @PartGroup NVARCHAR(500) = NULL,
    @PartDescription NVARCHAR(MAX) = NULL,
    @PartNumber NVARCHAR(200) = NULL,
    @RootPartNumber NVARCHAR(200) = NULL
AS
BEGIN
    DECLARE @id int = 0;

    IF (@Action = 1)
    BEGIN
        ---  Find duplicate record for update.
        SELECT @id = Id
        FROM RequestPartFilter WHERE DistributorId = @DistributorId and CarMake=@CarMake and Model=@Model and [Year]=@Year
		and Modification=@Modification and PartGroup=@PartGroup and PartDesc=@PartDescription and PartNum=@PartNumber 
		and RootPartNum=@RootPartNumber 

        IF (@id=0)
        BEGIN
		insert into RequestPartFilter (DistributorId,CarMake,Model,[Year],Modification,PartGroup,PartDesc,PartNum,RootPartNum)
		values (@DistributorId,@CarMake,@Model,@Year,@Modification,@PartGroup,@PartDescription,@PartNumber,@RootPartNumber )
        END;
		-- Now not need to update because duplicate record compare with all fields

		else
        BEGIN
		update RequestPartFilter set DistributorId = @DistributorId , CarMake=@CarMake , Model=@Model , [Year]=@Year
		, Modification=@Modification , PartGroup=@PartGroup , PartDesc=@PartDescription , PartNum=@PartNumber 
		, RootPartNum=@RootPartNumber where Id=@id
        END;
   END;

END;
