import { ThrowStmt } from '@angular/compiler';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { PageChangedEvent, PagesModel, PaginationComponent } from 'ngx-bootstrap/pagination';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit, OnDestroy {
  @ViewChild('paginationForm') paginationForm: PaginationComponent;
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]
  orderByList = [{value: 'lastActive', display: 'Last Active'}, {value: 'created', display: 'Created Profile'}]
  loading = false;

  constructor(private memberService: MembersService, private accountService: AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(response => {
      this.user = response;
      if(!this.memberService.userParams)  this.memberService.userParams = new UserParams(this.user);
    })
   }
   
   //constructor calls first, than calls ngOnInit()!

   ngOnInit(): void {
     this.userParams = this.memberService.userParams;
     this.loadMembers();
    }

    ngOnDestroy(): void {
      if (localStorage.getItem('user')) 
        this.memberService.userParams = this.userParams; //this method calls AFTER the logout method
    }

  loadMembers() {
    this.loading = true;
    this.memberService.getMembers(this.userParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
      this.loading = false;
    });
    //we do not add pipe and take(1) because it is http response and it is completed once we subscribe
  }

  resetFilters() {
    this.userParams = new UserParams(this.user);
  }

  pageChanged(event: PageChangedEvent) {
    this.userParams.pageNumber = event.page;
    this.loadMembers();
  }

  applyFilters() {
    this.userParams.pageNumber = 1;
    this.paginationForm.page = 1;
    this.loadMembers();
  }
}
