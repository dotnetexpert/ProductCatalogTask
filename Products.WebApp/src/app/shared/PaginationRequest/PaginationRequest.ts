export interface Filter {
    [key: string]: any;
}
export interface SortEvent {
    sortBy: string;
    sortDirection: 'asc' | 'desc';
}

export class PaginationRequest {
    filter?: Filter;
    sort?: string;
    ascending?: boolean;
    id?: number;
    page?: number;
    pageSize?: number;

    constructor(init?: Partial<PaginationRequest>) {
        this.filter = init?.filter ?? {
            additionalProp1: 'string',
            additionalProp2: 'string',
            additionalProp3: 'string',
        };
        this.sort = init?.sort ?? 'string';
        this.ascending = init?.ascending ?? true;
        this.id = init?.id ?? 0;
        this.page = init?.page ?? 0;
        this.pageSize = init?.pageSize ?? 0;
    }
}
