using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RailwayResultTests.Examples;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.ResultTests
{
    [TestClass]
    public class ChainingTests
    {
        //[TestMethod]
        public void WillNotCompileExample()
        {
            //Result<Customer> customerResult =
            //    Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
            //        .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
            //        .OnFailure(error => Result<bool>.Failure());

            // OnFailure returns a Bool TReturn type but should return a Customer type
            // Depending if the OnSuccess flow or OnFailure flow is executed, a different
            // return type is returned. This is not possible. Eventually the same type should
            // be returned. The idea is to chain OnSuccess methods to create a happy-flow
            // with an optional OnFailure handler that is only usefull of a failure can be corrected.
            // Else, pass the failure to the caller and handle it on a higher level.
            //
            //
            //
            // Or a mix of types using Continue will also not Compile:
            //
            //Result<Customer> result = Result<Offer>.ToResult(
            //        () => Repository.GetOffer(Const.ExceptionOfferId))
            //    .Continue(
            //        success => Result<Customer>.ToResult(Repository.GetCustomer(offer.CustomerId)),
            //        error => new Customer() { Name = "Bar" });
            //
            // Above will fail because success will return Result<Customer> and error Customer.
            // Type arguments cannot be inferred from usage error. Keep success and error return values 
            // the same, or return nothing on error.
            // 
        }


        [TestMethod]
        public void NoChaining_ToResult()
        {
            Result<Offer> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId));

            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Offer));
        }

        [TestMethod]
        [ExpectedException(typeof(Repository.RepositoryException))]
        public void NoChaining_ToResultException()
        {
            Result<Offer> result = Result<Offer>.ToResult(Repository.GetOffer(Const.ExceptionOfferId));
        }

        [TestMethod]
        public void NoChaining_ToResultFailure()
        {
            Result<Offer> result = Result<Offer>.ToResult(
                () => Repository.GetOffer(Const.ExceptionOfferId));

            result.IsException.Should().BeTrue();
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess()
        {
            Result<Customer> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId));

            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Customer));
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnSuccess()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnSuccess(offer => Repository.GetProduct(Const.ProductId));


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnSuccess_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnSuccess(offer => Repository.GetProduct(Const.ProductId))
                .OnFailure(error => new Product() { Name = "Foo" });


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().NotBe("Foo");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnSuccessException_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnSuccess(offer => Repository.GetProduct(Const.ExceptionOfferId))
                .OnFailure(error => new Product() { Name = "Foo" });


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().Be("Foo");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnSuccessException()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnSuccess(offer => Repository.GetProduct(Const.ExceptionOfferId));

            result.IsSuccess.Should().BeFalse();
            result.IsException.Should().BeTrue();
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccessException_OnFailure_OnSuccess()
        {
            Result<string> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => Repository.GetProduct(Const.ExceptionOfferId))
                .OnFailure(error => new Product() { Name = "Foo" })
                .OnSuccess(product => product.Name);


            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Should().Be("Foo");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull()
        {
            Result<Customer> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" });


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Customer));
            result.ReturnValue.Name.Should().Be("Foobar");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_OnSuccess()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.ProductId));


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_OnSuccess_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.ProductId))
                .OnFailure(error => new Product() { Id = -100, Name = "zero", Price = 0 });


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().Be("Bar");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccessException_OnNull_OnSuccess_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(Const.ExceptionCustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.ProductId))
                .OnFailure(error => new Product() { Id = -100, Name = "zero", Price = 0 });


            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().Be("zero");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccessException_OnNull_OnSuccess_OnException()
        {
            bool shouldBeFalse = false;

            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(Const.ExceptionCustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => { shouldBeFalse = true; return new Product(); })
                .OnException(error => new Product() { Id = -100, Name = "zero", Price = 0 });


            // Assert that the code { shouldBeFalse = true; return new Product(); }is not executed
            shouldBeFalse.Should().BeFalse();

            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().Be("zero");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_OnSuccessFailure_OnException()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.NullProductId))
                .OnException(error => new Product() { Id = -100, Name = "zero", Price = 0 });


            result.IsSuccess.Should().BeFalse();
            result.ReturnValue.Should().BeNull();
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccessException_OnNull_OnSuccess_OnException_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(Const.ExceptionCustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.ProductId))
                .OnException(error => new Product() { Id = -100, Name = "zero", Price = 0 })
                .OnFailure(error => new Product() { Id = -200, Name = "none", Price = 0 });

            // Expect Exception result
            result.IsSuccess.Should().BeTrue();
            Assert.IsTrue(result.ReturnValue.GetType() == typeof(Product));
            result.ReturnValue.Name.Should().Be("zero");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_OnSuccessFailure_OnException_OnFailure()
        {
            Result<Product> result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .OnSuccess(customer => Repository.GetProduct(Const.NullProductId))
                .OnException(error => new Product() { Id = -100, Name = "zero", Price = 0 })
                .OnFailure(error => new Product() { Id = -100, Name = "none", Price = 0 });

            // Expect Failure result
            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Name.Should().Be("none");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_FinallyOrNull()
        {
            Customer result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .FinallyOrNull();

            result.Should().NotBeNull();
            result.Name.Should().Be("Foobar");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_OnNull_FinallyOrThrow()
        {
            Customer result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .OnNull(error => new Customer() { Name = "Foobar" })
                .FinallyOrThrow();

            result.Should().NotBeNull();
            result.Name.Should().Be("Foobar");
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccess_FinallyOrNull_ExpectNull()
        {
            Customer result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .FinallyOrNull();

            result.Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(ResultException))]
        public void Chaining_ToResult_OnSuccess_FinallyOrThrow_ShouldThrow()
        {
            Customer result = Result<Offer>.ToResult(Repository.GetOffer(Const.OfferWithNullCustomerId))
                .OnSuccess(offer => Repository.GetCustomer(offer.CustomerId))
                .FinallyOrThrow();

        }

        [TestMethod]
        public void Chaining_ToResult_OnFailureAction()
        {
            Result<Offer> result = Result<Offer>.ToResult(
                Repository.GetOffer(Const.NullOfferId))
                .OnFailure(error => Logger.LogMessage(error.ToString()));

            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void Chaining_ToResult_OnSuccessTypeCustomer_OnSuccessTypeResultCustomer()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    Repository.GetOffer(Const.OfferId))
                .OnSuccess(offer => new Customer())
                .OnSuccess(customer => Result<Customer>.ToResult(new Customer() { Id = 500 }));

            result.ReturnValue.Id.Should().Be(500);
        }

        [TestMethod]
        public void Chaining_Continue_SuccessResult()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    Repository.GetOffer(Const.OfferId))
                .Continue(offer => Repository.GetCustomer(offer.CustomerId));

            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Name.Should().Be("Foo");
        }

        [TestMethod]
        public void Chaining_Continue_FailureResult()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    Repository.GetOffer(Const.OfferWithNullCustomerId))
                .Continue(offer => Repository.GetCustomer(offer.CustomerId));

            result.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void Chaining_ContinueOrError_ShouldFollowSuccesFlow()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    Repository.GetOffer(Const.OfferId))
                .Continue(
                    offer => Repository.GetCustomer(offer.CustomerId),
                    error => new Customer() { Name = "Bar" });

            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Name.Should().Be("Foo");
        }

        [TestMethod]
        public void Chaining_ContinueOrError_ShouldFollowFailureFlow()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    () => Repository.GetOffer(Const.ExceptionOfferId))
                .Continue(
                    offer => Repository.GetCustomer(offer.CustomerId),
                    error => new Customer() { Name = "Bar" });

            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Name.Should().Be("Bar");
        }

        [TestMethod]
        public void Chaining_ContinueOrError_WhenUsingResultT()
        {
            Result<Customer> result = Result<Offer>.ToResult(
                    () => Repository.GetOffer(Const.ExceptionOfferId))
                .Continue(
                    offer => Result<Customer>.ToResult(Repository.GetCustomer(offer.CustomerId)),
                    error => Result<Customer>.ToResult(new Customer() { Name = "Bar" }));

            result.IsSuccess.Should().BeTrue();
            result.ReturnValue.Name.Should().Be("Bar");
        }

        [TestMethod]
        public void Chaining_ContinueOrError_WhenUsingAction()
        {
            string logOutput = string.Empty;

            Result<Customer> result = Result<Offer>.ToResult(
                    () => Repository.GetOffer(Const.ExceptionOfferId))
                .Continue(
                    offer => Repository.GetCustomer(offer.CustomerId),
                    error => { logOutput = error.ToString();});

            result.IsSuccess.Should().BeFalse();
            logOutput.Should().NotBeNullOrEmpty();
        }

    }
}
