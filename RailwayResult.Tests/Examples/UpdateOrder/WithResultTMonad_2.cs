using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.Examples.UpdateOrder
{
    public class WithResultTMonad_2
    {
        public WithResultTMonad_2()
        {
            //var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");
            //simpleLogger.ClearLog();
            //ResultLogger.Logger = simpleLogger;
        }

        [Fact]
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
            Result<Product> productResult = GetProduct(productId);
            Result<Order> orderResult = GetOrder(orderId);

            var result = productResult
                .Ensure(orderResult)
                .OnSuccess(_ => GetCustomer(orderResult.ReturnValue.CustomerId))
                .OnSuccess(customerResult => AddProductToCustomerOrder(orderResult, customerResult, productResult))
                .ContinueIf(
                    r => r == OrderUpdateResult.OK,
                    _ => Repository.UpdateOrder(orderResult).ToResult()
                        .OnSuccess(s => OrderUpdateResult.OK)
                )
                .OnFailure(err => OrderUpdateResult.Error)
                .FinallyOrThrow();

            return result;
        }
        private Result<Product> GetProduct(int productId)
        {
            return Result<Product>.ToResult(() => Repository.GetProduct(productId));
        }

        private Result<Order> GetOrder(int orderId)
        {
            return Result<Order>.ToResult(() => Repository.GetOrder(orderId));
        }

        private Result<Customer> GetCustomer(int customerId)
        {
            return Result<Customer>.ToResult(() => Repository.GetCustomer(customerId));
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
