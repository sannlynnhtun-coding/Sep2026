using Sep2026.ClinicPatientRecord.API.Data;
using Sep2026.ClinicPatientRecord.API.Repositories;
using Sep2026.ClinicPatientRecord.API.Service;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddSingleton<DatabaseConnection>();

// Repositories
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<DoctorRepository>();
builder.Services.AddScoped<TokenRepository>();

// Services
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<DoctorService>();
builder.Services.AddScoped<TokenService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();