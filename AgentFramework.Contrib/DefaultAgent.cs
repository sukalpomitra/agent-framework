using System;
using System.Threading.Tasks;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Extensions;
using AgentFramework.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace AgentFramework.Contrib
{
    public class DefaultAgent
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

        /// <summary>
        /// Gets the credentials repository
        /// </summary>
        /// <value>The credentials.</value>
        public CredentialRepository Credentials { get; }

        /// <summary>
        /// Gets the proofs.
        /// </summary>
        /// <value>The proofs.</value>
        public ProofRepository Proofs { get; }

        private Task<IAgentContext> GetContextAsync(string agentId = null)
        {
            return ServiceProvider.GetRequiredService<IAgentContextProvider>().GetContextAsync(agentId);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAgent"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public DefaultAgent(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Context = new AsyncLazy<IAgentContext>(() => GetContextAsync());
            Connections = new ConnectionRepository(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AgentFramework.Contrib.DefaultAgent"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public DefaultAgent(IServiceProvider serviceProvider, string agentId)
        {
            ServiceProvider = serviceProvider;
            Context = new AsyncLazy<IAgentContext>(() => GetContextAsync(agentId));
            Connections = new ConnectionRepository(this);
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public static DefaultAgent Create() => new DefaultAgent(BuildServiceProvider(new ServiceCollection()));

        /// <summary>
        /// Create the specified agentId.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="agentId">Agent identifier.</param>
        public static DefaultAgent Create(string agentId) => new DefaultAgent(BuildServiceProvider(new ServiceCollection()), agentId);

        /// <summary>
        /// Creates the specified wallet options.
        /// </summary>
        /// <param name="walletOptions">The wallet options.</param>
        /// <param name="poolOptions">The pool options.</param>
        /// <returns></returns>
        public static DefaultAgent Create(WalletOptions walletOptions, PoolOptions poolOptions)
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
            return new DefaultAgent(BuildServiceProvider(serviceCollection));
        }

        private static IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAgentFrameworkDefaultServices();
            return serviceCollection.BuildServiceProvider();
        }
    }
}