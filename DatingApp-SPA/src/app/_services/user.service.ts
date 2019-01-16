import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { User } from "../_models/user";
import { Observable } from "rxjs";
import { PaginatedResult } from "../_models/pagination";
import { map } from "rxjs/operators";

@Injectable({
  providedIn: "root"
})
export class UserService {
  baseurl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  //get all users
  getUsers(
    page?,
    itemsPerPage?,
    userParams?,
    likesParam?
  ): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<
      User[]
    >();
    let params = new HttpParams();
    if (page != null && itemsPerPage != null) {
      params = params.append("pageNumber", page);
      params = params.append("pageSize", itemsPerPage);
    }

    if (userParams != null) {
      params = params.append("minAge", userParams.minAge);
      params = params.append("maxAge", userParams.maxAge);
      params = params.append("gender", userParams.gender);
      params = params.append("orderBy", userParams.orderBy);
    }

    if (likesParam == "likers") {
      params = params.append("likers", "true");
    }
    if (likesParam == "likees") {
      params = params.append("likees", "true");
    }

    return this.http
      .get<User[]>(this.baseurl + "users", { observe: "response", params })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get("Pagination") != null) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get("pagination")
            );
          }
          return paginatedResult;
        })
      );
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

  //Delete photos
  deletePhoto(userId: number, id: number) {
    return this.http.delete(this.baseurl + "users/" + userId + "/photos/" + id);
  }

  //like users
  sendLike(id: number, recipientId: number) {
    return this.http.post(
      this.baseurl + "users/" + id + "/like/" + recipientId,
      {}
    );
  }
}
