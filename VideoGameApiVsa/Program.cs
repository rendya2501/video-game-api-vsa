using Carter;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<VideoGameDbContext>(options =>
    options.UseInMemoryDatabase("GameDB"));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddCarter();

var app = builder.Build();

// 起動時シード（InMemory / 実プロバイダー 両方で動作）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VideoGameDbContext>();
    db.Database.EnsureCreated();

    if (!db.VideoGames.Any())
    {
        db.VideoGames.AddRange(
            new VideoGame { Id = 1, Genre = "Action", Title = "The Legend of Zelda: Breath of the Wild", ReleaseYear = 2017 },
            new VideoGame { Id = 2, Genre = "RPG", Title = "The Witcher 3: Wild Hunt", ReleaseYear = 2015 },
            new VideoGame { Id = 3, Genre = "Shooter", Title = "DOOM Eternal", ReleaseYear = 2020 },
            new VideoGame { Id = 4, Genre = "Adventure", Title = "Red Dead Redemption 2", ReleaseYear = 2018 },
            new VideoGame { Id = 5, Genre = "Strategy", Title = "Civilization VI", ReleaseYear = 2016 }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapCarter();

app.Run();
