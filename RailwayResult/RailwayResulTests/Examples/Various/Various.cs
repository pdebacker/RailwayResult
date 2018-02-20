using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Railway.Result;

namespace RailwayResultTests.Examples.Various
{
    [TestClass]
    public class Various
    {

        [TestMethod]
        public void GivenDifferentEmail_UpdateCustomer()
        {
            int customerId = Const.CustomerId;
            var newMailAddress = "someAddress@mail.com";

            var result =
                GetCustomer(customerId)
                    .OnSuccess<Customer>(customer => UpdateCustomerEmailWhenDifferent(customer, newMailAddress));

            if (result.IsFailure)
            {
                string diagnostics = result.ToString();
                //Logger.Log(diagnostics);
            }


            // OR

            try
            {
                Customer customer = Repository.GetCustomer(customerId);
                if (customer != null)
                {
                    UpdateCustomerEmailWhenDifferent(customer, newMailAddress);
                }

                // do some other work with customer;
            }
            catch (Exception e)
            {
                //Logger.Log(e);
            }
        }

        private Customer UpdateCustomerEmailWhenDifferent(Customer customer, string newMailAddress)
        {
            if (!customer.EmailAddress.Equals(newMailAddress))
            {
                customer.EmailAddress = newMailAddress;
                if (Repository.UpdateCustomer(customer) == false)
                    throw new ApplicationException("failed to update customer");
            }

            return customer;
        }

        private Result<Customer> GetCustomer(int id)
        {
            return Result<Customer>.ToResult(() => Repository.GetCustomer(id));
        }
    }
}
