using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public class OrderProcessingStatus
    {
        public enum Status
        {
            Success,
            NotExists,
            ErpProcessFailure,
            UpdateFailure,
            InformCustomerFailure
        }
        public Status OrderStatus { get; set; }
        public int OrderId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
