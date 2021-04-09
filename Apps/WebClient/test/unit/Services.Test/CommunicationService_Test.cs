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
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using HealthGateway.Database.Constants;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Caching.Memory;

    public class CommunicationServiceTest
    {
        private Tuple<RequestResult<Communication>, Communication> ExecuteGetActiveCommunication(Database.Constants.DBStatusCode dbResultStatus = Database.Constants.DBStatusCode.Read)
        {
            Communication communication = new Communication
            {
                Id = Guid.NewGuid(),
                EffectiveDateTime = DateTime.UtcNow.AddDays(-1),
                ExpiryDateTime = DateTime.UtcNow.AddDays(2)
            };

            DBResult<Communication> dbResult = new DBResult<Communication>
            {
                Payload = communication,
                Status = dbResultStatus
            };

            ServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IMemoryCache memoryCache = serviceProvider.GetService<IMemoryCache>();

            Mock<ICommunicationDelegate> communicationDelegateMock = new Mock<ICommunicationDelegate>();
            communicationDelegateMock.Setup(s => s.GetActiveBanner()).Returns(dbResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                memoryCache
            );
            RequestResult<Communication> actualResult = service.GetActiveBanner();

            return new Tuple<RequestResult<Communication>, Communication>(actualResult, communication);
        }

        [Fact]
        public void ShouldGetActiveCommunication()
        {
            Tuple<RequestResult<Communication>, Communication> result = ExecuteGetActiveCommunication(Database.Constants.DBStatusCode.Read);
            var actualResult = result.Item1;
            var communication = result.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(communication));
        }

        [Fact]
        public void ShouldGetActiveCommunicationWithDBError()
        {
            Tuple<RequestResult<Communication>, Communication> result = ExecuteGetActiveCommunication(Database.Constants.DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", actualResult.ResultError.ErrorCode);
        }
    }
}
