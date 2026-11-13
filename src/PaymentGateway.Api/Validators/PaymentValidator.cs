
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

/// <summary>
/// Validates incoming payment requests to ensure all required
/// payment details meet the gateway's validation rules.
/// </summary>
public class PaymentValidator
{
    /// <summary>
    /// Validates all payment request fields and returns a collection
    /// of validation errors. An empty collection indicates a valid request.
    /// </summary>
    /// <param name="request">The payment request to validate.</param>
    /// <returns>A list of validation errors, or an empty list if valid.</returns>
    public List<string> Validate(PostPaymentRequest request)
    {
        if (request == null)
        {
            return new List<string> { "Payment request is required." };
        }

        var errors = new List<string>();

        // Validate each payment field independently.
        ValidateCardNumber(request.CardNumber, errors);
        ValidateCvv(request.Cvv, errors);
        ValidateExpiryDate(request.ExpiryMonth, request.ExpiryYear, errors);
        ValidateCurrency(request.Currency, errors);
        ValidateAmount(request.Amount, errors);

        return errors;
    }

    /// <summary>
    /// Ensures the card number contains between 14 and 19 numeric digits.
    /// </summary>
    private void ValidateCardNumber(string cardNumber, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) ||
            cardNumber.Length < 14 ||
            cardNumber.Length > 19 ||
            !cardNumber.All(char.IsDigit))
        {
            errors.Add("Card number must contain between 14 and 19 numeric digits.");
        }
    }

    /// <summary>
    /// Ensures the CVV contains either three or four numeric digits.
    /// </summary>
    private void ValidateCvv(string cvv, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(cvv) ||
            (cvv.Length != 3 && cvv.Length != 4) ||
            !cvv.All(char.IsDigit))
        {
            errors.Add("CVV must contain 3 or 4 numeric digits.");
        }
    }

    /// <summary>
    /// Validates the expiry month and ensures the card has not expired.
    /// The card remains valid through its stated expiry month.
    /// </summary>
    private void ValidateExpiryDate(int expiryMonth,int expiryYear,List<string> errors)
    {
        if (expiryMonth < 1 || expiryMonth > 12)
        {
            errors.Add("Expiry month must be between 1 and 12.");
            return;
        }

        var currentDate = DateTime.UtcNow;

        if (expiryYear < currentDate.Year ||
            (expiryYear == currentDate.Year && expiryMonth < currentDate.Month))
        {
            errors.Add("Card has expired.");
        }
    }

    /// <summary>
    /// Ensures the supplied currency is one of the currencies
    /// supported by the gateway.
    /// Currency comparison is case-insensitive.
    /// </summary>
    private void ValidateCurrency(string currency, List<string> errors)
    {
        var supportedCurrencies = new[] { "GBP", "USD", "EUR" };

        if (string.IsNullOrWhiteSpace(currency) ||
            !supportedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add("Currency must be one of the supported currencies: GBP, USD or EUR.");
        }
    }

    /// <summary>
    /// Ensures the payment amount is a positive integer
    /// expressed in minor currency units.
    /// </summary>
    private void ValidateAmount(int amount, List<string> errors)
    {
        if (amount <= 0)
        {
            errors.Add("Amount must be greater than zero.");
        }
    }
}
