using Pure.Common.Auth;
using Pure.Common.Aws;
using Pure.Common.Contracts;
using Pure.Common.Mongo;
using Pure.Common.Security;
using Pure.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Principal;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Services;
using Pure.api.Models;

namespace Pure.api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private readonly string corsPolicy = "AllowOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicy,
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddSignalR(x => x.EnableDetailedErrors = true)
                .AddAzureSignalR(Configuration["AzureSignalR:ConnectionString"]);

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            services.AddJwt(Configuration);
            services.AddMongoDB(Configuration);
            services.AddAws(Configuration);

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IPostCommentService, PostCommentService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IShoppingService, ShoppingService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Func<IServiceProvider, IPrincipal> getPrincipal =
                (sp) => sp.GetService<IHttpContextAccessor>().HttpContext.User;

            services.AddScoped(typeof(Func<IPrincipal>), sp =>
            {
                Func<IPrincipal> func = () => getPrincipal(sp);
                return func;
            });

            services.AddScoped<IUserAppContext, UserAppContext>();
            services.AddScoped<IFileService, FileService>();
            services.AddSingleton<IPasswordStorage, PasswordStorage>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddControllers();
            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(corsPolicy);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseAzureSignalR(route =>
            {
                route.MapHub<ChatMessageHub>("/chat");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Could not find anything");
            });
        }
    }
}
