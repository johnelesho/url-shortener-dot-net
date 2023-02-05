using Microsoft.EntityFrameworkCore;
using UrlShortnerDotNet.Models;

var builder = WebApplication.CreateBuilder(args);

var Config = builder.Configuration;
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppContext>(options =>
{
    options.UseSqlServer(Config.GetConnectionString("DefaultConnection"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();


class AppContext : DbContext
{
    
    public AppContext(DbContextOptions<AppContext> options): base(options)
    {
        
    }

    public DbSet<UrlManagement> UrlManagements { get; set; }
}
