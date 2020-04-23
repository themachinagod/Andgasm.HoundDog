import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { UserProfileComponent } from './user-profile/user-profile.component';
import { UserLoginComponent } from './user-login/user-login.component';
import { AccountSettingsComponent } from './account-settings/account-settings.component';
import { VerifyPhoneComponent } from './verify-phone/verify-phone.component';
import { Configure2FAComponent } from './configure-2fa/configure-2fa.component';
import { AuthGuard } from '../shared/guards/auth.guards';
import { MailAddressConfirmationComponent } from './mail-confirmation/mail-confirmation.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ChangePasswordComponent } from './change-password/change-password.component';

export const routing: ModuleWithProviders = RouterModule.forChild([
  { path: 'register', component: UserProfileComponent, data: { formMode: 'CreateNew' } },
  { path: 'login', component: UserLoginComponent },
  { path: 'mailconfirmation', component: MailAddressConfirmationComponent },
  { path: 'forgotpassword', component: ForgotPasswordComponent },
  { path: 'resetpassword', component: ChangePasswordComponent, data: { formMode: 'ResetExisting' } },

  { path: 'settings', component: AccountSettingsComponent, canActivate: [AuthGuard] },
  { path: 'verifyphone', component: VerifyPhoneComponent, canActivate: [AuthGuard] },
  { path: 'configure2fa', component: Configure2FAComponent, canActivate: [AuthGuard] }

]);
