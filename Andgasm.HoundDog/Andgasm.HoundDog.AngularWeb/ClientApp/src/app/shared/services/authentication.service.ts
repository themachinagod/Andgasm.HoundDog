import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { ConfigService } from '../utils/config.service';
import { BaseService } from "./base.service";
import { TokenService } from "./token.service";

import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';

import { User } from '../models/user.interface';

@Injectable({ providedIn: 'root' })
export class AuthenticationService extends BaseService{

  private baseUrl: string = '';
  private currentUserSubject: BehaviorSubject<User>;

  public currentUser: Observable<User>;

  constructor(private http: HttpClient,
              private configService: ConfigService,
              private tokenService: TokenService) {
    super();
    this.currentUserSubject = new BehaviorSubject<User>(JSON.parse(localStorage.getItem('currentUser')));
    this.currentUser = this.currentUserSubject.asObservable();
    this.baseUrl = configService.getAuthApiURI();
  }

  public get currentUserValue(): User {
    return this.currentUserSubject.value;
  }

  login(username: string, password: string) {

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    };

    return this.http.post<any>(this.baseUrl + '/api/authentication',
      JSON.stringify({ "SuppliedUserName": username, "SuppliedPassword": password }), httpOptions)
      .pipe(map(user => {
        debugger;
        // verify auth service returned valid user and valid user token to ensure authorised user
        if (user && user.token && this.tokenService.isValidToken(user.token)) {
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
        else { this.logout(); }
        return user;
      }))
      .pipe(catchError(this.handleErrorArray));
  }

  logout() {

    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  isLoggedIn() {

    return (this.currentUserValue &&
            this.tokenService.isValidToken(this.currentUserValue.token));
  }
}
