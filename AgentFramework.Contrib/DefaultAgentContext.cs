using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentFramework.Contrib.Options;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Extensions;
using AgentFramework.Core.Messages.Connections;
using AgentFramework.Core.Models.Connections;
using AgentFramework.Core.Models.Records;
using Hyperledger.Indy.PoolApi;
using Hyperledger.Indy.WalletApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace AgentFramework.Contrib
{
    public class DefaultAgentContext
    {
        internal IServiceProvider ServiceProvider { get; }
        internal AsyncLazy<IAgentContext> Context;

        /// <summary>
        /// Gets the connections repository
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public ConnectionRepository Connections { get; }

        private async Task<IAgentContext> GetContextAsync()
        {
            var walletService = ServiceProvider.GetRequiredService<IWalletService>();
            var walletOptions = ServiceProvider.GetService<IOptions<WalletOptions>>().Value;

            return new InternalAgentContext
            {
                Wallet = await walletService.GetWalletAsync(
                    walletOptions.WalletConfiguration, walletOptions.WalletCredentials),
                Pool = null
            };
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAgentContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public DefaultAgentContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Context = new AsyncLazy<IAgentContext>(GetContextAsync);
            Connections = new ConnectionRepository(this);
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public static DefaultAgentContext Create() => new DefaultAgentContext(BuildServiceProvider(new ServiceCollection()));

        /// <summary>
        /// Creates the specified wallet options.
        /// </summary>
        /// <param name="walletOptions">The wallet options.</param>
        /// <param name="poolOptions">The pool options.</param>
        /// <returns></returns>
        public static DefaultAgentContext Create(WalletOptions walletOptions, PoolOptions poolOptions)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<WalletOptions>(opt =>
            {
                opt.WalletConfiguration = walletOptions.WalletConfiguration;
                opt.WalletCredentials = walletOptions.WalletCredentials;
            });
            serviceCollection.Configure<PoolOptions>(opt =>
            {
                opt.GenesisFilename = poolOptions.GenesisFilename;
                opt.PoolName = poolOptions.PoolName;
                opt.ProtocolVersion = poolOptions.ProtocolVersion;
            });
            return new DefaultAgentContext(BuildServiceProvider(serviceCollection));
        }

        private static IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAgentFrameworkDefaultServices();
            return serviceCollection.BuildServiceProvider();
        }
    }

    internal class InternalAgentContext : IAgentContext
    {
        public Wallet Wallet { get; set; }
        public Pool Pool { get; set; }
        public Dictionary<string, string> State { get; set; }
        public ConnectionRecord Connection { get; set; }
    }

    public class ConnectionRepository
    {
        internal DefaultAgentContext AgentContext { get; }

        internal IConnectionService ConnectionService { get; }

        internal ConnectionRepository(DefaultAgentContext agentContext)
        {
            AgentContext = agentContext;
            ConnectionService = agentContext.ServiceProvider.GetRequiredService<IConnectionService>();
        }

        /// <summary>
        /// Creates the invitation asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<CreateInvitationResult> CreateInvitationAsync()
        {
            return await ConnectionService.CreateInvitationAsync(await AgentContext.Context);
        }

        /// <summary>
        /// Accepts the invitation asynchronous.
        /// </summary>
        /// <param name="invitationMessage">The invitation message.</param>
        /// <returns></returns>
        public async Task<AcceptInvitationResult> AcceptInvitationAsync(ConnectionInvitationMessage invitationMessage)
        {
            return await ConnectionService.AcceptInvitationAsync(await AgentContext.Context, invitationMessage);
        }
    }
}