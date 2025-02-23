USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportProduct]    Script Date: 4-11-2020 11:50:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_ImportProduct]
    @Action INT = NULL,
    @DistributorId INT = NULL,
    @PartNo NVARCHAR(50) = NULL,
    @RootPartNum NVARCHAR(50) = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @PackQuantity NVARCHAR(50) = NULL,
    @TaxValue NVARCHAR(50) = NULL,
    @Price DECIMAL(18, 4) = 0,
    @PartCategoryCode NVARCHAR(50) = NULL,
    @ProductType NVARCHAR(500) = NULL,
    @GroupName NVARCHAR(500) = NULL,
    @BrandName NVARCHAR(500) = NULL,
    @GroupId INT = NULL,
    @BrandId INT = NULL
AS
BEGIN

    -- Find GroupId based on GroupName. And create parent group if not found 
    IF (@Action = 1)
    BEGIN
        INSERT INTO ProductGroup
        (
            GroupName,
            CreatedDate,
            DistributorId
        )
        VALUES
        (@GroupName, GETDATE(), @DistributorId);
        SET @GroupId = SCOPE_IDENTITY();
        RETURN @GroupId;
    END;

    -- Find BrandId based on BrandName. And create brand if not found
    ELSE IF (@Action = 2)
    BEGIN
        INSERT INTO Brand
        (
            Name,
            CreatedDate
        )
        VALUES
        (@BrandName, GETDATE());
        SET @BrandId = SCOPE_IDENTITY();
        RETURN @BrandId;
    END;

    -- Check if product already exists
    ELSE IF (@Action = 3)
    BEGIN
        UPDATE dbo.Product
        SET RootPartNum = @RootPartNum,
            Description = @Description,
            GroupId = @GroupId,
            PackQuantity = @PackQuantity,
            TaxValue = @TaxValue,
            Price = @Price,
            BrandId = @BrandId,
            ProductType = @ProductType,
            PartCategoryCode = @PartCategoryCode
        WHERE PartNo = @PartNo
              AND DistributorId = @DistributorId;

        -- Insert the row if the UPDATE statement failed.  
        IF (@@ROWCOUNT = 0)
        BEGIN
            INSERT INTO dbo.Product
            (
                PartNo,
                RootPartNum,
                Description,
                PackQuantity,
                TaxValue,
                Price,
                ProductType,
                PartCategoryCode,
                GroupId,
                BrandId,
                DistributorId,
                CreatedDate
            )
            VALUES
            (@PartNo, @RootPartNum, @Description, @PackQuantity, @TaxValue, @Price, @ProductType, @PartCategoryCode,
             @GroupId, @BrandId, @DistributorId, GETDATE());
        END;
    END;
	--Last update on 14-09-2020 delete all product before insert new
	 ELSE IF (@Action = 4)
    BEGIN
        delete from dbo.Product where DistributorId=@DistributorId
    END;
END;

