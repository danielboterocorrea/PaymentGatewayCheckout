
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PaymentGateway.Application.RequestModels;
using PaymentGateway.Application.ResponseModels;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Services.Interfaces;
using PaymentGateway.Domain.Common;
using PaymentGateway.Domain.Toolbox;
using PaymentGateway.Infrastructure.Repositories.Cache;
using PaymentGateway.Infrastructure.Toolbox;
using PaymentGateway.IntegrationTests.Helpers;
using PaymentGateway.SharedTests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.IntegrationTests.Application
{
    [TestFixture]
    public class ConsumerSenderTests
    {
        private WebApplicationFactory<FakePaymentGatewayApiStartup> paymentGatewayFactory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SharedTestsHelper.LaunchIdentityServer();
            paymentGatewayFactory = TestHelper.CreateCustomWebApplicationFactory();
        }


        [Test]
        public void ConsumerSenderPaymentOk()
        {
            var queue = SharedTestsHelper.GetInMemoryQueue();
            var dbContext = SharedTestsHelper.GetPaymentGatewayContext();
            var paymentService = SharedTestsHelper.GetPaymentService(dbContext, queue);
            var paymentRequest = SharedTestsHelper.GetValidPaymentRequest();
            var id = paymentService.ProcessAsync(paymentRequest).GetAwaiter().GetResult();
            paymentService.OnProcessSuccessAsync(paymentRequest);
            id.Should().Be(paymentRequest.Id);

            SharedTestsHelper.DetachObjectFromDb(dbContext);

            SharedTestsHelper.PaymentExists(id).Should().BeTrue();
            var httpClient = SharedTestsHelper.GetHttpClientAcquiringSimulatorMockedSuccess(id);
            var consumer = TestHelper.GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(httpClient, paymentGatewayFactory, queue);
            Task.Run(() => consumer.ConsumeAsync().GetAwaiter().GetResult());
           

            while(queue.Requests.Count != 0)
            {
                Thread.Sleep(100);
            }
            SharedTestsHelper.PaymentExistsWithStatusCode(id, StatusCode.Success).Should().BeTrue();
        }

        [Test]
        public void ConsumerSenderPaymentNotOk()
        {
            var queue = SharedTestsHelper.GetInMemoryQueue();
            var dbContext = SharedTestsHelper.GetPaymentGatewayContext();
            var paymentService = SharedTestsHelper.GetPaymentService(dbContext, queue);
            var paymentRequest = SharedTestsHelper.GetValidPaymentRequest();
            var id = paymentService.ProcessAsync(paymentRequest).GetAwaiter().GetResult();
            paymentService.OnProcessSuccessAsync(paymentRequest);
            id.Should().Be(paymentRequest.Id);

            SharedTestsHelper.DetachObjectFromDb(dbContext);

            SharedTestsHelper.PaymentExists(id).Should().BeTrue();

            var httpClient = SharedTestsHelper.GetHttpClientAcquiringSimulatorMockedFailure(id);

            var consumer = TestHelper.GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(httpClient, paymentGatewayFactory, queue);
            Task.Run(() => consumer.ConsumeAsync().GetAwaiter().GetResult());
            

            while (queue.Requests.Count != 0)
            {
                Thread.Sleep(100);
            }
            SharedTestsHelper.PaymentExistsWithStatusCode(id, StatusCode.Failure).Should().BeTrue();
        }
    }
}
