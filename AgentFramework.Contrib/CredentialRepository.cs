using System.Collections.Generic;
using System.Threading.Tasks;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Models.Credentials;
using AgentFramework.Core.Models.Records;
using AgentFramework.Core.Models.Records.Search;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFramework.Contrib
{
    public class CredentialRepository
    {
        internal DefaultAgent Agent { get; }

        internal IConnectionService ConnectionService { get; }

        internal ICredentialService CredentialService { get; }

        internal IProvisioningService ProvisioningService { get; }

        internal IMessageService MessageService { get; }

        internal CredentialRepository(DefaultAgent agent)
        {
            Agent = agent;
            ConnectionService = agent.ServiceProvider.GetRequiredService<IConnectionService>();
            CredentialService = agent.ServiceProvider.GetRequiredService<ICredentialService>();
            MessageService = agent.ServiceProvider.GetRequiredService<IMessageService>();
        }

        /// <summary>
        /// Sends the offer async.
        /// </summary>
        /// <returns>The offer async.</returns>
        /// <param name="definitionId">Definition identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        public async Task<CredentialRecord> SendOfferAsync(string definitionId, string connectionId)
        {
            var context = await Agent.Context;
            var connection = await ConnectionService.GetAsync(context, connectionId);

            var result = await CredentialService.CreateOfferAsync(context, 
                new OfferConfiguration { CredentialDefinitionId = definitionId }, connectionId);

            await MessageService.SendToConnectionAsync(context.Wallet, result.Item1, connection);

            return result.Item2;
        }

        /// <summary>
        /// Accepts the offer async.
        /// </summary>
        /// <returns>The offer async.</returns>
        /// <param name="credentialId">Credential identifier.</param>
        public async Task AcceptOfferAsync(string credentialId)
        {
            var context = await Agent.Context;
            var credential = await CredentialService.GetAsync(context, credentialId);
            var connection = await ConnectionService.GetAsync(context, credential.ConnectionId);

            var result = await CredentialService.AcceptOfferAsync(context, credentialId);

            await MessageService.SendToConnectionAsync(context.Wallet, result, connection);
        }

        /// <summary>
        /// Issues the credential async.
        /// </summary>
        /// <returns>The credential async.</returns>
        /// <param name="credentialId">Credential identifier.</param>
        /// <param name="values">Values.</param>
        public async Task IssueCredentialAsync(string credentialId, Dictionary<string, string> values)
        {
            var context = await Agent.Context;
            var credential = await CredentialService.GetAsync(context, credentialId);
            var connection = await ConnectionService.GetAsync(context, credential.ConnectionId);
            var provisioning = await ProvisioningService.GetProvisioningAsync(context.Wallet);

            var result = await CredentialService.IssueCredentialAsync(context,provisioning.IssuerDid, 
                credentialId, values);

            await MessageService.SendToConnectionAsync(context.Wallet, result, connection);
        }

        /// <summary>
        /// Gets the credential record with the specified identifer
        /// </summary>
        /// <returns>The credential record.</returns>
        /// <param name="credentialId">Connection identifier.</param>
        public async Task<CredentialRecord> GetAsync(string credentialId)
        {
            var context = await Agent.Context;
            return await CredentialService.GetAsync(context, credentialId);
        }

        /// <summary>
        /// Get a list of credential records for the supplied search query
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="searchQuery">Search query.</param>
        /// <param name="count">Count.</param>
        public async Task<IEnumerable<CredentialRecord>> ListAsync(ISearchQuery searchQuery, int count)
        {
            var context = await Agent.Context;
            return await CredentialService.ListAsync(context, searchQuery, count);
        }

        /// <summary>
        /// Get a list of credential records.
        /// </summary>
        /// <returns>The async.</returns>
        public Task<IEnumerable<CredentialRecord>> ListAsync() => ListAsync(null, 100);

        /// <summary>
        /// Get a list of credential records with the given credential state
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="state">State.</param>
        public Task<IEnumerable<CredentialRecord>> ListAsync(CredentialState state)
            => ListAsync(SearchQuery.Equal(nameof(CredentialRecord.State), state.ToString("G")), 100);
    }
}