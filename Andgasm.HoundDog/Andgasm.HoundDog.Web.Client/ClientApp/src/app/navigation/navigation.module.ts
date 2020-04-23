import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { UserWidgetComponent } from './user-widget/user-widget.component';

import { routing } from './navigation.routing';

@NgModule({
  imports: [CommonModule, routing],
  exports: [NavMenuComponent, UserWidgetComponent],
  declarations: [NavMenuComponent, UserWidgetComponent]
})
export class NavigationModule { }
