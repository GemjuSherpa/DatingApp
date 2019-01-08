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

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseurl + "users");
  }

  getUser(id): Observable<User> {
    return this.http.get<User>(this.baseurl + "users/" + id);
  }
}
