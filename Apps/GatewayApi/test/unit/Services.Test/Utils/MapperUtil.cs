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

namespace HealthGateway.GatewayApi.Test.Services.Utils
{
    using AutoMapper;
    using HealthGateway.GatewayApi.MapProfiles;

    /// <summary>
    /// Static utility class to provide a fully initialized AutoMapper.
    /// NOTE:  Any newly added profiles will have to be registered.
    /// </summary>
    public static class MapperUtil
    {
        /// <summary>
        /// Creates an AutoMapper.
        /// </summary>
        /// <returns>A configured AutoMapper.</returns>
        public static IMapper InitializeAutoMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DependentInformationProfile());
                cfg.AddProfile(new DependentProfile());
                cfg.AddProfile(new TermsOfServiceProfile());
                cfg.AddProfile(new UserCommentProfile());
                cfg.AddProfile(new UserNoteProfile());
                cfg.AddProfile(new UserPreferenceProfile());
                cfg.AddProfile(new UserProfileProfile());
            });

            return config.CreateMapper();
        }
    }
}
