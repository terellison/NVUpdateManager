using Microsoft.AspNetCore.Authentication.Negotiate;
using NVUpdateManager.Core.Extensions;
using NVUpdateManager.Listener.Data;
using NVUpdateManager.Listener.Services;

namespace NVUpdateManager.Listener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });

            builder.Services.AddSwaggerGen();
            builder.Services.AddDriverManager();
            builder.Services.AddSingleton<IUpdateService, DriverUpdateService>();
            builder.Services.AddSingleton<IDriverService, NVDriverService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            if(app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.MapControllers();

            app.Run();
        }
    }
}
