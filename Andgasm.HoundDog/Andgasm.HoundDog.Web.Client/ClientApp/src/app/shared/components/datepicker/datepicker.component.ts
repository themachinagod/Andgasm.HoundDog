// #region Imports
import { Component, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SimpleDateViewModel, SelectListItemViewModel } from '../../models/user.profile.viewmodel.interface';
import { NgbTooltip } from '@ng-bootstrap/ng-bootstrap';
// #endregion

@Component({
  selector: 'app-date-picker',
  templateUrl: './datepicker.component.html',
  styleUrls: ['./datepicker.component.css'],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: DatePickerComponent,
    multi: true
  }, { provide: NgbTooltip }] 
})
export class DatePickerComponent implements ControlValueAccessor {

  // #region Input Properties
  @Input() isDataEntryMode: boolean = true;

  @Input() dayLabel: string = "Day";
  @Input() monthLabel: string = "Month";
  @Input() yearLabel: string = "Year";
  @Input() viewOnlyLabel: string = "Date";

  @Input() dayTooltip: string = "The day of the date";
  @Input() monthTooltip: string = "The month of the date";
  @Input() yearTooltip: string = "The year of the date";
  @Input() viewOnlyTooltip: string = "Date";

  @Input() showTooltips: boolean = true;
  @Input() showsubLabels: boolean = true;
  @Input() showViewOnlyLabel: boolean = true;

  @Input() viewModeDateFormat: string = "dd MMM yyyy";
  // #endregion

  // #region Form State Fields
  value: SimpleDateViewModel;
  months: SelectListItemViewModel[] = [{ text: "January", id: 1 }, { text: "February", id: 2 }, { text: "March", id: 3 }, { text: "April", id: 4 }, { text: "May", id: 5 }, { text: "June", id: 6 },
    { text: "July", id: 7 }, { text: "August", id: 8 }, { text: "September", id: 9 }, { text: "October", id: 10 }, { text: "November", id: 11 }, { text: "December", id: 12 }];

  private get fomattedDate(): string {
    // TODO: we need to account for the specified format here
    if (this.value) {
      var result = this.value.day + ' ' + this.months[this.value.month - 1].text + ' ' + this.value.year;
      return result;
    }
    return '';
  }
  // #endregion

  // #region Updates
  updateYear(val) {
    var newVal: SimpleDateViewModel = { day: Number(this.value.day), month: Number(this.value.month), year: Number(val) };
    this.propagateChange(newVal);
  }

  updateMonth(val) {
    var newVal: SimpleDateViewModel = { day: Number(this.value.day), month: Number(val), year: Number(this.value.year) };
    this.propagateChange(newVal);
  }

  updateDay(val) {
    var newVal: SimpleDateViewModel = { day: Number(val), month: Number(this.value.month), year: Number(this.value.year) };
    this.propagateChange(newVal);
  }
  //#endregion

  // #region List Options
  getMonth(index: number) {

    return this.months[index - 1].text;
  }

  getYearListOptions() {

    var stop = new Date().getFullYear();
    var start = stop - 100;
    var step = 1;
    return Array.from({ length: (stop - start) / step + 1 }, (_, step) => start + (step * 1));
  }

  getDayListOptions() {

    var stop = new Date(this.value.year, this.value.month, 0).getDate();
    var start = 1
    var step = 1;
    return Array.from({ length: (stop - start) / step + 1 }, (_, step) => start + (step * 1));
  }
  // #endregion

  // #region Control Value Accessor Impl
  propagateChange = (_: any) => { };

  writeValue(obj: any): void {

    this.value = obj;
  }

  registerOnChange(fn: any): void {

    this.propagateChange = fn;
  }

  registerOnTouched(fn: any): void { }
  // #endregion
}
