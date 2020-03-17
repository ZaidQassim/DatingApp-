import { Component, OnInit, ViewChild, HostListener } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { User } from "src/app/_models/user";
import { AlertifyService } from "src/app/_services/alertify.service";
import { NgForm } from "@angular/forms";
import { UserService } from "src/app/_services/user.service";
import { AuthService } from "src/app/_services/auth.service";

@Component({
  selector: "app-member-edit",
  templateUrl: "./member-edit.component.html",
  styleUrls: ["./member-edit.component.css"]
})
export class MemberEditComponent implements OnInit {
  //  to  get editForm of from
  @ViewChild("editForm", { static: true }) editForm: NgForm;
  user: User;

  // To prevent close the brawser when edit in form
  @HostListener("window:beforeunload", ["$event"])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private router: ActivatedRoute,
    private alertify: AlertifyService,
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.router.data.subscribe(data => {
      this.user = data["user"];
    });
  }

  updateUSer() {
    this.userService
      .updateUser(this.authService.decodedToken.nameid, this.user)
      .subscribe(
        next => {
          this.alertify.success("Profile updated Succesfully!");
          this.editForm.reset(this.user);
        },
        error => {
          this.alertify.error(error);
        }
      );
  }
}
