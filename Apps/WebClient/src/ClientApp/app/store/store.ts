import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { RootState } from "@/models/storeState";
import { auth } from "./modules/auth/auth";
import { config } from "./modules/config/config";
import { user } from "./modules/user/user";
import { medication } from "./modules/medication/medication";
import { pharmacy } from "./modules/pharmacy/pharmacy";

Vue.use(Vuex);

const storeOptions: StoreOptions<RootState> = {
  state: {
    version: "1.0.0" // a simple property
  },
  modules: {
    auth,
    config,
    user,
    medication,
    pharmacy
  }
};

export default new Vuex.Store<RootState>(storeOptions);