
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserRoleMangement.Database;
using UserRoleMangement.Database.Repositories;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.Middleware;
using UserRoleMangement.TokenGeneration;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddDbContext<DatabaseContext>(options =>
         options.UseSqlServer(
         builder.Configuration.GetConnectionString("DefaultConnection"),
         sqlOptions => sqlOptions.CommandTimeout(120) 
        ));
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        builder.Services.AddSingleton<IUserTokenStore, StoreTokenInMemory>();
        builder.Services.AddAuthorization();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAuthorization();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseSession();
        app.UseMiddleware<LogMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<SessionAuthMiddleware>();
        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}