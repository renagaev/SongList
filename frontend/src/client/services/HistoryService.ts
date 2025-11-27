/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ServiceDto } from '../models/ServiceDto';
import type { SongLastHistoryDto } from '../models/SongLastHistoryDto';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class HistoryService {

    /**
     * @param songId
     * @returns string OK
     * @throws ApiError
     */
    public static getSongHistory(
        songId: number,
    ): CancelablePromise<Array<string>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/history/{songId}',
            path: {
                'songId': songId,
            },
        });
    }

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

    /**
     * @returns SongLastHistoryDto OK
     * @throws ApiError
     */
    public static getSongsLastHistory(): CancelablePromise<Array<SongLastHistoryDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/history/last-songs',
        });
    }

}
