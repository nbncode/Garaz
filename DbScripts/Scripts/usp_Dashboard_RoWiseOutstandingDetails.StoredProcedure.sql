USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_RoWiseOutstandingDetails]    Script Date: 10/8/2020 12:21:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 25-03-2020
-- Last modified: 08-10-2020 (Add server side sorting)
-- Description:	Display customer outstanding Details Filter by RoWise and Group by Customer Type and Group by RO.
-- Usage: EXEC usp_Dashboard_RoWiseOutstandingDetails @Action=0,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType=Null,@BranchCode=Null,@Skip=0,@Take=10,@SortBy='',@SortOrder=''
-- Usage: EXEC usp_Dashboard_RoWiseOutstandingDetails @Action=1,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='Independent Workshop',@BranchCode=Null,@Skip=0,@Take=10,@SortBy='',@SortOrder=''
-- Usage: EXEC usp_Dashboard_RoWiseOutstandingDetails @Action=2,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='INDEPENDENT WORKSHOP',@BranchCode='TRS',@Skip=0,@Take=10,@SortBy='',@SortOrder=''
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_RoWiseOutstandingDetails]
    @Action int,
    @Role NVARCHAR(100),
    @UserId NVARCHAR(128),
    @StartDate DATE,
    @EndDate DATE,
	@BranchCode nvarchar(50),
	@CustomerType nvarchar(200),
	@Skip INT,
    @Take INT,
	@SearchTxt NVARCHAR(200),
	@SortBy NVARCHAR(200),
	@SortOrder NVARCHAR(10)
	
AS
BEGIN

