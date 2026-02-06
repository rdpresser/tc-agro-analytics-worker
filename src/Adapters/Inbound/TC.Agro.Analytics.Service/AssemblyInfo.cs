global using System;
global using System.Diagnostics.CodeAnalysis;
global using FastEndpoints;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using TC.Agro.Analytics.Application;
global using TC.Agro.Analytics.Application.UseCases.Shared;
global using TC.Agro.Analytics.Infrastructure;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace TC.Agro.Analytics.Service
{
    public partial class Program;
}
