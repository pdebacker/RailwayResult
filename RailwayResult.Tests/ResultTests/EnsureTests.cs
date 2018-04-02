using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.ResultTests
{
    public class EnsureTests
    {
        [Fact]
        public void GivenValue_WhenEnsure_ThenExpectSuccess()
        {
            Customer customer = Repository.GetCustomer(Const.CustomerId);

            var result = customer.Ensure();

            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenNullValue_WhenEnsure_ThenExpectFailure()
        {
            Customer customer = Repository.GetCustomer(Const.NullCustomerId);

            var result = customer.Ensure();

            result.IsSuccess.Should().Be(false);
        }


        [Fact]
        public void GivenSuccessResult_WhenPredicateSucceeds_ThenExpectSuccess()
        {
            Result<bool> ok = Result<bool>.FromBool(true);

            var result = ok.Ensure(value => value == true);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenSuccessResult_WhenPredicateFails_ThenExpectFailure()
        {
            Result<bool> ok = Result<bool>.FromBool(true);

            var result = ok.Ensure(value => value == false);
            result.IsFailure.Should().Be(true);
        }


        [Fact]
        public void GivenFailureResult_WhenPredicateSucceeds_ThenExpectFailure()
        {
            Result<bool> ok = Result<bool>.FromBool(false);

            var result = ok.Ensure(value => value == false);
            result.IsFailure.Should().Be(true);
        }

        //
        // tests with Result<bool> predicate
        //

        [Fact]
        public void GivenSuccessResult_WhenResultPredicateSucceeds_ThenExpectSuccess()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> predicate = Result<bool>.FromBool(true);

            var result = ok.Ensure(value => predicate);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenSuccessResult_WhenResultPredicateFails_ThenExpectFailure()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> predicate = Result<bool>.FromBool(false);

            var result = ok.Ensure(value => predicate);
            result.IsFailure.Should().Be(true);
        }


        [Fact]
        public void GivenFailureResult_WhenResultPredicateSucceeds_ThenExpectFailure()
        {
            Result<bool> ok = Result<bool>.FromBool(false);
            Result<bool> predicate = Result<bool>.FromBool(true);

            var result = ok.Ensure(value => predicate);
            result.IsFailure.Should().Be(true);
        }

        //
        // tests with Result<T> combinations
        //

        [Fact]
        public void GivenSuccessResult_When1ResultPredicatesSucceeds_ThenExpectSuccess()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> result1 = Result<bool>.FromBool(true);

            var result = ok.Ensure(result1);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenSuccessResult_When2ResultPredicatesSucceeds_ThenExpectSuccess()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> result1 = Result<bool>.FromBool(true);
            Result<bool> result2 = Result<bool>.FromBool(true);

            var result = ok.Ensure(result1, result2);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenSuccessResult_When3ResultPredicatesSucceeds_ThenExpectSuccess()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> result1 = Result<bool>.FromBool(true);
            Result<bool> result2 = Result<bool>.FromBool(true);
            Result<bool> result3 = Result<bool>.FromBool(true);

            var result = ok.Ensure(result1, result2, result3);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void GivenSuccessResult_WhenAnyResultPredicatesFails_ThenExpectFailure()
        {
            Result<bool> ok = Result<bool>.FromBool(true);
            Result<bool> result1 = Result<bool>.FromBool(true);
            Result<bool> result2 = Result<bool>.FromBool(false);
            Result<bool> result3 = Result<bool>.FromBool(true);

            var result = ok.Ensure(result1, result2, result3);
            result.IsFailure.Should().Be(true);
        }
    }
}
