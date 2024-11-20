import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { Router } from '@angular/router';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { UserRole } from '../../../core/model/user-role';
import { RouterTestingModule } from '@angular/router/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let mockUserService: jasmine.SpyObj<UserService>;

  beforeEach(async () => {
    mockUserService = jasmine.createSpyObj('UserService', ['register']);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, RegisterComponent, RouterTestingModule, NoopAnimationsModule],
      providers: [
      { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // it('should have a form with 4 controls', () => {
  //   expect(component.registerForm.contains('username')).toBeTruthy();
  //   expect(component.registerForm.contains('email')).toBeTruthy();
  //   expect(component.registerForm.contains('password')).toBeTruthy();
  //   expect(component.registerForm.contains('role')).toBeTruthy();
  // });

  // it('should make the username control required', () => {
  //   let control = component.registerForm.get('username');
  //   control?.setValue('');
  //   expect(control?.valid).toBeFalsy();
  // });

  // it('should make the email control required and validate email format', () => {
  //   let control = component.registerForm.get('email');
  //   control?.setValue('');
  //   expect(control?.valid).toBeFalsy();

  //   control?.setValue('not-an-email');
  //   expect(control?.valid).toBeFalsy();

  //   control?.setValue('test@example.com');
  //   expect(control?.valid).toBeTruthy();
  // });

  // it('should make the password control required', () => {
  //   let control = component.registerForm.get('password');
  //   control?.setValue('');
  //   expect(control?.valid).toBeFalsy();
  // });

  // it('should call the register method of UserService on form submit', () => {
  //   component.registerForm.setValue({
  //     username: 'testuser',
  //     email: 'test@example.com',
  //     password: 'password',
  //     role: UserRole.USER
  //   });

  //   mockUserService.register.and.returnValue(of({}));
  //   component.onSubmit();

  //   expect(mockUserService.register).toHaveBeenCalledWith({
  //     username: 'testuser',
  //     email: 'test@example.com',
  //     password: 'password',
  //     role: UserRole.USER
  //   });
  //   expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  // });
});