import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of, scheduled } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { Photo } from '../_models/photo';
import { UserParams } from '../_models/userParams';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = environment.apiUrl;
  member: Member;
  memberCache = new Map(); //like a dictionary: key - value pair
  private key: string;
  userParams: UserParams;

  constructor(private http: HttpClient) { }

  //The purpose of these methods is send a request one time to get the data and then store it in properties
  //This is made to prevent send http request each time when user click on link
  getMembers(userParams: UserParams) {
    this.key = Object.values(userParams).join('-');
    var response = this.memberCache.get(this.key);
    
    if(response) return of(response);

    //caching based on keys - every query has its own values in userParams - based on this keys we will send the data from memory
    let params = this.getPaginationHeader(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    params = params.append('orderIsDescending', userParams.orderIsDescending);

    //when we just http.get - then we just get response body, when add 'observe': 'response' -  we get all http response and add params
    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params);
  }

  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        paginatedResult.result = response.body; //push array of members to pagination result

        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }

        this.memberCache.set(this.key, paginatedResult);
        return paginatedResult;
      })
    );
  }

  private getPaginationHeader(pageNumber: number, pageSize: number) {
      let params = new HttpParams(); //params of http query
      params = params.append('pageNumber', pageNumber.toString());
      params = params.append('pageSize', pageSize.toString());
      return params;
  }

  getMember(username: string) {
    const member = this.memberCache.get(this.key)?.result?.find((m: Member) => m.userName === username);

    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  getLoggedInMember(username: string) {
    if (this.member) return of(this.member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username).pipe(
      map(response => this.member = response)
    );
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member);
  }

  makeMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + "users/delete-photo/" + photoId);
  }
}
