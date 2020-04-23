import { Injectable } from '@angular/core';

@Injectable()
export class ConfigService {

  _apiURI: string;
  _authApiURI: string;

  constructor() {
    this._apiURI = 'http://localhost:5000/api';
    this._authApiURI = 'https://localhost:44367';
  }

  getApiURI() {
    return this._apiURI;
  }

  getAuthApiURI() {
    return this._authApiURI;
  }
}
