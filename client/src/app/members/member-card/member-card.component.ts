import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  //encapsulation: ViewEncapsulation.Emulated - default value
  //it allows us to us css classes in this component and the styles will not effect the same classes in other components
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member;

  constructor(private memberService: MembersService, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    this.memberService.addLike(member.userName).subscribe(() => {
      this.toastr.success("You liked " + member.knownAs + '!');
    });
  }

  addDisLike(member: Member) {
    this.memberService.addDisLike(member.userName).subscribe(() => {
      this.toastr.success("You unliked " + member.knownAs + '!');
    });
  }
  
}
