import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VerifyauthenticatorComponent } from './verifyauthenticator.component';

describe('VerifyauthenticatorComponent', () => {
  let component: VerifyauthenticatorComponent;
  let fixture: ComponentFixture<VerifyauthenticatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VerifyauthenticatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VerifyauthenticatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
