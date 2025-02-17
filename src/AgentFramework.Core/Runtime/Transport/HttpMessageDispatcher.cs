﻿using AgentFramework.Core.Contracts;
using AgentFramework.Core.Exceptions;
using AgentFramework.Core.Messages;
using AgentFramework.Core.Runtime.Responses;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AgentFramework.Core.Runtime.Transport
{
    /// <summary>
    /// Http message dispatcher.
    /// </summary>
    public class HttpMessageDispatcher : IMessageDispatcher
    {
        /// <summary>The HTTP client</summary>
        protected readonly HttpClient HttpClient;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public HttpMessageDispatcher(IHttpClientFactory httpClientFactory)
        {
            HttpClient = httpClientFactory.CreateClient();
        }

        /// <inheritdoc />
        public string[] TransportSchemes => new[] { "http", "https" };
        
        /// <inheritdoc />
        public async Task<MessageContext> DispatchAsync(Uri endpointUri, MessageContext message)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = endpointUri,
                Method = HttpMethod.Post,
                Content = new ByteArrayContent(message.Payload)
            };

            var agentContentType = new MediaTypeHeaderValue(DefaultMessageService.AgentWireMessageMimeType);
            request.Content.Headers.ContentType = agentContentType;

            var response = await HttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new AgentFrameworkException(
                    ErrorCode.A2AMessageTransmissionError, $"Failed to send A2A message with an HTTP status code of {response.StatusCode} and content {responseBody}");
            }

            if (response.Content?.Headers.ContentType?.Equals(agentContentType) ?? false)
            {
                var rawContent = await response.Content.ReadAsByteArrayAsync();

                //TODO this assumes all messages are packed
                if (rawContent.Length > 0)
                {
                    return new MessageContext(rawContent, true);
                }
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<List<MessageContext>> ConsumeAsync(Uri endpointUri)
        {
            var response = await HttpClient.GetAsync(endpointUri);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new AgentFrameworkException(
                    ErrorCode.A2AMessageTransmissionError, $"Failed to consume A2A message with an HTTP status code of {response.StatusCode} and content {responseBody}");
            }

            var messages = JsonConvert.DeserializeObject<List<CloudAgentResponse>>(responseBody);
            List<MessageContext> messageContexts = new List<MessageContext>();

            foreach (var message in messages)
            {
                messageContexts.Add(new MessageContext(message.message, message.packed));
            }
            return messageContexts;
        }
    }
}
