// #region Imports;
import { Component, OnInit, Input, ViewChild } from '@angular/core';
import {  ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../shared/services/user.service';
import { UserPasswordService } from '../../shared/services/user.password.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ChangePasswordViewModel } from '../../shared/models/change-password.viewmodel.interface';
import { finalize } from 'rxjs/operators';
// #endregion

@Component({
  selector: 'change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {

  // #region Input Properties
  @Input() formMode: string = "ViewOnly";
  // #endregion

  // #region Form Elements
  @ViewChild("f", null) userForm: NgForm;
  // #endregion

  // #region Fields
  errors: [];
  isRequesting: boolean;
  userDataInitialised: boolean = false;
  submitted: boolean = false;
  passwordChangeSuccess = false;
  passwordResetSuccess = false;
  originalFormMode: string = "ViewOnly";
  requires2fachallenge: boolean = false;
  subscription: Subscription;
  passwordChangeData: ChangePasswordViewModel = {} as ChangePasswordViewModel;
  // #endregion
  
  // #region Derived Properties
  private get isEditMode(): boolean {
    if (this.formMode == "EditExisting" || this.formMode == "ResetExisting")
      return true;
    else
      return false;
  }

  private get isResetMode(): boolean {
    if (this.formMode == "ResetExisting")
      return true;
    else
      return false;
  }
  // #endregion

  // #region Constructor
  constructor(
    private _userService: UserService,
    private _passwordService: UserPasswordService,
    private _toastrService: ToastrService,
    private _routerService: Router,
    private _activatedroute: ActivatedRoute,
    private _authenticationService: AuthenticationService) {

    this.errors = [];
  }
  // #endregion

  // #region Initialise/Destroy
  ngOnInit() {

    this.readFormModeFromState();
    this.initialiseCurrentUser();
  }
  // #endregion

  // #region Password Management
  save({ valid }: { valid: boolean }) {

    this.submitted = true;
    if (valid) {
      this.isRequesting = true;
      if (this.isResetMode) {
        this.resetUserPassword();
      }
      else if (this.isEditMode) {
        this.changeUserPassword();
      }
    }
  }

  changeUserPassword() {

    this._passwordService.changePassword(this.passwordChangeData)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          if (result) {
            this.resetFormState();
          }
          this.setChangePasswordResult(result);
          this._toastrService.success('You have successfully changed your account password! Please use these new credentials for future sign ons!', 'Password Changed!');
        },
        errors => {
          this._toastrService.error('There was an error while trying to change your password. Please try again, if this problem continues please contact a member of our suppport team!', 'Password Change Failed!');
          this.errors = errors;
        });
  }

  resetUserPassword() {

    this._passwordService.resetPassword(this.passwordChangeData)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this._toastrService.success('You have successfully reset your account password! Please sign in using your new credentials!', 'Password Reset!');
          this._routerService.navigate(["/login"]);
        },
        errors => {
          this._toastrService.error('There was an error while trying to reset your password. Please try again, if this problem continues please contact a member of our suppport team!', 'Password Reset Failed!');
          this.errors = errors;
        });
  }
  // #endregion

  // #region User Retrieval
  initialiseCurrentUser() {

    if (!this.isResetMode) {
      this.initialiseCurrentUserFromBackend();
    }
    else {
      this.initialiseCurrentUserFromRequest()
    }
  }

  initialiseCurrentUserFromBackend() {

    this.isRequesting = true;
    this._userService.getCurrentUser()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(data => {
        this.passwordChangeData.userid = data.id;
        this.requires2fachallenge = data.twoFactorEnabled;
      },
      errors => { this.errors = errors; });
  }

  initialiseCurrentUserFromRequest() {

    this.subscription = this._activatedroute.queryParams.subscribe(
      (param: any) => {
        this.passwordChangeData.userid = param['userid'];
        this.passwordChangeData.resetToken = param['token'];
        this.determine2faStatus(this.passwordChangeData.userid)
      });
  }

  determine2faStatus(identifier: string) {
    
    if (identifier) {
      this._authenticationService.userNameRequires2FA(identifier)
        .subscribe(
          result => {
            this.requires2fachallenge = result;
          },
          errors => {
            this.errors = errors;
          });
    }
  }
  // #endregion

  // #region State Management Operations
   changeToEditMode() {

    this.formMode = "EditExisting";
  }

  resetFormState() {

    this.errors = [];
    this.isRequesting = false;
    this.submitted = false;
    this.formMode = this.originalFormMode;

    this.userForm.form.markAsPristine();
    this.userForm.form.markAsUntouched();
  }

  setChangePasswordResult(result) {
    this.passwordChangeSuccess = result;
  }

  setResetPasswordResult(result) {
    this.passwordResetSuccess = result;
  }

  readFormModeFromState() {
    this._activatedroute.data.subscribe(d => {
      this.formMode = d["formMode"];
      this.originalFormMode = this.formMode;
    });
  }
  // #endregion
}
