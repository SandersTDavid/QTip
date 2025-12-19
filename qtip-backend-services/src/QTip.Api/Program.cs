using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using QTip.Application.Submissions.CreateSubmission;
using QTip.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

// MediatR + FluentValidation (scan Application assembly)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSubmissionCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateSubmissionCommand).Assembly);

builder.Services.AddFluentValidationAutoValidation();

// CORS for local dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("dev");

app.MapControllers();

app.Run();