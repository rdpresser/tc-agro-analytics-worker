global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Threading.Tasks;
global using Ardalis.Result;
global using FastEndpoints;
global using FluentValidation;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using TC.Agro.Analytics.Application.Abstractions;
global using TC.Agro.Analytics.Application.Abstractions.Ports;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetAlertHistory;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlerts;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;
global using TC.Agro.Analytics.Application.UseCases.Alerts.GetSensorStatus;
global using TC.Agro.Analytics.Domain.Aggregates;
global using TC.Agro.Analytics.Domain.ValueObjects;
global using TC.Agro.Contracts.Events;
global using TC.Agro.Contracts.Events.SensorIngested;
global using TC.Agro.SharedKernel.Application.Commands;
global using TC.Agro.SharedKernel.Application.Handlers;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Domain.Aggregate;
global using TC.Agro.SharedKernel.Domain.Events;
global using TC.Agro.SharedKernel.Infrastructure.Caching.Service;
global using TC.Agro.SharedKernel.Infrastructure.Messaging;
global using TC.Agro.SharedKernel.Infrastructure.Pagination;
global using TC.Agro.SharedKernel.Infrastructure.UserClaims;
global using Wolverine;
global using CacheTagCatalog = TC.Agro.Analytics.Application.Abstractions.CacheTags;
global using TC.Agro.Analytics.Domain.Snapshots;
global using TC.Agro.Contracts.Events.Identity;
global using TC.Agro.Contracts.Events.Farm;
global using TC.Agro.Analytics.Application.Abstractions.Options.AlertThreshold;

//**//
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
