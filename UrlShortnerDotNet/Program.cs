using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;
using UrlShortnerDotNet.Dtos;
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

app.MapPost("/api/v1/shorten", async (UrlsRequestDto url, AppContext dbContext, HttpContext context) =>
{
    // Validating the url
    if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var uri))
        return Results.BadRequest("Please provide a valid url");

    var random = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var randomString = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

    var surl = new UrlManagement()
    {
        Url = url.Url,
        ShortUrl = randomString
    };

    await dbContext.UrlManagements.AddAsync(surl);
    await dbContext.SaveChangesAsync();

    var response = new UrlsResponseDto()
    {
        shortenUrl = $"{context.Request.Scheme}://{context.Request.Host}/{surl.ShortUrl}",


    };
    return Results.Ok(response);
});

app.MapFallback(async (HttpContext context, AppContext db) =>
{
    var path = context.Request.Path.ToUriComponent().TrimStart('/');
    var urlMatch = await db.UrlManagements.FirstOrDefaultAsync(x => x.ShortUrl.Trim() == path.Trim());
    if (urlMatch is null)
        return Results.BadRequest("Invalid Short url");

    return Results.Redirect(urlMatch.Url);
});

app.Run();


class AppContext : DbContext
{
    
    public AppContext(DbContextOptions<AppContext> options): base(options)
    {
        
    }

    public DbSet<UrlManagement> UrlManagements { get; set; }
}
