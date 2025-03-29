/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AuthService {

    /**
     * @param requestBody
     * @returns string OK
     * @throws ApiError
     */
    public static getTgAdminToken(
        requestBody?: Record<string, any>,
    ): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/auth/tg-admin',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns string OK
     * @throws ApiError
     */
    public static getUser(): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/auth/user',
        });
    }

}
