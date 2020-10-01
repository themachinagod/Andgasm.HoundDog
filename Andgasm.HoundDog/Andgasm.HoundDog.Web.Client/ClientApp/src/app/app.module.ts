import { NgModule } from '@angular/core';
import { NgbModule, NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { QRCodeModule } from 'angularx-qrcode';
import { AccountModule } from './account/account.module';
import { NavigationModule } from './navigation/navigation.module';
import { RouterModule } from '@angular/router';
import { ToastrModule } from 'ngx-toastr';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { DashboardComponent } from './dashboard/dashboard.component';

import { ConfigService } from './shared/utils/config.service';
import { AuthenticationService } from './shared/services/authentication.service';

import { routing } from './app.routing';
import { ConfirmationComponent } from './shared/components/confirmation/confirmation.component';
import { ConfirmationService } from './shared/services/confirmation.service';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    DashboardComponent,
    ConfirmationComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    RouterModule,
    AccountModule,
    NavigationModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgbModule,
    BrowserAnimationsModule,
    QRCodeModule,
    ToastrModule,
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-full-width',
      preventDuplicates: true,

    }),
    routing
  ],
  providers: [ConfigService, AuthenticationService, ConfirmationService],
  bootstrap: [AppComponent]
})
export class AppModule { }
