import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { User } from '../../models/user.model';

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
    private router: Router
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
        password: this.profileForm.value.password,
        isAdmin: this.currentUser.isAdmin
      };

      this.authService.updateUser(this.currentUser.userId, updatedUser).subscribe({
        next: () => {
          this.currentUser = updatedUser;
          this.successMessage = 'הפרטים עודכנו בהצלחה!';
          this.isLoading = false;
        },
        error: (error) => {
          if (error.status === 400) {
            this.errorMessage = 'הסיסמה חלשה מדי - נסה סיסמה חזקה יותר.';
          } else {
            this.errorMessage = 'שגיאה בעדכון הפרטים. נסה שוב.';
          }
          this.isLoading = false;
        }
      });
    }
  }

  goBack() {
    this.router.navigate(['/design']);
  }
}
