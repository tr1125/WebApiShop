import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { User } from '../../models/user.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  currentUser: User | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.formBuilder.group({
      userName: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      address: ['', Validators.required],
      phone: ['', Validators.required],
      password: ['']
    });
  }

  ngOnInit() {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.router.navigate(['/auth']);
      return;
    }
    this.profileForm.patchValue({
      userName: this.currentUser.userName?.trim(),
      firstName: this.currentUser.firstName?.trim(),
      lastName: this.currentUser.lastName?.trim(),
      address: (this.currentUser.address || '').trim(),
      phone: (this.currentUser.phone || '').trim(),
      password: ''
    });
  }

  onUpdate() {
    if (this.profileForm.valid && this.currentUser) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const updatedUser = {
        userId: this.currentUser.userId,
        userName: this.profileForm.value.userName,
        firstName: this.profileForm.value.firstName,
        lastName: this.profileForm.value.lastName,
        address: this.profileForm.value.address,
        phone: this.profileForm.value.phone,
        password: this.profileForm.value.password || this.currentUser.password,
        isAdmin: this.currentUser.isAdmin
      };

      this.authService.updateUser(this.currentUser.userId, updatedUser)
        .pipe(
          finalize(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          })
        )
        .subscribe({
          next: (response) => {
            this.currentUser = response;
            this.successMessage = 'הפרטים עודכנו בהצלחה!';
            this.cdr.markForCheck();
            setTimeout(() => {
              this.successMessage = '';
              this.cdr.markForCheck();
            }, 3000);
          },
          error: (error) => {
            if (error.status === 400) {
              this.errorMessage = 'הסיסמה חלשה מדי - נסה סיסמה חזקה יותר.';
            } else {
              this.errorMessage = 'שגיאה בעדכון הפרטים. נסה שוב.';
            }
            this.cdr.markForCheck();
          }
        });
    }
  }

  goBack() {
    this.router.navigate(['/design']);
  }
}
