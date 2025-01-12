import {
    AppErrorType,
    ErrorSourceType,
    ErrorType,
} from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { QuickLink } from "@/models/quickLink";
import RequestResult from "@/models/requestResult";
import { UserPreference } from "@/models/userPreference";
import { CreateUserRequest } from "@/models/userProfile";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IPatientService,
    IUserProfileService,
} from "@/services/interfaces";
import { QuickLinkUtil } from "@/utility/quickLinkUtil";

import { UserActions } from "./types";

export const actions: UserActions = {
    createProfile(
        context,
        params: { request: CreateUserRequest }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .createProfile(params.request)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Create,
                        source: ErrorSourceType.Profile,
                    });
                    reject(error);
                })
        );
    },
    retrieveProfile(context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .getProfile(context.state.user.hdid)
                .then((userProfile) => {
                    logger.verbose(
                        `User Profile: ${JSON.stringify(userProfile)}`
                    );
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                })
        );
    },
    updateAcceptedTerms(
        context,
        params: { termsOfServiceId: string }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .updateAcceptedTerms(
                    context.state.user.hdid,
                    params.termsOfServiceId
                )
                .then((userProfile) => {
                    logger.debug(
                        `Update accepted terms of service result: ${JSON.stringify(
                            userProfile
                        )}`
                    );
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                })
        );
    },
    updateUserEmail(context, params: { emailAddress: string }): Promise<void> {
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .updateEmail(context.state.user.hdid, params.emailAddress)
                .then(() => resolve())
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                })
        );
    },
    updateSMSResendDateTime(context, params: { dateTime: DateWrapper }): void {
        context.commit("setSMSResendDateTime", params.dateTime);
    },
    setUserPreference(
        context,
        params: { preference: UserPreference }
    ): Promise<void> {
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        return new Promise((resolve, reject) => {
            let setPreferencePromise: Promise<UserPreference>;
            if (params.preference.hdId == undefined) {
                logger.debug(`setUserPreference: creating new preference`);
                setPreferencePromise = userProfileService.createUserPreference(
                    context.state.user.hdid,
                    { ...params.preference, hdId: context.state.user.hdid }
                );
            } else {
                logger.debug(`setUserPreference: updating existing preference`);
                setPreferencePromise = userProfileService.updateUserPreference(
                    context.state.user.hdid,
                    params.preference
                );
            }

            return setPreferencePromise
                .then((result) => {
                    logger.debug(
                        `setUserPreference: ${JSON.stringify(result)}`
                    );
                    if (result) {
                        context.commit("setUserPreference", result);
                    }
                    resolve();
                })
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                });
        });
    },
    updateQuickLinks(
        context,
        params: { hdid: string; quickLinks: QuickLink[] }
    ): Promise<void> {
        const jsonString = QuickLinkUtil.toString(params.quickLinks);

        let preference: UserPreference =
            context.getters.user.preferences[UserPreferenceType.QuickLinks];

        if (preference === undefined) {
            preference = {
                preference: UserPreferenceType.QuickLinks,
                value: jsonString,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            };
        } else {
            preference = { ...preference, value: jsonString };
        }

        return context.dispatch("setUserPreference", { preference });
    },
    validateEmail(
        context,
        params: { inviteKey: string }
    ): Promise<RequestResult<boolean>> {
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) => {
            userProfileService
                .validateEmail(context.state.user.hdid, params.inviteKey)
                .then((result) => {
                    if (result.resourcePayload === true) {
                        context.commit("setEmailVerified");
                    }
                    resolve(result);
                })
                .catch((error: ResultError) => {
                    context.dispatch("handleError", {
                        error,
                        errorType: ErrorType.Update,
                        source: ErrorSourceType.User,
                    });
                    reject(error);
                });
        });
    },
    closeUserAccount(context): Promise<void> {
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .closeAccount(context.state.user.hdid)
                .then((userProfile) => {
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                })
        );
    },
    recoverUserAccount(context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve, reject) =>
            userProfileService
                .recoverAccount(context.state.user.hdid)
                .then((userProfile) => {
                    logger.debug(
                        `recoverUserAccount User Profile: ${JSON.stringify(
                            userProfile
                        )}`
                    );
                    context.commit("setProfileUserData", userProfile);
                    resolve();
                })
                .catch((error) => {
                    context.commit("userError");
                    reject(error);
                })
        );
    },
    retrieveEssentialData(context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const patientService = container.get<IPatientService>(
            SERVICE_IDENTIFIER.PatientService
        );
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );

        return new Promise((resolve) => {
            context.commit("setRequested");
            patientService
                .getPatientData(context.state.user.hdid)
                .then((result) => {
                    if (result.resultStatus === ResultType.Success) {
                        context.commit(
                            "setPatientData",
                            result.resourcePayload
                        );

                        userProfileService
                            .getProfile(context.state.user.hdid)
                            .then((userProfile) => {
                                context.commit(
                                    "setProfileUserData",
                                    userProfile
                                );
                                resolve();
                            })
                            .catch((error: ResultError) => {
                                if (error.statusCode === 429) {
                                    logger.debug(
                                        "User profile retrieval failed because of too many requests"
                                    );
                                    context.commit(
                                        "setAppError",
                                        AppErrorType.TooManyRequests,
                                        { root: true }
                                    );
                                } else {
                                    logger.debug(
                                        "User profile retrieval failed"
                                    );
                                    context.commit(
                                        "setAppError",
                                        AppErrorType.General,
                                        { root: true }
                                    );
                                }
                                resolve();
                            });
                    } else {
                        if (result.resultError?.statusCode === 429) {
                            logger.debug(
                                "Patient retrieval failed because of too many requests"
                            );
                            context.commit(
                                "setAppError",
                                AppErrorType.TooManyRequests,
                                { root: true }
                            );
                        } else {
                            logger.debug("Patient retrieval failed");
                            context.commit("setPatientRetrievalFailed");
                        }
                        resolve();
                    }
                })
                .catch((error: ResultError) => {
                    if (error.statusCode === 429) {
                        logger.debug(
                            "Patient retrieval failed because of too many requests"
                        );
                        context.commit(
                            "setAppError",
                            AppErrorType.TooManyRequests,
                            { root: true }
                        );
                    } else {
                        logger.debug("Patient retrieval failed");
                        context.commit("setPatientRetrievalFailed");
                    }
                    resolve();
                });
        });
    },
    handleError(
        context,
        params: {
            error: ResultError;
            errorType: ErrorType;
            source: ErrorSourceType;
        }
    ) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        logger.error(`ERROR: ${JSON.stringify(params.error)}`);
        context.commit("userError", params.error);

        if (params.error.statusCode === 429) {
            let action = "errorBanner/setTooManyRequestsError";
            if (params.errorType === ErrorType.Retrieve) {
                action = "errorBanner/setTooManyRequestsWarning";
            }
            context.dispatch(action, { key: "page" }, { root: true });
        } else {
            context.dispatch(
                "errorBanner/addError",
                {
                    errorType: params.errorType,
                    source: params.source,
                    traceId: params.error.traceId,
                },
                { root: true }
            );
        }
    },
    setSeenTutorialComment: function (
        context,
        params: { value: boolean }
    ): void {
        context.commit("setSeenTutorialComment", params.value);
    },
};
