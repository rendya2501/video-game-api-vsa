using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VideoGameApiVsa.Behaviors;
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

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>)
);

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

// グローバル例外ハンドラを登録する
// ここでキャッチされるのは「どこでも未処理で投げられた例外」
app.UseExceptionHandler(errorApp =>
{
    // 例外発生時に実行されるパイプラインを定義
    errorApp.Run(async context =>
    {
        // 現在の HTTP コンテキストから例外情報を取得
        // IExceptionHandlerFeature は UseExceptionHandler が内部で設定してくれる
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        // 例外が FluentValidation の ValidationException の場合
        if (exception is ValidationException validationException)
        {
            // HTTP ステータスコードを 400 Bad Request に設定
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            // レスポンスの Content-Type を JSON に設定
            context.Response.ContentType = "application/json";

            // ValidationException が持つ Errors をプロパティ名ごとにグルーピングする
            var errors = validationException.Errors
                // PropertyName（例: "Name", "Price"）ごとにまとめる
                .GroupBy(e => e.PropertyName)   
                // Dictionary<string, string[]> に変換
                .ToDictionary(                  
                    g => g.Key,     // プロパティ名
                    g => g.Select(e => e.ErrorMessage).ToArray()    // エラーメッセージ配列
                );

            // JSON としてレスポンスを書き込む
            // {
            //   "Message": "Validation failed",
            //   "Errors": {
            //     "Name": ["Name is required"],
            //     "Price": ["Price must be greater than 0"]
            //   }
            // }
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Validation failed",
                Errors = errors
            });
        }
    });
});

app.MapCarter();

app.Run();
