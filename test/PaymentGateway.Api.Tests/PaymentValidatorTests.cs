
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests;

public class PaymentValidatorTests
{
    private readonly PaymentValidator _validator = new();

    #region Valid Payment Tests

    [Fact]
    public void Validate_ValidPayment_ReturnsNoErrors()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Card Number Validation Tests

    [Theory]
    [InlineData("1234567890123")]          // 13 digits
    [InlineData("12345678901234567890")]   // 20 digits
    [InlineData("222240534324ABCD")]       // Non-numeric characters
    [InlineData("")]                       // Empty
    public void Validate_InvalidCardNumber_ReturnsErrors(string cardNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Contains(errors, error =>
            error.Contains("Card number"));
    }

    [Theory]
    [InlineData("12345678901234")]          // 14 digits
    [InlineData("1234567890123456789")]     // 19 digits
    public void Validate_CardNumberWithinAllowedLength_ReturnsNoErrors(string cardNumber)
    {
        // Arrange
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region CVV Validation Tests

    [Theory]
    [InlineData("12")]       // Too short
    [InlineData("12345")]    // Too long
    [InlineData("12A")]      // Non-numeric
    [InlineData("")]         // Empty
    public void Validate_InvalidCvv_ReturnsErrors(string cvv)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = cvv;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Contains(errors, error => error.Contains("CVV"));
    }

    [Theory]
    [InlineData("123")]      // 3 digits
    [InlineData("1234")]     // 4 digits
    public void Validate_ValidCvv_ReturnsNoErrors(string cvv)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Cvv = cvv;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Expiry Date Validation Tests

    [Theory]
    [InlineData(0, 2027)]    // Invalid month
    [InlineData(13, 2027)]   // Invalid month
    [InlineData(12, 2025)]   // Past year
    [InlineData(1, 2026)]    // Earlier month in current year
    public void Validate_InvalidExpiryDate_ReturnsErrors(int month, int year)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = month;
        request.ExpiryYear = year;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Contains(errors, error => error.Contains("expir", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(12, 2027)]   // Future year
    public void Validate_ValidExpiryDate_ReturnsNoErrors(int month, int year)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExpiryMonth = month;
        request.ExpiryYear = year;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Currency Validation Tests

    [Theory]
    [InlineData("GBP")]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("gbp")]      // Case insensitive
    public void Validate_SupportedCurrency_ReturnsNoErrors(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("INR")]
    [InlineData("AUD")]
    [InlineData("")]
    public void Validate_UnsupportedCurrency_ReturnsErrors(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Contains(errors, error => error.Contains("Currency"));
    }

    #endregion

    #region Amount Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_InvalidAmount_ReturnsErrors(int amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Contains(errors, error => error.Contains("Amount"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Validate_ValidAmount_ReturnsNoErrors(int amount)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Amount = amount;

        // Act
        var errors = _validator.Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Test Data Helpers

    private PostPaymentRequest CreateValidRequest()
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
}
