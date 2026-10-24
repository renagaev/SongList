/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Attachment } from './Attachment';

export type Song = {
    id: number;
    title: string;
    tags: Array<string>;
    text: string;
    number?: number | null;
    note?: string | null;
    noteId?: number | null;
    attachments?: Array<Attachment>;
    lastMorning?: string | null;
    lastEvening?: string | null;
};