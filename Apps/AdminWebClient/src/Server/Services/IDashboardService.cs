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
using System.Collections.Generic;

namespace HealthGateway.Admin.Services
{
    /// <summary>
    /// Service that provides functionality to the admin dashboard.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the count of registered users.
        /// </summary>
        /// <returns>The count of user profiles that accepted the terms of service.</returns>
        int GetRegisteredUserCount();

        /// <summary>
        /// Retrieves the count of logged in users in the current day.
        /// </summary>
        /// <param name="timeOffset">The time offset from the client browser to UTC.</param>
        /// <returns>The count of logged in user.</returns>
        IEnumerable<int> GetDailyLoggedInUsersCount(int timeOffset);

        /// <summary>
        /// Retrieves the count of users with notes on their timeline.
        /// </summary>
        /// <returns>The count of users with notes.</returns>
        int GetUsersWithNotesCount();

        /// <summary>
        /// Retrieves the count of dependents.
        /// </summary>
        /// <returns>The count of dependents.</returns>
        int GetDependentCount();
    }
}
