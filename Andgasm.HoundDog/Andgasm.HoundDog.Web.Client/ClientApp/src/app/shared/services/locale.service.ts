import { Injectable } from '@angular/core';
import { BaseService } from "./base.service";

@Injectable({ providedIn: 'root' })
export class LocaleService extends BaseService {

  constructor() {
    super();
  }

  getUsersLocale(defaultValue: string): string {
    if (typeof window === 'undefined' || typeof window.navigator === 'undefined') {
      return defaultValue;
    }
    const wn = window.navigator as any;
    let lang = wn.languages ? wn.languages[0] : defaultValue;
    lang = lang || wn.language || wn.browserLanguage || wn.userLanguage;
    return lang;
  }
}
