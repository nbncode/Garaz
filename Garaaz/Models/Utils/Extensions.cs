using Garaaz.Models.DashboardOverview;
using Garaaz.Models.DashboardOverview.DataTables;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Security.Principal;

namespace Garaaz.Models
{
    public static class Extensions
    {
        /// <summary>
        /// Get column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <returns>Return value if found else empty string.</returns>
        public static string ColumnValue(this DataTable dt, DataRow row, string colName)
        {
            var colValue = string.Empty;
            if (dt.Columns.Contains(colName))
            {
                colValue = Convert.ToString(row[colName]);

                if (colValue.Equals("N/A", StringComparison.OrdinalIgnoreCase) || colValue.Equals("-", StringComparison.OrdinalIgnoreCase) || colValue.Equals("#N/A", StringComparison.OrdinalIgnoreCase))
                    return string.Empty;
            }
            return colValue.Trim();
        }

        /// <summary>
        /// Get decimal column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <returns>Return decimal value if found else null.</returns>
        public static decimal? DecimalColumnValue(this DataTable dt, DataRow row, string colName)
        {
            decimal? colValue = null;
            if (dt.Columns.Contains(colName))
            {
                var cValue = Convert.ToString(row[colName]);
                if (decimal.TryParse(cValue, out decimal dValue))
                {
                    colValue = dValue;
                }
            }
            return colValue;
        }

        /// <summary>
        /// Get int column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <returns>Return decimal value if found else null.</returns>
        public static int? IntColumnValue(this DataTable dt, DataRow row, string colName)
        {
            int? colValue = null;
            if (dt.Columns.Contains(colName))
            {
                var cValue = Convert.ToString(row[colName]);
                if (int.TryParse(cValue, out int iValue))
                {
                    colValue = iValue;
                }
            }
            return colValue;
        }

        /// <summary>
        /// Get date column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <returns>Return decimal value if found else null.</returns>
        public static DateTime? DateColumnValue(this DataTable dt, DataRow row, string colName)
        {
            DateTime? colValue = null;
            if (dt.Columns.Contains(colName))
            {
                var cValue = Convert.ToString(row[colName]);
                if (DateTime.TryParse(cValue, out DateTime dtValue))
                {
                    colValue = dtValue;
                }
            }
            return colValue;
        }

        /// <summary>
        /// Get date column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <param name="formats">The formats to use for parsing.</param>
        public static DateTime? DateColumnValue(this DataTable dt, DataRow row, string colName, string[] formats)
        {
            DateTime? colValue = null;
            if (dt.Columns.Contains(colName))
            {
                var cValue = Convert.ToString(row[colName]);
                if (DateTime.TryParseExact(cValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dtValue))
                {
                    colValue = dtValue;
                }
            }
            return colValue;
        }

        /// <summary>
        /// Get bool column value from particular row after checking column existence.
        /// </summary>
        /// <param name="dt">The instance of DataTable from which columns need to check.</param>
        /// <param name="row">The current row of the table.</param>
        /// <param name="colName">The column name of the column.</param>
        /// <returns>Return value if found else empty string.</returns>
        public static bool? BoolColumnValue(this DataTable dt, DataRow row, string colName)
        {
            bool? colValue = null;
            if (dt.Columns.Contains(colName))
            {
                var cValue = Convert.ToString(row[colName]);
                if (bool.TryParse(cValue, out bool bValue))
                {
                    colValue = bValue;
                }
            }
            return colValue;
        }

