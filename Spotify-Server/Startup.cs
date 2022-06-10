using Application.IService;
using Application.Service;
using Application.Ultilities;
using Data.Entities;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            services.AddTransient<IMusicService, MusicService>();
            services.AddTransient<IAlbumService, AlbumService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IBackupDataService, BackupDataService>();
            services.AddTransient<IHangfireService, HangfireService>();

            services.AddHangfireServer();
            services.AddControllers();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
            RecurringJob.AddOrUpdate<IBackupDataService>("BackupData1", x => x.Backup(), "00 22 * * *", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate<IBackupDataService>("BackupData2", x => x.Backup(), "00 9 * * *", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate("PingServer", () => Pinger.Ping(), "*/5 * * * *");
            RecurringJob.AddOrUpdate<IHangfireService>("TruncateDatabaseHangfire", x => x.TruncateDB(), "0 0 * * 7", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));
            RecurringJob.AddOrUpdate<IHangfireService>("DeleteOldFile", x => x.DeleteOldFile(), "0 0 * * 7", TimeZoneInfo.FindSystemTimeZoneById(Configuration["Timezone"]));

            app.UseCors("spotify");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard();
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
