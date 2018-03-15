using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Railway.Result2;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.Result2Tests
{
    public enum OrderUpdateResult
    {
        OK,
        Error,
        ExceedLimit
    }

    public class Result2Tests
    {
        [Fact]
        public void Result2_OnSuccessBindTest()
        {
           var result = Result2<Order, string>.Succeeded(new Order());
           var result2 = result.OnSuccess(order => Result2<Customer, string>.Succeeded(new Customer()));

            result2.IsSuccess.Should().BeTrue();
            result2.SuccessResult.GetType().Should().Be(typeof(Customer));
        }

        [Fact]
        public void Result2_OnSuccessMapTest()
        {
            var result = Result2<Order, string>.Succeeded(new Order());
            var result2 = result.OnSuccess(order => new Customer());

            result2.IsSuccess.Should().BeTrue();
            result2.SuccessResult.GetType().Should().Be(typeof(Customer));
        }

        [Fact]
        public void Result2_OnSuccessActionTest()
        {
            Order orderResult = null;
            var result = Result2<Order, string>.Succeeded(new Order(){Id=123});
            var result2= result.OnSuccess(order => { orderResult = order;});

            result2.IsSuccess.Should().BeTrue();
            result2.SuccessResult.GetType().Should().Be(typeof(Order));
            orderResult.Id.Should().Be(123);
        }

        [Fact]
        public void Result2_OnFailureActionTest()
        {
            string output = null;
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.OnFailure(e => { output = e.ToUpper(); });

            result2.IsFailure.Should().BeTrue();
            output.Should().Be("ERROR");
        }

        [Fact]
        public void Result2_GivenOnSuccessBind_WhenIsFailure_ExpectNoOperation()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.OnSuccess(order => Result2<Customer, string>.Succeeded(new Customer()));

            result2.IsSuccess.Should().BeFalse();
            result2.SuccessResult.Should().BeNull();
            result2.FailureResult.Should().Be("error");
        }

        [Fact]
        public void Result2_GivenOnSuccesMap_WhenIsFailure_ExpectNoOperation()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.OnSuccess(order => new Customer());

            result2.IsSuccess.Should().BeFalse();
            result2.SuccessResult.Should().BeNull();
            result2.FailureResult.Should().Be("error");
        }

        [Fact]
        public void Result2_GivenAFailure_WhenOnFailureMap_ExpectCorrectedSuccess()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.OnFailure(e => new Order());

            result2.IsSuccess.Should().BeTrue();
            result2.SuccessResult.GetType().Should().Be(typeof(Order));
            result2.FailureResult.Should().BeNull();
        }

        [Fact]
        public void Result2_GivenAFailure_WhenOnFailureBind_ExpectCorrectedSuccess()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.OnFailure(e => Result2<Order, string>.Succeeded(new Order()));

            result2.IsSuccess.Should().BeTrue();
            result2.SuccessResult.GetType().Should().Be(typeof(Order));
            result2.FailureResult.Should().BeNull();
        }

        [Fact]
        public void Result2_GivenAFailure_WhenOnFailureBind_MapToOtherFailure()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.ConvertFailure(e => e.Length);

            result2.IsFailure.Should().BeTrue();
            result2.FailureResult.Should().Be(5);
            result2.SuccessResult.Should().BeNull();
        }

        [Fact]
        public void Result2_GivenAFailure_WhenOnFailureBind_BindToOtherFailure()
        {
            var result = Result2<Order, string>.Failed("error");
            var result2 = result.ConvertFailure(e => Result2<Order,int>.Failed(e.Length));

            result2.IsFailure.Should().BeTrue();
            result2.FailureResult.Should().Be(5);
            result2.SuccessResult.Should().BeNull();
        }

        [Fact]
        public void Result2_GivenTwoDifferentFailureType_ConvertToOtherFailure()
        {
            // The code below will not return a Result2<Customer, int> on failure
            // var result =
            //    Result2<Order, string>.Failed("error")
            //        .OnSuccess(o => Result2<Customer, int>.Succeeded(new Customer()));            
            //
            // result.FailureResult.Should().Be(-1) 
            // will not compile unless a ConvertFailure is added to the chain.
            //
            var result =
                Result2<Order, string>.Failed("error")
                    .ConvertFailure( e => Result2<Order, int>.Failed(-1)) // bind
                    .OnSuccess(o => Result2<Customer, int>.Succeeded(new Customer()));

            result.IsFailure.Should().BeTrue();
            result.FailureResult.Should().Be(-1);
        }


        [Fact]
        public void Result2_GivenTwoDifferentFailureType_MapToOtherFailure()
        {
            // The code below will not return a Result2<Customer, int> on failure
            // var result =
            //    Result2<Order, string>.Failed("error")
            //        .OnSuccess(o => Result2<Customer, int>.Succeeded(new Customer()));            
            //
            // result.FailureResult.Should().Be(-1) 
            // will not compile unless a ConvertFailure is added to the chain.
            //

            var result =
                Result2<Order, string>.Failed("error")
                    .ConvertFailure(e => -e.Length)     // map
                    .OnSuccess(o => Result2<Customer, int>.Succeeded(new Customer()));

            result.IsFailure.Should().BeTrue();
            result.FailureResult.Should().Be(-5);
        }


        [Fact]
        public void UpdateOrderExampleTest()
        {
            var ordersToProcess = GetTestOrderIds();

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

        public OrderUpdateResult UpdateOrder(int orderId, int productId)
        {
            Order order = null;
            Customer customer = null;
    
            var result2 = GetOrderResult(orderId)
                .OnSuccess(o => order = o)
                .OnSuccess(o => GetCustomerResult(o.CustomerId))
                .OnSuccess(c => customer = c)
                .OnSuccess(c => GetProductResult(productId))
                .OnSuccess(p => AddProductToCustomerOrder(order, customer, p))
                .If(a => a == OrderUpdateResult.OK, 
                         a => UpdateModifiedOrder(order), 
                         a => ToOrderUpdateResult(a)
                    );

            var diagnostics = result2.FailureResult;

            if (result2.IsSuccess)
                return result2.SuccessResult;

            return OrderUpdateResult.Error;
        }

        private Result2<OrderUpdateResult, string[]> ToOrderUpdateResult(OrderUpdateResult result)
        {
            return Result2<OrderUpdateResult, string[]>.Succeeded(result);
        }

        private Result2<OrderUpdateResult, string[]> UpdateModifiedOrder(Order order)
        {
            try
            {
                if (Repository.UpdateOrder(order))
                    return Result2<OrderUpdateResult, string[]>.Succeeded(OrderUpdateResult.OK);

                return Result2<OrderUpdateResult, string[]>.Failed(new[] { "Failed to Update Order" });

            }
            catch (Exception e)
            {
                return Result2<OrderUpdateResult, string[]>.Failed(new[] { e.Message });
            }
        }

        // eigenlijk zou ik hier Result2<OrderUpdateResult,OrderUpdateResult> willen teruggeven.
        // maar dat kan niet omdat een failure string[] niet geconverteerd kan worden naar een OrderUpdateResult
        private Result2<OrderUpdateResult, string[]> AddProductToCustomerOrder(
            Order order,
            Customer customer,
            Product product)
        {
            if ((order.TotalOrderAmount() + product.Price) > customer.OrderLimit)
                return Result2<OrderUpdateResult, string[]>.Succeeded(OrderUpdateResult.ExceedLimit);

            order.AddProduct(product);
            return Result2<OrderUpdateResult, string[]>.Succeeded(OrderUpdateResult.OK);
        }

        private Result2<Order, string[]> GetOrderResult(int orderId)
        {
            try
            {
                Order order = Repository.GetOrder(orderId);
                if (order != null)
                    return Result2<Order, string[]>.Succeeded(order);

                return Result2<Order, string[]>.Failed(new [] {$"Order with id {orderId} does not exist."});
            }
            catch (Exception ex)
            {
                return Result2<Order, string[]>.Failed(new[] { ex.Message });
            }
        }

        private Result2<Customer, string[]> GetCustomerResult(int customerId)
        {
            try
            {
                return Result2<Customer, string[]>.Succeeded(Repository.GetCustomer(customerId));

            }
            catch (Exception ex)
            {
                return Result2<Customer, string[]>.Failed(new[] { ex.Message });
            }
        }

        private Result2<Product, string[]> GetProductResult(int productId)
        {
            try
            {
                return Result2<Product, string[]>.Succeeded(Repository.GetProduct(productId));

            }
            catch (Exception ex)
            {
                return Result2<Product, string[]>.Failed(new[] { ex.Message });
            }
        }

        public List<int> GetTestOrderIds()
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
