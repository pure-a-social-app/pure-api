using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Aws
{
    public static class Extension
    {
        public static void AddAws(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new AwsOptions();
            var section = configuration.GetSection("aws");
            section.Bind(options);
            services.Configure<AwsOptions>(configuration.GetSection("aws"));
            services.AddSingleton<IAwsService, AwsService>();
        }
    }
}
