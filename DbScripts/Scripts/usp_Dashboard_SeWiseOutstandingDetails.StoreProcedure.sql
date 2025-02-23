USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_SeWiseOutstandingDetails]    Script Date: 27-03-2020 03:56:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 27-03-2020
-- Last modified: NA
-- Description:	Display customer outstanding Details Filter by SalesExecutivesWise and Group by Customer Type and Group by SE.
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=3,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType=Null,@BranchCode=Null,@Skip=0,@Take=10
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=4,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='Independent Workshop',@BranchCode=Null,@Skip=0,@Take=10
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=5,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='INDEPENDENT WORKSHOP',@BranchCode='TRS',@Skip=0,@Take=10
-- ===================================================================================
CREATE PROCEDURE [dbo].[usp_Dashboard_SeWiseOutstandingDetails]
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

	-- Declare Tables for current user Seworkshops
	DECLARE @SeWorkshop TABLE (WorkshopId int)
	
	  
	  IF (@Role = 'SuperAdmin')
    BEGIN
	   -- Set SeWise workshopId in temp table @SeWorkshop
	insert into @SeWorkshop (WorkshopId)select WorkshopId from dbo.SalesExecutiveWorkshop 
	   	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	declare @DistributorId int;

	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	   -- Set SeWise workshopId in temp table @SeWorkshop
	insert into @SeWorkshop (WorkshopId)select w.WorkshopId from dbo.SalesExecutiveWorkshop w
	 inner join dbo.DistributorWorkShops dw on dw.WorkshopId=w.WorkShopid
     where dw.DistributorId=@DistributorId

    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	 -- Set SeWise workshopId in temp table @SeWorkshop

	  insert into @SeWorkshop (WorkshopId)select w.WorkshopId from dbo.SalesExecutiveWorkshop w
	  inner join dbo.RoSalesExecutive e on w.UserId=e.SeUserId where e.RoUserId=@UserId
  
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   -- Set SeWise workshopId in temp table @SeWorkshop
	   insert into @SeWorkshop (WorkshopId)select WorkshopId  from dbo.SalesExecutiveWorkshop 
       where UserId=@UserId

    END;

	if(@Action=3)
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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate
	   group by o.CustomerType

	END

	else if(@Action=4)
	BEGIN
	   -- Group BY Outlet code Wise Outstanding

	   select 
	  cast(ROW_NUMBER() OVER(ORDER BY w.outletId) as int)As SlNo,
	  
	   (select OutletCode from dbo.Outlets where outletId=max(w.outletId))as BranchCode,
	   (select u.FirstName+' '+u.LastName from dbo.UserDetails u
         inner join dbo.SalesExecutiveWorkshop se on u.UserId=se.UserId
         where se.WorkshopId=max(w.WorkshopId))as SalesExName,
	   count(w.OutletId) as TotalCustomer,
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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate and o.CustomerType=@CustomerType
	   group by w.outletId order by SlNo OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END
	else if(@Action=5)
	BEGIN
	   -- Group BY customer Wise Outstanding

	  select 
	  cast(ROW_NUMBER() OVER(ORDER BY o.PartyCode) as int)As SlNo,
	  
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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate and o.CustomerType=@CustomerType and w.outletId=(select top 1 OutletId from dbo.Outlets where OutletCode=@BranchCode)
	   group by o.PartyCode order by SlNo OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END
	
END


