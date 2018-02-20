﻿using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RailwayResultTests.Examples;
using Railway.Result;

namespace RailwayResultTests.ResultTests
{
    [TestClass]
    public class ExceptionAndFailureTests
    {
        [TestMethod]
        public void WhenReturnTypeIsNotBool_CannotUseFailure()
        {
            // This will not compile:
            //   Result<Order> result = Result<Order>.Failure();
            //
            // A failure of Type TReturn is not acceptable without an Error code or Message
            //
            // Same for Success:
            //   Result<Order> result = Result<Order>.Success();
            //
            // A success result without an actual value of type TReturn is not possible. 
            // And a Null is not a valid result.
        }

        [TestMethod]
        public void WhenReturnTypeIsNotBool_ExcpectANullValueToBeAFailure()
        {
            // And a Null is considered a failure result:
            Result<Order> result = Result<Order>.ToResult(default(Order));

            result.IsFailure.Should().BeTrue();

            // a Null value is also a special failure case:
            result.IsNull.Should().BeTrue();
            result.ReturnValue.Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void WhenMethodThrows_ExpectException()
        {
            var result = Result<Order>.ToResult(ThrowsExceptionDirectly());

            // To catch the exception and retun a failure result use a lamda function: 
            // Result<Order>.ToResult( () => ThrowsExceptionDirectly());
        }

        [TestMethod]
        public void WhenFuncThrows_ExpectFailureResultWithException()
        {
            // To catch the exception and retun a failure result use a lamda function: 
            var result = Result<Order>.ToResult(() => ThrowsExceptionDirectly());

            string diagnostics = result.ToString(); // contains detailed failure message

            result.IsException.Should().BeTrue();
            result.FailureInfo.Ex.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenIndirectException_ExpectTraceWithFailureLocation()
        {
            var result = Result<Order>.ToResult(ThrowsExceptionIndirectly);

            string diagnostics = result.ToString(); // contains detailed failure message

            result.IsException.Should().BeTrue();
            result.FailureInfo.Ex.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenFailure_ExpectTraceWithFailureLocation()
        {
            var result = Result<Order>.ToResult(() => ReturnsNullOrderResult());

            string diagnostics = result.ToString(); // contains detailed failure message

            result.IsException.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void WhenOrderFailure_ExpectFailedResult()
        {
            var result = Result<Order>.ToResult(ReturnsFailedOrderResult());

            result.IsException.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.FailureInfo.ReturnType.Should().Be(typeof(Order));
        }


        [TestMethod]
        public void WhenExceptionIsThrowed_ExcecuteOnExceptionThatReturnsTypeT()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnException(error => Repository.GetOrder(Const.OrderId));

            result.IsException.Should().Be(false);
            result.ReturnValue.GetType().Should().Be(typeof(Order));
            result.ReturnValue.Id.Should().Be(Const.OrderId);
        }

        [TestMethod]
        public void WhenExceptionIsThrowed_ExcecuteOnExceptionThatReturnsTypeResultT()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnException(error => Repository.GetOrder(Const.OrderId).ToResult());

            result.IsException.Should().Be(false);
            result.ReturnValue.GetType().Should().Be(typeof(Order));
            result.ReturnValue.Id.Should().Be(Const.OrderId);
        }

        [TestMethod]
        public void WhenExceptionIsThrowed_ExcecuteOnExceptionAction()
        {
            bool actionIsExcuted = false;
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnException(error => actionIsExcuted = true);

            result.IsException.Should().Be(true);
            actionIsExcuted.Should().Be(true);
        }

        [TestMethod]
        public void GivenExceptionIsThrowed_WhenExceptionIsRepositoryException_ExcecuteOnException()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnException<Repository.RepositoryException, Order>(error => Repository.GetOrder(Const.OrderId));

            result.IsException.Should().Be(false);
            result.ReturnValue.GetType().Should().Be(typeof(Order));
            result.ReturnValue.Id.Should().Be(Const.OrderId);
        }

        [TestMethod]
        public void GivenExceptionIsThrowed_WhenExceptionIsNotRepositoryException_NotExcecuteOnException()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnException<NotSupportedException, Order>(error => Repository.GetOrder(Const.OrderId));

            result.IsException.Should().Be(true);
            result.ReturnValue.Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(Repository.RepositoryException))]
        public void GivenExceptionIsThrowed_WhenThrowFinally_ExpectException()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnSuccess(order => Repository.GetCustomer(Const.CustomerId)) // this is not executed
                .ThrowOnException();
        }

        [TestMethod]
        [ExpectedException(typeof(ResultException))]
        public void GivenAFailure_WhenThrowFinally_ExpectException()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.ExceptionOrderId))
                .OnSuccess(order => Repository.GetCustomer(Const.CustomerId)) // this is not executed
                .ThrowOnFailure();
        }

        [TestMethod]
        public void GivenASuccess_WhenThrowFinally_NotExpectException()
        {
            var result = Result<Order>.ToResult(() => Repository.GetOrder(Const.OrderId))
                .OnSuccess(order => Repository.GetCustomer(Const.CustomerId)) 
                .ThrowOnFailure();

            result.IsSuccess.Should().Be(true);
        }

        private Order ThrowsExceptionDirectly()
        {
            throw new ApplicationException("exception test message");
        }

        private Order ThrowsExceptionIndirectly()
        {
            var order = Repository.GetOrder(Const.ExceptionOrderId);
            return order;
        }

        private Order ReturnsNullOrderResult()
        {
            return Repository.GetOrder(Const.NullOrderId);
        }

        private Result<Order> ReturnsFailedOrderResult()
        {
            return Result<Order>.Failed(-1);
        }
    }
}
