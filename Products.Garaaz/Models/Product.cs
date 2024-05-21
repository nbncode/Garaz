//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Products.Garaaz.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        public int ProductId { get; set; }
        public Nullable<int> DistributorId { get; set; }
        public Nullable<int> GroupId { get; set; }
        public string ProductName { get; set; }
        public string PartNo { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string ImagePath { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ProductType { get; set; }
        public string BinLocation { get; set; }
        public Nullable<decimal> PurchasePrice { get; set; }
        public Nullable<decimal> LatestLandedCost { get; set; }
        public Nullable<decimal> TaxPaidSellingPrice { get; set; }
        public Nullable<decimal> TaxableSellingPrice { get; set; }
        public Nullable<int> CurrentStock { get; set; }
        public Nullable<decimal> InventoryValue { get; set; }
        public string Abc { get; set; }
        public string Fms { get; set; }
        public string Xyz { get; set; }
        public string MovementCode { get; set; }
        public Nullable<int> PackQuantity { get; set; }
        public Nullable<int> Margin { get; set; }
        public Nullable<int> SequenceNo { get; set; }
        public string IssueIndicator { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }
        public string ModelsApplicable { get; set; }
        public string SalesTaxCategory { get; set; }
        public string TaxDesc { get; set; }
        public Nullable<int> OnOrderQtyMul { get; set; }
        public Nullable<int> InTransitQty { get; set; }
        public Nullable<int> AllocQty { get; set; }
        public Nullable<int> FloatStock { get; set; }
        public string MinimumLevel { get; set; }
        public string MaximumLevel { get; set; }
        public string ReorderLevel { get; set; }
        public Nullable<int> Last12MonthAvgConsumption { get; set; }
        public string ReservationQty { get; set; }
        public string SafetyStock { get; set; }
        public string SeasonalPartYn { get; set; }
        public string DeadStockYn { get; set; }
        public string ReasonToEditInPo { get; set; }
        public string VorPartYn { get; set; }
        public Nullable<int> HsCode { get; set; }
        public string QuarantineQty { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string TaxValue { get; set; }
        public string RootPartNum { get; set; }
        public string PartCategoryCode { get; set; }
    
        public virtual Product Product1 { get; set; }
        public virtual Product Product2 { get; set; }
    }
}
