using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class WsTransactionResponse
    {
        public int TransactionId { get; set; }
        public int WorkShopId { get; set; }
        public string Workshop { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string Sign { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}