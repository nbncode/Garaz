
-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 27-03-2020
-- Last modified: NA
-- Description:	Display customer outstanding Details Filter by SalesExecutivesWise and Group by Customer Type and Group by SE.
-- Usage: EXEC usp_Dashboard_CsWiseOutstandingDetails @Action=6,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType=Null,@BranchCode=Null,@Skip=0,@Take=10
-- Usage: EXEC usp_Dashboard_CsWiseOutstandingDetails @Action=7,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='Independent Workshop',@BranchCode=Null,@Skip=0,@Take=10
-- ===================================================================================
CREATE PROCEDURE [dbo].[usp_Dashboard_CsWiseOutstandingDetails]
    @Action int,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(32),
    @StartDate DATE,
    @EndDate DATE,
	@BranchCode nvarchar(50),
	@CustomerType nvarchar(200),
	@Skip INT,
    @Take INT
	
AS
BEGIN

	-- Declare Tables for current user workshops
	DECLARE @UserWorkshop TABLE (WorkshopId int)
	
	  -- Set CustomerType If Null Then All Type Customer
	IF(@CustomerType='' or @CustomerType Is Null)
	Begin
	Set @CustomerType ='MASS'+','+'TRADER/RETAILER'+','+'INDEPENDENT WORKSHOP'+','+'WALK-IN CUSTOMER' +','+'DISTRIBUTOR'+','+'CO-DEALER'
	End

	  IF (@Role = 'SuperAdmin')
    BEGIN
	   -- Set all workshopId in temp table @UserWorkshop
	insert into @UserWorkshop (WorkshopId)select WorkshopId from dbo.Workshops 
	where Type in(select * from dbo.Split(@CustomerType,','))
	   	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	declare @DistributorId int;

	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	   -- Set distributor workshopId in temp table @UserWorkshop
	insert into @UserWorkshop (WorkshopId)select WorkshopId from dbo.DistributorWorkShops 
     where DistributorId=@DistributorId

    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	 -- Set RoIncharge workshopId in temp table @UserWorkshop

	  insert into @UserWorkshop (WorkshopId)select w.WorkshopId from dbo.WorkShops w
        inner join dbo.DistributorsOutlets do on w.OutletId=do.OutletId
		where w.Type in(select * from dbo.Split(@CustomerType,',')) and do.Userid=@UserId
	  
  
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   -- Set SalesExecutive workshopId in temp table @UserWorkshop
	   insert into @UserWorkshop (WorkshopId)select w.WorkshopId  from dbo.WorkShops w
	   inner join dbo.SalesExecutiveWorkshop sew on w.WorkshopId=sew.WorkshopId
       where w.Type in(select * from dbo.Split(@CustomerType,',')) and UserId=@UserId

    END;

	if(@Action=6)
	BEGIN
	   -- Group BY Customer Type Wise Outstanding

	  select 
	 cast(ROW_NUMBER() OVER(ORDER BY o.CustomerType) as int)As SlNo,
	   max(o.CustomerType)as CustomerType,count(o.CustomerType) as TotalCustomer,sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(w.CreditLimit)as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then 0 else o.LessThan7Days end as decimal)+
       cast(case when TRIM(o.[7To14Days]) IS NULL then 0 else o.[7To14Days] end as decimal)))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then 0 else o.[14To21Days] end as decimal)+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then 0 else o.[21To28Days] end as decimal)))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then 0 else o.[28To35Days] end as decimal)+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then 0 else o.[35To50Days] end as decimal)))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then 0 else o.[50To70Days] end as decimal))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then 0 else o.MoreThan70Days end as decimal))as MoreThan70Days

	   from Outstanding o 
	   inner join workshops w on o.workshopid=w.workshopid
	   inner join @UserWorkshop uw on w.workshopid=uw.workshopid
	   where o.CreatedDate between @StartDate and @EndDate and  o.CustomerType in(select * from dbo.Split(@CustomerType,','))
	   group by o.CustomerType

	END

	else if(@Action=7)
	BEGIN
	   -- Group BY customer Wise Outstanding

	  select 
	  cast(ROW_NUMBER() OVER(ORDER BY o.PartyCode) as int)As SlNo,
	   (select OutletCode from dbo.Outlets where outletId=max(w.outletId))as BranchCode,
	   (select ConsPartyCode from dbo.UserDetails where ConsPartyCode=max(o.PartyCode))as CustomerCode,
	   (select FirstName+' '+ LastName from dbo.UserDetails where ConsPartyCode=max(o.PartyCode))as CustomerName,
	    max(o.CustomerType)as CustomerType,
	   sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(w.CreditLimit)as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then 0 else o.LessThan7Days end as decimal)+
       cast(case when TRIM(o.[7To14Days]) IS NULL then 0 else o.[7To14Days] end as decimal)))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then 0 else o.[14To21Days] end as decimal)+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then 0 else o.[21To28Days] end as decimal)))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then 0 else o.[28To35Days] end as decimal)+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then 0 else o.[35To50Days] end as decimal)))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then 0 else o.[50To70Days] end as decimal))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then 0 else o.MoreThan70Days end as decimal))as MoreThan70Days
	   	  
	   from dbo.Outstanding o 
	   inner join dbo.workshops w on o.workshopid=w.workshopid
	   inner join @UserWorkshop uw on w.workshopid=uw.workshopid
	   where o.CreatedDate between @StartDate and @EndDate and  o.CustomerType in(select * from dbo.Split(@CustomerType,','))
	   group by o.PartyCode order by SlNo OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END
	
END


