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
// <auto-generated />
namespace HealthGateway.Immunization.Test.Controller
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using System;

    public class ImmunizationController_Test
    {
        [Fact]
        public async Task ShouldGetImmunizations()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Fake Access Token";
            string userId = "1001";
            var expectedAgents = new List<ImmunizationAgent>();
            expectedAgents.Add(new ImmunizationAgent()
            {
                Name = "mocked agent",
                Code = "mocked code",
                LotNumber = "mocekd lot number",
                ProductName = "mocked product",
            });
            var expectedImmunizations = new List<ImmunizationEvent>();
            expectedImmunizations.Add(new ImmunizationEvent()
            {
                DateOfImmunization = DateTime.Today,
                ProviderOrClinic = "Mocked Clinic",
                Immunization = new ImmunizationDefinition()
                {
                    Name = "Mocked Name",
                    ImmunizationAgents = expectedAgents
                }
            });
            // Add a blank agent
            expectedImmunizations.Add(new ImmunizationEvent()
            {
                DateOfImmunization = DateTime.Today,
                Immunization = new ImmunizationDefinition()
                {
                    Name = "Mocked Name",
                    ImmunizationAgents = expectedAgents
                }
            });
            var expectedImmzResult = new ImmunizationResult(
                new LoadStateModel() { RefreshInProgress = false },
                expectedImmunizations,
                new List<ImmunizationRecommendation>());

            RequestResult<ImmunizationResult> expectedRequestResult = new RequestResult<ImmunizationResult>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = expectedImmzResult,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<IImmunizationService> svcMock = new Mock<IImmunizationService>();
            svcMock.Setup(s => s.GetImmunizations(token, 0)).ReturnsAsync(expectedRequestResult);
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            ImmunizationController controller = new ImmunizationController(loggerFactory.CreateLogger<ImmunizationController>(), svcMock.Object, httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetImmunizations(hdid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<ImmunizationResult>>(jsonResult.Value);

            RequestResult<ImmunizationResult> result = (RequestResult<ImmunizationResult>)jsonResult.Value;
            Assert.Equal(Common.Constants.ResultType.Success, result.ResultStatus);
            int count = 0;
            foreach (var immz in result.ResourcePayload.Immunizations)
            {
                count++;
            }
            Assert.Equal(2, count);
        }
    }
}
