import { Component, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { ConfirmService } from 'src/app/_services/confirm.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm: NgForm; //to access editForm in component
  private initialMember: Member; //for saving initial value when user made changes and does not save them
  member: Member;
  user: User;

  //this is used when user applies some changes and does not click Save Changes and is about to leave our site or exit browser - it
  //will show them a notification that changes will be lost
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private accountService: AccountService, 
    private memberService: MembersService, 
    private toastr: ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
  }
  
  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getLoggedInMember(this.user.userName).subscribe(member => {
      this.initialMember = member;
      this.member = Object.assign({}, this.initialMember);
    });
  }

  updateMember(){
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastr.success("Profile updated successfully");
      this.editForm.reset(this.member); //to apply changes to member on client side - also needed to add changes on server
    });
  }

}
