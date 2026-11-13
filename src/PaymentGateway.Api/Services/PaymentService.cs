
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Results;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Coordinates payment processing, including request validation,
/// bank authorisation, payment persistence and idempotency handling.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly PaymentValidator _paymentValidator;
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IBankClient _bankClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly IdempotencyRepository _idempotencyRepository;
    private readonly PaymentRequestHasher _paymentRequestHasher;

    /// <summary>
    /// Initialises the payment service with its required dependencies.
    /// </summary>
    public PaymentService(
        PaymentValidator paymentValidator,
        PaymentsRepository paymentsRepository,
        IBankClient bankClient,
        ILogger<PaymentService> logger,
        IdempotencyRepository idempotencyRepository,
        PaymentRequestHasher paymentRequestHasher)
    {
        _paymentValidator = paymentValidator;
        _paymentsRepository = paymentsRepository;
        _bankClient = bankClient;
        _logger = logger;
        _idempotencyRepository = idempotencyRepository;
        _paymentRequestHasher = paymentRequestHasher;
    }

    /// <summary>
    /// Validates and processes a payment request by communicating
    /// with the acquiring bank and storing the resulting payment.
    /// </summary>
    /// <param name="request">The payment details submitted by the merchant.</param>
    /// <param name="idempotencyKey">Optional key identifying the payment operation.</param>
    /// <returns>The outcome of the payment-processing operation.</returns>
    public async Task<PaymentProcessingResult> ProcessPaymentAsync(PostPaymentRequest request,string? idempotencyKey)
    {
        _logger.LogInformation("Payment processing request received.");

        var errors = _paymentValidator.Validate(request);

        if (errors.Any())
        {
            _logger.LogWarning("Payment validation failed. Errors: {Errors}",string.Join("; ", errors));
            return CreateFailureResult(errors);
        }

        idempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : idempotencyKey.Trim();

        IdempotencyRecord? idempotencyRecord = null;

        if (idempotencyKey != null)
        {
            if (TryReserveIdempotencyKey(idempotencyKey,request,out var reservedRecord))
            {
                idempotencyRecord = reservedRecord;

                _logger.LogInformation("Idempotency key reserved successfully.");
            }
            else
            {
                var existingRecord = _idempotencyRepository.Get(idempotencyKey);

                if (existingRecord == null)
                {
                    return new PaymentProcessingResult
                    {
                        Outcome = PaymentProcessingOutcome.IdempotencyInProgress,
                        Errors = new List<string>
                        {
                            "Unable to determine the status of the existing request."
                        }
                    };
                }

                _logger.LogInformation("Existing idempotency key detected. State: {State}",existingRecord.State);

                return HandleExistingIdempotencyKey(existingRecord,request);
            }
        }

        _logger.LogInformation("Sending payment authorization request to bank.");

        var bankResult = await AuthorizePaymentAsync(request);

        if (bankResult.IsSuccess && bankResult.Response != null)
        {
            _logger.LogInformation("Bank authorization completed. Authorized: {Authorized}",bankResult.Response.Authorized);
        }
        else
        {
            _logger.LogError("Bank authorization failed. StatusCode: {StatusCode}, Error: {ErrorMessage}", bankResult.StatusCode, bankResult.ErrorMessage);
        }

        // A communication failure does not necessarily mean the bank declined or did not process the payment.
        if (!bankResult.IsSuccess || bankResult.Response == null)
        {
            var failureResult = CreateBankFailureResult();

            if (idempotencyRecord != null)
            {
                var updated = UpdateIdempotencyRecord(
                    idempotencyRecord,
                    IdempotencyState.Unknown,
                    failureResult);

                if (!updated)
                {
                    _logger.LogCritical("Unable to update idempotency record after bank failure.");
                }
            }

            return failureResult;
        }

        var payment = CreatePayment(request,bankResult.Response.Authorized);

        SavePayment(payment);

        _logger.LogInformation("Payment processed and persisted. PaymentId: {PaymentId}, Status: {Status}",payment.Id,payment.Status);

        var result = CreateSuccessResult(payment);

        if (idempotencyRecord != null)
        {
            var updated = UpdateIdempotencyRecord(idempotencyRecord, IdempotencyState.Completed,    result);

            if (!updated)
            {
                _logger.LogCritical("Payment {PaymentId} was processed, but its idempotency record could not be completed.", payment.Id);
            }
        }

        return result;
    }


    #region "Payment Operations"

    /// <summary>
    /// Maps the incoming payment request to the format expected by the bank.
    /// </summary>
    private BankPaymentRequest CreateBankPaymentRequest(PostPaymentRequest request)
    {
        return new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency.ToUpperInvariant(),
            Amount = request.Amount,
            Cvv = request.Cvv
        };
    }

    /// <summary>
    /// Sends the payment authorisation request through the bank client abstraction.
    /// </summary>
    private async Task<BankAuthorizationResult> AuthorizePaymentAsync(
        PostPaymentRequest request)
    {
        var bankRequest = CreateBankPaymentRequest(request);

        return await _bankClient.AuthorizePaymentAsync(bankRequest);
    }

    /// <summary>
    /// Creates a payment entity using the bank's authorisation decision.
    /// Only the last four card digits are retained.
    /// </summary>
    private Payment CreatePayment(PostPaymentRequest request, bool authorized)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Status = authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency.ToUpperInvariant(),
            Amount = request.Amount
        };
    }

    /// <summary>
    /// Stores the processed payment in the payment repository.
    /// </summary>
    private void SavePayment(Payment payment)
    {
        if (!_paymentsRepository.Add(payment))
        {
            throw new InvalidOperationException("Unable to store payment.");
        }
    }


    /// <summary>
    /// Retrieves an existing payment and maps it to the public GET response model.
    /// </summary>
    /// <param name="id">The unique payment identifier.</param>
    /// <returns>The payment details, or null if the payment does not exist.</returns>
    public GetPaymentResponse? GetPayment(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment == null)
        {
            return null;
        }

        return new GetPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
    }

    #endregion

    #region "Return Result Creations"

    /// <summary>
    /// Creates a completed result for a payment rejected during gateway validation.
    /// No bank authorisation request is made.
    /// </summary>
    private PaymentProcessingResult CreateFailureResult(List<string> errors)
    {
        return new PaymentProcessingResult
        {
            Outcome = PaymentProcessingOutcome.Completed,
            PaymentResponse = new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Rejected
            },
            Errors = errors
        };
    }

    /// <summary>
    /// Creates the successful processing result containing the payment identifier
    /// and its final business status.
    /// </summary>
    private PaymentProcessingResult CreateSuccessResult(Payment payment)
    {
        return new PaymentProcessingResult
        {
            Outcome = PaymentProcessingOutcome.Completed,
            PaymentResponse = new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status
            }
        };
    }

    /// <summary>
    /// Creates a result representing an unsuccessful or uncertain
    /// communication with the acquiring bank.
    /// </summary>
    private PaymentProcessingResult CreateBankFailureResult()
    {
        return new PaymentProcessingResult
        {
            Outcome = PaymentProcessingOutcome.BankUnavailable,
            Errors = new List<string>
            {
                "Unable to process payment with the bank. Please try again later."
            }
        };
    }

    #endregion

    #region "Idempotency Section"
    /// <summary>
    /// Attempts to reserve a new idempotency key using an atomic repository operation.
    /// </summary>
    private bool TryReserveIdempotencyKey(string idempotencyKey,PostPaymentRequest request, out IdempotencyRecord record)
    {
        record = new IdempotencyRecord
        {
            Key = idempotencyKey,
            RequestHash = _paymentRequestHasher.GenerateHash(request),
            State = IdempotencyState.Processing
        };

        return _idempotencyRepository.TryAdd(record);
    }

    /// <summary>
    /// Handles requests using an existing idempotency key.
    /// Detects conflicting payment details, requests already processing,
    /// and previously stored results.
    /// </summary>
    private PaymentProcessingResult HandleExistingIdempotencyKey(IdempotencyRecord existingRecord,PostPaymentRequest request)
    {
        var requestHash = _paymentRequestHasher.GenerateHash(request);

        // The same key cannot be reused with different payment details.
        if (existingRecord.RequestHash != requestHash)
        {
            return new PaymentProcessingResult
            {
                Outcome = PaymentProcessingOutcome.IdempotencyConflict,
                Errors = new List<string>
                {
                    "The idempotency key has already been used with different payment details."
                }
            };
        }

        // Do not initiate another bank request while the original is processing.
        if (existingRecord.State == IdempotencyState.Processing)
        {
            return new PaymentProcessingResult
            {
                Outcome = PaymentProcessingOutcome.IdempotencyInProgress,
                Errors = new List<string>
                {
                    "A request with this idempotency key is already being processed."
                }
            };
        }

        // Return the previously stored result when available.
        if (existingRecord.Result == null)
        {
            return new PaymentProcessingResult
            {
                Outcome = PaymentProcessingOutcome.IdempotencyInProgress,
                Errors = new List<string>
                {
                    "The original payment result is not currently available."
                }
            };
        }

        return existingRecord.Result;
    }

    /// <summary>
    /// Creates a replacement idempotency record and attempts an atomic state update.
    /// </summary>
    private bool UpdateIdempotencyRecord(IdempotencyRecord currentRecord,IdempotencyState newState,PaymentProcessingResult result)
    {
        var updatedRecord = new IdempotencyRecord
        {
            Key = currentRecord.Key,
            RequestHash = currentRecord.RequestHash,
            State = newState,
            Result = result
        };

        return _idempotencyRepository.Update(
            currentRecord.Key,
            currentRecord,
            updatedRecord);
    }

    #endregion

}
