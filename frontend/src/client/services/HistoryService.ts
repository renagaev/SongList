/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServiceDto } from '../models/ServiceDto';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class HistoryService {

    /**
     * @returns ServiceDto OK
     * @throws ApiError
     */
    public static getServices(): CancelablePromise<Array<ServiceDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/history/services',
        });
    }

}
