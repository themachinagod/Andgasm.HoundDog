import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { myFocus } from '../../shared/directives/focus.directive';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { DatePickerComponent } from '../../shared/components/datepicker/datepicker.component';
import { NgbDatepicker, NgbDatepickerModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule } from '@angular/forms';

@NgModule({
  imports: [CommonModule, NgbDatepickerModule, NgbTooltipModule, FormsModule],
  declarations: [myFocus, SpinnerComponent, DatePickerComponent],
  exports: [myFocus, SpinnerComponent,  DatePickerComponent],
  providers: []
})
export class SharedModule { }
