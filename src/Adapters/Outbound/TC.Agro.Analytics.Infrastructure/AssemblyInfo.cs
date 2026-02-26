// Global usings for TC.Agro.Analytics.Infrastructure
global using System;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using TC.Agro.Analytics.Application.Abstractions.Ports;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;
global using TC.Agro.Analytics.Domain.Aggregates;
global using TC.Agro.Analytics.Domain.ValueObjects;
global using TC.Agro.Analytics.Infrastructure.Messaging;
global using TC.Agro.Analytics.Infrastructure.Repositories;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Domain.Aggregate;
global using TC.Agro.SharedKernel.Infrastructure;
global using TC.Agro.SharedKernel.Infrastructure.Database;
global using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;
global using TC.Agro.SharedKernel.Infrastructure.Messaging.Outbox;
global using TC.Agro.SharedKernel.Infrastructure.Pagination;
global using TC.Agro.SharedKernel.Infrastructure.UserClaims;
global using Wolverine.EntityFrameworkCore;
global using TC.Agro.Analytics.Domain.Snapshots;
global using TC.Agro.SharedKernel.Domain.Events;
global using TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold;


[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
