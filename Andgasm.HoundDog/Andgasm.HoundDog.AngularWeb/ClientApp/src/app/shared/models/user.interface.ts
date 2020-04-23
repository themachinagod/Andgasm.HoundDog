import { Role } from "./role.interface";

export class User {
  id: number;
  userName: string;
  password: string; // dont really want this in here as i dont want the password maintained on the client beyond login!!
  firstName: string;
  lastName: string;
  location: string
  role: Role[];
  token?: string;
}
