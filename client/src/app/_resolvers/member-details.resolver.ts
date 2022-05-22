import { Injectable } from '@angular/core';
import {
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';

@Injectable({
  providedIn: 'root'
})
export class MemberDetailsResolver implements Resolve<Member> {

  constructor(private memberService: MembersService) {}

  //get data of member and ONLT THEN finish the navigation - we should not subscribe to this - router make it automatically

  //this is used when you want to get data before the template html is constructed
  resolve(route: ActivatedRouteSnapshot): Observable<Member> {
    return this.memberService.getMember(route.paramMap.get('username'));
  }
}
