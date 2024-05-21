using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Garaaz.Models
{
    public class DataTableRequest
    {
        public int draw { get; set; }
        public string column { get; set; }
        public string dir { get; set; }
        public string search { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public int UserId { get; set; }
        public string DomainName { get; set; }
    }
}