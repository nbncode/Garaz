USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[sp_wallet]    Script Date: 19-10-2020 3:11:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[sp_wallet](        
@QueryType nvarchar(max)='',        
@WorkshopId int =null,        
@UserId nvarchar(128)=null,        
@WalletAmount decimal(18,2)=null  ,      
@Index int =null,      
@NextFetchRow bigint =10,      
@CouponNumber nvarchar(100)=null      ,
@DistributorId int =null
)        
As        
 --exec sp_wallet @QueryType='WalletDetail', @Index=-1, @CouponNumber=''  
Begin        
if(@QueryType='WalletDetail')        
Begin      
 select @NextFetchRow= (case when @Index < 0 then 999999999999 else @NextFetchRow end)   
 select @Index = (case when @Index is null or @Index < 0 then 0 else @Index end)      
 declare @Skip int set @Skip  =10 * @Index      
  
  select  w.WorkShopId,max(w.workshopName)as WorkshopName, max(d.UserId)as UserId, max(isnull(ud.WalletAmount,0)) as WalletAmt,
    count(wc.WorkshopId)as TotalCoupons
  from workshops w        
  join DistributorWorkShops d on d.WorkShopId = w.WorkShopId        
  join userdetails ud on ud.UserId = d.UserId        
  join AspNetUserRoles au on au.userId= ud.userId        
  left outer join workshopcoupon wc on wc.WorkshopId = d.WorkShopId    
  where au.roleId=5 and isnull(ud.IsDeleted,0)=0        
  and isnull(wc.CouponNo,'') = (case when Isnull(@CouponNumber,'') ='' then  isnull(wc.CouponNo,'') else @CouponNumber end)    
  and d.distributorId= (case when (@DistributorId is null or @DistributorId =0)  then d.distributorId else @DistributorId end )
   group by w.WorkShopId order by max(WorkShopName)    
  OFFSET(@Skip) ROWS FETCH NEXT @NextFetchRow ROWS ONLY;      
End      
End        