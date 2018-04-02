using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.Examples.Various
{
    public class UpdateCustomerEmail
    {
        public UpdateCustomerEmail()
        {
            //var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");
            //simpleLogger.ClearLog();
            //ResultLogger.Logger = simpleLogger;
        }


        [Fact]
        public void Example_UpdateCustomerWithNewEmail()
        {
            UpdateEmailAddress(Const.CustomerId, "valid@mail.com").Should().BeTrue();
            UpdateEmailAddress(Const.CustomerId, "invalid#mail.com").Should().BeFalse();
            UpdateEmailAddress(Const.CustomerId, "failed@mail.com").Should().BeFalse();
            UpdateEmailAddress(Const.CustomerId, "exception@mail.com").Should().BeFalse();

            UpdateEmailAddress(Const.NullCustomerId, "valid@mail.com").Should().BeFalse();
            UpdateEmailAddress(Const.ExceptionCustomerId, "valid@mail.com").Should().BeFalse();
        }

        private bool UpdateEmailAddress(int customerId, string newEmailAddress)
        {
            bool success = ValidateEmail(newEmailAddress)
                .OnSuccess(_ => GetCustomer(customerId))
                .OnSuccess(customer =>
                {
                    string oldEmail = customer.EmailAddress;
                    customer.EmailAddress = newEmailAddress;

                    return Repository.UpdateCustomer(customer).FromBool()                            // better to let repository return a Result<bool> type
                        .OnSuccess(_ => SendMailChangeVerification(customer.EmailAddress, customer)) // send confirmation to new email address.
                        .OnSuccess(_ => SendMailChangeVerification(oldEmail, customer))              // inform customer on old email address.
                        .OnSuccess(_ => Logger.LogMessage("..."));                                   // action, returns void
                })
                .OnFailure(_ => false)                                                               // cast any failure to false
                .FinallyOrThrow();

            return success;
        }

        private Result<Customer> GetCustomer(int id)
        {
            return Result<Customer>.ToResult(() => Repository.GetCustomer(id));
        }

        private Result<bool> ValidateEmail(string email)
        {
            Regex regexEmail = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            return Result<bool>.FromBool(regexEmail.IsMatch(email));
        }

        private Result<bool> SendMailChangeVerification(string email, Customer customer)
        {
            string body = $"Dear {customer.Name}, your email address has been changed to {customer.EmailAddress}.";
            return Result<bool>.FromBool(SmtpService.SendMail(email, "Your email address has changed", body));
        }

    }
}
