import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject } from "rxjs";

import { map } from "rxjs/operators";
import { JwtHelperService } from "@auth0/angular-jwt";
import { environment } from "src/environments/environment";
import { User } from "../_models/user";

@Injectable({
  providedIn: "root"
})
export class AuthService {
  baseUrl: string = environment.apiUrl + "auth/";
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;

  // defualt image
  photoUrl = new BehaviorSubject<string>("../../assets/images/user.png");
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient) {}


  //  to achange the main photo member
  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }



  login(model: any) {
    return this.http.post(this.baseUrl + "login", model).pipe(
      map((response: any) => {
        const user = response;
        if (user) {
          localStorage.setItem("token", user.token);
          localStorage.setItem("user", JSON.stringify(user.user));
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          this.currentUser = user.user;
          this.changeMemberPhoto(this.currentUser.photoUrl);
          console.log(this.decodedToken);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + "register", model);
  }

  loggedIn() {
    const token = localStorage.getItem("token");
    return !this.jwtHelper.isTokenExpired(token);
  }
}
