/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { Attachment } from '../models/Attachment';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AttachmentsService {

    /**
     * @param songId
     * @returns Attachment OK
     * @throws ApiError
     */
    public static getAttachments(
        songId: number,
    ): CancelablePromise<Array<Attachment>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/attachments/song/{songId}',
            path: {
                'songId': songId,
            },
        });
    }

    /**
     * @param songId
     * @param formData
     * @returns any OK
     * @throws ApiError
     */
    public static uploadAttachment(
        songId: number,
        formData?: {
            file?: Blob;
            displayName?: string;
        },
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/attachments/song/{songId}',
            path: {
                'songId': songId,
            },
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }

    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static getAttachment(
        id: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/attachments/{id}/download',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static deleteAttachment(
        id: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/attachments/{id}',
            path: {
                'id': id,
            },
        });
    }

}
