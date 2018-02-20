using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Railway.Result2;

namespace RailwayResultTests.Result2Tests
{
    [TestClass]
    public class SelectManyTests
    {
        [TestMethod]
        public void Result2_SelectMany_ChainSuccess2x_ExpectSuccess()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                select b;

            result.IsSuccess.Should().Be(true);
            result.SuccessResult.Should().Be(123);
        }

        [TestMethod]
        public void Result2_SelectMany_ChainSuccess3x_ExpectSuccess()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                from c in StringSuccess()
                select c;

            result.IsSuccess.Should().Be(true);
            result.SuccessResult.Should().Be("foo");
        }

        [TestMethod]
        public void Result2_SelectMany_ChainSuccessAndFailureFirst_ExpectFailure()
        {
            var result = from a in BoolFailure()
                from b in IntSuccess()
                from c in StringSuccess()
                select c;

            result.IsSuccess.Should().Be(false);
        }

        [TestMethod]
        public void Result2_SelectMany_ChainSuccessAndFailureLast_ExpectFailure()
        {
            var result = from a in BoolSuccess()
                from b in IntSuccess()
                from c in StringFailure()
                select c;

            result.IsSuccess.Should().Be(false);
        }

        private Result2<bool, string[]> BoolSuccess()
        {
            return Result2<bool, string[]>.Succeeded(true);
        }

        private Result2<bool, string[]> BoolFailure()
        {
            return Result2<bool, string[]>.Failed(new []{"error"});
        }

        private Result2<int, string[]> IntSuccess()
        {
            return Result2<int, string[]>.Succeeded(123);
        }

        private Result2<string, string[]> StringSuccess()
        {
            return Result2<string, string[]>.Succeeded("foo");
        }

        private Result2<string, string[]> StringFailure()
        {
            return Result2<string, string[]>.Failed(new[] { "error" });
        }
        
    }
}
