USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[Sp_AvailableStock]    Script Date: 16-3-2020 11:10:32 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE PROCEDURE [dbo].[Sp_AvailableStock]          
 @Role NVARCHAR(500)=NULL,          
 @UserId NVARCHAR(500)=NULL,        
 @Index INT=NULL,          
 @Number INT=10,        
 @PartNumber NVARCHAR(MAX)=NULL,          
 @OutletId INT=NULL,          
 @StockColorId INT=NULL        
AS          
BEGIN          
-- exec Sp_AvailableStock @UserId='ab4d9167-29bf-4178-a74e-37737116319c', @Role='Workshop',@Index=1,@StockColorId=null,@OutletId=1040      
 declare @From int          
 declare @To int          
         
        
 if(@Index!=-1)        
 begin        
  if(@Index=1)          
  begin          
   set @From= 1          
   SET @To = @From + @Number          
  END          
  ELSE           
  BEGIN          
   set @From= (@Number * (@Index-1))          
   set @To = @From + @Number          
   SET @From = @From + 1          
  END        
 END        
        
 declare @DistributorId int         
 declare @WorkShopId int          
         
 set @DistributorId=dbo.getDistributorId(@UserId, @Role)      
 set @WorkShopId=dbo.getWorkshopId(@UserId, @Role)         
        
 declare @sql nvarchar(max)          
 set @sql=';WITH search          
   AS  
   (select Num = ROW_NUMBER() OVER (order by a.CurrentStock desc),Total=Count(*) OVER(),a.*'
   
   if(@OutletId is not null)
   begin
	set @sql=@sql + ',d.StockQuantity'
   end

   set @sql=@sql + ' from Product a '        
   --(select Num = ROW_NUMBER() OVER (order by a.CreatedDate desc),Total=Count(*) OVER(),a.*          
   --from Product a '        
	     
 if(@OutletId is not null or @StockColorId is not null)          
 begin          
	set @sql=@sql + ' left outer join DailyStock d on a.PartNo=d.PartNum '
 end         
        
	set @sql=@sql + ' where 1=1'          
        
 /* Role wise data filter*/          
 if(@Role<>'SuperAdmin')          
 begin          
	set @sql=@sql + ' and a.DistributorId= '+cast(@DistributorId as nvarchar)       
        
  --IF(@WorkShopId IS NOT NULL)      
  --BEGIN      
  --SET @sql=@sql + ' and a.WorkShopId= '+CAST(@WorkShopId AS NVARCHAR)      
  --END      
 END         
      
      
        
 /* Part Number data filter*/          
 IF(@PartNumber IS NOT NULL AND @PartNumber <> '')          
 BEGIN          
	SET @sql=@sql + ' and a.PartNo like ''%'+@PartNumber+'%'''         
 END         
        
  /* OutletId data filter*/          
 IF(@OutletId IS NOT NULL)          
 BEGIN          
	SET @sql=@sql + ' and d.OutletId = '+CAST(@OutletId AS NVARCHAR)          
 END        
        
 /* StockColorId data filter*/          
 DECLARE @Qty INT        
 DECLARE @StockType NVARCHAR(500)       

 IF(@StockColorId IS NOT NULL And @OutletId is not null)          
 BEGIN          
         
	 SELECT @Qty=Qty,@StockType=StockType FROM StockColor        
	 WHERE StockId=@StockColorId        
        
	 IF(@StockType='Low')        
	 BEGIN        
		SET @sql=@sql + ' and CAST(ISNULL(d.StockQuantity,0) AS decimal) <= '+CAST(@Qty AS NVARCHAR)          
	 END
	 ELSE IF(@StockType='Medium')        
	 BEGIN        
		SET @sql=@sql + ' and d.StockQuantity is not null and d.StockQuantity !='''' and CAST(d.StockQuantity AS decimal) <= '+CAST(@Qty AS NVARCHAR)          
	 END         
	 ELSE        
	 BEGIN        
		  --Case of high        
		  SELECT @Qty=Qty,@StockType=StockType FROM StockColor        
		  WHERE StockType='Medium'        
        
		  SET @sql=@sql + ' and d.StockQuantity is not null and d.StockQuantity !='''' and CAST(d.StockQuantity AS decimal) >= '+CAST(@Qty AS NVARCHAR)          
	 END        
 END
 ELSE
 IF(@StockColorId IS NOT NULL)          
 BEGIN                
	 SELECT @Qty=Qty,@StockType=StockType FROM StockColor        
	 WHERE StockId=@StockColorId        
        
	 IF(@StockType='Low')        
	 BEGIN        
		SET @sql=@sql + ' and CAST(ISNULL(a.CurrentStock,0) AS decimal) <= '+CAST(@Qty AS NVARCHAR)          
	 END
	 ELSE IF(@StockType='Medium')        
	 BEGIN        
		SET @sql=@sql + ' and a.CurrentStock is not null and a.CurrentStock !='''' and CAST(a.CurrentStock AS decimal) <= '+CAST(@Qty AS NVARCHAR)          
	 END         
	 ELSE        
	 BEGIN        
	  --Case of high        
		  SELECT @Qty=Qty,@StockType=StockType FROM StockColor        
		  WHERE StockType='Medium'        
        
		  SET @sql=@sql + ' and a.CurrentStock is not null and a.CurrentStock !='''' and CAST(a.CurrentStock AS decimal) >= '+CAST(@Qty AS NVARCHAR)          
	 END        
 END   
           
 SET @sql=@sql + ')'          
 IF(@Index!=-1)        
 BEGIN          
	 SET @sql=@sql+' select top('+CAST(@Number AS NVARCHAR)+') *from search'         
	 SET @sql=@sql+' where Num between '+CAST(@From AS NVARCHAR)+' and '+CAST(@To AS NVARCHAR)          
 END        
 ELSE        
 BEGIN        
	SET @sql=@sql+' select *from search'         
 END        
 PRINT @sql            
 EXEC(@sql)  
        
          
END 
GO
