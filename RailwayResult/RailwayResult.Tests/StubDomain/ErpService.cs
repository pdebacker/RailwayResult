using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class ErpService
    {
        public enum ShippingStatus
        {
            Success,
            NotDeliverable,
        }
        public static ShippingInfo ProcessOrder(Order order)
        {
            var info = new ShippingInfo
            {
                Status = ShippingStatus.Success,
                OrderId = order.Id,
                TrackingCode = Guid.NewGuid(),
                EstimatedShippingDate = DateTime.Today.AddDays(1)
            };

            switch (order.Id)
            {
                case Const.OrderWithErpProcessException:
                    throw new ErpException("Some unknown ERP exception");

                case Const.OrderWithErpProcessFailure:
                    info.Status = ShippingStatus.NotDeliverable;
                    break;
            }

            return info;
        }
        public class ShippingInfo
        {
            public ShippingStatus Status { get; set; }
            public int OrderId { get; set; }
            public Guid TrackingCode { get; set; }
            public DateTime EstimatedShippingDate { get; set; }
        }

        public class ErpException : Exception
        {
            public ErpException(string message) : base(message)
            {
            }
        }
    }
}
