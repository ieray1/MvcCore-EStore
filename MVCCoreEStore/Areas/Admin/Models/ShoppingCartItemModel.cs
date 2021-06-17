using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCoreEStore.Areas.Admin.Models
{
    public class ShoppingCartItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
