﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SapphireDb.Command.Subscribe;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Nlb.Models;

namespace SapphireDb.Nlb
{
    public class NlbManager
    {
        private readonly SapphireDatabaseOptions options;
        private readonly IHttpClientFactory httpClientFactory;

        public NlbManager(SapphireDatabaseOptions options, IHttpClientFactory httpClientFactory)
        {
            this.options = options;
            this.httpClientFactory = httpClientFactory;
        }

        public void SendChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            SendChangesRequest sendChangesRequest = new SendChangesRequest()
            {
                Changes = changes,
                DbType = dbContextType.FullName
            };

            SendToNlbs(sendChangesRequest, "changes");
        }

        public void SendPublish(string topic, object message)
        {
            SendPublishRequest sendPublishRequest = new SendPublishRequest()
            {
                Topic = topic,
                Message = message
            };

            SendToNlbs(sendPublishRequest, "publish");
        }

        public void SendMessage(object message)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest()
            {
                Message = message
            };

            SendToNlbs(sendMessageRequest, "message");
        }

        private void SendToNlbs(object messageObject, string path)
        {
            if (!options.Nlb.Enabled)
            {
                return;
            }

            string requestString = JsonHelper.Serialize(messageObject).Encrypt(options.Nlb.EncryptionKey);

            options.Nlb.Entries.ForEach(nlbEntry =>
            {
                Task.Run(async () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                        $"{(nlbEntry.Url.EndsWith('/') ? nlbEntry.Url : nlbEntry.Url + "/")}sapphire/nlb/{path}");
                    request.Headers.Add("Secret", nlbEntry.Secret);
                    request.Headers.Add("OriginId", options.Nlb.Id);
                    request.Content = new StringContent(requestString);

                    HttpClient client = httpClientFactory.CreateClient();
                    await client.SendAsync(request);
                });
            });
        }
    }
}
