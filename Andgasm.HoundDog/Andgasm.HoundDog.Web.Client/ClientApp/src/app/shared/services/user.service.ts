import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { UserProfileViewModel } from '../models/user.profile.viewmodel.interface';
import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';

@Injectable()
export class UserService extends BaseService {

  baseUrl: string = '';

  constructor(private http: HttpClient, private configService: ConfigService, private authService: AuthenticationService) {
    super();

    this.baseUrl = configService.getAuthApiURI();
  }

  getCurrentUser(): Observable<UserProfileViewModel> {
    if (this, this.authService.isLoggedIn) {
      return this.http.get<UserProfileViewModel>(this.baseUrl + "/api/user",
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this.authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<UserProfileViewModel>;
    }
  }

  register(user: UserProfileViewModel): Observable<any> {
    return this.http.post<any>(this.baseUrl + "/api/user",
      JSON.stringify(user),
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      })
      .pipe(map(res => true))
      .pipe(catchError(this.handleErrorArray));
  }

  updateProfile(user: UserProfileViewModel, changePassword: boolean): Observable<UserProfileViewModel> {
    if (this, this.authService.isLoggedIn) {
      user.hasChangePassword = changePassword;
      return this.http.put<UserProfileViewModel>(this.baseUrl + "/api/user/" + this.authService.currentUserValue.id,
        JSON.stringify(user),
        {
          headers: new HttpHeaders({
            'Content-Type': 'application/json',
            "Authorization": "Bearer " + this.authService.currentUserValue.token
          })
        })
        .pipe(catchError(this.handleErrorArray)) as Observable<UserProfileViewModel>;
    }
  }

  cloneUser(objToClone: UserProfileViewModel): UserProfileViewModel {
    return JSON.parse(JSON.stringify(objToClone))
  }
}
