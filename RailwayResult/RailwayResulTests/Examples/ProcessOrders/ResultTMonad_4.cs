using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Railway.Result;

namespace RailwayResultTests.Examples.ProcessOrders
{
    [TestClass]
    public class ResultTMonad_4
    {

        [TestMethod]
        public void ExampleOrderProcessing_Monad_2()
        {
            var ordersToProcess = TestSupport.GetTestOrderIds();

            var failedOrders = new List<OrderProcessingStatus>();
            int countSuccesOrders = 0;

            foreach (var orderId in ordersToProcess)
            {
                Order orderResult = null;
                Customer customerResult = null;

                var result = Result<Order>.ToResult(() => orderResult = Repository.GetOrder(orderId))
                    .OnSuccess(order => customerResult = Repository.GetCustomer(order.CustomerId))
                    .OnSuccess(customer => ErpProcessOrder(orderResult))
                    .OnSuccess(shippingInfo => UpdateOrderAndInformCustomer(
                        orderResult, 
                        customerResult, 
                        shippingInfo, 
                        failedOrders));


                if (result.IsFailure)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        OrderId = orderId,
                        ErrorMessage = result.FailureInfo.ToString()
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
        
        private Result<bool> UpdateOrderAndInformCustomer(
                Order orderResult,
                Customer customerResult,
                ErpService.ShippingInfo shippingInfo,
                List<OrderProcessingStatus> failedOrders)
        {
            UpdateShippedOrder(orderResult, shippingInfo)
                .OnFailure(error =>
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        OrderId = orderResult.Id,
                        ErrorMessage = error.ToString()
                    }); // see note *
                });

            InformCustomer(shippingInfo, customerResult);
            return Result<bool>.Succeeded();
        }


        private ErpService.ShippingInfo ErpProcessOrder(Order order)
        {
            ErpService.ShippingInfo info = ErpService.ProcessOrder(order);
            if (info.Status == ErpService.ShippingStatus.Success)
                return info;

            throw new ErpService.ErpException("Info status = " + info.Status);
        }

        private void InformCustomer(ErpService.ShippingInfo info, Customer customer)
        {
            string mailTemplate = Repository.GetMailTemplate("ORDER");
            string mailBody =
                String.Format(mailTemplate,
                    customer.Name,
                    info.EstimatedShippingDate);

            if (SmtpService.SendMail(customer.EmailAddress, "Order status", mailBody) == false)
            {
                throw new SmptException("Failed to send mail");
            }
        }

        private Result<bool> UpdateShippedOrder(Order order, ErpService.ShippingInfo info)
        {
            order.Status = Order.OrderStatus.Shipped;
            order.TrackingId = info.TrackingCode;
            return Result<bool>.FromBool(() => Repository.UpdateOrder(order));
        }
    }
}
