using System;
using Azure.Messaging.ServiceBus;
using EasyGroceries.Common.Configuration;
using EasyGroceries.Common.Messaging;
using EasyGroceries.Common.Messaging.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyGroceries.Common.DependencyInjection;

public static class MessagingExtensions
{
    public static IFunctionsHostBuilder AddMessaging(this IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
        var messagingSection = configuration.GetSection(MessagingOptions.SectionName);
        var messagingOptions = messagingSection.Get<MessagingOptions>();

        ArgumentNullException.ThrowIfNull(messagingOptions);
        ArgumentNullException.ThrowIfNull(messagingOptions.ConnectionString);

        builder.Services.TryAddSingleton(s => new ServiceBusClient(messagingOptions.ConnectionString));
        builder.Services.TryAddSingleton<IMessagingService, MessagingService>();

        return builder;
    }
}