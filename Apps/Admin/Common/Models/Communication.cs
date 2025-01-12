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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// A system communication.
    /// </summary>
    public class Communication
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        public DateTime ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled datetime.
        /// </summary>
        public DateTime? ScheduledDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the Communication (Banner, In-App).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommunicationType CommunicationTypeCode { get; set; } = CommunicationType.Banner;

        /// <summary>
        /// Gets or sets the state of the Communication (Draft, Pending, ...).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommunicationStatus CommunicationStatusCode { get; set; } = CommunicationStatus.New;

        /// <summary>
        /// Gets or sets the priority of the email communication.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; } = EmailPriority.Standard;

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string CreatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was created.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string UpdatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was updated.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the version number to be used for backend locking.
        /// </summary>
        public uint Version { get; set; }
    }
}
