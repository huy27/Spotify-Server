using Application.GraphQL;
using Application.IService;
using Application.Service;
using Application.Ultilities;
using Data.Entities;
using Data.Models.Album;
using Data.Models.Mail;
using Data.Models.Song;
using Data.Models.User;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using HotChocolate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace Spotify_Server
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
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("SpotifyConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddCors(options =>
            {
                options.AddPolicy("spotify",
                builder =>
                {
                    // Not a permanent solution, but just trying to isolate the problem
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddDbContext<SpotifyContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SpotifyConnection")));

            // Identity
            services.AddIdentity<AppUser, IdentityRole>()
                    .AddEntityFrameworkStores<SpotifyContext>()
                    .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30); // Khóa 30 giây
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

            });

            //GraphQL
            services.AddScoped<Query>();
            services.AddScoped<Mutation>();
            services.AddScoped<IMusicService, MusicService>();
            services.AddGraphQLServer()
                    .AddQueryType<Query>()
                    .AddMutationType<Mutation>();

            services.AddTransient<IMusicService, MusicService>();
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IBackupDataService, BackupDataService>();
            services.AddTransient<IHangfireService, HangfireService>();
            services.AddTransient<IElasticSearchService, ElasticSearchService>();
            services.AddTransient<IUserService, UserService>();

            //Redis
            services.AddTransient<ICacheStrigsStack, CacheStrigsStack>();

            //Scope for background services
            services.AddScoped<IElasticSearchService, ElasticSearchService>();
            services.AddScoped<IMailService, MailService>();

            //Validator
            services.AddTransient<IValidator<CreateSongModel>, CreateSongModelValidator>();
            services.AddTransient<IValidator<UpdateSongModel>, UpdateSongModelValidator>();
            services.AddTransient<IValidator<CreateAlbumModel>, CreateAlbumModelValidator>();
            services.AddTransient<IValidator<UpdateAlbumModel>, UpdateAlbumModelValidator>();
            services.AddTransient<IValidator<SendMailModel>, SendMailModelValidator>();
            services.AddTransient<IValidator<RegisterModel>, RegisterModelValidator>();

            services.AddHangfireServer();
            services.AddControllers().AddFluentValidation();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API WSVAP", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                  {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                        },
                        new List<string>()
                      }
                    });
            });

            string issuer = Configuration["Tokens:Issuer"];
            string audience = Configuration["Tokens:Audience"];
            string signingKey = Configuration["Tokens:Secret"];
            byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
            }

            app.UseHttpsRedirection();

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "Jobs",
                Authorization = new[]
            {
                new  HangfireAuthorizationFilter("admin")
            }
            });
            RecurringJob.AddOrUpdate<IBackupDataService>("BackupData1", x => x.Backup(true), "00 22 * * *", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate<IBackupDataService>("BackupData2", x => x.Backup(true), "00 9 * * *", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate("PingServer", () => Pinger.Ping(), "*/5 * * * *");
            RecurringJob.AddOrUpdate<IHangfireService>("TruncateDatabaseHangfire", x => x.TruncateDB(), "0 0 * * 7", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate<IHangfireService>("DeleteOldFile", x => x.DeleteOldFile(), "0 0 * * SUN,TUE,FRI", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));

            app.UseCors("spotify");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
                endpoints.MapGraphQL();
            });
        }
    }

    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string[] _roles;

        public HangfireAuthorizationFilter(params string[] roles)
        {
            _roles = roles;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;

            //Your authorization logic goes here.

            return true; //I'am returning true for simplicity
        }
    }
}
