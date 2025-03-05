using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.Entidades
{
    public class Promotion
    {
        public int? promotionBrandId { get; set; }
        public int? promotionId { get; set; }
        public string description { get; set; }
        public int? isFeatured { get; set; }
        public string restrictions { get; set; }
        public string validityStart { get; set; }
        public string validityEnd { get; set; }
        public string promotionKey { get; set; }
        public int? bannerOrder { get; set; }
        public int? promoHomeOrder { get; set; }
        public string title { get; set; }
        public string discountType { get; set; }
        public string cuponTypeKey { get; set; }
        public int? isFavorite { get; set; }
        public int? orderPromotion { get; set; }
        public int? serverOrder { get; set; }
    }
}
