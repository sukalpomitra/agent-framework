using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AgentFramework.Core.Extensions
{
    /// <summary>
    /// Dependency extensions
    /// </summary>
    public static class DependencyExtensions
    {
        /// <summary>
        /// Adds the agent framework default services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void AddAgentFrameworkDefaultServices(this IServiceCollection builder)
        {
            builder.AddLogging();

            builder.TryAddSingleton<IEventAggregator, EventAggregator>();
            builder.TryAddSingleton<IConnectionService, DefaultConnectionService>();
            builder.TryAddSingleton<ICredentialService, DefaultCredentialService>();
            builder.TryAddSingleton<ILedgerService, DefaultLedgerService>();
            builder.TryAddSingleton<IPoolService, DefaultPoolService>();
            builder.TryAddSingleton<IProofService, DefaultProofService>();
            builder.TryAddSingleton<IProvisioningService, DefaultProvisioningService>();
            builder.TryAddSingleton<IMessageService, DefaultMessageService>();
            builder.TryAddSingleton<IAgentContextProvider, DefaultAgentContextProvider>();
            builder.TryAddSingleton<HttpMessageHandler, HttpClientHandler>();
            builder.TryAddSingleton<ISchemaService, DefaultSchemaService>();
            builder.TryAddSingleton<ITailsService, DefaultTailsService>();
            builder.TryAddSingleton<IWalletRecordService, DefaultWalletRecordService>();
            builder.TryAddSingleton<IWalletService, DefaultWalletService>();
        }
    }
}
