using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure.Identity
{
    public static class ServiceExtensions
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            //{
            //    services.AddDbContext<IdentityContext>(options =>
            //        options.UseInMemoryDatabase("IdentityDb"));
            //}
            //else
            {
                services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityConnection"),
                    b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));
            }
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();
            #region Services
            services.AddTransient<IAccountService, AccountService>();
            #endregion
            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            )
                .AddJwtBearer(options =>
                {
                    //options.RequireHttpsMetadata = false;
                    //options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JWTSettings:Issuer"],
                        ValidAudience = configuration["JWTSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                    };
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    OnAuthenticationFailed = c =>
                    //    {
                    //        c.NoResult();
                    //        c.Response.StatusCode = 500;
                    //        c.Response.ContentType = "text/plain";
                    //        return c.Response.WriteAsync(c.Exception.ToString());
                    //    },
                    //    OnChallenge = context =>
                    //    {
                    //        context.HandleResponse();
                    //        context.Response.StatusCode = 401;
                    //        context.Response.ContentType = "application/json";
                    //        var result = JsonConvert.SerializeObject(new Response<string>("You are not Authorized"));
                    //        return context.Response.WriteAsync(result);
                    //    },
                    //    OnForbidden = context =>
                    //    {
                    //        context.Response.StatusCode = 403;
                    //        context.Response.ContentType = "application/json";
                    //        var result = JsonConvert.SerializeObject(new Response<string>("You are not authorized to access this resource"));
                    //        return context.Response.WriteAsync(result);
                    //    },
                    //};
                });
        }
    }
}
