// #region Imports
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
// #endregion

@Component({
  selector: 'mail-confirmation',
  templateUrl: './mail-confirmation.component.html',
  styleUrls: ['./mail-confirmation.component.css']
})
export class MailAddressConfirmationComponent implements OnInit {

  // #region Form State Fields
  success: boolean = false;

  get confirmationResultTitle(): string {
    if (this.success) {
      return "Your email address has been successfully confirmed!";
    }
    else {
      return "Unfortunately we could not confirm your email address at the moment!";
    }
  }

  get confirmationResultBody(): string {
    if (this.success) {
      return "Thank you for taking the time to confirm your email address, your account is now fully activated and all site functionality is unlocked for your use!";
    }
    else {
      return "Please try to follow the confirmation link that we sent to your mailbox, if this still fails please contact application support!";
    }
  }
  // #endregion

  // #region Constructors
  constructor(private _activatedRoute: ActivatedRoute) { }
  // #endregion

  // #region Initialise
  ngOnInit() {
    this._activatedRoute.queryParams.subscribe(queryParams => {
      this.success = queryParams['success'] == "True";
    });
  }
  // #endregion
}
