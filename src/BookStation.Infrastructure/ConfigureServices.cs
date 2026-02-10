using BookStation.Application.Services;
using BookStation.Application.Users.Commands;
using BookStation.Core.SharedKernel;
using BookStation.Domain.Repositories;
using BookStation.Infrastructure.Authentication;
using BookStation.Infrastructure.Persistence;
using BookStation.Infrastructure.Repositories;
using BookStation.Infrastructure.Services;
using BookStation.Infrastructure.Queries;
using BookStation.Query.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStation.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<WriteDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName)));

        // Register DbContext as IUnitOfWork and IReadDbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<WriteDbContext>());

        // Redis Distributed Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "BookStation:";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            // Fallback to in-memory cache if Redis is not configured
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAddressRepository, UserAddressRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Cloudinary Image Upload Service
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<IImageUploadService, CloudinaryService>();

        // VNPay Payment Service
        services.Configure<VnPaySettings>(configuration.GetSection(VnPaySettings.SectionName));
        services.AddScoped<IPaymentService, VnPayService>();

        // Dapper Query Services (for complex raw SQL queries)
        services.AddScoped<DapperQueryService>();
        services.AddScoped<BookDapperQueries>();

        // JWT Authentication
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

