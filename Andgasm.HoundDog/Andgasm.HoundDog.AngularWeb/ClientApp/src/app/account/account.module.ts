import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/modules/shared.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { RegistrationFormComponent } from './registration-form/registration-form.component';
import { LoginFormComponent } from './login-form/login-form.component';

import { UserService } from '../shared/services/user.service';
import { AuthenticationService } from '../shared/services/authentication.service';
import { TokenService } from '../shared/services/token.service';

import { EmailValidator } from '../directives/email.validator.directive';
import { PasswordValidator } from '../directives/password.validator.directive';
import { EqualValidator } from '../directives/equal.validator.directive';
import { BootstrapTabDirective } from '../directives/bootstrap-tab.directive';

import { routing } from './account.routing';
import { SettingsComponent } from './settings/settings.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  imports: [
    CommonModule, FormsModule, SharedModule, BrowserAnimationsModule, NgbModule, routing
  ],
  declarations: [RegistrationFormComponent, LoginFormComponent, BootstrapTabDirective,
                 EmailValidator, PasswordValidator, EqualValidator, SettingsComponent],
  providers: [UserService, AuthenticationService, TokenService]
})
export class AccountModule { }
