import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ErrorType } from "@/constants/errorType";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { StringISODate } from "@/models/dateWrapper";
import { CustomBannerError, ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationRecord from "@/models/vaccinationRecord";
import VaccinationStatus from "@/models/vaccinationStatus";
import { RootState } from "@/store/types";

export interface VaccinationStatusState {
    public: {
        vaccinationStatus?: VaccinationStatus;
        error?: CustomBannerError;
        status: LoadStatus;
        statusMessage: string;
    };
    publicVaccineRecord: {
        vaccinationRecord?: CovidVaccineRecord;
        error?: CustomBannerError;
        status: LoadStatus;
        statusMessage: string;
    };
    authenticated: {
        vaccinationStatus?: VaccinationStatus;
        error?: ResultError;
        status: LoadStatus;
        statusMessage: string;
    };
    authenticatedVaccineRecord: {
        activeHdid: string;
        statusChanges: number;
        vaccinationRecords: Map<string, VaccinationRecord>;
    };
}

export interface VaccinationStatusGetters
    extends GetterTree<VaccinationStatusState, RootState> {
    publicVaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined;
    publicIsLoading(state: VaccinationStatusState): boolean;
    publicError(state: VaccinationStatusState): CustomBannerError | undefined;
    publicStatusMessage(state: VaccinationStatusState): string;
    publicVaccineRecord(
        state: VaccinationStatusState
    ): CovidVaccineRecord | undefined;
    publicVaccineRecordIsLoading(state: VaccinationStatusState): boolean;
    publicVaccineRecordError(
        state: VaccinationStatusState
    ): CustomBannerError | undefined;
    publicVaccineRecordStatusMessage(state: VaccinationStatusState): string;
    authenticatedVaccinationStatus(
        state: VaccinationStatusState
    ): VaccinationStatus | undefined;
    authenticatedIsLoading(state: VaccinationStatusState): boolean;
    authenticatedError(state: VaccinationStatusState): ResultError | undefined;
    authenticatedStatusMessage(state: VaccinationStatusState): string;
    authenticatedVaccineRecords(
        state: VaccinationStatusState
    ): Map<string, VaccinationRecord>;
    authenticatedVaccineRecordStatusChanges(
        state: VaccinationStatusState
    ): number;
    authenticatedVaccineRecordActiveHdid(state: VaccinationStatusState): string;
}

type StoreContext = ActionContext<VaccinationStatusState, RootState>;
export interface VaccinationStatusActions
    extends ActionTree<VaccinationStatusState, RootState> {
    retrievePublicVaccineStatus(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<void>;
    handlePublicError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
    retrievePublicVaccineRecord(
        context: StoreContext,
        params: {
            phn: string;
            dateOfBirth: StringISODate;
            dateOfVaccine: StringISODate;
        }
    ): Promise<CovidVaccineRecord>;
    handlePdfError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
    retrieveAuthenticatedVaccineStatus(
        context: StoreContext,
        params: {
            hdid: string;
        }
    ): Promise<void>;
    handleAuthenticatedError(
        context: StoreContext,
        params: { error: ResultError; errorType: ErrorType }
    ): void;
    retrieveAuthenticatedVaccineRecord(
        context: StoreContext,
        params: {
            hdid: string;
        }
    ): Promise<CovidVaccineRecord>;
    handleAuthenticatedPdfError(
        context: StoreContext,
        params: { hdid: string; error: ResultError; errorType: ErrorType }
    ): void;
    stopAuthenticatedVaccineRecordDownload(
        context: StoreContext,
        params: { hdid: string }
    ): void;
}

export interface VaccinationStatusMutations
    extends MutationTree<VaccinationStatusState> {
    setPublicRequested(state: VaccinationStatusState): void;
    setPublicVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ): void;
    publicVaccinationStatusError(
        state: VaccinationStatusState,
        error: CustomBannerError
    ): void;
    setPublicStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setPublicVaccineRecordRequested(state: VaccinationStatusState): void;
    setPublicVaccineRecord(
        state: VaccinationStatusState,
        vaccineRecord: CovidVaccineRecord
    ): void;
    setPublicVaccineRecordError(
        state: VaccinationStatusState,
        error: CustomBannerError
    ): void;
    setPublicVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setAuthenticatedRequested(state: VaccinationStatusState): void;
    setAuthenticatedVaccinationStatus(
        state: VaccinationStatusState,
        vaccinationStatus: VaccinationStatus
    ): void;
    authenticatedVaccinationStatusError(
        state: VaccinationStatusState,
        error: ResultError
    ): void;
    setAuthenticatedStatusMessage(
        state: VaccinationStatusState,
        statusMessage: string
    ): void;
    setAuthenticatedVaccineRecordRequested(
        state: VaccinationStatusState,
        params: { hdid: string }
    ): void;
    setAuthenticatedVaccineRecord(
        state: VaccinationStatusState,
        params: { hdid: string; vaccinationRecord: CovidVaccineRecord }
    ): void;
    setAuthenticatedVaccineRecordError(
        state: VaccinationStatusState,
        params: { hdid: string; error: ResultError }
    ): void;
    setAuthenticatedVaccineRecordStatusMessage(
        state: VaccinationStatusState,
        params: { hdid: string; statusMessage: string }
    ): void;
    setAuthenticatedVaccineRecordResultMessage(
        state: VaccinationStatusState,
        params: { hdid: string; resultMessage: string }
    ): void;
    setAuthenticatedVaccineRecordDownload(
        state: VaccinationStatusState,
        params: { hdid: string; download: boolean }
    ): void;
}

export interface VaccinationStatusModule
    extends Module<VaccinationStatusState, RootState> {
    namespaced: boolean;
    state: VaccinationStatusState;
    getters: VaccinationStatusGetters;
    actions: VaccinationStatusActions;
    mutations: VaccinationStatusMutations;
}
