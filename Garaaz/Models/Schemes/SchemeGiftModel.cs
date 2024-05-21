using System.Collections.Generic;

namespace Garaaz.Models
{  
    public class SchemeGiftModel
    {
        public Scheme Scheme { get; set; }
        public List<GiftData> GiftDatas { get; set; }        

        public SchemeGiftModel()
        {
            GiftDatas = new List<GiftData>();            
        }
    }
}