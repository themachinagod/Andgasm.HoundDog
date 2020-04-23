import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VerifyphoneComponent } from './verifyphone.component';

describe('VerifyphoneComponent', () => {
  let component: VerifyphoneComponent;
  let fixture: ComponentFixture<VerifyphoneComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VerifyphoneComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VerifyphoneComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
