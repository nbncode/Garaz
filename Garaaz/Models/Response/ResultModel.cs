using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResultModel
    {
        public int ResultFlag { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}