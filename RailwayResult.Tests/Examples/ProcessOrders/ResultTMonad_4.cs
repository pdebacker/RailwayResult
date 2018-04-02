using System;
using System.Collections.Generic;
using FluentAssertions;
using Railway.Result;
using RailwayResultTests.StubDomain;
using Xunit;

namespace RailwayResultTests.Examples.ProcessOrders
{
    public class ResultTMonad_4
    {
        [Fact]
        public void ExampleOrderProcessing_Monad_2()
        {
            var ordersToProcess = TestSupport.GetTestOrderIds();

            var failedOrders = new List<OrderProcessingStatus>();
            int countSuccesOrders = 0;

            foreach (var orderId in ordersToProcess)
            {
                Result<Order> orderResult = Result<Order>
                    .ToResult(() => Repository.GetOrder(orderId));

                Result<Customer> customerResult =
                    orderResult.OnSuccess(order => Repository.GetCustomer(order.CustomerId));

                var result = customerResult
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
