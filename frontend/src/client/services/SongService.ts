/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Note } from '../models/Note';
import type { Song } from '../models/Song';
import type { SongOpeningStats } from '../models/SongOpeningStats';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SongService {

    /**
     * @returns Song OK
     * @throws ApiError
     */
    public static getAllSongs(): CancelablePromise<Array<Song>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Song',
        });
    }

    /**
     * @returns Note OK
     * @throws ApiError
     */
    public static getNotes(): CancelablePromise<Array<Note>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Song/notes',
        });
    }

    /**
     * @param id
     * @param requestBody
     * @returns Song OK
     * @throws ApiError
     */
    public static updateSong(
        id: number,
        requestBody?: Song,
    ): CancelablePromise<Song> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/Song/{id}/edit',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns SongOpeningStats OK
     * @throws ApiError
     */
    public static getOpenedSongs(): CancelablePromise<Array<SongOpeningStats>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Song/opened',
        });
    }

}
