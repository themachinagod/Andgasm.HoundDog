using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.AccountManagement.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System;
using FluentValidation.AspNetCore;
using FluentValidation;
using Andgasm.HoundDog.Core.Email.Interfaces;
using Andgasm.HoundDog.Core.Email;
using Twilio;
using Andgasm.HoundDog.Core.SMSVerification;
using Andgasm.HoundDog.AccountManagement.Database;
using AutoMapper;
using Andgasm.HoundDog.AccountManagement.Mapping;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Andgasm.HoundDog.AccountManagment.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers().AddFluentValidation();

            services.AddDbContext<HoundDogDbContext>(x => x.UseSqlServer(Configuration.GetConnectionString("UsersDb")));

            services.AddIdentity<HoundDogUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<HoundDogDbContext>();

            services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "HoundDog";
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, CurrentUserPolicyHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MatchesCurrentUser", policy =>
                    policy.Requirements.Add(new CurrentUserRequirement()));
            });

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection(ITokenGenerator.TokenConfigName).Value);
            services.AddAuthentication((cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }))
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });

            var accountSid = Configuration["Twilio:AccountSID"];
            var authToken = Configuration["Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);

            services.AddAutoMapper(typeof(UserMappingProfile).GetTypeInfo().Assembly);

            services.AddTransient<IValidator<UserDTO>, UserDTOValidator>();
            services.AddTransient<IValidator<UserSignInDTO>, UserSignInDTOValidator>();

            services.AddScoped<ITokenGenerator, TokenGenerator>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<ISMSVerification, TwilioSMSVerification>();

            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IUserAuthenticationManager, UserAuthenticationManager>();
            services.AddScoped<IUserPasswordManager, UserPasswordManager>();
            services.AddScoped<IUserEmailManager, UserEmailManager>();
            services.AddScoped<IUserPhoneManager, UserPhoneManager>();
            services.AddScoped<IParsePhoneNumber, InternationalPhoneNumberParser>();
            services.AddScoped<IUserTwoFactorAuthManager, UserAuthenticatorManager>();
            services.AddScoped<IUserAvatarManager, UserAvatarManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            InitialiseData(app.ApplicationServices);
        }

        public static async void InitialiseData(IServiceProvider svcs)
        {
            // but of a hack here but this just ensure that the db exists and that the basic data is configured
            // helps support basic ui tests as well as expected config for the admin web help instructions

            using (var servicescope = svcs.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = servicescope.ServiceProvider.GetService<HoundDogDbContext>();
                await context.Database.EnsureCreatedAsync();
                if (!await context.Roles.AnyAsync(x => x.NormalizedName == "USER"))
                {
                    await context.Roles.AddAsync(new IdentityRole("User") { Id = Guid.NewGuid().ToString(), NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
