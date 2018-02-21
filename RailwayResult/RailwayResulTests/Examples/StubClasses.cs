using System;
using System.Collections.Generic;

namespace RailwayResultTests.Examples
{
    #region Constants / ID

    public static class Const
    {
        public const int CustomerId = 123;
        public const int NullCustomerId = 0;
        public const int ExceptionCustomerId = -1;
        public const int NullMailCustomerId = 20;
        public const int ValidMailCustomerId = 21;
        public const int InvalidMailCustomerId = 22;
        public const int ExcptionMailCustomerId = 23;
        public const int CustomerWithLowLimitId = 30;

        public const int OfferId = 123;
        public const int NullOfferId = 0;
        public const int ExceptionOfferId = -1;
        public const int OfferWithNullCustomerId = 1;

        public const int ProductId = 123;
        public const int NullProductId = 0;
        public const int ExceptionProductId = -1;

        public const int OrderId = 1;
        public const int NullOrderId = 0;
        public const int ExceptionOrderId = -1;
        public const int UpdateExceptionOrderId = -2;
        public const int OrderWithNullCustomerId = 10;
        public const int OrderWithExceptionCustomerId = 11;
        public const int OrderWithNullMailMailCustomerId = NullMailCustomerId;
        public const int OrderWithValidMailCustomerId = ValidMailCustomerId;
        public const int OrderWithInvalidMailCustomerId = InvalidMailCustomerId;
        public const int OrderWithExceptionMailCustomerId = ExcptionMailCustomerId;
        public const int OrderWithErpProcessFailure = 30;
        public const int OrderWithErpProcessException = 31;
        public const int OrderWithLowLimitCustomerCustomerId = 40;

    }
    #endregion

    #region stub classes
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal OrderLimit { get; set; }

        public string EmailAddress { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }
    }

    public class Offer
    {
        public Offer()
        {
            Products = new List<Product>();
        }
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public List<Product> Products { get; set; }
        public decimal Discount { get; set; }
    }

    public class Order
    {
        public enum OrderStatus
        {
            New,
            Processed,
            Shipped
        }
        public Order()
        {
            Products = new List<Product>();
        }
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public List<Product> Products { get; set; }

        public Guid TrackingId { get; set; }
        public OrderStatus Status { get; set; }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }
        public decimal TotalOrderAmount()
        {
            decimal total = 0;
            foreach (var product in Products)
            {
                total += product.Price;
            }

            return total;
        }
    }

    public class OrderProcessingStatus
    {
        public enum ProcessingStatus
        {
            Success,
            NotExists,
            ErpProcessFailure,
            UpdateFailure,
            InformCustomerFailure
        }
        public ProcessingStatus Status { get; set; }
        public int OrderId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ErpService
    {
        public enum ShippingStatus
        {
            Success,
            NotDeliverable,
        }
        public static ShippingInfo ProcessOrder(Order order)
        {
            var info =  new ShippingInfo
            {
                Status = ShippingStatus.Success,
                OrderId = order.Id,
                TrackingCode = Guid.NewGuid(),
                EstimatedShippingDate = DateTime.Today.AddDays(1)
            };

            switch (order.Id)
            {
                case Const.OrderWithErpProcessException: 
                    throw new ErpException("Some unknown ERP exception");

                case Const.OrderWithErpProcessFailure:
                    info.Status = ShippingStatus.NotDeliverable;
                    break;
            }

            return info;
        }
        public class ShippingInfo
        {
            public ShippingStatus Status { get; set; }
            public int OrderId { get; set; }
            public Guid TrackingCode { get; set; }
            public DateTime EstimatedShippingDate { get; set; }
        }

        public class ErpException : ApplicationException
        {
            public ErpException(string message) : base(message)
            {
            }
        }
    }

    public class SmtpService
    {
        public static bool SendMail(string recipient, string subject, string body)
        {
            switch (recipient)
            {
                case "valid@mail.com": return true;
                case "failed@mail.com": return false;
                case "exception@mail.com": throw new SmptException("SmptService Exception");
            }
            throw new SmptException("don't know what to do");
        }
    }

    public class SmptException : ApplicationException
    {
        public SmptException(string message): base(message) { }
    }

    public static class Logger
    {
        public static void LogMessage(string message)
        {
            //NOP;
        }
    }
    public static class Repository
    {
        public static Customer GetCustomer(int id)
        {
            switch (id)
            {
                case -1: throw new RepositoryException("GetCustomer exception");
                case 0: return null;
            }
            return Stub.GetCustomer(id);
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
                case -1: throw new RepositoryException("GetOffer exception");
                case 0: return null;
                case 1: return Stub.GetOfferWithNullCustomer(id);
            }
            return Stub.GetOffer(id);
        }

        public static Product GetProduct(int id)
        {
            switch (id)
            {
                case -1: throw new RepositoryException("GetProduct exception");
                case 0: return null;
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
            order.Products.Add(Stub.GetProduct(10));
            order.Products.Add(Stub.GetProduct(11));
            order.Products.Add(Stub.GetProduct(12));
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
    #endregion

    #region Stub support methods
    public static class Stub
    {
        public static Customer GetNullCustomer(int id)
        {
            return null;
        }

        public static Customer GetCustomerThrows(int id)
        {
            throw new ApplicationException("test exception");
        }
        public static Customer GetCustomer(int id)
        {
            var customer=  new Customer()
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

        public static Product GetProduct(int id)
        {
            return new Product()
            {
                Id = id,
                Name = "Bar",
                Price = 24.90m
            };
        }
        public static Offer GetOfferWithNullCustomer(int id)
        {
            return new Offer()
            {
                Id = id,
                CustomerId = 0,
                Discount = 15.0m
            };
        }

        public static Offer GetOffer(int id)
        {
            return new Offer()
            {
                Id = id,
                CustomerId = 123,
                Discount = 25.0m
            };
        }
        public static Order GetOrderWithNullCustomer(int id)
        {
            var order = new Order()
            {
                Id = id,
                CustomerId = 0
            };

            order.Products.Add(Stub.GetProduct(10));
            order.Products.Add(Stub.GetProduct(11));
            order.Products.Add(Stub.GetProduct(12));
            return order;
        }
    }


    #endregion
}
