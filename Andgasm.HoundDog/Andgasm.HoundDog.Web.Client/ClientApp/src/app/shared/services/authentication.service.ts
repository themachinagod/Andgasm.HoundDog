// #region Imports
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { ConfigService } from '../utils/config.service';
import { BaseService } from "./base.service";
import { TokenService } from "./token.service";

import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';

import { CredentialsViewModel } from '../models/credentials.viewmodel.interface';
import { UserSecurityViewModel } from '../models/user.security.viewmodel.interface';
import { SignInResultViewModel } from '../models/sign-in-result.viewmodel.interface';
import { SignInResultEnum } from '../enums/sign-in-result.enum';
import { Router } from '@angular/router';
// #endregion

@Injectable({ providedIn: 'root' })
export class AuthenticationService extends BaseService{

  // #region State Properties
  baseUrl: string = '';
  currentUserSubject: BehaviorSubject<UserSecurityViewModel>;
  currentUser: Observable<UserSecurityViewModel>;

  get currentUserValue(): UserSecurityViewModel {

    return this.currentUserSubject.value;
  }

  get isLoggedIn() : boolean {

    return (this.currentUserValue &&
      this.tokenService.isValidToken(this.currentUserValue.token));
  }
  // #endregion

  // #region Constructor
  constructor(private http: HttpClient,
              private router: Router,
              private configService: ConfigService,
              private tokenService: TokenService) {
    super();

    this.currentUserSubject = new BehaviorSubject<UserSecurityViewModel>(JSON.parse(localStorage.getItem('currentUser')));
    this.currentUser = this.currentUserSubject.asObservable();
    this.baseUrl = configService.getAuthApiURI();
  }
  // #endregion

  // #region Authorisation Management
  login(credentials: CredentialsViewModel): Observable<SignInResultViewModel> {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };
    var res = new SignInResultViewModel();
    return this.http.post<UserSecurityViewModel>(this.baseUrl + '/api/authentication',
      JSON.stringify({ "SuppliedUserName": credentials.username, "SuppliedPassword": credentials.password, "VerificationCode": credentials.verificationCode }), httpOptions)
      .pipe<SignInResultViewModel>(map(user => {
        if (user && user.token && this.tokenService.isValidToken(user.token)) {
          res.signinResult = SignInResultEnum.Success;
          this.storeUserLogin(user);
          return res;
        }

        this.logout();
        res.signinResult = SignInResultEnum.Failure
        return res;
      }))
      .pipe(catchError(this.handleErrorArray)) as Observable<SignInResultViewModel>;
  }

  logout() {

    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  userNameRequires2FA(userName: string): Observable<boolean> {
    return this.http.get<boolean>(this.baseUrl + '/api/authentication/' + userName)
        .pipe(catchError(this.handleErrorArray)) as Observable<boolean>;
  }

  storeUserLogin(user: UserSecurityViewModel) {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }
  // #endregion
}
