"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var router_1 = require("@angular/router");
var user_profile_component_1 = require("./user-profile/user-profile.component");
var user_login_component_1 = require("./user-login/user-login.component");
var account_settings_component_1 = require("./account-settings/account-settings.component");
var verify_phone_component_1 = require("./verify-phone/verify-phone.component");
var configure_2fa_component_1 = require("./configure-2fa/configure-2fa.component");
var auth_guards_1 = require("../shared/guards/auth.guards");
var mail_confirmation_component_1 = require("./mail-confirmation/mail-confirmation.component");
var forgot_password_component_1 = require("./forgot-password/forgot-password.component");
var change_password_component_1 = require("./change-password/change-password.component");
exports.routing = router_1.RouterModule.forChild([
    { path: 'register', component: user_profile_component_1.UserProfileComponent, data: { formMode: 'CreateNew' } },
    { path: 'login', component: user_login_component_1.UserLoginComponent },
    { path: 'mailconfirmation', component: mail_confirmation_component_1.MailAddressConfirmationComponent },
    { path: 'forgotpassword', component: forgot_password_component_1.ForgotPasswordComponent },
    { path: 'resetpassword', component: change_password_component_1.ChangePasswordComponent, data: { formMode: 'ResetExisting' } },
    { path: 'settings', component: account_settings_component_1.AccountSettingsComponent, canActivate: [auth_guards_1.AuthGuard] },
    { path: 'verifyphone', component: verify_phone_component_1.VerifyPhoneComponent, canActivate: [auth_guards_1.AuthGuard] },
    { path: 'configure2fa', component: configure_2fa_component_1.Configure2FAComponent, canActivate: [auth_guards_1.AuthGuard] }
]);
