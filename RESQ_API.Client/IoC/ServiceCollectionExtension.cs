using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RESQ_API.Client.Models;

namespace RESQ_API.Client.IoC
{
    public static class ServiceCollectionExtension
    {
        public static void AddDemoApiClientService(this IServiceCollection services, Action<RESQApiClientOptions> options)
        {
            services.Configure(options);
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RESQApiClientOptions>>().Value;
                return new RESQApiClientService(options);
            });
        }
    }
}
