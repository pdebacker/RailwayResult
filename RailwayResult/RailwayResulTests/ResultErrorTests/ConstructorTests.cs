using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RailwayResultTests.Examples;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.ResultFailureTests
{
    [TestClass]
    public class ConstructorTests
    {

        [TestMethod]
        public void ConstructResultErrorWithType_ExpectIsNull()
        {
            var resultError = new ResultFailure(typeof(Customer));

            resultError.IsNull.Should().BeTrue();
            resultError.Errors.Count.Should().Be(0);
            resultError.Code.Should().Be(0);
            resultError.Message.Should().BeNull();
            resultError.Ex.Should().BeNull();
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructEmptyResultError_ExpectIsFalse()
        {
            var resultError = new ResultFailure();

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(0);
            resultError.Code.Should().Be(0);
            resultError.Message.Should().BeNull();
            resultError.Ex.Should().BeNull();
            resultError.StackTrace.Should().NotBeNull();
        }
        [TestMethod]
        public void ConstructResultErrorWithNothing_ExpectIsFalse() 
        {
            var resultError = new ResultFailure();

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(0);
            resultError.Code.Should().Be(0);
            resultError.Message.Should().BeNull();
            resultError.Ex.Should().BeNull();
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructResultErrorWithException()
        {
            var ex = new ApplicationException("test");
            var resultError = new ResultFailure(typeof(bool), ex);

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(1);
            resultError.Code.Should().Be(ex.HResult);
            resultError.Message.Should().Be("test");
            resultError.Ex.Should().Be(ex);
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructResultErrorWithExceptionMessage()
        {
            var ex = new ApplicationException("test");
            var resultError = new ResultFailure(typeof(bool), ex, "Foo");

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(1);
            resultError.Code.Should().Be(ex.HResult);
            resultError.Message.Should().Be("Foo");
            resultError.Ex.Should().Be(ex);
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructResultErrorWithExceptionMessageAndObjectInstance()
        {
            var customer = new Customer() {Id = 123, Name = "FooBar", EmailAddress = "foo@bar.com"};
            var ex = new ApplicationException("test");
            var resultError = new ResultFailure(typeof(Customer), ex, "Foo", customer);

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(1);
            resultError.Code.Should().Be(ex.HResult);
            resultError.Message.Should().Be("Foo");
            resultError.Ex.Should().Be(ex);
            resultError.Object.Should().Be(customer);
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructResultErrorWithCodeAndMessage()
        {
            var resultError = new ResultFailure(typeof(bool), 123, "Foo");

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(1);
            resultError.Code.Should().Be(123);
            resultError.Message.Should().Be("Foo");
            resultError.Ex.Should().BeNull();
            resultError.StackTrace.Should().NotBeNull();
        }

        [TestMethod]
        public void ConstructResultErrorWithCodeMessageAndObjectInstance()
        {
            var customer = new Customer() {Id = 123, Name = "FooBar", EmailAddress = "foo@bar.com"};
            var resultError = new ResultFailure(typeof(Customer), 123, "Foo", customer);

            resultError.IsNull.Should().BeFalse();
            resultError.Errors.Count.Should().Be(1);
            resultError.Code.Should().Be(123);
            resultError.Message.Should().Be("Foo");
            resultError.Ex.Should().BeNull();
            resultError.Object.Should().Be(customer);
            resultError.StackTrace.Should().NotBeNull();
        }
    }
}
