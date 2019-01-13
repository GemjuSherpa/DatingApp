import { AlertifyService } from "./../../_services/alertify.service";
import { UserService } from "./../../_services/user.service";
import { AuthService } from "./../../_services/auth.service";
import { environment } from "./../../../environments/environment";
import { Photo } from "./../../_models/photo";
import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { FileUploader } from "ng2-file-upload";

@Component({
  selector: "app-photo-editor",
  templateUrl: "./photo-editor.component.html",
  styleUrls: ["./photo-editor.component.css"]
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>();
  baseUrl = environment.apiUrl;
  currentMain: Photo;

  // for file uploader -> ng2-file-uploader
  uploader: FileUploader;
  hasBaseDropZoneOver: boolean = false;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url:
        this.baseUrl +
        "users/" +
        this.authService.decodedToken.nameid +
        "/photos",
      authToken: "Bearer " + localStorage.getItem("token"),
      isHTML5: true,
      allowedFileType: ["image"],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    //Not to allow credentials with request
    this.uploader.onAfterAddingFile = file => {
      file.withCredentials = false;
    };

    //on successful upload
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);

        //if the user is uploading first photo then photo must be profile photo.//
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            "user",
            JSON.stringify(this.authService.currentUser)
          );
        }
      }
    };
  }

  //set Profile picture
  setMainPhoto(photo: Photo) {
    this.userService
      .setMainPhoto(this.authService.decodedToken.nameid, photo.id)
      .subscribe(
        () => {
          this.currentMain = this.photos.filter(p => p.isMain === true)[0];
          this.currentMain.isMain = false; //set current main: false

          photo.isMain = true; //set new image to main photo

          // this.getMemberPhotoChange.emit(photo.url); //change the profile picture in member component
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            "user",
            JSON.stringify(this.authService.currentUser)
          );
        },
        error => {
          this.alertify.error("Error occured.");
        }
      );
  }

  //Delete photos
  deletePhoto(id: number) {
    this.alertify.confirm("Are you sure you want to delete this photo?", () => {
      this.userService
        .deletePhoto(this.authService.decodedToken.nameid, id)
        .subscribe(
          () => {
            this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
            this.alertify.success("Successfully deleted.");
          },
          error => {
            this.alertify.error("Could not delete this photo");
          }
        );
    });
  }
}
