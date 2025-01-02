/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ServiceType } from './ServiceType';

export type ServiceDto = {
    date?: string;
    type?: ServiceType;
    songs?: Array<number> | null;
};

