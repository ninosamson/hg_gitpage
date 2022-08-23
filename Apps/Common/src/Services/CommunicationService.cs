// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Services
{
    using System;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class CommunicationService : ICommunicationService
    {
        /// <summary>
        /// The Cache key used to store Communication Banners.
        /// </summary>
        public const string BannerCacheKey = "Communication:Banner";

        /// <summary>
        /// The Cache key used to store Communication InApp Banners.
        /// </summary>
        public const string InAppCacheKey = "Communication:InApp";

        private const string Update = "UPDATE";
        private const string Insert = "INSERT";

        private readonly ILogger logger;
        private readonly ICommunicationDelegate communicationDelegate;
        private readonly ICacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="communicationDelegate">Injected Note delegate.</param>
        /// <param name="cacheProvider">The cache to use to reduce lookups.</param>
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate, ICacheProvider cacheProvider)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
            this.cacheProvider = cacheProvider;
        }

        /// <inheritdoc/>
        public RequestResult<Communication?> GetActiveBanner(CommunicationType communicationType)
        {
            if (communicationType != CommunicationType.Banner && communicationType != CommunicationType.InApp)
            {
                throw new ArgumentOutOfRangeException(nameof(communicationType), "Communication Type must be Banner or InApp");
            }

            RequestResult<Communication?>? cacheEntry = this.GetCommunicationFromCache(communicationType);
            if (cacheEntry == null)
            {
                this.logger.LogInformation("Active Communication not found in cache, getting from DB...");
                DBResult<Communication?> dbResult = this.communicationDelegate.GetNext(communicationType);
                if (dbResult.Status == DBStatusCode.Read || dbResult.Status == DBStatusCode.NotFound)
                {
                    cacheEntry = this.AddCommunicationToCache(dbResult.Payload, communicationType);
                }
                else
                {
                    this.logger.LogInformation($"Error getting Communication from DB {dbResult.Message}");
                    cacheEntry = new()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new()
                        {
                            ResultMessage = dbResult.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                    };
                }
            }

            if (DateTime.UtcNow < cacheEntry.ResourcePayload?.EffectiveDateTime)
            {
                this.logger.LogDebug("Banner is future dated, returning empty RequestResult");
                cacheEntry = new()
                {
                    ResultStatus = ResultType.Success,
                    TotalResultCount = 0,
                };
            }

            return cacheEntry;
        }

        /// <inheritdoc/>
        public void ProcessChange(BannerChangeEvent changeEvent)
        {
            Communication? communication = changeEvent.Data;
            if (communication is not null &&
                (communication.CommunicationTypeCode == CommunicationType.Banner ||
                 communication.CommunicationTypeCode == CommunicationType.InApp))
            {
                RequestResult<Communication?>? cacheEntry = this.GetCommunicationFromCache(communication.CommunicationTypeCode);
                if (cacheEntry?.ResourcePayload != null)
                {
                    Communication cachedComm = cacheEntry.ResourcePayload;
                    if (cachedComm.Id == communication.Id)
                    {
                        this.logger.LogInformation($"{changeEvent.Action} ChangeEvent for Communication {communication.Id} found in Cache");
                        this.RemoveCommunicationFromCache(communication.CommunicationTypeCode);
                        if (changeEvent.Action == Insert || changeEvent.Action == Update)
                        {
                            this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
                        }
                        else
                        {
                            // Delete: We don't cache the empty result as a future dated comm may exist and the next call to GetActiveBanner will find it.
                            this.logger.LogInformation($"{changeEvent.Action} ChangeEvent for Communication {communication.Id} was processed and removed from Cache");
                        }
                    }
                    else
                    {
                        // Check the new comm to see if it is effective earlier and not expired
                        if (DateTime.UtcNow < communication.ExpiryDateTime && communication.EffectiveDateTime < cachedComm.EffectiveDateTime)
                        {
                            this.logger.LogInformation($"{changeEvent.Action} ChangeEvent for Communication {communication.Id} replacing {cachedComm.Id}");
                            this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
                        }
                        else
                        {
                            this.logger.LogInformation($"{changeEvent.Action} ChangeEvent for Communication {communication.Id} being ignored");
                        }
                    }
                }
                else
                {
                    this.logger.LogInformation($"No Communications in the Cache, processing {changeEvent.Action} for communication {communication.Id}");
                    if (changeEvent.Action == Insert || changeEvent.Action == Update)
                    {
                        this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            this.RemoveCommunicationFromCache(CommunicationType.Banner);
            this.RemoveCommunicationFromCache(CommunicationType.InApp);
        }

        private void RemoveCommunicationFromCache(CommunicationType cacheType)
        {
            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            this.cacheProvider.RemoveItem(cacheKey);
        }

        private RequestResult<Communication?>? GetCommunicationFromCache(CommunicationType cacheType)
        {
            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            RequestResult<Communication?>? cacheEntry = this.cacheProvider.GetItem<RequestResult<Communication?>>(cacheKey);
            return cacheEntry;
        }

        private RequestResult<Communication?> AddCommunicationToCache(Communication? communication, CommunicationType cacheType)
        {
            RequestResult<Communication?> cacheEntry = new()
            {
                ResourcePayload = communication,
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
            };
            DateTime now = DateTime.UtcNow;
            TimeSpan? expiry = null;
            if (communication != null && now < communication.ExpiryDateTime)
            {
                if (now < communication.EffectiveDateTime)
                {
                    this.logger.LogInformation($"Communication {communication.Id} is not effective, cached empty communication until {communication.EffectiveDateTime}");
                    expiry = communication.EffectiveDateTime - now;
                }
                else
                {
                    this.logger.LogInformation($"Caching communication {communication.Id} until {communication.ExpiryDateTime}");
                    expiry = communication.ExpiryDateTime - now;
                    cacheEntry.TotalResultCount = 1;
                }
            }
            else
            {
                this.logger.LogInformation("Communication is expired or null, caching Empty RequestResult forever");
                cacheEntry.ResourcePayload = null;
            }

            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            this.cacheProvider.AddItem(cacheKey, cacheEntry, expiry);
            return cacheEntry;
        }
    }
}