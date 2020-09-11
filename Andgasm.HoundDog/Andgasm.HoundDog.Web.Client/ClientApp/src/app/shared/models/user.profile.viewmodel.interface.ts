import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";

export interface UserProfileViewModel {
  id: string;
  email: string;
  passwordClear: string;
  passwordClearConfirm: string;
  oldPasswordClear: string;
  firstName: string;
  lastName: string;
  userName: string;
  location: string;
  phoneNumber: string;
  doB: NgbDateStruct;

  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;

  hasChangePassword: boolean;

  twoFactorEnabled: boolean;
}
