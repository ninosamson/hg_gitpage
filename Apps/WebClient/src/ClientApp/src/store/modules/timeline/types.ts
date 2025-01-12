import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { RootState } from "@/store/types";

export interface TimelineState {
    filter: TimelineFilter;
    keyword: string;
    linearDate: DateWrapper;
    selectedDate: DateWrapper | null;
}

export interface TimelineGetters extends GetterTree<TimelineState, RootState> {
    filter(state: TimelineState): TimelineFilter;
    hasActiveFilter(state: TimelineState): boolean;
    keyword(state: TimelineState): string;
    linearDate(state: TimelineState): DateWrapper;
    selectedDate(state: TimelineState): DateWrapper | null;
}

type StoreContext = ActionContext<TimelineState, RootState>;
export interface TimelineActions extends ActionTree<TimelineState, RootState> {
    setFilter(
        context: StoreContext,
        filterBuilder: TimelineFilterBuilder
    ): void;
    clearFilter(context: StoreContext): void;
    setLinearDate(context: StoreContext, currentPage: number): void;
    setSelectedDate(
        context: StoreContext,
        selectedDate: DateWrapper | null
    ): void;
}

export interface TimelineMutations extends MutationTree<TimelineState> {
    setFilter(state: TimelineState, filter: TimelineFilter): void;
    clearFilter(state: TimelineState): void;
    setLinearDate(state: TimelineState, linearDate: DateWrapper): void;
    setSelectedDate(
        state: TimelineState,
        selectedDate: DateWrapper | null
    ): void;
}

export interface TimelineModule extends Module<TimelineState, RootState> {
    namespaced: boolean;
    state: TimelineState;
    getters: TimelineGetters;
    actions: TimelineActions;
    mutations: TimelineMutations;
}
