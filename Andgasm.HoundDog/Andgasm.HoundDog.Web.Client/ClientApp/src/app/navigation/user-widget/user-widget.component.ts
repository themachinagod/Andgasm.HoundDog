import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { UserAvatarService } from '../../shared/services/user.avatar.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'user-widget',
  templateUrl: './user-widget.component.html',
  styleUrls: ['./user-widget.component.css']
})
export class UserWidgetComponent implements OnInit {

  authenticatedUserName: string;
  avatarImage: any;

  avatarChangeSubscription: Subscription;
  userAuthenticatedSubscription: Subscription;

  constructor(
    private authenticationService: AuthenticationService,
    private toastrService: ToastrService,
    private router: Router,
    private avatarService: UserAvatarService) { }

  ngOnInit() {

    this.subscribeToAvatarImageChanges();
    this.subscribeToUserAuthenticated();

    if (this.authenticationService.isLoggedIn) {
      this.authenticatedUserName = this.authenticationService.currentUserValue.userName;
      this.getProfileImage();
    }
  }

  register() {

    this.router.navigate(["/register"]);
  }

  login() {

    this.router.navigate(["/login"]);
  }

  logout() {

    this.authenticationService.logout();
    this.toastrService.success('You have successfully logged out and your security tokens have been flushed, hope to see you again soon!', 'Logout Complete!');
    this.login();
  }

  getProfileImage() {

    this.avatarService.getAvatar().subscribe(result => {
      this.avatarImage = this.avatarService.constructImageUrlFromBlob(result)
    });
  }

  subscribeToAvatarImageChanges() {

    this.avatarChangeSubscription = this.avatarService.$userProfileAvatarData.subscribe(avatarblob => {
      this.avatarImage = this.avatarService.constructImageUrlFromBlob(avatarblob);
    });
  }

  subscribeToUserAuthenticated() {

    this.userAuthenticatedSubscription = this.authenticationService.currentUserSubject.subscribe(user => {
      if (user) {
        this.getProfileImage();
        this.authenticatedUserName = user.userName;
      }
    });
  }

  isAuthenticated() {

    return this.authenticationService.isLoggedIn;
  }
}
