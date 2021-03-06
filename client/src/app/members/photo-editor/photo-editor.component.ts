import { Component, Input, OnInit, Output } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';
import { MembersService } from 'src/app/_services/members.service';
import { Photo } from 'src/app/_models/photo';
import { ToastrService } from 'ngx-toastr';
import { ConfirmService } from 'src/app/_services/confirm.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input("member") member: Member;

  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  user: User;

  constructor(private accountService: AccountService, 
    private memberService: MembersService, 
    private toastr: ToastrService,
    private confirmService: ConfirmService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({next: user => this.user = user});
   }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: any) {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/upload-photo',
      authToken: 'Bearer ' + this.user.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo: Photo = JSON.parse(response);

        if (photo.isMain) {
          this.user.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
          this.member.photoUrl = photo.url;
        }
        this.member.photos.push(photo);
      }
    }

  }

  makePhotoMain(photo: Photo) {
    if (photo) {
      this.memberService.makeMainPhoto(photo.id).subscribe(() => {
        this.user.photoUrl = photo.url;
        this.accountService.setCurrentUser(this.user);
        this.member.photoUrl = photo.url;

        this.member.photos.forEach(p => {
          if (p.isMain) p.isMain = false;
          if (p.id === photo.id) p.isMain = true;
        })
        this.toastr.info("Your main photo has been changed!");
      })
    }
  }

  deletePhoto(photo: Photo) {
    this.confirmService.confirm('Confirm deletion', 'Are you sure you want to delete the photo?', 'Yes', 'No').subscribe(result => {
      if (result) {
        this.memberService.deletePhoto(photo.id).subscribe(() => {
          const index = this.member.photos.indexOf(photo);
          this.member.photos.splice(index, 1);
          this.toastr.success("You successfully deleted the photo!")
        })
      };
    });
  }

}
