using System;
using FluentAssertions;
using Railway.Result;
using Xunit;

namespace RailwayResultTests.ResultTests
{
    public class SelectManyTests
    {
        [Fact]
        public void SelectMany_ChainSuccess2x_ExpectSuccess()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                select b;

            result.IsSuccess.Should().Be(true);
            result.ReturnValue.Should().Be(123);
        }

        [Fact]
        public void SelectMany_ChainSuccess3x_ExpectSuccess()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                from c in StringSuccess()
                select c;

            result.IsSuccess.Should().Be(true);
            result.ReturnValue.Should().Be("foo");
        }

        [Fact]
        public void SelectMany_ChainSuccessAndFailureFirst_ExpectFailure()
        {
            var result = from a in BoolFailure()
                from b in IntSuccess()
                from c in StringSuccess()
                select c;

            result.IsSuccess.Should().Be(false);
        }

        [Fact]
        public void SelectMany_ChainSuccessAndFailureLast_ExpectFailure()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                from c in StringFailure()
                select c;

            result.IsSuccess.Should().Be(false);
        }

        private Result<bool> BoolSuccess()
        {
            return Result<bool>.Succeeded();
        }

        private Result<bool> BoolFailure()
        {
            return Result<bool>.Failed(new Exception("error"));
        }

        private Result<int> IntSuccess()
        {
            return Result<int>.ToResult(123);
        }

        private Result<string> StringSuccess()
        {
            return Result<string>.ToResult("foo");
        }

        private Result<string> StringFailure()
        {
            return Result<string>.Failed(new Exception("error"));
        }
    }
}
