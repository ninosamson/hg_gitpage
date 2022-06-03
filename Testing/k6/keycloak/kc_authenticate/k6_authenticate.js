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

import { sleep, check } from "k6";
import * as common from "../../inc/common.js";

export let options = common.OptionConfig();

export default function () {
    let user = common.users[__VU % common.users.length];

    let loginRes = common.authenticateUser(user);
    check(loginRes, {
        "Authenticated successfully": loginRes === 200,
    });
    sleep(1);
}