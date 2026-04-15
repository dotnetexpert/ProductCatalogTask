import { Injectable } from '@angular/core';
import { Filter, PaginationRequest, SortEvent } from '../PaginationRequest/PaginationRequest';


//#region  sync interface
export interface SyncRequest<T> {
  entity: string;
  data: T[];
  deviceId: string;
  storeId: string;
  direction: 'POS_TO_BO' | 'BO_TO_POS';
}

//#endregion

@Injectable({
  providedIn: 'root',
})
export class RequestHelperService {
  buildFilterObject(filters: { [key: string]: string }): Filter {
    return Object.fromEntries(
      Object.entries(filters).filter(([_, value]) => !!value)
    );
  }

  buildPaginationRequest(
    sort?: SortEvent,
    filter: Filter = {},
    page: number = 1,
    pageSize: number = 5
  ): PaginationRequest {
    return {
      page,
      pageSize,
      sort: sort?.sortBy,
      ascending: sort?.sortDirection === 'desc',
      filter,
    };
  }

  buildPaginationRequestForDropDown(
    sort?: SortEvent,
    filter: Filter = {},
    page: number = 1,
    pageSize: number = 5,
    isDropDown: boolean = false
  ): PaginationRequest {
    const request: PaginationRequest = {
      page,
      sort: sort?.sortBy,
      ascending: sort?.sortDirection === 'desc',
      filter,
    };
    //  Send safe max value only if isDropDown
    if (isDropDown) {
      request.pageSize = 2147483647;
    } else {
      request.pageSize = pageSize;
    }

    return request;
  }
}
