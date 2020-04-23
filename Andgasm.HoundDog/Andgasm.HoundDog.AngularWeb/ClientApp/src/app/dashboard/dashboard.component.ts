import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { Router } from '@angular/router';

import { User } from '../shared/models/user.interface';
import { AuthenticationService } from '../shared/services/authentication.service';
import { UserService } from '../shared/services/user.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentUser: User;
  userFromApi: User;

  constructor(
    private userService: UserService,
    private authenticationService: AuthenticationService,
    private router: Router
  ) {
    debugger;
    this.currentUser = this.authenticationService.currentUserValue;
  }

  ngOnInit() {
    //this.userService.getById(this.currentUser.id).pipe(first()).subscribe(user => {
    //  this.userFromApi = user;
    //});
  }
}
