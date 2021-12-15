//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommunicationService's Unit Tests.
    /// </summary>
    public class CommunicationServiceTests
    {
        /// <summary>
        /// GetActiveCommunication - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetActiveCommunication()
        {
            (RequestResult<Communication> actualResult, Communication communication) = ExecuteGetActiveCommunication(DBStatusCode.Read);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            communication.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetActiveCommunication - Database Error.
        /// </summary>
        [Fact]
        public void ShouldGetActiveCommunicationWithDBError()
        {
            (RequestResult<Communication> actualResult, _) = ExecuteGetActiveCommunication(DBStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        private static (RequestResult<Communication> ActualResult, Communication Communication) ExecuteGetActiveCommunication(DBStatusCode dbResultStatus = DBStatusCode.Read)
        {
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                EffectiveDateTime = DateTime.UtcNow.AddDays(-1),
                ExpiryDateTime = DateTime.UtcNow.AddDays(2),
            };

            DBResult<Communication> dbResult = new()
            {
                Payload = communication,
                Status = dbResultStatus,
            };

            ServiceCollection services = new();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IMemoryCache? memoryCache = serviceProvider.GetService<IMemoryCache>();

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.GetActiveBanner(CommunicationType.Banner)).Returns(dbResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                memoryCache);
            RequestResult<Communication> actualResult = service.GetActiveBanner(CommunicationType.Banner);

            return (actualResult, communication);
        }
    }
}
