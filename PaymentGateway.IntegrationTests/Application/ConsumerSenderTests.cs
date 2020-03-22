
using FluentAssertions;
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
        ILogger<ConsumerSenderTests> _logger;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SharedTestsHelper.LaunchIdentityServer();
            _logger = TestLogger.Create<ConsumerSenderTests>();
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

            var consumer = SharedTestsHelper.GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(httpClient, queue);
            Task.Run(() => consumer.ConsumeAsync().GetAwaiter().GetResult());
           

            while(((InMemoryQueue<PaymentRequest>)queue).Requests.Count != 0)
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

            var consumer = SharedTestsHelper.GetProducerConsumerSender<PaymentRequest, AcquiringBankPaymentResponse>(httpClient, queue);
            Task.Run(() => consumer.ConsumeAsync().GetAwaiter().GetResult());
            

            while (((InMemoryQueue<PaymentRequest>)queue).Requests.Count != 0)
            {
                Thread.Sleep(100);
            }
            SharedTestsHelper.PaymentExistsWithStatusCode(id, StatusCode.Failure).Should().BeTrue();
        }
    }
}
