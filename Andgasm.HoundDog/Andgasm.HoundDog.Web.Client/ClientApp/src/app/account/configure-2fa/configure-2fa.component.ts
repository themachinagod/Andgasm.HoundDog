//#region Imports
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../shared/services/user.service';
import { TwoFactorAuthenticationService } from '../../shared/services/twofactor.authentication.service';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
//#endregion

@Component({
  selector: 'configure-2fa',
  templateUrl: './configure-2fa.component.html',
  styleUrls: ['./configure-2fa.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class Configure2FAComponent implements OnInit {

  // #region Form State Fields
  qrUri: string = "invaliddata";
  errors: [];
  isRequesting: boolean;
  verificationCode: string;
  sharedKey: string = "invaliddata";
  userConfigured2FA: boolean;
  // #endregion

  //#region Constructor
  constructor(
    private _userService: UserService,
    private _twofaService: TwoFactorAuthenticationService,
    private _toastrService: ToastrService,
    private _router: Router) {

    this.errors = [];
  }
  //#endregion

  //#region Init/Destroy
  ngOnInit() {

    this.initialiseUser2FAStatus();
  }
  //#endregion

  //#region 2FA Management
  getAuthenticatorSharedKey() {

    this.errors = [];
    this.isRequesting = true;

    this._twofaService.generateAuthenticatorSharedKey()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this.sharedKey = result.sharedKey;
          this.qrUri = result.qrCodeUri
        },
        errors => { this.errors = errors; });
  }

  verifyAuthenticatorCode() {

    this.errors = [];
    this.isRequesting = true;
    this._twofaService.enableTwoFactorAuthentication(this.verificationCode)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this._toastrService.success('You have successfully configured your account with Two-Factor authentication. We will use this security measure to protect your account from unauthorised access and changes!', '2FA Configured!');
          this.userConfigured2FA = true;
        },
        errors => {
          this._toastrService.error('There was an error while trying to configure two factor authentication. Please try again, if this problem continues please contact a member of our suppport team!', '2FA Configuration Failed!');
          this.errors = errors;
        });
  }

  disable2FA() {

    this.errors = [];
    this.isRequesting = true;
    this._twofaService.disableTwoFactorAuthentication()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this._toastrService.success('You have successfully disabled 2FA on your account. Note that having 2FA configured on your account is reccomended and that by bypassing 2FA your account is less secure than it should be!', '2FA Disabled!');
          this.userConfigured2FA = false;
        },
        errors => {
          this._toastrService.error('There was an error while trying to disable two factor authentication. Please try again, if this problem continues please contact a member of our suppport team!', '2FA Disable Failed!');
          this.errors = errors;
        });
  }
  //#endregion

  // #region User Retrieval
  initialiseUser2FAStatus() {

    this.isRequesting = true;
    this._userService.getCurrentUser()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(data => {
        this.userConfigured2FA = data.twoFactorEnabled;
        if (!this.userConfigured2FA) {
          this.getAuthenticatorSharedKey();
        }
      },
      errors => { this.errors = errors; });
  }
  // #endregion
}
