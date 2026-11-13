using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddTransient<PaymentValidator>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IdempotencyRepository>();
builder.Services.AddSingleton<PaymentRequestHasher>();

builder.Services.AddHttpClient<IBankClient, BankSimulatorClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["BankSimulator:BaseUrl"];
    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var bankUri))
    {
        throw new InvalidOperationException(
            "BankSimulator:BaseUrl is missing or invalid.");
    }

    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = bankUri;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
