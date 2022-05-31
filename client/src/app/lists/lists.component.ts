import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { LikesParams } from '../_models/likesParams';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Partial<Member[]>;
  pagination: Pagination;
  likesParams: LikesParams;
  loading = false;


  constructor(private memberService: MembersService) {
   }

  ngOnInit(): void {
    this.likesParams = new LikesParams();
    this.loadLikes();
  }

  loadLikes() {
    this.loading = true;
    this.memberService.getLikes(this.likesParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
      this.loading = false;
    })
  }

  pageChanged(event: PageChangedEvent) {
    this.likesParams.pageNumber = event.page;
    this.loadLikes();
  }
}
