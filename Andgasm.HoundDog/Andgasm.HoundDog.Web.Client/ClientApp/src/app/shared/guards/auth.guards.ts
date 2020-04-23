import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AuthenticationService } from '../../shared/services/authentication.service';
import { TokenService } from '../../shared/services/token.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private authenticationService: AuthenticationService,
    private tokenService: TokenService
  ) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {

    const currentUser = this.authenticationService.currentUserValue;
    if (currentUser) {
      if (!this.tokenService.isValidToken(currentUser.token)) return false;
      if (route.data.roles && route.data.roles.indexOf(currentUser.roles) === -1) {
        // TODO: review roles part of this - needs more work!
        this.router.navigate(['/']);
        return false;
      }
      return true; 
    }

    // not logged in so redirect to login page with the return url
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
