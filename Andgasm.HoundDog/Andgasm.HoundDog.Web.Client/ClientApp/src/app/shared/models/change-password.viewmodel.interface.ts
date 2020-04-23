export interface ChangePasswordViewModel {
  userid: string;
  username: string;
  password: string;
  passwordConfirm: string;
  oldPassword: string;
  verificationCode: string;
  resetToken: string;
}
