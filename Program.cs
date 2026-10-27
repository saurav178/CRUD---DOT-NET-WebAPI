using MongoDB.Driver;
using TaskApi.Models;
using TaskApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configure MongoDB Settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// 2. Register MongoClient as a Singleton (manages internal connection pool)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings?.ConnectionString ?? "mongodb://localhost:27017");
});

// 3. Register TaskService as Scoped
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
