import { User } from "./user";

export class UserParams {
    pageNumber = 1;
    pageSize = 5;
    gender: string;
    minAge = 18;
    maxAge = 99;
    orderBy = 'lastActive';
    orderIsDescending = true;

    constructor(user: User) {
        this.gender = user.gender === 'male' ? 'female' : 'male';
    }
}