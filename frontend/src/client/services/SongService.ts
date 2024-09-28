/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Song } from '../models/Song';
import type { SongOpeningStats } from '../models/SongOpeningStats';

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
     * @returns SongOpeningStats Success
     * @throws ApiError
     */
    public static getOpenedSongs(): CancelablePromise<Array<SongOpeningStats>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Song/opened',
        });
    }

}
