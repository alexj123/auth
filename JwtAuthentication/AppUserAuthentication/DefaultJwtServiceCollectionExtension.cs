using System.Threading.Tasks;
using AppUserAuthentication.Access.Repositories;
using AppUserAuthentication.Models.Identity;
using AppUserAuthentication.Persistence;
using AppUserAuthentication.TokenGeneration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AppUserAuthentication
{
    /// <summary>
    /// Contains extension methods to <see cref="IServiceCollection"/> for configuring default Jwt services.
    /// </summary>
    public static class DefaultJwtServiceCollectionExtension
    {
        /// <summary>
        /// Adds and configures default services for JwtAuthentication.
        /// </summary>
        /// <remarks>
        /// Does not add or configure any db services. Use <see cref="AddDefaultJwtDbContext{T,TF}"/> for this.
        /// </remarks>
        /// <param name="services">The services available in your application.</param>
        /// <param name="parameters">The <see cref="TokenValidationParameters"/> for the JwtBearer.</param>
        /// <typeparam name="TUserAuthenticationService">The type representing an implementation of
        /// <see cref="IUserAuthenticationService"/>.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDefaultJwtAuthentication<TUserAuthenticationService>(this IServiceCollection services, 
            TokenValidationParameters parameters) where TUserAuthenticationService : class, IUserAuthenticationService
        {
            services.Configure<CookiePolicyOptions>(cookiePolicyOptions =>
            {
                cookiePolicyOptions.CheckConsentNeeded = context => true;
                cookiePolicyOptions.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            
            services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.TokenValidationParameters = parameters;
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            }).AddCookie();

            services.AddScoped<IJwtHandler, DefaultJwtHandler>();
            services.AddScoped<IRefreshTokenGenerator, DefaultRefreshTokenGenerator>();
            services.AddScoped<IUserAuthenticationService, TUserAuthenticationService>();
            
            return services;
        }

        /// <summary>
        /// Adds and configures default services for db interaction for JwtAuthentication.
        /// </summary>
        /// <remarks>
        /// Does not add or configure any default services for JwtAuthentication.
        /// Use <see cref="AddDefaultJwtAuthentication{TUserAuthenticationService}"/> for this.
        /// </remarks>
        /// <param name="services">The services available in your application.</param>
        /// <param name="connectionString">The connection string for the db.</param>
        /// <typeparam name="TAppUser">The type representing a <see cref="AppUser"/> in the system.</typeparam>
        /// <typeparam name="TDbContext">The type representing an implementation <see cref="AbstractAppDbContext{T}"/>.</typeparam>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDefaultJwtDbContext<TAppUser, TDbContext>(this IServiceCollection services,
            string connectionString) where TAppUser : AppUser where TDbContext : AbstractAppDbContext<TAppUser>
        {
            services.AddDbContext<TDbContext>(builder =>
                builder.UseNpgsql(connectionString));
            
            services.AddIdentityCore<TAppUser>(identityOptions =>
            {
                identityOptions.User.RequireUniqueEmail = true;
                identityOptions.Password.RequireDigit = true;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireUppercase = true;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<TDbContext>();

            services.AddScoped<IUserRepository<TAppUser>, UserRepository<TAppUser>>();

            return services;
        }
    }

}