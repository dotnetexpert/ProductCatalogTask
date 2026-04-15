export interface PaginationResponse<T> {
    success: boolean;
    statusCode: number;
    message: string;
    result: PagedResult<T>;
    errors: string[];
}

export interface PagedResult<T> {
    data: T[];
    total: number;
    page: number;
}