IF(@Action!=4)
	BEGIN
	-- Declare Tables for current user Roworkshops
	DECLARE @RoWorkshop TABLE (WorkshopId int)
	
	 -- Set CustomerTypes
	 if(@CustomerType IS NULL OR @CustomerType='')
	 Begin
	 set  @CustomerType=Null;
	 End
	  
	  IF (@Role = 'SuperAdmin')
    BEGIN
	   -- Set RoWise workshopId in temp table @RoWorkshop
	insert into @RoWorkshop (WorkshopId)select WorkshopId from dbo.WorkShops 
     where outletId is Not NULL AND Type=Case When @CustomerType Is NULL THEN Type Else @CustomerType END
	   	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	declare @DistributorId int;

	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	   -- Set RoWise workshopId in temp table @RoWorkshop
	insert into @RoWorkshop (WorkshopId)select w.WorkshopId from dbo.WorkShops w
	 inner join dbo.DistributorWorkShops dw on dw.WorkshopId=w.WorkShopid
     where dw.DistributorId=@DistributorId and w.outletId is Not NULL AND w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END

    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	 -- Set RoWise workshopId in temp table @RoWorkshop
	   insert into @RoWorkshop (WorkshopId)select w.WorkshopId from dbo.WorkShops w 
	   inner join dbo.DistributorsOutlets do on do.OutletId=w.outletId
       where do.UserId=@UserId AND w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
  
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   -- Set RoWise workshopId in temp table @RoWorkshop
	   insert into @RoWorkshop (WorkshopId)select w.WorkshopId  from dbo.WorkShops w
	   inner join dbo.SalesExecutiveWorkshop sw on sw.WorkshopId=w.WorkShopId
       where sw.UserId=@UserId and w.outletId is Not NULL AND w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END

    END;
	END;

	IF(@Action=0)
	BEGIN
	SELECT SUM(w.TotalOutstanding) AS TotalOs, SUM(w.OutstandingAmount) AS TotalOsAmount 
	    FROM dbo.Outstanding o 
		 INNER JOIN dbo.WorkShops w on o.workshopid=w.workshopid
	     INNER JOIN @RoWorkshop ro on w.workshopid=ro.workshopid
	    WHERE o.CreatedDate BETWEEN @StartDate AND @EndDate
	END 

	ELSE if(@Action=1)
	BEGIN
	   -- Group BY Customer Type Wise Outstanding

	  select 	 
	   max(o.CustomerType)as CustomerType,count(DISTINCT o.PartyCode) as TotalCustomer,sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then '0' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then '0' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then '0' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then '0' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then '0' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then '0' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then '0' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then '0' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days

	   from dbo.Outstanding o 
	   inner join dbo.WorkShops w on o.workshopid=w.workshopid
	   inner join @RoWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate
	   group by o.CustomerType ORDER BY CriticalPayment DESC

	END

	else if(@Action=2)
	BEGIN
	   -- Group BY Outlet code Wise Outstanding

	   select   
	   MAX(ou.OutletCode)as BranchCode,
	   MAX(ou.OutletName)as BranchName,
	   count(w.OutletId) as TotalCustomer,
	   sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then '0' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then '0' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then '0' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then '0' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then '0' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then '0' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then '0' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then '0' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days

	   from dbo.Outstanding o 
	   inner join dbo.workshops w on o.workshopid=w.workshopid
	   inner join @RoWorkshop ro on w.workshopid=ro.workshopid
	   INNER JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   where o.CreatedDate between @StartDate and @EndDate
	   group by w.outletId ORDER BY CriticalPayment DESC

	END

	ELSE if(@Action=3)
	BEGIN
	   -- Show OS filtered by BranchCode and Group by CustomerType

	  select 	 
	   max(o.CustomerType)as CustomerType,count(DISTINCT o.PartyCode) as TotalCustomer,sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then '0' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then '0' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then '0' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then '0' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then '0' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then '0' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then '0' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then '0' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days

	   from dbo.Outstanding o 
	   inner join dbo.WorkShops w on o.workshopid=w.workshopid
	   inner join @RoWorkshop ro on w.workshopid=ro.workshopid
	   INNER JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   where o.CreatedDate between @StartDate and @EndDate AND ou.OutletCode=@BranchCode
	   group by o.CustomerType ORDER BY CriticalPayment DESC

	END

	else if(@Action=4)
	BEGIN
	   -- Group BY customer Wise Outstanding
	    DECLARE @sql NVARCHAR(MAX);
	  SET @sql
            = ';WITH search          
   AS  
   (select 	  
	   MAX(o.PartyCode)as CustomerCode,
	   MAX(o.PartyName) AS CustomerName,
	    max(o.CustomerType)as CustomerType,
	   sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then ''0'' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then ''0'' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then ''0'' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then ''0'' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then ''0'' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then ''0'' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then ''0'' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then ''0'' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days,
	   	 TotalRows =  COUNT(*) OVER()
	   from dbo.Outstanding o 
	   inner join dbo.workshops w on o.workshopid=w.workshopid
	   where o.CreatedDate between '''+cast(@StartDate as varchar)+''' and '''+cast(@EndDate as varchar)+''' and w.outletId=(select top 1 OutletId from dbo.Outlets where OutletCode='''+cast(@BranchCode as varchar)+''')';
    IF (@Role = 'Distributor')
    BEGIN
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId
	
	SET @sql=@sql+' and w.WorkshopId in(select w.WorkshopId from dbo.WorkShops w
	 inner join dbo.DistributorWorkShops dw on dw.WorkshopId=w.WorkShopid
     where dw.DistributorId=(select top 1 DistributorId from dbo.DistributorUser where UserId='''+cast(@UserId as varchar(200))+'''))';

    END;
    IF (@Role = 'RoIncharge')
    BEGIN
	 
	 SET @sql=@sql+' and w.WorkshopId in(select w.WorkshopId from dbo.WorkShops w 
	   inner join dbo.DistributorsOutlets do on do.OutletId=w.outletId
       where do.UserId='''+cast(@UserId as nvarchar(200))+''')';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   SET @sql=@sql+' and w.WorkshopId in(select w.WorkshopId  from dbo.WorkShops w
	   inner join dbo.SalesExecutiveWorkshop sw on sw.WorkshopId=w.WorkShopId
       where sw.UserId='''+cast(@UserId as nvarchar(200))+''')';
    END;

	   if(@CustomerType IS NOT NULL) 
	   Begin
	   SET @sql=@sql+' and o.CustomerType='''+cast(@CustomerType as varchar(200))+'''';
	   End
	  SET @sql=@sql+' group by o.PartyCode ORDER BY ';

	  if(@SortBy <> '' and @SortBy IS NOT NULL) 
	   Begin
	   SET @sql=@sql+''+cast(@SortBy as varchar(200))+' ' +cast(@SortOrder as varchar);
	   End
	   Else
	   Begin
	   SET @sql=@sql+' CriticalPayment  DESC';
	   End
	   SET @sql=@sql+' OFFSET '+cast(@Skip as varchar)+' ROWS FETCH NEXT '+cast(@Take as varchar)+' ROWS ONLY) select *from search';
	   
	     PRINT @sql;
         EXEC (@sql)  
	END

	else if(@Action=5)
	BEGIN
	   -- Group BY customer Wise Outstanding

	  select 	  
	   MAX(o.PartyCode)as CustomerCode,
	   MAX(o.PartyName) AS CustomerName,
	    max(o.CustomerType)as CustomerType,
	   sum(o.TotalOutstanding)as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(w.OutstandingAmount) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then '0' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then '0' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then '0' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then '0' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then '0' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then '0' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then '0' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then '0' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days,
	   	 TotalRows =  COUNT(*) OVER()
	   from dbo.Outstanding o 
	   inner join dbo.workshops w on o.workshopid=w.workshopid
	   inner join @RoWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate 
	   and o.CustomerType=CASE WHEN @CustomerType IS NOT NULL THEN @CustomerType ELSE o.CustomerType END 
	   and w.outletId=(select top 1 OutletId from dbo.Outlets where OutletCode=@BranchCode)
	   AND o.PartyCode LIKE '%'+@SearchTxt+'%' OR o.PartyName LIKE '%'+@SearchTxt+'%'
	   group by o.PartyCode ORDER BY CriticalPayment DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END

	
	
END


