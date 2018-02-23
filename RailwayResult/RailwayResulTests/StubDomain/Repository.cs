using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public static class Repository
    {
        public static Customer GetCustomer(int id)
        {
            switch (id)
            {
                case Const.ExceptionCustomerId: throw new RepositoryException("GetCustomer exception");
                case Const.NullCustomerId: return null;
            }

            var customer = new Customer()
            {
                Id = id,
                Name = "Foo",
                EmailAddress = "valid@mail.com",
                OrderLimit = 100
            };

            switch (id)
            {
                case Const.NullMailCustomerId:
                    customer.EmailAddress = null;
                    break;
                case Const.InvalidMailCustomerId:
                    customer.EmailAddress = "failed@mail.com";
                    break;
                case Const.ExcptionMailCustomerId:
                    customer.EmailAddress = "exception@mail.com";
                    break;
                case Const.CustomerWithLowLimitId:
                    customer.OrderLimit = 10;
                    break;
            }
            return customer;

        }

        public static bool UpdateCustomer(Customer customer)
        {
            //throw new RepositoryException("Entity validation error");
            return true;
        }


        public static Offer GetOffer(int id)
        {
            switch (id)
            {
                case Const.ExceptionOfferId: throw new RepositoryException("GetOffer exception");
                case Const.NullOfferId: return null;
            }

            var offer = new Offer()
            {
                Id = id,
                CustomerId = id == Const.OfferWithNullCustomerId? 0: 123,
                Discount = 25.0m
            };

            return offer;
        }

        public static Product GetProduct(int id)
        {
            switch (id)
            {
                case Const.ExceptionProductId: throw new RepositoryException("GetProduct exception");
                case Const.NullProductId: return null;
            }
            return new Product()
            {
                Id = id,
                Name = "Bar",
                Price = 10.5m
            };
        }

        public static Order GetOrder(int id)
        {
            switch (id)
            {
                case Const.ExceptionOrderId: throw new RepositoryException("GetOrder exception");
                case Const.NullOrderId: return null;
            }

            var order = new Order()
            {
                Id = id,
                CustomerId = Const.CustomerId
            };


            switch (id)
            {
                case Const.OrderWithNullCustomerId:
                    order.CustomerId = Const.NullCustomerId;
                    break;
                case Const.OrderWithExceptionCustomerId:
                    order.CustomerId = Const.ExceptionCustomerId;
                    break;
                case Const.OrderWithNullMailMailCustomerId:
                    order.CustomerId = Const.NullMailCustomerId;
                    break;
                case Const.OrderWithValidMailCustomerId:
                    order.CustomerId = Const.ValidMailCustomerId;
                    break;
                case Const.OrderWithInvalidMailCustomerId:
                    order.CustomerId = Const.InvalidMailCustomerId;
                    break;
                case Const.OrderWithExceptionMailCustomerId:
                    order.CustomerId = Const.ExcptionMailCustomerId;
                    break;
                case Const.OrderWithLowLimitCustomerCustomerId:
                    order.CustomerId = Const.CustomerWithLowLimitId;
                    break;
            }

            order = AddProducts(order);
            return order;
        }

        private static Order AddProducts(Order order)
        {
            order.Products.Add(GetProduct(10));
            order.Products.Add(GetProduct(11));
            order.Products.Add(GetProduct(12));
            return order;
        }

        public static bool UpdateOrder(Order order)
        {
            switch (order.Id)
            {
                case Const.UpdateExceptionOrderId: throw new RepositoryException("Order Update Exception");
            }

            return true;
        }
        public static string GetMailTemplate(string name)
        {
            switch (name)
            {
                case "EX": throw new ApplicationException("GetMailTemplate exception");
                case "NULL": return null;
            }
            return "Dear {0}, we have started processing your order. The estimated shipping date is {1}.";
        }

        public class RepositoryException : ApplicationException
        {
            public RepositoryException(string message) : base(message)
            {
            }
        }
    }
}
