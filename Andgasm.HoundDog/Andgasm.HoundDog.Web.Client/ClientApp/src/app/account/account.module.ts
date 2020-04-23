import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/modules/shared.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ImageCropperModule } from 'ngx-image-cropper';
import { QRCodeModule } from 'angularx-qrcode';

import { AccountSettingsComponent } from './account-settings/account-settings.component';
import { AvatarUploadComponent } from './avatar-upload/avatar-upload.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { Configure2FAComponent } from './configure-2fa/configure-2fa.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { MailAddressConfirmationComponent } from './mail-confirmation/mail-confirmation.component';
import { UserLoginComponent } from './user-login/user-login.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { VerifyPhoneComponent } from './verify-phone/verify-phone.component';

import { UserService } from '../shared/services/user.service';
import { AuthenticationService } from '../shared/services/authentication.service';
import { TokenService } from '../shared/services/token.service';
import { PhoneConfirmationService } from '../shared/services/phone.confirmation.service';
import { MailConfirmationService } from '../shared/services/mail.confirmation.service';
import { TwoFactorAuthenticationService } from '../shared/services/twofactor.authentication.service';
import { UserAvatarService } from '../shared/services/user.avatar.service';
import { UserPasswordService } from '../shared/services/user.password.service';

import { EmailValidator } from '../shared/directives/email.validator.directive';
import { PasswordValidator } from '../shared/directives/password.validator.directive';
import { EqualValidator } from '../shared/directives/equal.validator.directive';

import { routing } from './account.routing';

@NgModule({
  imports: [
    CommonModule, FormsModule, SharedModule, BrowserAnimationsModule, NgbModule, ImageCropperModule, QRCodeModule, routing
  ],
  declarations: [AvatarUploadComponent, UserLoginComponent, ChangePasswordComponent, Configure2FAComponent, ForgotPasswordComponent, MailAddressConfirmationComponent, UserLoginComponent, UserProfileComponent, VerifyPhoneComponent,
                 EmailValidator, PasswordValidator, EqualValidator, AccountSettingsComponent],
  providers: [UserService, AuthenticationService, TokenService, PhoneConfirmationService, MailConfirmationService, TwoFactorAuthenticationService, UserAvatarService, UserPasswordService],
  entryComponents: [AvatarUploadComponent]
})
export class AccountModule { }
