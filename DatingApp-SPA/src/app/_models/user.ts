import { Photo } from "./photo";

export interface User {
  /* for Dto  UserForlistDto   */
  id: number;
  userName: string;
  knownAs: string;
  age: number;
  gender: number;
  created: Date;
  lastActive: any;
  photoUrl: string;
  city: string;
  country: string;

  /* for Dto  UserForDetailedDto   */
  interests?: string;
  introduction?: string;
  lookingFor?: string;
  photos?: Photo[];
  roles?: string[];
}
