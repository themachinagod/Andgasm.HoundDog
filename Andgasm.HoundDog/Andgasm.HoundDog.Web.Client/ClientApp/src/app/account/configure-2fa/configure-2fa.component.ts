//#region Imports
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { UserService } from '../../shared/services/user.service';
import { TwoFactorAuthenticationService } from '../../shared/services/twofactor.authentication.service';
import { MailConfirmationService } from '../../shared/services/mail.confirmation.service';
import { ConfirmationService } from '../../shared/services/confirmation.service';
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
  userConfirmedEmail: boolean;
  // #endregion

  //#region Constructor
  constructor(
    private _userService: UserService,
    private _twofaService: TwoFactorAuthenticationService,
    private _mailService: MailConfirmationService,
    private _toastrService: ToastrService,
    private _confirmationService: ConfirmationService) {

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

  public openDisable2FAConfirmationDialog() {
    this._confirmationService.confirm('Please confirm you wish to disable 2FA', 'Are you sure you wish to disable Two-Factor Authentication on this account? <br/ >It is our advice that all accounts enable 2FA to ensure adequate security is in place to protect your account. Note that you will be able to renable 2FA at any time.', "Yes, disable 2FA", "No, leave 2FA enabled", "lg")
      .then((confirmed) => { if (confirmed) this.disable2FA() })
      .catch(() => console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)'));
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
        this.userConfirmedEmail = data.emailConfirmed;
        if (!this.userConfigured2FA && this.userConfirmedEmail) {
          this.getAuthenticatorSharedKey();
        }
      },
      errors => { this.errors = errors; });
  }
  // #endregion

  // DBr: Mailer for resend of email conf - can we make this shared - its used on profile and now here!
  resendEmailConfirmationEmail() {

    this._mailService.resendEmailConfirmationNotification()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this._toastrService.success('We have resent a confirmation email to your associated email address. Please follow the link contained within this email to complete the confirmation process. If you do not receive the confirmation email please check your junk folders before resending again!', 'Email Confirmation Resent!');
        },
        errors => {
          this._toastrService.warning('We could not resend a confirmation email your accounts associated email address. Please try again later, if this problem continues please contact a member of our support team!', 'Email Confirmation Failed!');
          this.errors = errors;
        });
  }
}
