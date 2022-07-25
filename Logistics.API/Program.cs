using Logistics.DataAccess.ChangeStream;
using Logistics.DataAccess.Initialize;
using Logistics.Service;
using Logistics.Service.Interfaces;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var mongoClient = new MongoClient(configuration.GetConnectionString("MongoDB"));

// Configure DI
builder.Services.AddSingleton<IMongoClient, MongoClient>(provider => mongoClient);
builder.Services.AddSingleton<IConfiguration>(provider => configuration);
builder.Services.AddSingleton<ICargoService, CargoService>();
builder.Services.AddSingleton<IPlaneService, PlaneService>();
builder.Services.AddSingleton<ICityService, CityService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
	// Initialize Database
	Initialize.CreateIndexes(mongoClient, configuration).ConfigureAwait(false);
	// Initiate ChangeStream
	ChangeStream.Monitor(mongoClient, configuration).ConfigureAwait(false);
});

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();