using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pure.api.Domain.Models.Shopping
{
    public class ShoppingItemDetailViewModel
    {
        public string LoginId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public uint Stock { get; set; }
        public string Description { get; set; }
    }
}
