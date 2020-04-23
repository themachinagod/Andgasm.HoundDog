import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";

import { Observable } from 'rxjs';

import { catchError } from 'rxjs/operators';

@Injectable()
export class MailConfirmationService extends BaseService {

  baseUrl: string = '';

  constructor(private http: HttpClient, private configService: ConfigService, private authService: AuthenticationService) {
    super();

    this.baseUrl = configService.getAuthApiURI();
  }

  resendEmailConfirmationNotification(): Observable<boolean> {
    if (this, this.authService.isLoggedIn) {
      return this.http.get<any>(this.baseUrl + "/api/user/" + this.authService.currentUserValue.id + "/emailconfirmation/",
        { headers: new HttpHeaders({ "Authorization": "Bearer " + this.authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray));
    }
    return new Observable<true>();
  }
}
