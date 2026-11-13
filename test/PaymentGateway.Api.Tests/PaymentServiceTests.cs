
using System.Net;
using System.Timers;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Results;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests;

public class PaymentServiceTests
{

    # region Setup

    private (PaymentService Service, PaymentsRepository Repository, Mock<IBankClient> BankClientMock)
    CreatePaymentService()
    {
        var bankClientMock = new Mock<IBankClient>();
        var paymentsRepository = new PaymentsRepository();
        var paymentValidator = new PaymentValidator();

        var paymentService = new PaymentService(
            paymentValidator,
            paymentsRepository,
            bankClientMock.Object,
            NullLogger<PaymentService>.Instance);

        return (paymentService, paymentsRepository, bankClientMock);
    }

    private PostPaymentRequest CreateValidPaymentRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
    }
    #endregion

    #region PaymentProcessStatus Check

    [Fact]
    public async Task ProcessPaymentAsync_ValidPaymentAuthorized_ReturnsAuthorized()
    {
        // Arrange
        var (paymentService, paymentsRepository, bankClientMock) =
            CreatePaymentService();

        bankClientMock
            .Setup(x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankAuthorizationResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new BankPaymentResponse
                {
                    Authorized = true
                }
            });

        var request = CreateValidPaymentRequest();

        // Act

        var result = await paymentService.ProcessPaymentAsync(request);

        // Assert

        Assert.Equal(PaymentProcessingOutcome.Completed, result.Outcome);

        Assert.NotNull(result.PaymentResponse);
        Assert.Equal(PaymentStatus.Authorized, result.PaymentResponse.Status);
        Assert.NotEqual(Guid.Empty, result.PaymentResponse.Id);

        var savedPayment = paymentsRepository.Get(result.PaymentResponse.Id);

        Assert.NotNull(savedPayment);
        Assert.Equal(PaymentStatus.Authorized, savedPayment.Status);
        Assert.Equal("8877", savedPayment.CardNumberLastFour);
        Assert.Equal(100, savedPayment.Amount);

        bankClientMock.Verify(
            checkFilter => checkFilter.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidPaymentDeclined_ReturnsDeclined()
    {
        // Arrange

        var (paymentService, paymentsRepository, bankClientMock) =
            CreatePaymentService();

        bankClientMock
            .Setup(x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankAuthorizationResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new BankPaymentResponse
                {
                    Authorized = false
                }
            });

        var request = CreateValidPaymentRequest();

        // Act

        var result = await paymentService.ProcessPaymentAsync(request);

        // Assert

        Assert.Equal(PaymentProcessingOutcome.Completed, result.Outcome);

        Assert.NotNull(result.PaymentResponse);
        Assert.Equal(PaymentStatus.Declined, result.PaymentResponse.Status);

        var savedPayment = paymentsRepository.Get(result.PaymentResponse.Id);

        Assert.NotNull(savedPayment);
        Assert.Equal(PaymentStatus.Declined, savedPayment.Status);
        Assert.Equal("8877", savedPayment.CardNumberLastFour);

        bankClientMock.Verify(
            checkFilter => checkFilter.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_InvalidPayment_ReturnsRejected()
    {
        // Arrange
        var (paymentService, paymentsRepository, bankClientMock) =
            CreatePaymentService();

        bankClientMock
            .Setup(x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankAuthorizationResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new BankPaymentResponse
                {
                    Authorized = true
                }
            });

        var request = CreateValidPaymentRequest();
        request.CardNumber = "123456";

        // Act

        var result = await paymentService.ProcessPaymentAsync(request);

        // Assert

        Assert.Equal(PaymentProcessingOutcome.Completed, result.Outcome);

        Assert.NotNull(result.PaymentResponse);
        Assert.Equal(PaymentStatus.Rejected, result.PaymentResponse.Status);
        Assert.NotEqual(Guid.Empty, result.PaymentResponse.Id);

        Assert.NotEmpty(result.Errors);

        // Verify that the bank was never contacted.
        bankClientMock.Verify(
            checkFilter => checkFilter.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()),
            Times.Never);

        // Verify that no payment was persisted.
        Assert.Null(paymentsRepository.Get(result.PaymentResponse.Id));
    }

    #endregion

    #region BankProcessing Check

    [Fact]
    public async Task ProcessPaymentAsync_BankUnavailable_ReturnsBankUnavailable()
    {
        // Arrange
        var (paymentService, paymentsRepository, bankClientMock) =
            CreatePaymentService();

        bankClientMock
            .Setup(x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankAuthorizationResult
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Response = null,
                ErrorMessage = "Bank unavailable"
            });

        var request = CreateValidPaymentRequest();

        // Act
        var result = await paymentService.ProcessPaymentAsync(request);

        // Assert
        Assert.Equal(
            PaymentProcessingOutcome.BankUnavailable,
            result.Outcome);

        Assert.Null(result.PaymentResponse);
        Assert.NotEmpty(result.Errors);

        // Verify the bank was contacted exactly once.
        bankClientMock.Verify(
            x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()),
            Times.Once);

        // No payment ID exists because no payment was created.
        Assert.Null(result.PaymentResponse);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidPayment_SendsCorrectDetailsToBank()
    {
        // Arrange
        var (paymentService, _, bankClientMock) = CreatePaymentService();

        var request = CreateValidPaymentRequest();

        bankClientMock
            .Setup(x => x.AuthorizePaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankAuthorizationResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new BankPaymentResponse
                {
                    Authorized = true
                }
            });

        // Act
        await paymentService.ProcessPaymentAsync(request);

        // Assert
        bankClientMock.Verify(x => x.AuthorizePaymentAsync(
            It.Is<BankPaymentRequest>(bankRequest =>
                bankRequest.CardNumber == request.CardNumber &&
                bankRequest.ExpiryDate == "12/2027" &&
                bankRequest.Currency == "GBP" &&
                bankRequest.Amount == 100 &&
                bankRequest.Cvv == request.Cvv)),
            Times.Once);
    }

    #endregion

}
