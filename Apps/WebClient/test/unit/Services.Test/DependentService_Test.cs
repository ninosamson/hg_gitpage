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
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using System;
    using HealthGateway.WebClient.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Services;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class DependentService_Test
    {
        private const string mockParentHdId = "MockFirstName";
        private const string mockPHN = "MockPHN";
        private const string mockFirstName = "MockFirstName";
        private const string mockLastName = "MockLastName";
        private DateTime mockTestDate = new DateTime(2020, 1, 10);
        private DateTime mockDateOfBirth = new DateTime(2010, 10, 10);
        private const string mockGender = "Male";
        private const string mockHdId = "MockHdId";
        private const string mismatchedError = "The information you entered does not match our records. Please try again.";
        private const string noHdIdError = "Please ensure you are using a current BC Services Card.";

        private IDependentService SetupCommonMocks(Mock<IResourceDelegateDelegate> mockDependentDelegate, Mock<IPatientService> mockPatientService)
        {
            Mock<IUserProfileDelegate> mockUserProfileDelegate = new Mock<IUserProfileDelegate>();
            Mock<INotificationSettingsService> mockNotificationSettingsService = new Mock<INotificationSettingsService>();
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration());

            return new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                mockPatientService.Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                configServiceMock.Object
            );
        }

        private IEnumerable<ResourceDelegate> GenerateMockResourceDelegatesList()
        {
            List<ResourceDelegate> resourceDelegates = new List<ResourceDelegate>();

            for (int i = 0; i < 10; i++)
            {
                resourceDelegates.Add(new ResourceDelegate()
                {
                    ProfileHdid = mockParentHdId,
                    ResourceOwnerHdid = $"{mockHdId}-{i}",
                });
            }
            return resourceDelegates;
        }

        private IDependentService SetupMockForGetDependents(RequestResult<PatientModel> patientResult = null)
        {
            // (1) Setup ResourceDelegateDelegate's mock
            IEnumerable<ResourceDelegate> expectedResourceDelegates = GenerateMockResourceDelegatesList();

            DBResult<IEnumerable<ResourceDelegate>> readResult = new DBResult<IEnumerable<ResourceDelegate>>
            {
                Status = DBStatusCode.Read,
            };
            readResult.Payload = expectedResourceDelegates;

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new Mock<IResourceDelegateDelegate>();
            mockDependentDelegate.Setup(s => s.Get(mockParentHdId, 0, 500)).Returns(readResult);

            // (2) Setup PatientDelegate's mock
            var mockPatientService = new Mock<IPatientService>();

            RequestResult<string> patientHdIdResult = new RequestResult<string>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = mockHdId
            };

            if (patientResult == null)
            {
                patientResult = new RequestResult<PatientModel>();
                patientResult.ResultStatus = Common.Constants.ResultType.Success;
                patientResult.ResourcePayload = new PatientModel()
                {
                    HdId = mockHdId,
                    PersonalHealthNumber = mockPHN,
                    FirstName = mockFirstName,
                    LastName = mockLastName,
                    Birthdate = mockDateOfBirth,
                    Gender = mockGender,
                };
            }
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).Returns(Task.FromResult(patientResult));

            // (3) Setup other common Mocks
            IDependentService dependentService = SetupCommonMocks(mockDependentDelegate, mockPatientService);

            return dependentService;
        }

        [Fact]
        public void GetDependents()
        {
            IDependentService service = SetupMockForGetDependents();
            RequestResult<IEnumerable<DependentModel>> actualResult = service.GetDependents(mockParentHdId, 0, 500);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(10, actualResult.TotalResultCount);

            // Validate masked PHN
            foreach (DependentModel model in actualResult.ResourcePayload)
            {
                Assert.Equal(model.DependentInformation.PHN, mockPHN);
            }
        }

        [Fact]
        public void GetDependentsWithEmptyPatientResPayloadError()
        {
            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>();

            IDependentService service = SetupMockForGetDependents(patientResult);
            RequestResult<IEnumerable<DependentModel>> actualResult = service.GetDependents(mockParentHdId, 0, 500);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CE-PAT", actualResult.ResultError.ErrorCode);
            Assert.Equal("Communication Exception when trying to retrieve Dependent(s) - HdId: MockHdId-0; HdId: MockHdId-1; HdId: MockHdId-2; HdId: MockHdId-3; HdId: MockHdId-4; HdId: MockHdId-5; HdId: MockHdId-6; HdId: MockHdId-7; HdId: MockHdId-8; HdId: MockHdId-9;", actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependent()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        [Fact]
        public void ValidateDependentWithEmptyPatientResPayloadError()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>();
            // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
            IDependentService service = SetupMockDependentService(addDependentRequest, null, patientResult);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CE-PAT", actualResult.ResultError.ErrorCode);
        }

        [Fact]
        public void ValidateDependentWithDbError()
        {
            DBResult<ResourceDelegate> insertResult = new DBResult<ResourceDelegate>
            {
                Payload = null,
                Status = DBStatusCode.Error
            };
            AddDependentRequest addDependentRequest = SetupMockInput();
            IDependentService service = SetupMockDependentService(addDependentRequest, insertResult);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            var serviceError = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database);
            Assert.Equal(serviceError, actualResult.ResultError.ErrorCode);
        }

        [Fact]
        public void ValidateDependentWithWrongFirstName()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.FirstName = "wrong";
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            var userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(Common.Constants.ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError.ErrorCode);
            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongLastName()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.LastName = "wrong";
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            var userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(Common.Constants.ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError.ErrorCode);
            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithWrongDateOfBirth()
        {
            AddDependentRequest addDependentRequest = SetupMockInput();
            addDependentRequest.DateOfBirth = DateTime.Now;
            IDependentService service = SetupMockDependentService(addDependentRequest);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            var userError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
            Assert.Equal(Common.Constants.ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError.ErrorCode);
            Assert.Equal(mismatchedError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateDependentWithNoHdId()
        {
            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>();
            patientResult.ResultStatus = Common.Constants.ResultType.Success;
            patientResult.ResourcePayload = new PatientModel()
            {
                HdId = string.Empty,
                PersonalHealthNumber = mockPHN,
                FirstName = mockFirstName,
                LastName = mockLastName,
                Birthdate = mockDateOfBirth,
                Gender = mockGender,
            };
            AddDependentRequest addDependentRequest = SetupMockInput();
            IDependentService service = SetupMockDependentService(addDependentRequest, patientResult: patientResult);
            RequestResult<DependentModel> actualResult = service.AddDependent(mockParentHdId, addDependentRequest);

            var userError = ErrorTranslator.ActionRequired(ErrorMessages.InvalidServicesCard, ActionType.NoHdId);
            Assert.Equal(Common.Constants.ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(userError.ErrorCode, actualResult.ResultError.ErrorCode);
            Assert.Equal(noHdIdError, actualResult.ResultError.ResultMessage);
        }

        [Fact]
        public void ValidateRemove()
        {
            DependentModel delegateModel = new DependentModel() { OwnerId = mockHdId, DelegateId = mockParentHdId };
            Mock<IResourceDelegateDelegate> mockDependentDelegate = new Mock<IResourceDelegateDelegate>();
            mockDependentDelegate.Setup(s => s.Delete(It.Is<ResourceDelegate>(d => d.ResourceOwnerHdid == mockHdId && d.ProfileHdid == mockParentHdId), true)).Returns(new DBResult<ResourceDelegate>()
            {
                Status = DBStatusCode.Deleted,
            });

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new Mock<IUserProfileDelegate>();
            mockUserProfileDelegate.Setup(s => s.GetUserProfile(mockParentHdId)).Returns(new DBResult<UserProfile>() { Payload = new UserProfile() });
            Mock<INotificationSettingsService> mockNotificationSettingsService = new Mock<INotificationSettingsService>();
            mockNotificationSettingsService.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration());
            IDependentService service = new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                new Mock<IPatientService>().Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                configServiceMock.Object
            );
            RequestResult<DependentModel> actualResult = service.Remove(delegateModel);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }


        private IDependentService SetupMockDependentService(AddDependentRequest addDependentRequest, DBResult<ResourceDelegate> insertResult = null, RequestResult<PatientModel> patientResult = null)
        {
            var mockPatientService = new Mock<IPatientService>();

            RequestResult<string> patientHdIdResult = new RequestResult<string>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = mockHdId
            };
            if (addDependentRequest.PHN.Equals(mockPHN))
            {
                // Test Scenario - Happy Path: HiId found for the mockPHN
                patientHdIdResult.ResultStatus = Common.Constants.ResultType.Success;
                patientHdIdResult.ResourcePayload = mockHdId;
            }
            if (patientResult == null)
            {
                patientResult = new RequestResult<PatientModel>();
                // Test Scenario - Happy Path: Found HdId for the PHN, Found Patient.
                patientResult.ResultStatus = Common.Constants.ResultType.Success;
                patientResult.ResourcePayload = new PatientModel()
                {
                    HdId = mockHdId,
                    PersonalHealthNumber = mockPHN,
                    FirstName = mockFirstName,
                    LastName = mockLastName,
                    Birthdate = mockDateOfBirth,
                    Gender = mockGender,
                };
            }
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).Returns(Task.FromResult(patientResult));

            ResourceDelegate expectedDbDependent = new ResourceDelegate() { ProfileHdid = mockParentHdId, ResourceOwnerHdid = mockHdId };

            if (insertResult == null)
            {
                insertResult = new DBResult<ResourceDelegate>
                {
                    Status = DBStatusCode.Created
                };
            }
            insertResult.Payload = expectedDbDependent;

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new Mock<IResourceDelegateDelegate>();
            mockDependentDelegate.Setup(s => s.Insert(It.Is<ResourceDelegate>(r => r.ProfileHdid == expectedDbDependent.ProfileHdid && r.ResourceOwnerHdid == expectedDbDependent.ResourceOwnerHdid), true)).Returns(insertResult);

            Mock<IUserProfileDelegate> mockUserProfileDelegate = new Mock<IUserProfileDelegate>();
            mockUserProfileDelegate.Setup(s => s.GetUserProfile(mockParentHdId)).Returns(new DBResult<UserProfile>() { Payload = new UserProfile() });
            Mock<INotificationSettingsService> mockNotificationSettingsService = new Mock<INotificationSettingsService>();
            mockNotificationSettingsService.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration());
            return new DependentService(
                new Mock<ILogger<DependentService>>().Object,
                mockUserProfileDelegate.Object,
                mockPatientService.Object,
                mockNotificationSettingsService.Object,
                mockDependentDelegate.Object,
                configServiceMock.Object
            );
        }

        private AddDependentRequest SetupMockInput()
        {
            return new AddDependentRequest
            {
                PHN = mockPHN,
                FirstName = mockFirstName,
                LastName = mockLastName,
                TestDate = mockTestDate,
                DateOfBirth = mockDateOfBirth
            };
        }
    }
}
