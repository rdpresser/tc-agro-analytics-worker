// Global usings for TC.Agro.Analytics.Service
global using System;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Runtime.CompilerServices;
global using System.Text;
global using FastEndpoints;
global using FastEndpoints.Security;
global using FastEndpoints.Swagger;
global using FluentValidation;
global using FluentValidation.Resources;
global using HealthChecks.UI.Client;
global using JasperFx.Resources;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Hosting;
global using Newtonsoft.Json.Converters;
global using NSwag.AspNetCore;
global using OpenTelemetry;
global using OpenTelemetry.Logs;
global using OpenTelemetry.Metrics;
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
global using Serilog;
global using TC.Agro.Analytics.Application;
global using TC.Agro.Analytics.Application.Abstractions.Ports;
global using TC.Agro.Analytics.Application.UseCases.GetAlertHistory;
global using TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;
global using TC.Agro.Analytics.Application.UseCases.GetPlotStatus;
global using TC.Agro.Analytics.Infrastructure;
global using TC.Agro.Analytics.Service.Extensions;
global using TC.Agro.Contracts.Events.Identity;
global using TC.Agro.SharedKernel.Api.Endpoints;
global using TC.Agro.SharedKernel.Api.Extensions;
global using TC.Agro.SharedKernel.Application.Behaviors;
global using TC.Agro.SharedKernel.Extensions;
global using TC.Agro.SharedKernel.Infrastructure.Authentication;
global using TC.Agro.SharedKernel.Infrastructure.Caching.HealthCheck;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Provider;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;
global using TC.Agro.SharedKernel.Infrastructure.MessageBroker;
global using TC.Agro.SharedKernel.Infrastructure.Messaging;
global using TC.Agro.SharedKernel.Infrastructure.Middleware;
global using TC.Agro.SharedKernel.Infrastructure.Pagination;
global using TC.Agro.SharedKernel.Infrastructure.Telemetry;
global using Wolverine;
global using Wolverine.EntityFrameworkCore;
global using Wolverine.ErrorHandling;
global using Wolverine.Postgresql;
global using Wolverine.RabbitMQ;
global using ZiggyCreatures.Caching.Fusion;
global using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
global using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
global using TC.Agro.Messaging.Extensions;
global using TC.Agro.Contracts.Events.Analytics;
global using TC.Agro.Analytics.Application.UseCases.AcknowledgeAlert;
global using TC.Agro.Analytics.Service.Telemetry;
global using System.Diagnostics.Metrics;
global using System.Diagnostics;
global using System.Security.Claims;
global using Serilog.Context;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;

[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

//**//REMARK: Required for functional and integration tests to work.
namespace TC.Agro.Analytics.Service
{
    public partial class Program;
}
