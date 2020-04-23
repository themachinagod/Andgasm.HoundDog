// #region Imports
import { Component, OnInit } from '@angular/core';
// #endregion

@Component({
  selector: 'account-settings',
  templateUrl: './account-settings.component.html',
  styleUrls: ['./account-settings.component.scss']
})
export class AccountSettingsComponent implements OnInit {

  // #region Fields
  selectedTabId: string
  // #endregion

  // #region Constructor
  constructor() { }
  // #endregion

  // #region Init/Destroy
  ngOnInit(): void {

    this.selectedTabId = 'profile';
  }
  // #endregion
}
