import { catchError } from "rxjs/operators";
import { AlertifyService } from "./../_services/alertify.service";
import { UserService } from "src/app/_services/user.service";
import { User } from "./../_models/user";
import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";

@Injectable()
export class ListsResolver implements Resolve<User[]> {
  pageNumber = 1;
  pageSize = 10;
  likesParam = "likers";
  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService
  ) {}

  resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
    return this.userService
      .getUsers(this.pageNumber, this.pageSize, null, this.likesParam)
      .pipe(
        catchError(error => {
          this.alertify.error("Problem retrieving data");
          this.router.navigate(["/home"]);
          return of(null);
        })
      );
  }
}
