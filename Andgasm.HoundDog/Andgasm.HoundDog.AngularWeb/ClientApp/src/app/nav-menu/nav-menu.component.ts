import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../shared/services/authentication.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent implements OnInit {
  public authenticatedUserName: string;

  isExpanded = false;

  constructor(
    private authenticationService: AuthenticationService,
    private router: Router
  ) { }

  ngOnInit() {
    if (this.authenticationService.isLoggedIn()) {
      this.authenticatedUserName = this.authenticationService.currentUserValue.userName;
    }
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  login() {
    this.router.navigate(["/login"]);
  }


  register() {
    this.router.navigate(["/register"]);
  }

  logout() {
    this.authenticationService.logout();
    this.login();
  }

  isAuthenticated() {
    return this.authenticationService.isLoggedIn();
  }
}
