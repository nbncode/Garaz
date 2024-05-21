using System;

namespace Garaaz.Models
{
    public class AccountEntry
    {
        public string Particulars { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public DateTime Date { get; set; }
    }
}