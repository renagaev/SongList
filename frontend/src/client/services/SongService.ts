/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Song } from '../models/Song';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SongService {

    /**
     * @returns Song Success
     * @throws ApiError
     */
    public static getAllSongs(): CancelablePromise<Array<Song>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Song',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postSong(
requestBody?: Array<Song>,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/Song',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
