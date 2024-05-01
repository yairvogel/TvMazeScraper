using TvMaze.Interfaces;
using TvMaze.LocalFsDocumentClient;
using TvMaze.MongoDb;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? mongodbConnectionString = args.FirstOrDefault(arg => arg.StartsWith("--mongodb"))?.Split("=")?.ElementAtOrDefault(1);
if (mongodbConnectionString is null)
{
    builder.Services.AddSingleton<IDocumentDbClient, LocalFileSystemDocumentDbClient>();
}
else
{
    Console.WriteLine($"using mongodb document provider at {mongodbConnectionString}");
    builder.Services.AddSingleton<IDocumentDbClient>(new MongoDbDocumentClient(mongodbConnectionString));
}

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseSwagger().UseSwaggerUI();
app.MapControllers();

app.Run();

