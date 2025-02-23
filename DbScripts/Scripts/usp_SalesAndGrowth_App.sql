USE [Garaaz]
GO
/****** Object:  StoredProcedure [dbo].[usp_SalesAndGrowth_App]    Script Date: 9/25/2020 3:42:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- ===================================================================================
-- Author: Lokesh Choudhary
-- Create date: 14-05-2020
-- Last Modify: 09-07-2020,25-9-2020 (show data for role salesExecutive and RoIncharge,change growth formula and for all part
-- Description: Display in app sales and growth data by frequency
-- Usage: EXEC usp_SalesAndGrowth_App @Frequency='Daily', @Growth='LastDay',@StartDate='2018-01-01',@EndDate='2019-12-01',@PartCategory='M',@Role='Distributor',@UserId='810292ef-d73d-4c8f-a35d-c88edf4a46cc'
-- ===================================================================================
Alter PROCEDURE [dbo].[usp_SalesAndGrowth_App]
    @StartDate DATE,
    @EndDate DATE,
    @PartCategory NVARCHAR(100)=NULL,
    @Frequency NVARCHAR(100),
    @Growth NVARCHAR(100),
    @Role NVARCHAR(500) = NULL,
    @UserId NVARCHAR(500) = NULL
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    -- SET DistributorId and WorkshopId
    DECLARE @DistributorId INT
    DECLARE @WorkshopId INT
    SET @DistributorId = dbo.getDistributorId(@UserId, @Role)
    SET @WorkshopId = dbo.getWorkshopId(@UserId, @Role)

    DECLARE @LastSaleCol NVARCHAR(100);
	DECLARE @PrevSaleCol NVARCHAR(100);
    DECLARE @sql NVARCHAR(MAX);

	 -- If PartCategory value is all then show all partCategory 
	if(@PartCategory IS NOT NULL)
	Begin    
		if(@PartCategory='All' or @PartCategory='')
		Begin
			set @PartCategory=NULL;
		End
	End

	SET @PrevSaleCol = 'PreviousSale';
    SET @sql = N'SELECT ';

    -- ===============================================================
    -- FREQUENCY: DAILY
    -- GROWTH: LastDay, LastWeek, LastMonth, LastQuarter, LastYearly    
    -- ===============================================================
    IF (@Frequency = 'Daily')
    BEGIN
        IF (@Growth = 'LastDay')
        BEGIN
            SET @LastSaleCol = N'LastDaySale';
        END;
        ELSE IF (@Growth = 'LastWeek')
        BEGIN
            SET @LastSaleCol = N'LastWeekSale';
        END;
        ELSE IF (@Growth = 'LastMonth')
        BEGIN
            SET @LastSaleCol = N'LastMonthSale';
        END;
        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @LastSaleCol = N'LastQuarterSale';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @LastSaleCol = N'LastYearSale';
        END;

        SET @sql
            = @sql
              + N' DAY(d.CreatedDate) AS Day, MONTH(d.CreatedDate) AS Month,
               YEAR(d.CreatedDate) AS Year,
               SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) AS CurrentSale,
               lastDay.PrevSale AS ' + @LastSaleCol
			   + N', lastDay.PrevSale AS ' + @PrevSaleCol
              + N',
               (FORMAT(CONVERT(DECIMAL(18, 2), (SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) - lastDay.PrevSale))
                          / lastDay.PrevSale,''p'')) AS Growth
             FROM dbo.DailySalesTrackerWithInvoiceData d
            LEFT OUTER JOIN
            (
                SELECT DAY(a.CreatedDate) AS D,
                       MONTH(a.CreatedDate) AS M,
                       YEAR(a.CreatedDate) AS Y,
                       SUM(CAST(a.NetRetailSelling AS DECIMAL(18, 2))) AS PrevSale
                FROM dbo.DailySalesTrackerWithInvoiceData a'

                if(@PartCategory!='' and @PartCategory IS NOT NULL)
                begin
                    SET @sql = @sql + ' WHERE a.PartCategory ='+N''''+@PartCategory+N''''
                end
                else
                begin
					SET @sql = @sql + ' WHERE 1=1'
                end

        -- Filter inner table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
			IF (@Role='RoIncharge')
			BEGIN
				--Get workshop Ids matching to RO (or outlet)								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
				INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
				WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
			ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND a.DistributorId=' + CAST(@DistributorId AS VARCHAR)
					END

					IF (@WorkshopId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND a.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
					END
		   END
        END

        SET @sql
            = @sql
              + ' GROUP BY DAY(a.CreatedDate),
                         MONTH(a.CreatedDate),
                         YEAR(a.CreatedDate)
            ) lastDay'

        IF (@Growth = 'LastDay')
        BEGIN
            SET @sql
                = @sql
                  + N' ON DAY((DATEADD(DAY, -1, d.CreatedDate))) = lastDay.D
               AND MONTH(DATEADD(DAY, -1, d.CreatedDate)) = lastDay.M
               AND YEAR((DATEADD(DAY, -1, d.CreatedDate))) = lastDay.Y';
        END;
        ELSE IF (@Growth = 'LastWeek')
        BEGIN
            SET @sql
                = @sql
                  + N' ON DAY(DATEADD(DAY, -7, d.CreatedDate)) = lastDay.D
           AND  MONTH(DATEADD(DAY, -7, d.CreatedDate)) = lastDay.M
           AND YEAR((DATEADD(DAY, -7, d.CreatedDate))) = lastDay.Y';
        END;
        ELSE IF (@Growth = 'LastMonth')
        BEGIN
            SET @sql
                = @sql
                  + N' ON DAY(d.CreatedDate) = lastDay.D
           AND MONTH(DATEADD(MONTH, -1, d.CreatedDate)) = lastDay.M
           AND YEAR((DATEADD(MONTH, -1, d.CreatedDate))) = lastDay.Y';
        END;
        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @sql
                = @sql
                  + N' ON DAY(d.CreatedDate) = lastDay.D
           AND  MONTH(DATEADD(MONTH, -3, d.CreatedDate)) = lastDay.M
           AND YEAR((DATEADD(MONTH, -3, d.CreatedDate))) = lastDay.Y';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @sql
                = @sql
                  + N' ON DAY(d.CreatedDate) = lastDay.D
           AND  MONTH(d.CreatedDate) = lastDay.M
           AND YEAR((DATEADD(YEAR, -1, d.CreatedDate))) = lastDay.Y';
        END;

        SET @sql
            = @sql + N' WHERE d.CreatedDate BETWEEN ' + N'''' + CAST(@StartDate AS VARCHAR) + N'''' + N' AND ' + N''''
              + CAST(@EndDate AS VARCHAR) + N'''' + N' '
              
              if(@PartCategory!='' and @PartCategory IS NOT NULL)
                begin
                    SET @sql = @sql + ' AND d.PartCategory ='+N''''+@PartCategory+N''''
                end

              

        -- Filter main table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
				--Get workshop Ids matching to RO (or outlet)								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
				INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
				WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
			ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
		   END
        END

        SET @sql
            = @sql
              + ' GROUP BY DAY(d.CreatedDate),
                     MONTH(d.CreatedDate),
                     YEAR(d.CreatedDate),
                     lastDay.PrevSale
            ORDER BY YEAR(d.CreatedDate),
                     MONTH(d.CreatedDate),
                     DAY(d.CreatedDate)';

        PRINT @sql;
        EXEC (@sql);
    END;

    -- =====================================================
    -- FREQUENCY: WEEKLY
    -- GROWTH: LastWeek, LastMonth, LastQuarter, LastYearly     
    -- ======================================================
    IF (@Frequency = 'Weekly')
    BEGIN
        IF (@Growth = 'LastWeek')
        BEGIN
            SET @LastSaleCol = N'LastWeekSale';
        END;
        ELSE IF (@Growth = 'LastMonth')
        BEGIN
            SET @LastSaleCol = N'LastMonthSale';
        END;

        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @LastSaleCol = N'LastQuarterSale';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @LastSaleCol = N'LastYearSale';
        END;

        SET @sql
            = @sql
              + ' DATEPART(wk, d.CreatedDate) AS Week,
       MONTH(d.CreatedDate) AS Month,
       YEAR(d.CreatedDate) AS Year,
       SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) AS CurrentSale,
       lastWeek.PrevSale AS ' + @LastSaleCol
	    + N', lastWeek.PrevSale AS ' + @PrevSaleCol
              + ',
       (FORMAT(
                  CONVERT(DECIMAL(18, 2), (SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) - lastWeek.PrevSale))
                  / lastWeek.PrevSale, ''p''
              )
       ) AS Growth
FROM dbo.DailySalesTrackerWithInvoiceData d
    LEFT OUTER JOIN
    (
        SELECT SUM(CAST(a.NetRetailSelling AS DECIMAL(18, 2))) AS PrevSale,
               DATEPART(wk, a.CreatedDate) AS W,
               MONTH(a.CreatedDate) AS M,
               YEAR(a.CreatedDate) AS Y
        FROM dbo.DailySalesTrackerWithInvoiceData a'
		if(@PartCategory!='' and @PartCategory IS NOT NULL)
        begin
			SET @sql = @sql + ' WHERE a.PartCategory ='+N''''+@PartCategory+N''''
        end
		else
		begin
			SET @sql = @sql + ' WHERE 1=1'
		end

        -- Filter inner table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
				--Get workshop Ids matching to RO (or outlet)								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
				INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
				WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
			ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
            END
        END

        SET @sql
            = @sql
              + '
        GROUP BY DATEPART(wk, a.CreatedDate),
                 MONTH(a.CreatedDate),
                 YEAR(a.CreatedDate)
    ) lastWeek';

        IF (@Growth = 'LastWeek')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(wk, d.CreatedDate) - 1 = lastWeek.W
                        AND MONTH(d.CreatedDate) = lastWeek.M
                        AND YEAR(d.CreatedDate) = lastWeek.Y';
        END;
        ELSE IF (@Growth = 'LastMonth')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(wk, d.CreatedDate) = lastWeek.W
           AND MONTH(DATEADD(MONTH, -1, d.CreatedDate)) = lastWeek.M
           AND YEAR(DATEADD(MONTH, -1, d.CreatedDate)) = lastWeek.Y';
        END
        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(wk, d.CreatedDate) = lastWeek.W
           AND MONTH(DATEADD(MONTH, -3, d.CreatedDate)) = lastWeek.M
           AND YEAR(DATEADD(MONTH, -3, d.CreatedDate)) = lastWeek.Y';
        END
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(wk, d.CreatedDate) = lastWeek.W
           AND MONTH(d.CreatedDate) = lastWeek.M
           AND YEAR(DATEADD(YEAR, -1, d.CreatedDate)) = lastWeek.Y';
        END

        SET @sql
            = @sql + N' WHERE d.CreatedDate BETWEEN ' + N'''' + CAST(@StartDate AS VARCHAR) + N'''' + N' AND ' + N''''
              + CAST(@EndDate AS VARCHAR) + N''''

			  if(@PartCategory!='' and @PartCategory IS NOT NULL)
				begin
					SET @sql = @sql + ' AND d.PartCategory ='+N''''+@PartCategory+N''''
				end


        -- Filter main table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
				--Get workshop Ids matching to RO (or outlet)								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
				INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
				WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
			ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
			END
        END

        SET @sql
            = @sql
              + N' GROUP BY DATEPART(wk, d.CreatedDate),
                     MONTH(d.CreatedDate),
                     YEAR(d.CreatedDate),
                     lastWeek.PrevSale
            ORDER BY YEAR(d.CreatedDate),
                     MONTH(d.CreatedDate),
                     DATEPART(wk, d.CreatedDate)';

        PRINT @sql;
        EXEC (@sql);

    END

    -- =============================================
    -- FREQUENCY: MONTHLY
    -- GROWTH: LastMonth, LastQuarter, LastYearly     
    -- =============================================
    IF (@Frequency = 'Monthly')
    BEGIN
        IF (@Growth = 'LastMonth')
        BEGIN
            SET @LastSaleCol = N'LastMonthSale';
        END;
        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @LastSaleCol = N'LastQuarterSale';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @LastSaleCol = N'LastYearSale';
        END;

        SET @sql = @sql + N' MONTH(d.CreatedDate) AS Month, ';
        SET @sql
            = @sql
              + N' YEAR(d.CreatedDate) AS Year, 
                    SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) AS CurrentSale,
                    lastMonth.SaleT AS ' + @LastSaleCol
					 + N', lastMonth.SaleT AS ' + @PrevSaleCol
              + ',
                    (FORMAT(CONVERT(DECIMAL(18, 2), (SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) - lastMonth.SaleT))/ lastMonth.SaleT,''p'')) AS Growth
                    FROM dbo.DailySalesTrackerWithInvoiceData d
                    LEFT OUTER JOIN (SELECT SUM(CAST(a.NetRetailSelling AS DECIMAL(18, 2))) AS SaleT,MONTH(a.CreatedDate) AS M,YEAR(a.CreatedDate) AS Y FROM   dbo.DailySalesTrackerWithInvoiceData a  ' 
                    
                    if(@PartCategory ='' or @PartCategory IS NULL)
                    begin
                        SET @sql = @sql + ' WHERE 1=1'
                    end
                    else
                    begin
                        SET @sql = @sql + ' WHERE a.PartCategory ='+N''''+@PartCategory+N''''
                    end

        -- Filter inner table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
			IF (@Role='RoIncharge')
				BEGIN
					--Get workshop Ids matching to RO (or outlet)								
					SET @sql = @sql + ' AND a.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
					INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
					WHERE d.UserId='+N''''+@UserId+N''''+')'
				END
				Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
				ELSE 
				BEGIN
					IF (@DistributorId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND a.DistributorId=' + CAST(@DistributorId AS VARCHAR)
					END

					IF (@WorkshopId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND a.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
					END
			END
        END

        SET @sql = @sql + N' GROUP BY MONTH(a.CreatedDate),YEAR(a.CreatedDate))lastMonth';

        IF (@Growth = 'LastMonth')
        BEGIN
            SET @sql
                = @sql
                  + N' ON MONTH((DATEADD(Month, -1, d.CreatedDate)))=lastMonth.M AND YEAR((DATEADD(Month, -1, d.CreatedDate))) = lastMonth.Y';
        END;
        ELSE IF (@Growth = 'LastQuarter')
        BEGIN
            SET @sql
                = @sql
                  + N' ON MONTH((DATEADD(Month, -3, d.CreatedDate)))=lastMonth.M AND YEAR((DATEADD(Month, -3, d.CreatedDate))) = lastMonth.Y';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @sql
                = @sql
                  + N' ON MONTH((DATEADD(Month, -12, d.CreatedDate)))=lastMonth.M AND YEAR((DATEADD(Month, -12, d.CreatedDate))) = lastMonth.Y';
        END;

        SET @sql
            = @sql + N' WHERE d.CreatedDate BETWEEN ' + N'''' + CAST(@StartDate AS VARCHAR) + N'''' + N' AND ' + N''''
              + CAST(@EndDate AS VARCHAR) + N'''' + N' ' 
              
              if(@PartCategory!='' and @PartCategory IS NOT NULL)
              begin
                SET @sql = @sql + ' AND d.PartCategory ='+N''''+@PartCategory+N''''
              end

        -- Filter main table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
				BEGIN
					--Get workshop Ids matching to RO (or outlet)								
					SET @sql = @sql + ' AND d.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
					INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
					WHERE d.UserId='+N''''+@UserId+N''''+')'
				END
				Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
				ELSE 
					BEGIN
					IF (@DistributorId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND d.DistributorId=' + CAST(@DistributorId AS VARCHAR)
					END

					IF (@WorkshopId IS NOT NULL)
					BEGIN
						SET @sql = @sql + ' AND d.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
					END
			END
        END

        SET @sql = @sql + N' GROUP BY  MONTH(d.CreatedDate),YEAR(d.CreatedDate), lastMonth.SaleT
        ORDER BY YEAR(d.CreatedDate),
                     MONTH(d.CreatedDate)';
        PRINT @sql;
        EXEC (@sql);
    END;

    -- ==================================
    -- FREQUENCY: QUARTERLY
    -- GROWTH: LastQuarter, LastYearly     
    -- ==================================
    IF (@Frequency = 'Quarterly')
    BEGIN
        IF (@Growth = 'LastQuarter')
        BEGIN
            SET @LastSaleCol = N'LastQuarterSale';
        END;
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @LastSaleCol = N'LastYearSale';
        END;

        SET @sql
            = @sql
              + ' DATEPART(QUARTER, d.CreatedDate) AS Quarter,
       YEAR(d.CreatedDate) AS Year,
       SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) AS CurrentSale,
       lastQuarter.PrevSale AS ' + @LastSaleCol
	    + N', lastQuarter.PrevSale AS ' + @PrevSaleCol
              + ',
       (FORMAT(
                  CONVERT(DECIMAL(18, 2), (SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) - lastQuarter.PrevSale))
                  / lastQuarter.PrevSale, ''p''
              )
       ) AS Growth
FROM dbo.DailySalesTrackerWithInvoiceData d
    LEFT OUTER JOIN
    (
        SELECT SUM(CAST(a.NetRetailSelling AS DECIMAL(18, 2))) AS PrevSale,
               DATEPART(QUARTER, a.CreatedDate) AS Q,
               YEAR(a.CreatedDate) AS Y
        FROM dbo.DailySalesTrackerWithInvoiceData a'

		if(@PartCategory!='' and @PartCategory IS NOT NULL)
        begin
			SET @sql = @sql + ' WHERE a.PartCategory ='+N''''+@PartCategory+N''''
        end
		else
		begin
			SET @sql = @sql + ' WHERE 1=1'
		end

        

        -- Filter inner table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
			 --Get workshop Ids matching to RO (or outlet)								
			 SET @sql = @sql + ' AND a.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
			 INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
			 WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
		  ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
		 END
        END

        SET @sql
            = @sql
              + '
        GROUP BY DATEPART(YEAR, a.CreatedDate),
                 DATEPART(QUARTER, a.CreatedDate)
    ) lastQuarter';

        IF (@Growth = 'LastQuarter')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(QUARTER, d.CreatedDate) - 1 = lastQuarter.Q
                       AND YEAR(d.CreatedDate) = lastQuarter.Y';
        END
        ELSE IF (@Growth = 'LastYearly')
        BEGIN
            SET @sql
                = @sql
                  + '  ON DATEPART(QUARTER, d.CreatedDate) = lastQuarter.Q
                       AND YEAR(DATEADD(YEAR, -1, d.CreatedDate)) = lastQuarter.Y';
        END

        SET @sql
            = @sql + N' WHERE d.CreatedDate BETWEEN ' + N'''' + CAST(@StartDate AS VARCHAR) + N'''' + N' AND ' + N''''
              + CAST(@EndDate AS VARCHAR) + N'''' 

				if(@PartCategory!='' and @PartCategory IS NOT NULL)
				begin
					SET @sql = @sql + ' AND d.PartCategory ='+N''''+@PartCategory+N''''
				end
				

        -- Filter main table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
			 --Get workshop Ids matching to RO (or outlet)								
			 SET @sql = @sql + ' AND d.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
			 INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
			 WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
		  ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
			END
        END

        SET @sql
            = @sql
              + N' GROUP BY DATEPART(QUARTER, d.CreatedDate),         
                   YEAR(d.CreatedDate),
                   lastQuarter.PrevSale
               ORDER BY DATEPART(YEAR, d.CreatedDate),
                        DATEPART(QUARTER, d.CreatedDate)';

        PRINT @sql;
        EXEC (@sql);
    END

    -- ======================
    -- FREQUENCY: YEARLY
    -- GROWTH: LastYearly     
    -- ======================
    IF (@Frequency = 'Yearly')
    BEGIN
        IF (@Growth = 'LastYearly')
        BEGIN
            SET @LastSaleCol = N'LastYearSale';
        END;

        SET @sql
            = @sql
              + ' (CASE WHEN DATEPART(YEAR, '''+ CAST(@StartDate as nvarchar)+''') = DATEPART(YEAR, '''+CAST(@EndDate as nvarchar)+''') THEN CONVERT(nvarchar, DATEPART(YEAR, '''+CAST(@StartDate as nvarchar)+''')) ELSE FORMAT(CONVERT(datetime,'''+CAST(@StartDate as nvarchar)+'''), ''yy'') + ''-'' + FORMAT(CONVERT(datetime,'''+CAST(@EndDate as nvarchar)+'''), ''yy'') END) as Year,
       SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) AS CurrentSale,
       lastYear.PrevSale AS ' + @LastSaleCol
	    + N', lastYear.PrevSale AS ' + @PrevSaleCol
              + ',
       (FORMAT(
                  CONVERT(DECIMAL(18, 2), (SUM(CAST(d.NetRetailSelling AS DECIMAL(18, 2))) - lastYear.PrevSale))
                  / lastYear.PrevSale, ''p''
              )
       ) AS Growth,''Row'' as Jn
FROM dbo.DailySalesTrackerWithInvoiceData d
    LEFT OUTER JOIN
    (
        SELECT top 1 SUM(CAST(a.NetRetailSelling AS DECIMAL(18, 2))) AS PrevSale,''Row'' as Jn
        FROM dbo.DailySalesTrackerWithInvoiceData a'

        if(@PartCategory ='' or @PartCategory IS NULL)
        begin
            SET @sql = @sql + ' WHERE 1=1'
        end
        else
        begin
            SET @sql = @sql + ' WHERE a.PartCategory ='+N''''+@PartCategory+N''''
        end

		SET @sql = @sql + ' AND a.CreatedDate BETWEEN DATEADD(YEAR, -1, '''+ CAST(@StartDate as nvarchar)+''') AND DATEADD(YEAR, -1, '''+CAST(@EndDate as nvarchar)+''')'

        -- Filter inner table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
			 --Get workshop Ids matching to RO (or outlet)								
			 SET @sql = @sql + ' AND a.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
			 INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
			 WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND a.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
		  ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND a.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
			END
        END



        SET @sql = @sql + ') lastYear';

        IF (@Growth = 'LastYearly')
        BEGIN
            SET @sql = @sql + '  ON Jn = lastYear.Jn';
        END

        SET @sql
            = @sql + N' WHERE d.CreatedDate BETWEEN ' + N'''' + CAST(@StartDate AS VARCHAR) + N'''' + N' AND ' + N''''
              + CAST(@EndDate AS VARCHAR) + N'''' + N' ' 
              
              if(@PartCategory!='' and @PartCategory IS NOT NULL)
              begin
                SET @sql = @sql + ' AND d.PartCategory ='+N''''+@PartCategory+N''''
              end
              

        -- Filter main table by role
        IF (@Role <> 'SuperAdmin')
        BEGIN
		IF (@Role='RoIncharge')
			BEGIN
			 --Get workshop Ids matching to RO (or outlet)								
			 SET @sql = @sql + ' AND d.WorkshopId IN(SELECT w.WorkShopId FROM Workshops w
			 INNER JOIN DistributorsOutlets d ON w.outletId=d.OutletId
			 WHERE d.UserId='+N''''+@UserId+N''''+')'
			END
			Else IF (@Role='SalesExecutive')
			BEGIN
				--Get workshop Ids matching to SalesExecutive								
				SET @sql = @sql + ' AND d.WorkshopId IN(SELECT WorkShopId FROM SalesExecutiveWorkshop
				WHERE UserId='+N''''+@UserId+N''''+')'
			END
		  ELSE 
			BEGIN
				IF (@DistributorId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.DistributorId=' + CAST(@DistributorId AS VARCHAR)
				END

				IF (@WorkshopId IS NOT NULL)
				BEGIN
					SET @sql = @sql + ' AND d.WorkshopId=' + CAST(@WorkshopId AS VARCHAR)
				END
			END
        END

        SET @sql
            = @sql
              + N' GROUP BY lastYear.PrevSale';

        PRINT @sql;
        EXEC (@sql);
    END

END;