using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using musly_api.Services;
using Microsoft.AspNetCore.Hosting;
//using CMTZ.Data;
//using CMTZ.Services;
//using Microsoft.EntityFrameworkCore;
//using CMTZ.Common;
//using Genbox.SimpleS3.Core;
//using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using musly_api.Repository;

namespace musly_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment _env { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddTransient<CollectionFileService>();
            //services.AddTransient<MuslyService>();

            MuslyService muslyService = new MuslyService(new CollectionFileService(Configuration), Configuration, _env);
            services.AddSingleton(muslyService);

            TimbreService timbreService = new TimbreService(muslyService);
            services.AddSingleton(muslyService);

            var connString = Configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"));
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "certifiedmixtapez.com");
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "certifiedmixtapes.com");

                        builder.WithOrigins(
                            "http://certifiedmixtapez.com",
                            "http://www.certifiedmixtapez.com",
                            "https://certifiedmixtapez.com",
                            "https://www.certifiedmixtapez.com",
                            "http://certifiedmixtapes.com",
                            "http://www.certifiedmixtapes.com",
                            "https://certifiedmixtapes.com",
                            "https://www.certifiedmixtapes.com",
                            "https://beta.certifiedmixtapes.com",
                            "https://embed.certifiedmixtapez.com",
                            "https://cmtz-widget.pages.dev",
                            "https://certifiedmixtapes-eepvd.ondigitalocean.app",
                            "https://ricky-shaw-client-app-default.layer0-limelight.link",
                            "http://local.certifiedmixtap.es:4200",
                            "http://localhost:4200"
                            )
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            // SQL Server
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlServer(connString));

            //var appSettings = new AppSettings();
            //Configuration.GetSection(nameof(AppSettings)).Bind(appSettings);
            //services.AddSingleton(appSettings);
            services.AddTransient<SearchService>();
            services.AddTransient<RedisPlaylistRepository>();
            services.AddSwaggerGen();


            //services.AddTransient<TrackExport>();
            //services.AddTransient<DTrackService>();


            // Pascal Case Responses
            services.AddControllers().AddJsonOptions(o => {
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                o.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                // o.JsonSerializerOptions.PropertyNamingPolicy = null;
            });


            //services.AddTransient<DTrackRepository>();

            //services.Configure<AppSettings>(options => Configuration.GetSection("AppSettings").Bind(options));


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.ApplicationServices.GetService<MuslyService>();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger();
            });
        }
    }
}
