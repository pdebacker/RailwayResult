using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.ResultTests
{
    public class NullResultTests
    {
        [Fact]
        public void WhenConstructedWithNull_ThenExpectFailure()
        {
            var result = Result<string>.ToResult((string)null);
            result.IsFailure.Should().Be(true);
            result.IsNull.Should().Be(true);
        }

        [Fact]
        public void GivenNullFailure_WhenNullDelegate_ThenExpectExecution()
        {
            Result<string> result = 
                Result<string>.ToResult((string)null)
                .OnNull(r => Result<string>.ToResult("bar"));

            result.ReturnValue.Should().Be("bar");
        }

        [Fact]
        public void GivenNotNullFailure_WhenNullDelegate_ThenNotExpectExecution()
        {
            Result<string> result =
                Result<string>.ToResult("foo")
                .OnNull(r => Result<string>.ToResult("bar"));
            result.ReturnValue.Should().Be("foo");
        }

        [Fact]
        public void GivenNotCustomer_WhenNullDelegate_ExpectNewCustomer()
        {
            Result<Customer> result =
                Result<Customer>.ToResult(Repository.GetCustomer(Const.NullCustomerId))
                    .OnNull(r => new Customer(){Name = "Bar"});
            result.ReturnValue.Name.Should().Be("Bar");
        }

        [Fact]
        public void GivenCustomerException_WhenNullDelegate_ExpectFailure()
        {
            Result<Customer> result =
                Result<Customer>.ToResult(() => Repository.GetCustomer(Const.ExceptionCustomerId))
                .OnNull(r => new Customer() { Name = "Bar" });

            result.IsFailure.Should().Be(true);
            result.IsException.Should().Be(true);
            result.ReturnValue.Should().BeNull();
        }

        [Fact]
        public void GivenCustomerResult_WhenNullDelegate_ExpectSkipDelegate()
        {
            Result<Customer> result =
                Result<Customer>.ToResult(() => Repository.GetCustomer(Const.CustomerId))
                    .OnNull(r => new Customer() { Name = "Bar" });

            result.ReturnValue.Name.Should().Be("Foo");
        }

      

    }


}
