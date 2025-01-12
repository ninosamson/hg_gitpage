﻿import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import UserFeedback from "@/models/userFeedback";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IHttpDelegate,
    ILogger,
    IUserFeedbackService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestUserFeedbackService implements IUserFeedbackService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly USER_FEEDBACK_BASE_URI: string = "UserFeedback";
    private http!: IHttpDelegate;
    private baseUri = "";

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.http = http;
        this.baseUri = config.serviceEndpoints["GatewayApi"];
    }

    public submitFeedback(
        hdid: string,
        feedback: UserFeedback
    ): Promise<boolean> {
        return new Promise((resolve, reject) =>
            this.http
                .post<void>(
                    `${this.baseUri}${this.USER_FEEDBACK_BASE_URI}/${hdid}`,
                    feedback
                )
                .then(() => resolve(true))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestUserFeedbackService.submitFeedback()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.HealthGatewayUser
                        )
                    );
                })
        );
    }
}
