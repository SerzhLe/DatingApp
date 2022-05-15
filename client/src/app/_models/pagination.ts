export interface Pagination { //MUST have the exact signature as we get from response header
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

//type that combines members and pagination details
export class PaginatedResult<T> { //T - Member[]
    result: T;
    pagination: Pagination;
}