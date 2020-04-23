import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent implements OnInit {
  
  isExpanded = false;

  constructor() { }

  ngOnInit() {}

  collapse() {

    this.isExpanded = false;
  }

  toggle() {

    this.isExpanded = !this.isExpanded;
  }
}
