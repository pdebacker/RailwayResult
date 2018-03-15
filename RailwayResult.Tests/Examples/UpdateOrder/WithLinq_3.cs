using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.Examples.UpdateOrder
{
    public class WithLinq_3
    {

        public WithLinq_3()
        {
            //var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");
            //simpleLogger.ClearLog();
            //ResultLogger.Logger = simpleLogger;
        }

        [Fact]
        public void AddProductToCustomerOrder_3()
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

            var result =
                from product in GetProductResult(productId)
                from order in GetOrderResult(orderId)
                from customer in GetCustomerResult(order.CustomerId)
                from added in AddProductToCustomerOrderWithLimitCheck(order, customer, product)
                from ok in UpdateCustomerOrderOnSuccess(added, order)
                select ok;

            return result.OnFailure(err => OrderUpdateResult.Error)
                         .FinallyOrThrow();
        }

        private Result<Product> GetProductResult(int productId)
        {
            return Result<Product>.ToResult(() => Repository.GetProduct(productId));
        }

        private Result<Order> GetOrderResult(int orderId)
        {
            return Result<Order>.ToResult(() => Repository.GetOrder(orderId));
        }

        private Result<Customer> GetCustomerResult(int customerId)
        {
            return Result<Customer>.ToResult(() => Repository.GetCustomer(customerId));
        }
        private Result<OrderUpdateResult> AddProductToCustomerOrderWithLimitCheck(
            Order order,
            Customer customer,
            Product product)
        {
            if ((order.TotalOrderAmount() + product.Price) > customer.OrderLimit)
                return OrderUpdateResult.ExceedLimit.ToResult();

            order.AddProduct(product);
            return OrderUpdateResult.OK.ToResult();
        }

        private Result<OrderUpdateResult> UpdateCustomerOrderOnSuccess(OrderUpdateResult result, Order order)
        {
            if (result == OrderUpdateResult.OK)
                return Result<bool>.ToResult(
                        () => Repository.UpdateOrder(order))
                       .OnSuccess(s => OrderUpdateResult.OK);

            return result.ToResult();
        }

    }
}
