using Microsoft.EntityFrameworkCore;
using AppContext = SongList.Web.AppContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<AppContext>(o => o.UseNpgsql(config["DbConnectionString"]));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(x => x.AddDefaultPolicy(x => x.AllowAnyOrigin()));


var app = builder.Build();
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppContext>().Database.MigrateAsync();
}

app.UseResponseCompression();
app.UseFileServer();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();