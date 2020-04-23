import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { ConfigService } from '../utils/config.service';
import { AuthenticationService } from '../services/authentication.service';
import { BaseService } from "./base.service";

import { Observable, BehaviorSubject } from 'rxjs';
import { DomSanitizer } from '@angular/platform-browser'

import { map } from 'rxjs/operators';
import { catchError } from 'rxjs/operators';

@Injectable()
export class UserAvatarService extends BaseService {

  baseUrl: string = '';

  // #region Behaviour Observables
  private _userProfileAvatarDataSource = new BehaviorSubject<Blob>(null);
  $userProfileAvatarData = this._userProfileAvatarDataSource.asObservable();
  // #endregion

  constructor(
    private http: HttpClient,
    private configService: ConfigService,
    private authService: AuthenticationService,
    private sanitizer: DomSanitizer) {
    super();

    this.baseUrl = configService.getAuthApiURI();
  }

  getAvatar(): Observable<Blob> {
    if (this, this.authService.isLoggedIn) {
      return this.http.get(this.baseUrl + "/api/user/" + this.authService.currentUserValue.id + "/avatar",
        { responseType: 'blob', headers: new HttpHeaders({ "Authorization": "Bearer " + this.authService.currentUserValue.token }) })
        .pipe(catchError(this.handleErrorArray)) as Observable<Blob>;
    }
  }

  uploadAvatar(imageBlob: Blob): Observable<boolean> {
    const imageFile = new File([imageBlob], "avatardata", { type: 'image/jpeg' });
    const avatardata = new FormData();
    avatardata.append('avatardata', imageFile, imageFile.name);

    if (this, this.authService.isLoggedIn) {
      return this.http.post<any>(this.baseUrl + "/api/user/" + this.authService.currentUserValue.id + "/avatar",
        avatardata,
        {
          headers: new HttpHeaders({
            "Authorization": "Bearer " + this.authService.currentUserValue.token
          })
        })
        .pipe(map(result => { this._userProfileAvatarDataSource.next(imageBlob) }))
        .pipe(map(result => true))
        .pipe(catchError(this.handleErrorArray)) as Observable<boolean>;
    }
  }

  constructImageUrlFromBlob(blobdata: Blob) : any {
    if (blobdata != null) {
      var objectURL = URL.createObjectURL(blobdata);
      return this.sanitizer.bypassSecurityTrustUrl(objectURL);
    }
  }
}
