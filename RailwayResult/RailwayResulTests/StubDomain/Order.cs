using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class Order
    {
        public enum OrderStatus
        {
            New,
            Processed,
            Shipped
        }
        public Order()
        {
            Products = new List<Product>();
        }
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public List<Product> Products { get; set; }

        public Guid TrackingId { get; set; }
        public OrderStatus Status { get; set; }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }
        public decimal TotalOrderAmount()
        {
            decimal total = 0;
            foreach (var product in Products)
            {
                total += product.Price;
            }

            return total;
        }
    }
}
