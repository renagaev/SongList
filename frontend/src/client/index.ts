/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export { ApiError } from './core/ApiError';
export { CancelablePromise, CancelError } from './core/CancelablePromise';
export { OpenAPI } from './core/OpenAPI';
export type { OpenAPIConfig } from './core/OpenAPI';

export type { Attachment } from './models/Attachment';
export { AttachmentType } from './models/AttachmentType';
export type { Note } from './models/Note';
export type { ServiceDto } from './models/ServiceDto';
export { ServiceType } from './models/ServiceType';
export type { Song } from './models/Song';
export type { SongLastHistoryDto } from './models/SongLastHistoryDto';
export type { SongOpeningStats } from './models/SongOpeningStats';

export { AttachmentsService } from './services/AttachmentsService';
export { AuthService } from './services/AuthService';
export { HistoryService } from './services/HistoryService';
export { SongService } from './services/SongService';
