﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NVUpdateManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NVUpdateManager.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUpdateFinder(this IServiceCollection services)
        {
            var client = new HttpClient();

            services.TryAddSingleton<IUpdateFinder>(new UpdateFinder(client));

            return services;
        }
    }
}
