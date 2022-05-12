import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { Member } from 'src/app/_models/member';
import { environment } from 'src/environments/environment';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';
import { MembersService } from 'src/app/_services/members.service';
import { Photo } from 'src/app/_models/photo';
import { ToastrService } from 'ngx-toastr';

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

  constructor(private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService) {
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
      maxFileSize: 10 * 1024 * 1024 //10 mb = 10485760 bytes
    });

    this.uploader.onAfterAddingFile = (file) => { //if we did not supply this config - we would need to adjust configuration to CORS on back-end
      //we are sending the credentials by Bearer in authToken
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
        this.accountService.setCurrentUser(this.user); //because our user and member HAVE references on the global users and members
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
    this.memberService.deletePhoto(photo.id).subscribe(() => {
      const index = this.member.photos.indexOf(photo);
      this.member.photos.splice(index, 1);
    })
  }

}