        /// <summary>
        /// Get description of the enum.
        /// </summary>
        /// For more details click <see href="https://www.codementor.io/cerkit/giving-an-enum-a-string-value-using-the-description-attribute-6b4fwdle0">here</see>.
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                foreach (int val in Enum.GetValues(type))
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val) ?? throw new InvalidOperationException());
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }

        /// <summary>
        /// Get specific number of records from list based on Page number and Page size.
        /// </summary>
        /// <typeparam name="T">The type of list.</typeparam>
        /// <param name="listData">The list that need to be filtered.</param>
        /// <param name="pageNo">The page number.</param>
        /// <param name="pageSize">The number of records to be retrieved on page.</param>
        /// <returns>Return filtered list.</returns>
        public static List<T> GetPaging<T>(this List<T> listData, int pageNo, int? pageSize) where T : class
        {
            int iPageSize = pageSize != null ? pageSize.Value > 0 ? pageSize.Value : 10 : 10;
            var skip = (pageNo - 1) * iPageSize;
            return listData.Skip(skip).Take(iPageSize).ToList();
        }

        public static int GetDistributorId(this string userId, string role)
        {
            RepoUsers db = new RepoUsers();
            return db.getDistributorIdByUserId(userId, role);
        }
        public static int GetWorkshopId(this string userId, string role)
        {
            var ru = new RepoUsers();
            return ru.GetWorkshopIdByUserId(userId, role);
        }

        /// <summary>
        /// Get paged records based on page number and page size.
        /// </summary>
        /// <typeparam name="T">The class of type T.</typeparam>
        /// <param name="orderedData">The ordered data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The number of records to retrieve.</param>
        /// <returns>Return paged data.</returns>
        public static IQueryable<T> PagedData<T>(this IOrderedQueryable<T> orderedData, int pageNumber, int? pageSize = null) where T : class
        {
            // We need ordered data because The method 'Skip' is only supported for sorted input in LINQ to Entities. The method 'OrderBy' must be called before the method 'Skip'.

            int iPageSize = pageSize != null ? (pageSize.Value > 0 ? pageSize.Value : 10) : 10;
            var skip = (pageNumber - 1) * iPageSize;

            // For 'Skip' and 'Take' performance - https://visualstudiomagazine.com/articles/2016/12/06/skip-take-entity-framework-lambda.aspx
            return orderedData.Skip(() => skip).Take(() => iPageSize);
        }

        /// <summary>
        /// Get paged records based on page number and page size.
        /// </summary>
        /// <typeparam name="T">The class of type T.</typeparam>
        /// <param name="orderedData">The ordered data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The number of records to retrieve.</param>
        /// <returns>Return paged data.</returns>
        public static IEnumerable<T> PagedData<T>(this IOrderedEnumerable<T> orderedData, int pageNumber, int? pageSize = null) where T : class
        {
            int iPageSize = pageSize != null ? (pageSize.Value > 0 ? pageSize.Value : 10) : 10;
            var skip = (pageNumber - 1) * iPageSize;

            return orderedData.Skip(skip).Take(iPageSize);
        }

        /// <summary>
        /// Check if string value is a valid json.
        /// </summary>
        /// <param name="strInput">The string that need to be checked.</param>
        /// <returns>Return true if valid Json else false.</returns>
        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get email address of the current logged in user.
        /// </summary>
        /// <param name="identity">The identity of current user.</param>
        /// <returns>Return email address of current user.</returns>
        public static string GetEmailAddress(this IIdentity identity)
        {
            var userId = identity.GetUserId();
            using (var context = new garaazEntities())
            {
                var user = context.AspNetUsers.FirstOrDefault(u => u.Id == userId);
                return user?.Email;
            }
        }

        /// <summary>
        /// Set data table plugin filter.
        /// </summary>
        /// <param name="dbFilter">The dashboard filter.</param>
        /// <param name="request">The data table request holding properties.</param>
        /// <returns>Return updated dashboard filter.</returns>
        public static DashboardFilter SetDataTableFilter(this DashboardFilter dbFilter, JqDataTableRequest request)
        {
            var colIndex = request.Order?[0]?.Column ?? 0;

            dbFilter.PageSize = request.Length;
            dbFilter.Skip = request.Start;
            dbFilter.SearchTxt = request.Search.Value;
            dbFilter.SortBy = request.Columns[colIndex].Data;
            dbFilter.SortOrder = request.Order?[0]?.Dir;

            return dbFilter;
        }

        /// <summary>
        /// Get distinct records by specified field.
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Check if array of string contains the string value with case insensitive.
        /// </summary>       
        public static bool Contains(this string[] source, string toCheck, StringComparison comp)
        {
            foreach (var s in source)
            {
                if (s.IndexOf(toCheck, comp) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get sheet name from schema.
        /// </summary>        
        public static string GetSheetName(this OleDbConnection oleDbCon)
        {
            // Fetch sheet name
            var dtSchema = oleDbCon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            var sheetName = string.Empty;
            foreach (DataRow row in dtSchema.Rows)
            {
                sheetName = row.Field<string>("TABLE_NAME");
                if (sheetName.Contains("FilterDatabase"))
                {
                    sheetName = string.Empty;
                }
                else
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new KeyNotFoundException("Sheet name not found.");
            }

            return sheetName;
        }
    }
}