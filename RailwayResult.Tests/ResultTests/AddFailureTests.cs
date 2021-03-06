﻿using System;
using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.ResultTests
{
    public class AddFailureTests
    {
        [Fact]
        public void GivenSuccessResult_WhenAddFailure_ThenFailureResultHasOne()
        {
            Result<Customer> result = Repository.GetCustomer(Const.CustomerId).ToResult();
            result.AddFailure(-1, "error");

            result.IsFailure.Should().BeTrue();
            result.FailureInfo.Errors.Count.Should().Be(1);
            result.FailureInfo.Code.Should().Be(-1);
            result.FailureInfo.Message.Should().Be("error");
        }

        [Fact]
        public void GivenFailureResult_WhenAddFailure_ThenExpectTwoErrors()
        {
            Result<Customer> result = Repository.GetCustomer(Const.CustomerId).ToResult();
            result.AddFailure(-1, "error1");
            result.IsFailure.Should().BeTrue();


            result.AddFailure(-2, "error2");
            result.FailureInfo.Errors.Count.Should().Be(2);
            result.FailureInfo.Code.Should().Be(-1);             // return first Error
            result.FailureInfo.Message.Should().Be("error1");

            result.FailureInfo.Errors[1].Code.Should().Be(-2);
            result.FailureInfo.Errors[1].Message.Should().Be("error2");
        }
    }
}
