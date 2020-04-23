import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";
import { AuthenticatorKeyViewModel } from '../models/authenticator-key.viewmodel.interface';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class TwoFactorAuthenticationService extends BaseService {

  baseUrl: string = '';

  constructor(
    private _http: HttpClient,
    private _configService: ConfigService,
    private _authService: AuthenticationService) {
    super();

    this.baseUrl = _configService.getAuthApiURI();
  }

  generateAuthenticatorSharedKey(): Observable<AuthenticatorKeyViewModel> {
    if (this, this._authService.isLoggedIn) {
      return this._http.get<AuthenticatorKeyViewModel>(this.baseUrl + "/api/user/" + this._authService.currentUserValue.id + "/twofactorconfiguration/",
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this._authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<AuthenticatorKeyViewModel>;
    }
    return new Observable<AuthenticatorKeyViewModel>();
  }

  enableTwoFactorAuthentication(verifyCode: string): Observable<void> {
    debugger;
    if (this, this._authService.isLoggedIn) {
      return this._http.post<void>(this.baseUrl + "/api/user/" + this._authService.currentUserValue.id + "/twofactorconfiguration?verifycode=" + verifyCode, null,
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this._authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<void>;
    }
  }

  disableTwoFactorAuthentication(): Observable<void> {
    if (this, this._authService.isLoggedIn) {
      return this._http.put<void>(this.baseUrl + "/api/user/" + this._authService.currentUserValue.id + "/twofactorconfiguration", null,
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this._authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<void>;
    }
  }
}
