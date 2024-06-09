import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'days-of-week',
  templateUrl: './days-of-week.component.html',
  styleUrls: ['./days-of-week.component.css']
})
export class DaysOfWeekComponent implements OnInit {
  @Input() readOnly: boolean = true;
  @Input() selectedDays: number[] = [];
  @Input() size: number = 12;
  @Output() selectedDaysChange: EventEmitter<number[]> = new EventEmitter<number[]>();

  daysOfWeek = [
    { id: 0, shortName: 'S' },
    { id: 1, shortName: 'M' },
    { id: 2, shortName: 'T' },
    { id: 3, shortName: 'W' },
    { id: 4, shortName: 'T' },
    { id: 5, shortName: 'F' },
    { id: 6, shortName: 'S' }
  ];

  constructor() { }

  ngOnInit(): void {
  }

  isSelected(dayId: number): boolean {
    return this.selectedDays.includes(dayId);
  }

  onDayClick(dayId: number) {
    const index = this.selectedDays.indexOf(dayId);
    if (index > -1) {
      this.selectedDays.splice(index, 1);
    } else {
      this.selectedDays.push(dayId);
    }
    this.selectedDaysChange.emit(this.selectedDays);
  }
}
