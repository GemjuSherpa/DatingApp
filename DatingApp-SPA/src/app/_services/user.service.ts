import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { User } from "../_models/user";
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class UserService {
  baseurl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  //get all users
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseurl + "users");
  }

  //get single user details
  getUser(id): Observable<User> {
    return this.http.get<User>(this.baseurl + "users/" + id);
  }

  //Update user Details
  updateUser(id: number, user: User) {
    return this.http.put(this.baseurl + "users/" + id, user);
  }

  //Set profile picture
  setMainPhoto(userId: number, id: number) {
    return this.http.post(
      this.baseurl + "users/" + userId + "/photos/" + id + "/setMain",
      {}
    );
  }
}
