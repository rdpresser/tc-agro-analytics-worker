// Global usings for TC.Agro.Analytics.Application
global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Threading.Tasks;
global using Ardalis.Result;
global using FluentValidation;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using GetAlertHistory = TC.Agro.Analytics.Application.UseCases.GetAlertHistory;
global using GetPendingAlerts = TC.Agro.Analytics.Application.UseCases.GetPendingAlerts;
global using GetPlotStatus = TC.Agro.Analytics.Application.UseCases.GetPlotStatus;
global using TC.Agro.Analytics.Application.Abstractions;
global using TC.Agro.Analytics.Application.Abstractions.Ports;
global using TC.Agro.Analytics.Application.Configuration;
global using TC.Agro.Analytics.Domain.Aggregates;
global using TC.Agro.Analytics.Domain.ValueObjects;
global using TC.Agro.SharedKernel.Application.Handlers;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Application.Queries;
global using TC.Agro.SharedKernel.Domain;
global using TC.Agro.SharedKernel.Domain.Aggregate;
global using TC.Agro.SharedKernel.Domain.Events;
global using TC.Agro.SharedKernel.Infrastructure.Pagination;
global using TC.Agro.SharedKernel.Infrastructure.UserClaims;
global using TC.Agro.Contracts.Events;
global using TC.Agro.SharedKernel.Infrastructure.Messaging;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
