using CoreWCF.Configuration;
using UnifiedFileGateway.Service;
using UnifiedFileGateway.Contracts;
using CoreWCF.Channels;
using CoreWCF;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services for REST API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register FileService for dependency injection
builder.Services.AddSingleton<UnifiedFileGateway.Service.FileService>();

builder.WebHost.UseKestrel(options =>
{
	options.ListenAnyIP(5000); // Changed to port 5000 to match frontend expectations
});

builder.Services.AddServiceModelServices();

var app = builder.Build();

// Configure MVC routing
app.UseRouting();

// Add CORS for frontend
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Map controllers for REST API
app.MapControllers();

var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
{
	MaxReceivedMessageSize = int.MaxValue,
	MaxBufferSize = int.MaxValue,
	MessageEncoding = WSMessageEncoding.Mtom
};

app.UseServiceModel(services =>
{
	services.AddService<FileService>();
	services.AddServiceEndpoint<FileService, IFileService>(binding, "/FileService");
});

app.Run();
