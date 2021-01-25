import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$: Observable<User> = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http
      .post(this.baseUrl + 'account/login', model)
      .pipe(
        map((response: User) => {
          const user = response;
          if (user) {
             this.setCurrentUser(user);
            this.currentUserSource.next(user);
          }
        })
      );
  }

  register(model: any) {
    return this.http
      .post(this.baseUrl + 'account/register', model)
      .pipe(
        map((user: User) => {
          if (user) {
            localStorage.setItem('user', JSON.stringify(user));
            this.currentUserSource.next(user);
          }
          return user;
        })
      );
  }

  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    // if there's only one role, it comes from the server as s atring, but we need an array
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
    console.log('roles', user.roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  getDecodedToken(token: any) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
