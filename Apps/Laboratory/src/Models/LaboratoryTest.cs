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
namespace HealthGateway.Laboratory.Models;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using HealthGateway.Laboratory.Models.PHSA;

/// <summary>
/// An instance of a Laboratory Test.
/// </summary>
public class LaboratoryTest
{
    /// <summary>
    /// Gets or sets a value for battery type.
    /// </summary>
    [JsonPropertyName("batteryType")]
    public string BatteryType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for obxId.
    /// </summary>
    [JsonPropertyName("obxId")]
    public string ObxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether laboratory test is out of range.
    /// </summary>
    [JsonPropertyName("outOfRange")]
    public bool OutOfRange { get; set; }

    /// <summary>
    /// Gets or sets a value for LOINC.
    /// </summary>
    [JsonPropertyName("loinc")]
    public string Loinc { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for test status.
    /// </summary>
    [JsonPropertyName("testStatus")]
    public string TestStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for the filtered test status.
    /// </summary>
    [JsonPropertyName("filteredTestStatus")]
    public string FilteredTestStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for the result.
    /// </summary>
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Creates a LaboratoryTest object from a PHSA model.
    /// </summary>
    /// <param name="model">The laboratory test to convert.</param>
    /// <returns>The newly created laboratory object.</returns>
    [SuppressMessage("Minor Code Smell", "S3440:Variables should not be checked against the values they\'re about to be assigned", Justification = "Team decision")]
    public static LaboratoryTest FromPhsaModel(PhsaLaboratoryTest model)
    {
        return new()
        {
            BatteryType = model.BatteryType,
            ObxId = model.ObxId,
            OutOfRange = model.OutOfRange,
            Loinc = model.Loinc,
            TestStatus = model.PlisTestStatus,
            FilteredTestStatus = model.PlisTestStatus switch
            {
                "Active" => "Pending",
                _ => model.PlisTestStatus,
            },
            Result = model.PlisTestStatus switch
            {
                "Completed" or "Corrected" when model.OutOfRange => "Out of Range",
                "Completed" or "Corrected" when !model.OutOfRange => "In Range",
                "Cancelled" => "Cancelled",
                _ => "Pending",
            },
        };
    }
}
