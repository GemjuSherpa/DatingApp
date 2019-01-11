import { AuthService } from "./../../_services/auth.service";
import { Component, OnInit, ViewChild, HostListener } from "@angular/core";
import { User } from "src/app/_models/user";
import { ActivatedRoute } from "@angular/router";
import { AlertifyService } from "../../_services/alertify.service";
import { NgForm } from "@angular/forms";
import { UserService } from "src/app/_services/user.service";

@Component({
  selector: "app-member-edit",
  templateUrl: "./member-edit.component.html",
  styleUrls: ["./member-edit.component.css"]
})
export class MemberEditComponent implements OnInit {
  @ViewChild("editForm") editForm: NgForm;
  user: User;

  // prevent browser to be closed if there is unsave changes
  @HostListener("window:beforeunload", ["$event"])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data["user"];
    });
  }

  updateUser() {
    this.userService
      .updateUser(this.authService.decodedToken.nameid, this.user)
      .subscribe(
        next => {
          this.alertify.success("Updated successfully!");
          this.editForm.reset(this.user);
        },
        error => {
          this.alertify.error(error);
        }
      );
  }

  //Update profile picture on profile edit
  updateMainPhoto(photoUrl) {
    this.user.photoUrl = photoUrl;
  }
}
