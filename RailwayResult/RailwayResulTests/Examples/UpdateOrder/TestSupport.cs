using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Railway.Result;

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

    public class SimpleLogger : IResultFailureLogger
    {
        private static object _lock = new object();
        private readonly string _fileName;

        public SimpleLogger(string logFileName)
        {
            _fileName = logFileName;
        }

        public void ClearLog()
        {
            if (System.IO.File.Exists(_fileName))
                System.IO.File.Delete(_fileName);
        }
        public void LogFailure(ResultFailure failureInfo)
        {
            lock (_lock)
            {
                System.IO.File.AppendAllText(_fileName, failureInfo.ToString());
            }
        }
    }
}
