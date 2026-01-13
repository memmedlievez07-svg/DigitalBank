using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using DigitalBank.Application.Repositories;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Persistence.Dal;
using DigitalBank.Persistence.Services;
using DigitalBank.Persistence.Services.AdminUsers;
using DigitalBank.Persistence.Services.Audit;
using DigitalBank.Persistence.Services.Auth;
using DigitalBank.Persistence.Services.Jwt;
using DigitalBank.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DigitalBank.Persistence.PersistenceServiceRegistration
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServiceRegistration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1) DbContext
            services.AddDbContext<DigitalBankDbContext>(options =>
               options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            // 2) Identity (artıq burdadır!)
            services
                .AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                })
                .AddEntityFrameworkStores<DigitalBankDbContext>()
                .AddDefaultTokenProviders();
            services.Configure<JwtOption>(configuration.GetSection("JwtOptions"));
            services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));

            services.AddScoped<IUnitOfWork, DigitalBank.Persistence.UnitOfWork.UnitOfWork>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();


            return services;
        }
    }
}
