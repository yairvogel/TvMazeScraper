using TvMaze.Interfaces;
using TvMaze.LocalFsDocumentClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDocumentDbClient, LocalFileSystemDocumentDbClient>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

