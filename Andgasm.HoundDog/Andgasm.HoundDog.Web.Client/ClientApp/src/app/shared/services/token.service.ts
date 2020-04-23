import { Injectable } from '@angular/core';
import { BaseService } from "./base.service";

import * as jwt_decode from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class TokenService extends BaseService {

  constructor() {
    super();
  }

  isValidToken(currentUserToken: string): boolean {

    if (!currentUserToken) return false;
    const tokenDate = this.getTokenExpirationDate(currentUserToken);
    if (tokenDate === undefined) return false;
    return (tokenDate.valueOf() > new Date().valueOf());
  }

  getTokenExpirationDate(token: string): Date {

    const decoded = jwt_decode(token);
    if (decoded.exp === undefined) return null;

    const date = new Date(0);
    date.setUTCSeconds(decoded.exp);
    return date;
  }
}
