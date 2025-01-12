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
namespace HealthGateway.Admin.Client.Components;

using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Configuration;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Backing logic for the navigation menu.
/// </summary>
public partial class NavMenu : FluxorComponent
{
    [Inject]
    private IState<ConfigurationState> ConfigurationState { get; set; } = default!;

    private string JobSchedulerUrl => this.ConfigurationState.Value.Result?.ServiceEndpoints.ContainsKey("JobScheduler") == true
        ? this.ConfigurationState.Value.Result.ServiceEndpoints["JobScheduler"].ToString()
        : string.Empty;
}
