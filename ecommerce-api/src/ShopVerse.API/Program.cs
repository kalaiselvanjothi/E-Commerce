using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using ShopVerse.API.Extensions;
using ShopVerse.API.Middleware;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopVerse API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ShopVerseDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await SeedData.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database");
    }
}

app.Run();
