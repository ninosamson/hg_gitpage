import { injectable } from "inversify";

import { ServiceCode } from "@/constants/serviceCodes";
import { ExternalConfiguration } from "@/models/configData";
import { HttpError } from "@/models/errors";
import PatientData from "@/models/patientData";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger, IPatientService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@injectable()
export class RestPatientService implements IPatientService {
    private logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    private readonly PATIENT_BASE_URI: string = "Patient";
    private baseUri = "";
    private http!: IHttpDelegate;

    public initialize(
        config: ExternalConfiguration,
        http: IHttpDelegate
    ): void {
        this.baseUri = config.serviceEndpoints["Patient"];
        this.http = http;
    }

    public getPatientData(hdid: string): Promise<RequestResult<PatientData>> {
        return new Promise((resolve, reject) =>
            this.http
                .get<RequestResult<PatientData>>(
                    `${this.baseUri}${this.PATIENT_BASE_URI}/${hdid}`
                )
                .then((requestResult) => resolve(requestResult))
                .catch((err: HttpError) => {
                    this.logger.error(
                        `Error in RestPatientService.getPatientData()`
                    );
                    reject(
                        ErrorTranslator.internalNetworkError(
                            err,
                            ServiceCode.Patient
                        )
                    );
                })
        );
    }
}
