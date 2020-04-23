import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { UserRegistration } from '../models/user.registration.interface';
import { ConfigService } from '../utils/config.service';

import { BaseService } from "./base.service";

import { Observable } from 'rxjs/Observable';

import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';

@Injectable()
export class UserService extends BaseService {

  baseUrl: string = '';

  constructor(private http: HttpClient, private configService: ConfigService) {
    super();

    this.baseUrl = configService.getAuthApiURI();
  }

  register(user: UserRegistration): Observable<boolean> {
    return this.http.post<any>(this.baseUrl + "/api/user",
      JSON.stringify({ "Email": user.email, "PasswordClear": user.password, "PasswordClearConfirm": user.confirmPassword, "FirstName": user.firstName, "LastName": user.lastName, "UserName": user.userName, "Location": user.location }),
      { headers: new HttpHeaders({ 'Content-Type': 'application/json' })})
      .pipe(map(res => true))
      .pipe(catchError(this.handleErrorArray));
  }
}
