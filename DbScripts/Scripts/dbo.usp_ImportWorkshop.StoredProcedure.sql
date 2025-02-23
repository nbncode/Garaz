USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_ImportWorkshop]    Script Date: 16-3-2020 11:10:32 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 CREATE PROCEDURE [dbo].[usp_ImportWorkshop]  
 @Action INT=NULL,         
 @distributorId INT=NULL,
 @SNO nvarchar(50)=NULL,
 @CustomerName nvarchar(500)=NULL,
 @CustomerType nvarchar(500)=NULL, 
 @Address1 nvarchar(max)=NULL,
 @Address2 nvarchar(max)=NULL,  
 @Address3 nvarchar(max)=NULL,
 @City nvarchar(500)=NULL,  
 @State nvarchar(500)=NULL,
 @Country nvarchar(500)=NULL,  
 @PinCode nvarchar(50)=NULL,
 @PartyOwner1 nvarchar(500)=NULL,
 @MobilePhone nvarchar(50)=NULL,  
 @AlternateNumber nvarchar(50)=NULL,
 @PartyOwner2 nvarchar(500)=NULL,  
 @Email nvarchar(50)=NULL,
 @PanNo nvarchar(50)=NULL,  
 @GstinNo nvarchar(50)=NULL,  
 @PaymentType nvarchar(50)=NULL,
 @DiscountPer nvarchar(50)=NULL,  
 @CreditDays nvarchar(50)=NULL,
 @BranchCode nvarchar(50)=NULL, 
 @BranchName nvarchar(500)=NULL,  
 @SalesExecName nvarchar(500)=NULL,  
 @SalesExecNumber nvarchar(50)=NULL,
 @CustomerCode nvarchar(50)=NULL

AS     
BEGIN     
        
 if(@Action=1)        
 begin        
       if exists (select workshopId from dbo.WorkShopData where CustomerCode=@CustomerCode)
       BEGIN
       update dbo.WorkShopData set
	    SNO = @SNO,CustomerName = @CustomerName,CustomerType = @CustomerType,Address1 = @Address1,
        Address2 = @Address2,Address3 = @Address3,City = @City,State = @State,Country = @Country,
        PinCode = @PinCode,PartyOwner1 = @PartyOwner1,MobilePhone = @MobilePhone,AlternateNumber = @AlternateNumber,
        PartyOwner2 = @PartyOwner2,Email = @Email,PanNo = @PanNo,GstinNo = @GstinNo,PaymentType = @PaymentType,
        DiscountPer = @DiscountPer,CreditDays = @CreditDays,BranchCode = @BranchCode,BranchName = @BranchName,
        SalesExecName = @SalesExecName,SalesExecNumber = @SalesExecNumber,CustomerCode = @CustomerCode                         
        where CustomerCode=@CustomerCode
       END

       ELSE  
       begin 
      insert into dbo.WorkShopData(SNO,CustomerName,CustomerType,Address1,Address2,Address3,City,State,
	  Country,PinCode,PartyOwner1,MobilePhone,AlternateNumber,PartyOwner2,Email,PanNo,GstinNo,
	  PaymentType,DiscountPer,CreditDays,BranchCode,BranchName,SalesExecName,SalesExecNumber,
	  CustomerCode)values
	  (@SNO,@CustomerName,@CustomerType,@Address1,@Address2,@Address3,@City,@State,
	  @Country,@PinCode,@PartyOwner1,@MobilePhone,@AlternateNumber,@PartyOwner2,@Email,@PanNo
	  ,@GstinNo,@PaymentType,@DiscountPer,@CreditDays,@BranchCode,@BranchName,@SalesExecName,
	  @SalesExecNumber,@CustomerCode)
       end

  end          
     
 END

  

GO
