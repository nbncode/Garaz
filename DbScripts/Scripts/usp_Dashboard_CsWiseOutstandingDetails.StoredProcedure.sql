USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_Dashboard_CsWiseOutstandingDetails]    Script Date: 10/8/2020 6:24:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ===================================================================================
-- Author: Lokesh Choudhary & Vikram Singh Saini
-- Create date: 27-03-2020
-- Last modified: 08-10-2020 (Add server side sorting)
-- Description:	Display customer outstanding Details Filter by SalesExecutivesWise and Group by Customer Type and Group by SE.
-- Usage: EXEC usp_Dashboard_CsWiseOutstandingDetails @Action=6,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType=Null,@BranchCode=Null,@Skip=0,@Take=10,@SortBy=null,@SortOrder=null
-- Usage: EXEC usp_Dashboard_CsWiseOutstandingDetails @Action=7,@Role='SuperAdmin', @UserId='E20ED292-C56A-425B-8D3B-D8E7E114ECBA', @StartDate='2019-01-01', @EndDate='2020-03-25',@CustomerType='Independent Workshop',@BranchCode=Null,@Skip=0,@Take=10,@SortBy=null,@SortOrder=null
-- ===================================================================================
ALTER PROCEDURE [dbo].[usp_Dashboard_CsWiseOutstandingDetails]
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
IF(@Action!=14)
	BEGIN
	-- Declare Tables for current user workshops
	DECLARE @UserWorkshop TABLE (WorkshopId int)

	 -- Set CustomerTypes
	 if(@CustomerType IS NULL OR @CustomerType='')
	 Begin
	 set  @CustomerType=Null;
	 End
	 

	  IF (@Role = 'SuperAdmin')
    BEGIN
	   -- Set all workshopId in temp table @UserWorkshop
	insert into @UserWorkshop (WorkshopId)select WorkshopId from dbo.Workshops 
	where Type =Case When @CustomerType Is NULL THEN Type Else @CustomerType END   	   
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
		where w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END and do.Userid=@UserId  
    END;

    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   -- Set SalesExecutive workshopId in temp table @UserWorkshop
	   insert into @UserWorkshop (WorkshopId)select w.WorkshopId  from dbo.WorkShops w
	   inner join dbo.SalesExecutiveWorkshop sew on w.WorkshopId=sew.WorkshopId
       where w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END and UserId=@UserId
    END;
 END;

	IF(@Action=12)
	BEGIN
	SELECT SUM(isnull(w.TotalOutstanding,0)) AS TotalOs, SUM(isnull(w.OutstandingAmount,0)) AS TotalOsAmount 
	    FROM dbo.Outstanding o 
		 INNER JOIN dbo.WorkShops w on o.workshopid=w.workshopid
	     INNER JOIN @UserWorkshop uw on w.workshopid=uw.workshopid
	    WHERE o.CreatedDate BETWEEN @StartDate AND @EndDate AND w.Type =Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	END 

	ELSE IF(@Action=13)
	BEGIN
	   -- Group BY Customer Type Wise Outstanding

	  select 	 
	   max(o.CustomerType)as CustomerType,count(DISTINCT o.PartyCode) as TotalCustomer,sum(isnull(o.TotalOutstanding,0))as TotalOutstanding,
	   sum(isnull(w.CreditLimit,0))as CreditLimit,sum(isnull(w.OutstandingAmount,0)) as CriticalPayment,

       sum((cast(case when TRIM(o.LessThan7Days) IS NULL then '0' else o.LessThan7Days end as decimal(18,2))+
       cast(case when TRIM(o.[7To14Days]) IS NULL then '0' else o.[7To14Days] end as decimal(18,2))))as [0To14Days],

	   sum((cast(case when TRIM(o.[14To21Days]) IS NULL then '0' else o.[14To21Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[21To28Days] ) IS NULL then '0' else o.[21To28Days] end as decimal(18,2))))as [14To28Days],

	   sum((cast(case when TRIM(o.[28To35Days]) IS NULL then '0' else o.[28To35Days] end as decimal(18,2))+
	   cast(case when TRIM(o.[35To50Days] ) IS NULL then '0' else o.[35To50Days] end as decimal(18,2))))as [28To50Days],

	  sum(cast(case when TRIM(o.[50To70Days]) IS NULL then '0' else o.[50To70Days] end as decimal(18,2)))as [50To70Days],
	   sum(cast(case when TRIM(o.MoreThan70Days ) IS NULL then '0' else o.MoreThan70Days end as decimal(18,2)))as MoreThan70Days

	   from Outstanding o 
	   inner join workshops w on o.workshopid=w.workshopid
	   inner join @UserWorkshop uw on w.workshopid=uw.workshopid
	   where o.CreatedDate between @StartDate and @EndDate and  w.Type=Case When @CustomerType Is NULL THEN w.Type Else @CustomerType END
	   group by o.CustomerType ORDER BY CriticalPayment DESC

	END

	else if(@Action=14)
	BEGIN
	   -- Group BY customer Wise Outstanding

	DECLARE @sql NVARCHAR(MAX);
	  SET @sql
            = ';WITH search          
   AS  
   (select 	
       MAX(ou.OutletCode)as BranchCode,  
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
	   left outer JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   where o.CreatedDate between '''+cast(@StartDate as varchar)+''' and '''+cast(@EndDate as varchar)+'''';
    IF (@Role = 'Distributor')
    BEGIN
	select @DistributorId=DistributorId from dbo.DistributorUser where UserId=@UserId
	
	SET @sql=@sql+' and w.WorkshopId in(select WorkshopId from dbo.DistributorWorkShops where DistributorId=(select top 1 DistributorId from dbo.DistributorUser where UserId='''+cast(@UserId as varchar(200))+'''))';

    END;
    IF (@Role = 'RoIncharge')
    BEGIN
	 
	 SET @sql=@sql+' and w.OutletId in(select OutletId from dbo.DistributorsOutlets where Userid='''+cast(@UserId as nvarchar(200))+''')';
    END;
    ELSE IF (@Role = 'SalesExecutive')
    BEGIN
	   SET @sql=@sql+' and w.WorkshopId in(select WorkshopId from dbo.SalesExecutiveWorkshop where Userid='''+cast(@UserId as nvarchar(200))+''')';
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

	else if(@Action=15)
	BEGIN
	   -- Group BY customer Wise Outstanding

	  select 	  
	    MAX(ou.OutletCode)as BranchCode,
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
	   inner join @UserWorkshop uw on w.workshopid=uw.workshopid
	   INNER JOIN dbo.Outlets ou ON w.OutletId=ou.OutletId
	   where o.CreatedDate between @StartDate and @EndDate and  o.CustomerType=Case When @CustomerType Is NULL THEN o.CustomerType Else @CustomerType END
	    AND o.PartyCode LIKE '%'+@SearchTxt+'%' OR o.PartyName LIKE '%'+@SearchTxt+'%'
	   group by o.PartyCode ORDER BY CriticalPayment DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END
	
END


