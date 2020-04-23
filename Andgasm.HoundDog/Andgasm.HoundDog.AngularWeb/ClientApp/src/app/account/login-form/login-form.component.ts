import { Subscription } from 'rxjs/Subscription';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Credentials } from '../../shared/models/credentials.interface';

import { AuthenticationService } from '../../shared/services/authentication.service';

import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-login-form',
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.scss']
})
export class LoginFormComponent implements OnInit, OnDestroy {

  private subscription: Subscription;

  credentials: Credentials = { username: '', password: '' };
  brandNew: boolean;
  errors: [];
  isRequesting: boolean;
  submitted: boolean = false;
  returnUrl: string;

  constructor(private authenticationService: AuthenticationService, private router: Router, private activatedRoute: ActivatedRoute) {

    this.errors = [];
    if (this.authenticationService.currentUserValue) {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnInit() {

    this.subscription = this.activatedRoute.queryParams.subscribe(
      (param: any) => {
        this.brandNew = param['brandNew'];
        this.credentials.username = param['email'];
      });
    this.returnUrl = this.activatedRoute.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  ngOnDestroy() {

    //this.subscription.unsubscribe();
  }

  login({ value, valid }: { value: Credentials, valid: boolean }) {
    debugger;
    if (valid) {
      this.submitted = true;
      this.isRequesting = true;

      this.authenticationService.login(value.username, value.password)
        .pipe(finalize(() => this.isRequesting = false))
        .subscribe(
          data => {
            this.router.navigate([this.returnUrl]);
          },
          errors => {
            this.errors = errors;
          });
    }
  }
}
