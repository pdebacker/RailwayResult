using System;
using RailwayResultTests.Examples;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.ResultTests
{
    public class FinallyTests
    {
        [Fact]
        public void FinallyThrowExample()
        {
            Customer customer =
                Result<Customer>.ToResult(Repository.GetCustomer(Const.CustomerId))
                    .OnNull(error => new Customer())
                    .FinallyOrThrow();

        }

        [Fact]
        public void FinallyNullExample()
        {
            Customer customer =
                Result<Customer>.ToResult(Repository.GetCustomer(Const.CustomerId))
                    .FinallyOrNull();

            if (customer != null)
            {
                // do other stuff
            }
        }
    }
}
