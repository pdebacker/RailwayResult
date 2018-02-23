using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class Offer
    {
        public Offer()
        {
            Products = new List<Product>();
        }
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public List<Product> Products { get; set; }
        public decimal Discount { get; set; }
    }

}
