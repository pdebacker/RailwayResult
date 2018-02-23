using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Railway.Result;
using RailwayResultTests.StubDomain;

namespace RailwayResultTests.Examples.ProcessOrders
{
    [TestClass]
    public class ResultTMonad_5_Linq
    {
        [TestInitialize]
        public void TestSetup()
        {
            var simpleLogger = new SimpleLogger(@"c:\tmp\log.txt");
            simpleLogger.ClearLog();
            ResultLogger.Logger = simpleLogger;
        }

        [TestMethod]
        public void ExampleOrderProcessing_Monad_Linq_ExtractedUpdateAndInform()
        {
            var ordersToProcess = TestSupport.GetTestOrderIds();

            var failedOrders = new List<OrderProcessingStatus>();
            int countSuccesOrders = 0;

            foreach (var orderId in ordersToProcess)
            {
                var result =
                    from order in GetOrderResult(orderId)
                    from customer in GetCustomerResult(order.CustomerId)
                    from shipping in ErpProcessOrder(order)
                    from update in HandleUpdateOrder(order, shipping, failedOrders)
                    from sendmail in HandleInformCustomer(shipping, customer)
                    select sendmail;

                if (result.IsFailure)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        OrderId = orderId,
                        ErrorMessage = result.ToString()
                    });
                }
                else
                {
                    countSuccesOrders++;
                }
            }


            failedOrders.Count.Should().Be(9);
            countSuccesOrders.Should().Be(3); // See note*

            //*) The customer did get a mail for the order which the status was not Updated
            //   in the repository. Therefore it counts both as success and failed.
            //   A better solutioon would be to roll back the transaction, but for the demo it is ok.
        }

        private Result<Order> GetOrderResult(int orderId)
        {
            return Result<Order>.ToResult(() => Repository.GetOrder(orderId));
        }

        private Result<Customer> GetCustomerResult(int customerId)
        {
            return Result<Customer>.ToResult(() => Repository.GetCustomer(customerId));
        }
        private Result<bool> HandleInformCustomer(ErpService.ShippingInfo info, Customer customer)
        {
            var result = Result<bool>.FromBool(() =>
            {
                string mailTemplate = Repository.GetMailTemplate("ORDER");
                string mailBody =
                    String.Format(mailTemplate,
                        customer.Name,
                        info.EstimatedShippingDate);

                return SmtpService.SendMail(customer.EmailAddress, "Order status", mailBody);
            });
            return result;
        }
        private Result<bool> HandleUpdateOrder(Order order, ErpService.ShippingInfo shipping, List<OrderProcessingStatus> failedOrders)
        {
            return UpdateShippedOrder(order, shipping)
                .OnFailure(error =>
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        OrderId = order.Id,
                        ErrorMessage = error.ToString()
                    }); // see note *
                    return Result<bool>.Succeeded();
                });
        }


        private Result<ErpService.ShippingInfo> ErpProcessOrder(Order order)
        {
            return Result<ErpService.ShippingInfo>.ToResult(() =>
            {
                ErpService.ShippingInfo info = ErpService.ProcessOrder(order);
                if (info.Status == ErpService.ShippingStatus.Success)
                    return info;

                throw new ErpService.ErpException("Info status = " + info.Status);
            });
        }

        private Result<bool> UpdateShippedOrder(Order order, ErpService.ShippingInfo info)
        {
            order.Status = Order.OrderStatus.Shipped;
            order.TrackingId = info.TrackingCode;
            return Result<bool>.FromBool(() => Repository.UpdateOrder(order));
        }
    }
}
