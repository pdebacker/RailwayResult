using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.Examples.UpdateOrder
{
    [TestClass]
    public class WithResultTMonad_2
    {
        [TestInitialize]
        public void TestSetup()
        {
            //var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");
            //simpleLogger.ClearLog();
            //ResultLogger.Logger = simpleLogger;
        }

        [TestMethod]
        public void AddProductToCustomerOrder_2()
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
            Product product = null;
            Order order = null;
            Customer customer = null;

            return Result<Product>.ToResult(() => product = Repository.GetProduct(productId))
                .OnSuccess(p => order = Repository.GetOrder(orderId))
                .OnSuccess(o => customer = Repository.GetCustomer(order.CustomerId))
                .OnSuccess(c => AddProductToCustomerOrder(order, customer, product))
                .ContinueIf( 
                        r => r == OrderUpdateResult.OK,
                        _ => Repository.UpdateOrder(order).ToResult().OnSuccess(s => OrderUpdateResult.OK)
                    )
                .OnFailure(err => OrderUpdateResult.Error)
                .FinallyOrThrow();
        }

        private Result<OrderUpdateResult> AddProductToCustomerOrder(
            Order order,
            Customer customer,
            Product product)
        {
            if ((order.TotalOrderAmount() + product.Price) > customer.OrderLimit)
                return OrderUpdateResult.ExceedLimit.ToResult();

            order.AddProduct(product);
            return OrderUpdateResult.OK.ToResult();
        }
    }
}
