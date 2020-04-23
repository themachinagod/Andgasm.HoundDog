export class UserSecurityViewModel {
  id: string;
  userName: string;
  roles: string;
  token?: string;
  authentication2FAChallenge: boolean;
}
