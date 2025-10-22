using RandomTrust.Core.EntropySources;
using RandomTrust.Core.Generators;
using RandomTrust.Core.Interfaces;
using RandomTrust.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//  ��������� ������������ � JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });

//  Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:3000"));
});
builder.Services.AddSwaggerGen();

//  ����������� ���� ������������ (IoC ���������)
builder.Services.AddSingleton<IEntropySource, HybridEntropySource>();
builder.Services.AddSingleton<IStatisticalTester, AdvancedStatisticalTester>();
builder.Services.AddSingleton<IRandomGenerator, TransparentRNG>();
builder.Services.AddSingleton<TestDataGenerator>();

//  ��������� ������ ��������� / ��������
builder.Services.AddSingleton<VerificationService>();

//  ������ ����������
var app = builder.Build();

//  ������������ HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

//  ��� ��� ������������
Console.WriteLine("RandomTrust Web API is starting...");
Console.WriteLine("Available endpoints:");
Console.WriteLine("  POST /api/random/entropy - Add entropy from mouse movement");
Console.WriteLine("  POST /api/random/initialize - Initialize generator");
Console.WriteLine("  GET  /api/random/lottery/{count} - Generate lottery numbers");
Console.WriteLine("  POST /api/random/generate-test-data - Generate test data for NIST");
Console.WriteLine("  POST /api/random/run-statistical-tests - Run statistical tests");
Console.WriteLine("  POST /api/random/verify - Verify fairness / randomness integrity");

app.Run();
