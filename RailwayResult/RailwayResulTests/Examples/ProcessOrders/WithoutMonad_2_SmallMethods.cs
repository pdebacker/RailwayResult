using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RailwayResultTests.Examples.ProcessOrders
{
    [TestClass]
    public class WithoutMonad_2_SmallMethods
    {
        [TestMethod]
        public void ExampleOrderProcessing_2_SmallMethods()
        {
            // Process Orders: for each Order in the list, the Order is processed
            // The Order is processed by an external ERP system. 
            // The ERP checks wether the products are in stock and returns an estimated
            // shipping status. The ERP system can reject a sigle order for various reasons.
            // The Customer is informed by email with the shipping status.
            // The ERP could be down or not accessible due to network connectivity issues.
            // The SMTP mail server could also be down.
            // Failed Orders should be logged for retry or manual intervention later.

            var ordersToProcess = TestSupport.GetTestOrderIds();

            var failedOrders = new List<OrderProcessingStatus>();
            int countSuccesOrders = 0;

            foreach (var orderId in ordersToProcess)
            {
                try
                {
                    Order order = GetOrderFromRepository(orderId);
                    Customer customer = GetCustomerFromRepository(order.CustomerId);
                    ErpService.ShippingInfo info = ErpProcessOrder(order);
                    HandleShippingSuccess(order, info, failedOrders, customer);
                    countSuccesOrders++;
                }
                catch (Repository.RepositoryException ex)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        Status = OrderProcessingStatus.ProcessingStatus.NotExists,
                        OrderId = orderId,
                        ErrorMessage = ex.Message
                    });

                }
                catch (ErpService.ErpException ex)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        Status = OrderProcessingStatus.ProcessingStatus.ErpProcessFailure,
                        OrderId = orderId,
                        ErrorMessage = ex.Message
                    });
                }
                catch (SmptException ex)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        Status = OrderProcessingStatus.ProcessingStatus.InformCustomerFailure,
                        OrderId = orderId,
                        ErrorMessage = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    failedOrders.Add(new OrderProcessingStatus
                    {
                        Status = OrderProcessingStatus.ProcessingStatus.NotExists,
                        ErrorMessage = ex.Message
                    });
                }
            }


            failedOrders.Count.Should().Be(9);
            countSuccesOrders.Should().Be(3); // See note*

            //*) The customer did get a mail for the order which the status was not Updated
            //   in the repository. Therefore it counts both as success and failed.
            //   A better solutioon would be to roll back the transaction, but for the demo it is ok.

        }

        private Order GetOrderFromRepository(int orderId)
        {
            Order order = Repository.GetOrder(orderId);
            if (order == null)
                throw new Repository.RepositoryException("Order with id " + orderId + " not found");

            return order;
        }

        private Customer GetCustomerFromRepository(int customerId)
        {
            Customer customer = Repository.GetCustomer(customerId);
            if (customer == null)
                throw new Repository.RepositoryException("Order with id " + customerId + " not found");

            return customer;
        }

        private ErpService.ShippingInfo ErpProcessOrder(Order order)
        {
            ErpService.ShippingInfo info = ErpService.ProcessOrder(order);
            if (info.Status == ErpService.ShippingStatus.Success)
                return info;

            throw new ErpService.ErpException("Info status = " + info.Status);
        }

        private void HandleShippingSuccess(Order order, ErpService.ShippingInfo info, List<OrderProcessingStatus> failedOrders, Customer customer)
        {
            order.Status = Order.OrderStatus.Shipped;
            order.TrackingId = info.TrackingCode;

            try
            {
                Repository.UpdateOrder(order);
            }
            catch (Exception ex)
            {
                failedOrders.Add(new OrderProcessingStatus
                {
                    Status = OrderProcessingStatus.ProcessingStatus.UpdateFailure,
                    OrderId = order.Id,
                    ErrorMessage = ex.Message
                });
            }

            InformCustomer(info, customer);
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
    }
}
