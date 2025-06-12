/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AttachmentType } from './AttachmentType';

export type Attachment = {
    id: number;
    songId: number;
    name: string | null;
    type: AttachmentType;
};

