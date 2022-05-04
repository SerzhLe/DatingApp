import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAction, NgxGalleryAnimation, NgxGalleryImage, NgxGalleryImageSize, NgxGalleryLayout, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadMember();
    //Here is a problem - when we load this component - member is undefined and angular show an error when we want to access member property in html
    //then it load member and should after loading show member details - we nned to add condition if the members exists - only after that display member details
  
    this.galleryOptions = [
      {
        width: '600px',
        height: '500px',
        thumbnailsColumns: 4,
        imagePercent: 80,
        preview: true,
        startIndex: 1,
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

  loadMember() {
    this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe(user => {
      this.member = user;
      this.galleryImages = this.getImages();
      this.galleryImages.push(
        {
          small: '../../assets/girl1.jpg',
          medium: '../../assets/girl1.jpg',
          big: '../../assets/girl1.jpg'
        },
        {
          small: '../../assets/girl2.jpg',
          medium: '../../assets/girl2.jpg',
          big: '../../assets/girl2.jpg'
        },
        {
          small: '../../assets/girl3.jpg',
          medium: '../../assets/girl3.jpg',
          big: '../../assets/girl3.jpg'
        }
      );
    }); 
    //retrieve username from url of this component
  }
}
