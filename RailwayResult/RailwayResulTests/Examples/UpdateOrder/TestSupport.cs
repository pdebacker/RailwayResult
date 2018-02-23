using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.Examples.UpdateOrder
{
    public enum OrderUpdateResult
    {
        OK,
        Error,
        ExceedLimit
    }
    public static class TestSupport
    {
        public static List<int> GetTestOrderIds()
        {
            var ordersToProcess = new List<int>();
            ordersToProcess.Add(Const.OrderId);
            ordersToProcess.Add(Const.NullOrderId);
            ordersToProcess.Add(Const.ExceptionOrderId);
            ordersToProcess.Add(Const.UpdateExceptionOrderId);
            ordersToProcess.Add(Const.OrderWithNullCustomerId);
            ordersToProcess.Add(Const.OrderWithExceptionCustomerId);
            ordersToProcess.Add(Const.OrderWithLowLimitCustomerCustomerId);
            return ordersToProcess;
        }
    }

   
}
