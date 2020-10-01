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
  doB: SimpleDateViewModel;

  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;

  hasChangePassword: boolean;

  twoFactorEnabled: boolean;
}

export interface SimpleDateViewModel {
  day: number,
  month: number,
  year: number
}

export interface SelectListItemViewModel {
  id: number,
  text: string
}
