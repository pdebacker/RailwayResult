using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RailwayResultTests.Examples.UpdateOrder
{
    [TestClass]
    public class WithoutResultTMonad_1
    {
        [TestInitialize]
        public void TestSetup()
        {
            //var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");   
            //simpleLogger.ClearLog();
            //ResultLogger.Logger = simpleLogger;
        }

        [TestMethod]
        public void AddProductToCustomerOrder_1()
        {
            var ordersToProcess = TestSupport.GetTestOrderIds();

            var returnValues = new List<OrderUpdateResult>();

            foreach (var orderId in ordersToProcess)
            {
                OrderUpdateResult result = UpdateOrder(orderId, Const.ProductId);
                returnValues.Add(result);
            }

            returnValues.Count(r => r == OrderUpdateResult.OK).Should().Be(1);
            returnValues.Count(r => r == OrderUpdateResult.Error).Should().Be(5);
            returnValues.Count(r => r == OrderUpdateResult.ExceedLimit).Should().Be(1);
        }


        // Add a product to a customer order. 
        // There is a business rule that the total order amount cannot exceed a customer order limit.
        // The method should return the following return information: OK, LimitExceeded, Error
        // so that the user can be informed properly. 
        // This test will run a series or test order and asserts the outcome.
        public OrderUpdateResult UpdateOrder(int orderId, int productId)
        {
            try
            {
                Order order = Repository.GetOrder(orderId);
                Product product = Repository.GetProduct(productId);

                if (order == null || product == null)
                    return OrderUpdateResult.Error;

                Customer customer = Repository.GetCustomer(order.CustomerId);
                if (customer == null)
                    return OrderUpdateResult.Error;

                if ((order.TotalOrderAmount() + product.Price) > customer.OrderLimit)
                    return OrderUpdateResult.ExceedLimit;

                order.AddProduct(product);
                Repository.UpdateOrder(order);
                return OrderUpdateResult.OK;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message);
            }
            return OrderUpdateResult.Error;
        }


    }
}
