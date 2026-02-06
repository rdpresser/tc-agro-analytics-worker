// Global usings for TC.Agro.Analytics.Application
global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Ardalis.Result;
global using FluentValidation;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using TC.Agro.Analytics.Application.Abstractions.Ports;
global using TC.Agro.Analytics.Application.Configuration;
global using TC.Agro.Analytics.Domain.Aggregates;
global using TC.Agro.Analytics.Domain.Entities;
global using TC.Agro.Contracts.Events.Analytics;
global using TC.Agro.SharedKernel.Application.Handlers;
global using TC.Agro.SharedKernel.Application.Ports;
global using TC.Agro.SharedKernel.Domain;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TC.Agro.Analytics.Tests")]
