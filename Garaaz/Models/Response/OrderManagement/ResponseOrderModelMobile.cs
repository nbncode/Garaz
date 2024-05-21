using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class ResponseOrderModelMobile
    {
        public List<ResponseOrderModel> PreviousOrders {get;set;}
        public List<ResponseOrderModel> CurrentOrders {get;set;}
    }
}