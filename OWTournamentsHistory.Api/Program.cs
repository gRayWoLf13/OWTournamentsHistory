using Coravel;
using Microsoft.IdentityModel.Logging;
using OWTournamentsHistory.Api;
using OWTournamentsHistory.Api.DI;
using OWTournamentsHistory.Api.GrpcServices;
using OWTournamentsHistory.Api.Services;
using OWTournamentsHistory.DataAccess.DI;
using OWTournamentsHistory.Tasks.DI;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc(x => x.EnableDetailedErrors = true);

builder.AddConfigurations();

builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddDatabaseService(builder.Configuration);

builder.Services.AddScheduler();
builder.Services.AddQueue();

builder.Services.AddDataAccess();

builder.Services.AddTasks();
builder.Services.AddTaskListeners();

builder.Services.AddServices();

builder.Services.AddAutoMapper();

var app = builder.Build();

app.Services.UseScheduler(SchedulerProfile.SchedulerTasks);

app.MapGrpcService<StatisticsHandlerService>();

app.Services.ConfigureQueue()
    .OnError(e =>
    {
        Debug.WriteLine($"QUEUE ERROR: {e}");
    });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
