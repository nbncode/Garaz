
ALTER PROCEDURE [dbo].[Sp_DeleteAccountsData] 
 @DistributorId int=null         
AS            
BEGIN           
 if(@DistributorId IS NOT NULL)          
 begin          
      delete from AccountLedger where distributorId=@DistributorId

  update WorkShops set OutstandingAmount=0, TotalOutstanding=0 where workshopId 
  in(select WorkshopId from outstanding where distributorId=@DistributorId)

   delete from outstanding where distributorId=@DistributorId     
          
 end       
            
END