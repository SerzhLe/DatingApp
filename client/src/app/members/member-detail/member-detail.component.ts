import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryImage, NgxGalleryImageSize, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent; //get tabset component to display messages chilc based on condition
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activeTab: TabDirective;
  loggedInUser: User;

  constructor(private memberService: MembersService, 
      private route: ActivatedRoute, 
      private messageService: MessageService,
      private toastr: ToastrService,
      public presence: PresenceService,
      public accountService: AccountService,
      private router: Router) {
         this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.loggedInUser = user);
        // //explained in 235 record
        this.router.routeReuseStrategy.shouldReuseRoute = () => false;
       }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    this.route.data.subscribe(data => { //we get data from resolver
      this.member = data.member;
    });

    //Here is a problem - when we load this component - member is undefined and angular show an error when we want to access member property in html
    //then it load member and should after loading show member details - we need to add condition if the members exists - only after that display member details
    //another issue with If condition of member - we cannot get our tab form before member is loaded - using resolver to get data of member
    //in order to remove condition for waiting member 


    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0); 
    })

    this.galleryOptions = [
      {
        width: '600px',
        height: '500px',
        thumbnailsColumns: 4,
        imagePercent: 80,
        preview: true,
        imageArrowsAutoHide: true,
        imageSwipe: true,
        imageSize: NgxGalleryImageSize.Contain,
        imageInfinityMove: true,
        previewArrowsAutoHide: true,
        previewCloseOnClick: true,
        previewCloseOnEsc: true,
        previewInfinityMove: true,
        previewZoom: true,
        previewFullscreen: true,
        previewSwipe: true,
        previewDownload: true,
        previewBullets: true,
        arrowPrevIcon: 'fa fa-chevron-left',
        arrowNextIcon: 'fa fa-chevron-right',
      }
    ];

    this.galleryImages = this.getImages();

    //если сюда поместить метод getImages() - система не получит юзера во время выполнения этого метода потому что запросы в бд
    //асинхронные и мы не ждем пока выполниться метод load.Members и сразу выполняем getImages()
  }

  getImages() : NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      })
    }
    return imageUrls;
  }

  addLike() {
    this.memberService.addLike(this.member.userName).subscribe(() => {
      this.toastr.info("You liked " + this.member.knownAs + '!');
      this.member.isLiked = true;
    })
  }

  deleteLike() {
    this.memberService.deleteLike(this.member.userName).subscribe(() => {
      this.toastr.info("You unliked " + this.member.knownAs + '!');
      this.member.isLiked = false;
    });
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages') {
      this.messageService.createHubConnection(this.loggedInUser, this.member.userName);
    } else {
      this.messageService.stopHubConnection(); 
    }
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true; //active tab
  }
}
