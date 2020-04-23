import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { UserProfileViewModel } from '../shared/models/user.profile.viewmodel.interface';
import { AuthenticationService } from '../shared/services/authentication.service';
import { UserService } from '../shared/services/user.service';

import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  errors: [];
  isRequesting: boolean;
  currentUser: UserProfileViewModel = {} as UserProfileViewModel;

  get UserToken() {
    return this.authenticationService.currentUserValue.token;
  }

  get UserRoles() {
    return this.authenticationService.currentUserValue.roles;
  }

  constructor(
    private userService: UserService,
    private authenticationService: AuthenticationService,
    private router: Router) {

    this.errors = [];
  }

  ngOnInit() {
    
    this.initialiseCurrentUser();
  }

  // #region User Retrieval
  initialiseCurrentUser() {
    this.isRequesting = true;
    this.userService.getCurrentUser()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(data => {
        this.currentUser = data;
      },
      errors => { this.errors = errors; });
  }
  // #endregion
}
