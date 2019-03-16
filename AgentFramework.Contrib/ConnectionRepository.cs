using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Messages.Connections;
using AgentFramework.Core.Models.Connections;
using AgentFramework.Core.Models.Records;
using AgentFramework.Core.Models.Records.Search;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Contrib
{
    public class ConnectionRepository
    {
        internal DefaultAgent Agent { get; }

        internal IConnectionService ConnectionService { get; }

        internal IMessageService MessageService { get; }

        internal ConnectionRepository(DefaultAgent agent)
        {
            Agent = agent;
            ConnectionService = agent.ServiceProvider.GetRequiredService<IConnectionService>();
            MessageService = agent.ServiceProvider.GetRequiredService<IMessageService>();
        }

        /// <summary>
        /// Creates the invitation asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<CreateInvitationResult> CreateInvitationAsync()
        {
            return await ConnectionService.CreateInvitationAsync(await Agent.Context);
        }

        /// <summary>
        /// Accepts the invitation asynchronous.
        /// </summary>
        /// <param name="invitationMessage">The invitation message.</param>
        /// <returns>The connection record created for this invitation</returns>
        public async Task<ConnectionRecord> AcceptInvitationAsync(ConnectionInvitationMessage invitationMessage)
        {
            var context = await Agent.Context;
            var result = await ConnectionService.AcceptInvitationAsync(context, invitationMessage);

            await MessageService.SendToConnectionAsync(context.Wallet, result.Request, result.Connection,
                invitationMessage.RecipientKeys.First());

            return result.Connection;
        }

        /// <summary>
        /// Gets the connection record with the specified identifer
        /// </summary>
        /// <returns>The connection record.</returns>
        /// <param name="connectionId">Connection identifier.</param>
        public async Task<ConnectionRecord> GetAsync(string connectionId)
        {
            var context = await Agent.Context;
            return await ConnectionService.GetAsync(context, connectionId);
        }

        /// <summary>
        /// Get a list of connection records for the supplied search query
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="searchQuery">Search query.</param>
        /// <param name="count">Count.</param>
        public async Task<IEnumerable<ConnectionRecord>> ListAsync(ISearchQuery searchQuery, int count)
        {
            var context = await Agent.Context;
            return await ConnectionService.ListAsync(context, searchQuery, count);
        }

        /// <summary>
        /// Get a list of connection records.
        /// </summary>
        /// <returns>The async.</returns>
        public Task<IEnumerable<ConnectionRecord>> ListAsync() => ListAsync(null, 100);

        /// <summary>
        /// Get a list of connection records with the given connection state
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="state">State.</param>
        public Task<IEnumerable<ConnectionRecord>> ListAsync(ConnectionState state) 
            => ListAsync(SearchQuery.Equal(nameof(ConnectionRecord.State), state.ToString("G")), 100);
    }
}