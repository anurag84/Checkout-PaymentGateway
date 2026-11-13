using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse> GetPayment(Guid id)
    {
        var payment = _paymentService.GetPayment(id);

        if (payment == null)
        {
            return NotFound();
        }

        return Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] PostPaymentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        //Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey);

        var result = await _paymentService.ProcessPaymentAsync(
            request,
            idempotencyKey);

        return result.Outcome switch
        {
            PaymentProcessingOutcome.Completed =>
                Ok(result.PaymentResponse),

            PaymentProcessingOutcome.BankUnavailable =>
                StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    result.Errors),

            PaymentProcessingOutcome.IdempotencyConflict =>
                Conflict(result.Errors),

            PaymentProcessingOutcome.IdempotencyInProgress =>
                Conflict(result.Errors),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}