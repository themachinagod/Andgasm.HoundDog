import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { UserProfileViewModel } from '../models/user.profile.viewmodel.interface';
import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";

import { Observable } from 'rxjs';

import { catchError } from 'rxjs/operators';

@Injectable()
export class PhoneConfirmationService extends BaseService {

  baseUrl: string = '';

  constructor(
    private _http: HttpClient,
    private _configService: ConfigService,
    private _authService: AuthenticationService) {
    super();

    this.baseUrl = _configService.getAuthApiURI();
  }

  sendPhoneVerificationCode(): Observable<void> {

    if (this, this._authService.isLoggedIn) {
      return this._http.get<void>(this.baseUrl + "/api/user/" + this._authService.currentUserValue.id + "/phoneconfirmation",
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this._authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<void>;
    }
    return new Observable<void>();
  }

  verifyPhoneVerificationCode(verifyCode: string): Observable<void> {

    if (this, this._authService.isLoggedIn) {
      return this._http.post<void>(this.baseUrl + "/api/user/" + this._authService.currentUserValue.id + "/phoneconfirmation?verifycode=" + verifyCode, null,
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this._authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<void>;
    }
    return new Observable<void>();
  }
}
