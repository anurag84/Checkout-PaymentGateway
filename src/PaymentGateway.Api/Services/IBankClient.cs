
using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Defines the contract for communicating with an acquiring bank
/// to request payment authorisation.
/// </summary>
public interface IBankClient
{
    /// <summary>
    /// Sends a payment authorisation request to the bank.
    /// </summary>
    /// <param name="request">The payment details to submit for authorisation.</param>
    /// <returns>
    /// The bank authorisation result, including the bank response
    /// or details of any communication failure.
    /// </returns>
    Task<BankAuthorizationResult> AuthorizePaymentAsync(
        BankPaymentRequest request);
}
