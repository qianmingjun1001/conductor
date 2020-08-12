using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Conductor.Domain;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Scripting;
using Conductor.Domain.Services;
using Conductor.Formatters;
using Conductor.Mappings;
using Conductor.Steps;
using Conductor.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkflowCore.Interface;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Conductor.Auth;
using Conductor.Domain.Backplane.SqlServer;
using Conductor.Domain.Scripting.ExpressionTree;
using Conductor.DynamicRoute;
using Conductor.Filters;
using Conductor.Middleware;
using Conductor.Storage.SqlServer;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;

namespace Conductor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add<ModelValidateFilter>();
                    options.Filters.Add<RequestObjectFilter>();
                    options.Filters.Add<ExceptionFilter>();
                    // options.InputFormatters.Add(new YamlRequestBodyInputFormatter());
                    // options.OutputFormatters.Add(new YamlRequestBodyOutputFormatter());
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            #region Auth

            var authConfig = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            var authEnabled = false;
            var authEnabledStr = EnvironmentVariables.Auth;
            if (string.IsNullOrEmpty(authEnabledStr))
                authEnabled = Configuration.GetSection("Auth").GetValue<bool>("Enabled");
            else
                authEnabled = Convert.ToBoolean(authEnabledStr);

            if (authEnabled)
                authConfig.AddJwtAuth(Configuration);
            else
                authConfig.AddBypassAuth();

            services.AddPolicies();

            #endregion

            services.AddWorkflow(options =>
            {
                //工作流程存储
                options.UseSqlServer(Configuration["SqlServerConnectionString"], true, true);

                //分布式锁
                options.UseSqlServerLocking(Configuration["SqlServerConnectionString"]);

                //分布式队列
                options.UseSqlServerBroker(Configuration["SqlServerConnectionString"], true, true);
            });
            services.ConfigureDomainServices();
            services.ConfigureExpressionTreeScripting();
            services.AddSteps();

            //定义存储
            services.UseSqlServer(Configuration["SqlServerConnectionString"]);
            //集群通知底板
            services.UseSqlServerBackplane(Configuration["SqlServerConnectionString"],
                (serviceProvider, path) => serviceProvider.GetRequiredService<EntryPointRouteRegistry>().RegisterRoute(path));

            services.AddEntryPointRoute();

            var config = new MapperConfiguration(cfg => { cfg.AddProfile<APIProfile>(); });

            services.AddSingleton<IMapper>(x => new Mapper(config));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "WorkflowCore API",
                    Description = "WorkflowCore Management Web API"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkflowCore API"); });

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //将入口点映射到路由
                endpoints.MapEntryPoint();
            });

            app.UseWorkflow();
        }
    }
}