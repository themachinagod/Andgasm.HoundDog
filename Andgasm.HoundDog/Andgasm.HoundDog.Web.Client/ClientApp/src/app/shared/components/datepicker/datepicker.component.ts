import { Component, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbDateParserFormatter } from '@ng-bootstrap/ng-bootstrap';
import { NgbDateCustomParserFormatter } from './NgbDateCustomParserFormatter';

@Component({
  selector: 'app-date-picker',
  templateUrl: './datepicker.component.html',
  styleUrls: ['./datepicker.component.css'],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: DatePickerComponent,
    multi: true
  },
  { provide: NgbDateParserFormatter, useClass: NgbDateCustomParserFormatter }] 
})
export class DatePickerComponent implements ControlValueAccessor {

  @Input() isDataEntryMode: boolean = true;
  value;

  propagateChange = (_: any) => { };

  writeValue(obj: any): void {
    debugger;
    this.value = obj;
  }

  registerOnChange(fn: any): void {
    debugger;
    this.propagateChange = fn;
  }

  registerOnTouched(fn: any): void { }

  update($event) {
    this.propagateChange($event);
  }
}
