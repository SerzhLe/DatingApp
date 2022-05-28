import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { stringify } from 'querystring';
import { of, scheduled } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { LikesParams } from '../_models/likesParams';
import { Member } from '../_models/member';
import { UserParams } from '../_models/userParams';
import { getPaginatedResult, getPaginationHeader } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private baseUrl = environment.apiUrl;

  loggedInMember: Member;

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
    let params = getPaginationHeader(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    params = params.append('orderIsDescending', userParams.orderIsDescending);

    //when we just http.get - then we just get response body, when add 'observe': 'response' -  we get all http response and add params
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(result => {
        this.memberCache.set(this.key, result);
        return result;
      })
    );
  }

  getMember(username: string) {
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.userName === username);

    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  getLoggedInMember(username: string) {
    if (this.loggedInMember) return of(this.loggedInMember);

    return this.http.get<Member>(this.baseUrl + 'users/' + username).pipe(
      map(response => this.loggedInMember = response)
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

  addLike(username: string) {
    return this.http.post(this.baseUrl + 'likes/' + username, {}).pipe(
      map(() => {
        const member: Member[] = [...this.memberCache.values()]
          .reduce((arr, elem) => arr.concat(elem.result), [])
          .filter((member: Member) => member.userName === username);
        if (member) {
          member.forEach(element => {
            element.isLiked = true;
          });
        }
      })
    );
  }

  deleteLike(username: string) {
    return this.http.delete(this.baseUrl + 'likes/' + username, {}).pipe(
      map(() => {
        const member: Member[] = [...this.memberCache.values()]
          .reduce((arr, elem) => arr.concat(elem.result), [])
          .filter((member: Member) => member.userName === username);
        if (member) {
          member.forEach(element => {
            element.isLiked = false;
          });
        }
      })
    );
  }

  getLikes(likesParams: LikesParams) { 
    let params = getPaginationHeader(likesParams.pageNumber, likesParams.pageSize);

    params = params.append('predicate', likesParams.predicate);

    return getPaginatedResult<Member[]>(this.baseUrl + 'likes', params, this.http);
  }

}
