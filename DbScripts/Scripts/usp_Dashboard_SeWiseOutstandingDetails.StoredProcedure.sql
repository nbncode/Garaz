USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_SeWiseOutstandingDetails]    Script Date: 10/8/2020 5:20:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 27-03-2020
-- Last modified: 08-10-2020 (Add server side sorting)
-- Description:	Display customer outstanding Details Filter by SalesExecutivesWise and Group by Customer Type and Group by SE.
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=6,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType=Null,@BranchCode=Null,@Skip=0,@Take=10,@SortBy=null,@SortOrder=null
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=8,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='Independent Workshop',@BranchCode=Null,@Skip=0,@Take=10,@SortBy=null,@SortOrder=null
-- Usage: EXEC usp_Dashboard_SeWiseOutstandingDetails @Action=10,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='INDEPENDENT WORKSHOP',@BranchCode='TRS',@Skip=0,@Take=10,@SortBy=null,@SortOrder=null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_SeWiseOutstandingDetails]
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

IF(@Action!=10)
	BEGIN
	-- Declare Tables for current user Seworkshops
	DECLARE @SeWorkshop TABLE (WorkshopId INT, UserId NVARCHAR(128))

	 -- Set CustomerTypes
	 if(@CustomerType IS NULL OR @CustomerType='')
	 Begin
	 set  @CustomerType=Null;
	 End
	
	  
	  IF (@Role = 'SuperAdmin')
    BEGIN
	   -- Set SeWise workshopId in temp table @SeWorkshop
	insert into @SeWorkshop (WorkshopId, UserId)select WorkshopId,UserId from dbo.SalesExecutiveWorkshop 
	   	   
    END;
    ELSE IF (@Role = 'Distributor')
    BEGIN

	-- Get distributorId using UserId
	declare @DistributorId int;

	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId

	   -- Set SeWise workshopId in temp table @SeWorkshop
	insert into @SeWorkshop (WorkshopId,UserId)select w.WorkshopId,w.UserId from dbo.SalesExecutiveWorkshop w
	 inner join dbo.DistributorWorkShops dw on dw.WorkshopId=w.WorkShopid
     where dw.DistributorId=@DistributorId

    END;
    ELSE IF (@Role = 'RoIncharge')
    BEGIN
	 -- Set SeWise workshopId in temp table @SeWorkshop

	  insert into @SeWorkshop (WorkshopId,UserId)select w.WorkshopId,w.UserId from dbo.SalesExecutiveWorkshop w
	  inner join dbo.RoSalesExecutive e on w.UserId=e.SeUserId where e.RoUserId=@UserId
  
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   -- Set SeWise workshopId in temp table @SeWorkshop
	   insert into @SeWorkshop (WorkshopId,UserId)select WorkshopId,UserId from dbo.SalesExecutiveWorkshop 
       where UserId=@UserId

    END;
END;

	IF(@Action=6)
	BEGIN
	SELECT SUM(isnull(w.TotalOutstanding,0)) AS TotalOs, SUM(isnull(w.OutstandingAmount,0)) AS TotalOsAmount 
	    FROM dbo.Outstanding o 
		 INNER JOIN dbo.WorkShops w on o.workshopid=w.workshopid
	     INNER JOIN @SeWorkshop sw on w.workshopid=sw.workshopid
	    WHERE o.CreatedDate BETWEEN @StartDate AND @EndDate AND w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	END 

	ELSE IF(@Action=7)
	BEGIN
	   -- Group BY Customer Type Wise Outstanding

	  select 	 
	   max(o.CustomerType)as CustomerType,count(DISTINCT o.PartyCode) as TotalCustomer,
	   SUM(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate AND w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	   group by o.CustomerType ORDER BY CriticalPayment DESC

	END

	else if(@Action=8)
	BEGIN
	   -- Group BY Outlet code Wise Outstanding

	   select 	 
	  
	   MAX(ou.OutletCode)as BranchCode,
	   MAX(u.FirstName +' '+ u.LastName) AS SalesExName,
	   count(w.OutletId) as TotalCustomer,
	   sum(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   INNER JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   INNER JOIN dbo.UserDetails u ON ro.UserId=u.UserId 
	   where o.CreatedDate between @StartDate and @EndDate AND w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	   group by w.outletId ORDER BY CriticalPayment DESC

	END

	ELSE if(@Action=9)
	BEGIN
	   -- Show OS filtered by BranchCode and Group by CustomerType

	   select 	 
	   max(o.CustomerType)as CustomerType,count(DISTINCT o.PartyCode) as TotalCustomer,
	   SUM(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   INNER JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   where o.CreatedDate between @StartDate and @EndDate AND ou.OutletCode=@BranchCode AND w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	   group by o.CustomerType  ORDER BY CriticalPayment DESC

	END

	else if(@Action=10)
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
	   sum(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

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
	   inner join dbo.SalesExecutiveWorkshop se on w.workshopid=se.workshopid
	   where o.CreatedDate between '''+cast(@StartDate as varchar)+''' and '''+cast(@EndDate as varchar)+''' and w.outletId=(select top 1 OutletId from dbo.Outlets where OutletCode='''+cast(@BranchCode as varchar)+''')';
    IF (@Role = 'Distributor')
    BEGIN
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId
	
	SET @sql=@sql+' and se.WorkshopId in(select WorkshopId from dbo.DistributorWorkShops where DistributorId=(select top 1 DistributorId from dbo.DistributorUser where UserId='''+cast(@UserId as varchar(200))+'''))';

    END;
    IF (@Role = 'RoIncharge')
    BEGIN
	 
	 SET @sql=@sql+' and se.UserId in(select SeUserId from dbo.RoSalesExecutive
       where RoUserId='''+cast(@UserId as nvarchar(200))+''')';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   SET @sql=@sql+' and se.UserId='''+cast(@UserId as nvarchar(200))+'''';
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

	else if(@Action=11)
	BEGIN
	select  	  
	    MAX(o.PartyCode)as CustomerCode,
	   MAX(o.PartyName) AS CustomerName,
	    max(o.CustomerType)as CustomerType,
	   sum(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

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
	   inner join @SeWorkshop ro on w.workshopid=ro.workshopid
	   where o.CreatedDate between @StartDate and @EndDate 
	  and o.CustomerType=CASE WHEN @CustomerType IS NOT NULL THEN @CustomerType ELSE o.CustomerType END 
	   and w.outletId=(select top 1 OutletId from dbo.Outlets where OutletCode=@BranchCode)
	   AND w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	   AND o.PartyCode LIKE '%'+@SearchTxt+'%' OR o.PartyName LIKE '%'+@SearchTxt+'%'
	   group by o.PartyCode ORDER BY CriticalPayment DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	END
	
END


