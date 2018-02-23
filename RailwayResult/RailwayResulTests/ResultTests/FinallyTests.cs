using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RailwayResultTests.Examples;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.ResultTests
{
    [TestClass]
    public class FinallyTests
    {
        [TestMethod]
        public void FinallyThrowExample()
        {
            Customer customer =
                Result<Customer>.ToResult(Repository.GetCustomer(Const.CustomerId))
                    .OnNull(error => new Customer())
                    .FinallyOrThrow();

        }

        [TestMethod]
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
