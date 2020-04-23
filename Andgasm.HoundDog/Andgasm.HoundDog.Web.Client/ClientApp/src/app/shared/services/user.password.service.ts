import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { UserProfileViewModel } from '../models/user.profile.viewmodel.interface';
import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";

import { Observable } from 'rxjs';

import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';
import { ChangePasswordViewModel } from '../models/change-password.viewmodel.interface';

@Injectable()
export class UserPasswordService extends BaseService {

  baseUrl: string = '';
  currentUser: Observable<UserProfileViewModel>;

  constructor(
    private _http: HttpClient,
    private _configService: ConfigService,
    private _authService: AuthenticationService) {
    super();

    this.baseUrl = _configService.getAuthApiURI();
  }

  changePassword(user: ChangePasswordViewModel): Observable<any> {
    debugger;
    if (this, this._authService.isLoggedIn) {
      return this._http.put<any>(this.baseUrl + "/api/user/" + user.username + "/passwordreset/" + this._authService.currentUserValue.id,
        JSON.stringify(user),
        {
          headers: new HttpHeaders({
            'Content-Type': 'application/json',
            "Authorization": "Bearer " + this._authService.currentUserValue.token
          })
        })
        .pipe(map(res => true))
        .pipe(catchError(this.handleErrorArray));
    }
  }

  resetPassword(user: ChangePasswordViewModel): Observable<any> {

    return this._http.post<any>(this.baseUrl + "/api/user/" + user.username + "/passwordreset",
      JSON.stringify(user),
      {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        })
      })
      .pipe(map(res => true))
      .pipe(catchError(this.handleErrorArray));
  }

  sendPasswordReset(username: string, verifyCode: string, email: string): Observable<boolean> {

    return this._http.get<any>(this.baseUrl + "/api/user/" + username + "/passwordreset?verificationCode=" + verifyCode + "&email=" + email)
      .pipe(catchError(this.handleErrorArray));
  }
}
