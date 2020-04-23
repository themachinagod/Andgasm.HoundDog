// #region Imports
import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { UserProfileViewModel } from '../../shared/models/user.profile.viewmodel.interface';
import { UserService } from '../../shared/services/user.service';
import { MailConfirmationService } from '../../shared/services/mail.confirmation.service';
import { ToastrService } from 'ngx-toastr';
import { UserAvatarService } from '../../shared/services/user.avatar.service';
import { AvatarUploadComponent } from '../avatar-upload/avatar-upload.component';
import { NgForm } from '@angular/forms';
import { finalize } from 'rxjs/operators';
// #endregion

@Component({
  selector: 'user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit {

  // #region Input Properties
  @Input() formMode: string = "ViewOnly";

  @ViewChild("f", null) userForm: NgForm;
  // #endregion

  // #region Form State Fields
  errors: [];
  isRequesting: boolean;
  userDataInitialised: boolean = false;
  submitted: boolean = false;
  showPasswordChangeFields = false;
  originalFormMode: string = "ViewOnly";
  avatarImageData: any;
  hasAvatar: boolean;
  originalUserDataSnapshot: UserProfileViewModel = {} as UserProfileViewModel;
  currentUser: UserProfileViewModel = {} as UserProfileViewModel;

  private get isDataEntryMode(): boolean {
    if (this.formMode == "EditExisting" ||
      this.formMode == "CreateNew")
      return true;
    else
      return false;
  }

  private get isEditMode(): boolean {
    if (this.formMode == "EditExisting")
      return true;
    else
      return false;
  }

  private get isCreateMode(): boolean {
    if (this.formMode == "CreateNew")
      return true;
    else
      return false;
  }

  private get userLookupRequired(): boolean {
    if (this.formMode != "CreateNew")
      return true;
    else
      return false;
  }

  private get formTitle(): string {
    if (this.isEditMode) {
      return "Edit your account profile details";
    } else if (this.isCreateMode) {
      return "Create your user account"
    }
    else {
      return "Your account profile details";
    }
  }
  // #endregion

  // #region Constructor
  constructor(private _userService: UserService,
              private _avatarService: UserAvatarService,
              private _mailService: MailConfirmationService,
              private _toastrService: ToastrService,
              private _activatedroute: ActivatedRoute,
              private _modalService: NgbModal,
              private _router: Router) {

    this.errors = [];
  }
  // #endregion

  // #region Initialise/Destroy
  ngOnInit() {
    this.readFormModeFromState();
    this.initialiseCurrentUser();
    this.getProfileImage()
  }
  // #endregion

  // #region User Management
  save({ value, valid }: { value: UserProfileViewModel, valid: boolean }) {

    this.submitted = true;

    if (valid) {
      this.isRequesting = true;

      if (this.isCreateMode) {
        this.registerUser({ value });
      }
      else if (this.isEditMode) {
        this.updateUserProfile({ value });
      }
    }
  }

  registerUser({ value }: { value: UserProfileViewModel }) {

    this._userService.register(value)
      .pipe(finalize(() => { debugger; this.isRequesting = false }))
      .subscribe(
        result => {
          if (result) {
            this._toastrService.success('You have successfully registered as a HoundDog user, welcome to the community. Please sign in with your registered credentials to get started!', 'Account Created!');
            this._router.navigate(['/login'], { queryParams: { brandNew: true, email: value.userName } });
          }
        },
        errors => {
          this._toastrService.error('There was an error while trying to create your profile. Please try again, if this problem continues please contact a member of our suppport team!', 'Profile Registration Failed!');
          this.errors = errors;
        });
  }

  updateUserProfile({ value }: { value: UserProfileViewModel }) {

    this._userService.updateProfile(value, this.showPasswordChangeFields)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          if (result) {
            this._toastrService.success('You have successfully updated your account profile details!', 'Profile Updated!');
            this.currentUser.phoneNumberConfirmed = result.phoneNumberConfirmed;
            this.originalUserDataSnapshot = this._userService.cloneUser(this.currentUser);
            this.resetFormState();
          }
        },
        errors => {
          this._toastrService.error('There was an error while trying to update your profile. Please try again, if this problem continues please contact a member of our suppport team!', 'Profile Update Failed!');
          this.errors = errors;
        });
  }

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

  initiateVerifyPhoneProcess() {

    this._router.navigate(['/verifyphone']); 
  }

  openProfileImageUploadModal() {

    debugger;
    var modalRef = this._modalService.open(AvatarUploadComponent);
    modalRef.componentInstance.modalEventEmitter.subscribe((croppedimagedata) => {
      debugger;
      this.avatarImageData = croppedimagedata;
      this.hasAvatar = true;
    })
  }
  // #endregion

  // #region User Retrieval
  initialiseCurrentUser() {

    if (this.userLookupRequired) {
      this.isRequesting = true;
      this._userService.getCurrentUser()
        .pipe(finalize(() => this.isRequesting = false))
        .subscribe(
          data => {
          if (data) {
            this.initialiseUserData(data);
          }
        },
        errors => { this.errors = errors; });
    }
  }

  getProfileImage() {

    if (this.userLookupRequired) {
      this.isRequesting = true;
      this._avatarService.getAvatar()
        .pipe(finalize(() => this.isRequesting = false))
        .subscribe(
          data => {
            if (data.size > 0) {
              this.initialiseAvatarData(data);
              this.hasAvatar = true;
            }
            else {
              this.hasAvatar = false;
            }
          },
          errors => { this.errors = errors; });
    }
  }
  // #endregion

  // #region State Management Operations
  changeToEditMode() {

    this.formMode = "EditExisting";
    this.showPasswordChangeFields = false;
  }

  resetFormState() {

    this.errors = [];
    this.isRequesting = false;
    this.submitted = false;
    this.formMode = this.originalFormMode;
    this.showPasswordChangeFields = false;

    this.userForm.form.markAsPristine();
    this.userForm.form.markAsUntouched();

    this.currentUser = this._userService.cloneUser(this.originalUserDataSnapshot)
  }

  readFormModeFromState() {

    this._activatedroute.data.subscribe(d => {
      this.formMode = d["formMode"];
      this.originalFormMode = this.formMode;
    });
  }

  initialiseUserData(data: UserProfileViewModel) {

    this.userDataInitialised = true;
    this.currentUser = data;
    this.originalUserDataSnapshot = JSON.parse(JSON.stringify(data))
  }

  initialiseAvatarData(data) {
    
    this.avatarImageData = this._avatarService.constructImageUrlFromBlob(data);
    this.hasAvatar = (this.avatarImageData);
  }
  // #endregion
}
