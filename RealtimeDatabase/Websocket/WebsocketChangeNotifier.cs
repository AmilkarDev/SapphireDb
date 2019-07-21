﻿using RealtimeDatabase.Internal;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Prefilter;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Websocket
{
    public class WebsocketChangeNotifier
    {
        private readonly WebsocketConnectionManager connectionManager;
        private readonly DbContextAccesor dbContextAccessor;
        private readonly IServiceProvider serviceProvider;

        public WebsocketChangeNotifier(WebsocketConnectionManager connectionManager, DbContextAccesor dbContextAccessor, IServiceProvider serviceProvider)
        {
            this.connectionManager = connectionManager;
            this.dbContextAccessor = dbContextAccessor;
            this.serviceProvider = serviceProvider;
        }

        public void HandleChanges(List<ChangeResponse> changes)
        {
            foreach (WebsocketConnection connection in connectionManager.connections)
            {
                Task.Run(() =>
                {
                    RealtimeDbContext db = dbContextAccessor.GetContext();

                    foreach (IGrouping<string, CollectionSubscription> subscriptionGrouping in
                        connection.Subscriptions.GroupBy(s => s.CollectionName))
                    {
                        HandleSubscription(subscriptionGrouping, changes, connection, db);
                    }
                });
            }
        }

        private void HandleSubscription(IGrouping<string, CollectionSubscription> subscriptionGrouping, List<ChangeResponse> changes,
            WebsocketConnection connection, RealtimeDbContext db)
        {
            List<ChangeResponse> relevantChanges =
                        changes.Where(c => c.CollectionName == subscriptionGrouping.Key).ToList();

            KeyValuePair<Type, string> property = db.sets
                .FirstOrDefault(v => v.Value.ToLowerInvariant() == subscriptionGrouping.Key);

            if (property.Key != null)
            {
                relevantChanges = relevantChanges.Where(rc => property.Key.CanQuery(connection.HttpContext, rc.Value, serviceProvider)).ToList();

                List<object> collectionSet = db.GetValues(property).ToList();

                foreach (CollectionSubscription cs in subscriptionGrouping)
                {
                    Task.Run(() =>
                    {
                        cs.Lock.Wait();

                        try
                        {
                            IEnumerable<object> currentCollectionSet = collectionSet;

                            foreach (IPrefilter prefilter in cs.Prefilters.OfType<IPrefilter>())
                            {
                                currentCollectionSet = prefilter.Execute(currentCollectionSet);
                            }

                            IAfterQueryPrefilter afterQueryPrefilter =
                                cs.Prefilters.OfType<IAfterQueryPrefilter>().FirstOrDefault();

                            if (afterQueryPrefilter != null)
                            {
                                List<object> result = currentCollectionSet.Where(v =>
                                        property.Key.CanQuery(connection.HttpContext, v, serviceProvider))
                                    .Select(v => v.GetAuthenticatedQueryModel(connection.HttpContext, serviceProvider))
                                    .ToList();

                                _ = connection.Send(new QueryResponse()
                                {
                                    ReferenceId = cs.ReferenceId,
                                    Result = afterQueryPrefilter.Execute(result)
                                });
                            }
                            else
                            {
                                SendDataToClient(currentCollectionSet.ToList(), property, db, cs, relevantChanges,
                                    connection);
                            }
                        }
                        finally
                        {
                            cs.Lock.Release();
                        }
                    });
                }
            }
        }

        private void SendDataToClient(List<object> currentCollectionSetLoaded,
            KeyValuePair<Type, string> property, RealtimeDbContext db, CollectionSubscription cs, List<ChangeResponse> relevantChanges,
            WebsocketConnection connection)
        {
            List<object[]> currentCollectionPrimaryValues = new List<object[]>();

            foreach (object obj in currentCollectionSetLoaded)
            {
                SendRelevantFilesToClient(property, db, obj, currentCollectionPrimaryValues, cs, relevantChanges, connection);
            }

            foreach (object[] transmittedObject in cs.TransmittedData)
            {
                if (currentCollectionPrimaryValues.All(pks => pks.Except(transmittedObject).Any()))
                {
                    _ = connection.Send(new UnloadResponse
                    {
                        PrimaryValues = transmittedObject,
                        ReferenceId = cs.ReferenceId
                    });
                }
            }

            cs.TransmittedData = currentCollectionPrimaryValues;
        }

        private void SendRelevantFilesToClient(KeyValuePair<Type, string> property, RealtimeDbContext db, object obj,
            List<object[]> currentCollectionPrimaryValues, CollectionSubscription cs, List<ChangeResponse> relevantChanges,
            WebsocketConnection connection)
        {
            object[] primaryValues = property.Key.GetPrimaryKeyValues(db, obj);
            currentCollectionPrimaryValues.Add(primaryValues);

            bool clientHasObject = cs.TransmittedData.Any(pks => !pks.Except(primaryValues).Any());

            if (clientHasObject)
            {
                ChangeResponse change = relevantChanges
                    .FirstOrDefault(c => !c.PrimaryValues.Except(primaryValues).Any());

                if (change != null)
                {
                    change.ReferenceId = cs.ReferenceId;
                    change.Value = change.Value.GetAuthenticatedQueryModel(connection.HttpContext, serviceProvider);
                    _ = connection.Send(change);
                }
            }
            else
            {
                _ = connection.Send(new LoadResponse
                {
                    NewObject = obj.GetAuthenticatedQueryModel(connection.HttpContext, serviceProvider),
                    ReferenceId = cs.ReferenceId
                });
            }
        }
    }
}
