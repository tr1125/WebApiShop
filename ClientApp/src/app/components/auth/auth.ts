import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';

// קומפוננט אימות משתמשים - התחברות והרשמה
@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth.html',
  styleUrls: ['./auth.scss']
})
export class AuthComponent implements OnInit {
  // טפסי התחברות והרשמה
  loginForm: FormGroup;
  registerForm: FormGroup;
  
  // מצב הטפסים
  isLoginMode = true; // מצב ברירת מחדל - התחברות
  isLoading = false; // מצב טעינה
  errorMessage = ''; // הודעת שגיאה
  passwordStrength = 0; // חוזק סיסמה
  
  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // יצירת טופס התחברות
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(4)]]
    });
    
    // יצירת טופס הרשמה
    this.registerForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(4)]],
      address: ['', Validators.required],
      phone: ['', Validators.required]
    });
  }
  
  ngOnInit() {
    // בדיקה אם המשתמש כבר מחובר - בוטל לפי בקשת המשתמש (תמיד להציג דף התחברות)
    // if (this.authService.isAuthenticated()) {
    //   const dest = this.authService.isAdmin() ? '/admin' : '/design';
    //   this.router.navigate([dest]);
    // }
    
    // מעקב אחר חוזק הסיסמה
    this.registerForm.get('password')?.valueChanges.subscribe(password => {
      this.passwordStrength = this.calculatePasswordStrength(password);
    });
  }
  
  // כניסה ללא חשבון - מנווט ישירות לדף העיצוב (sessionStorage בלבד)
  guestLogin(): void {
    this.router.navigate(['/design']);
  }

  // מעבר בין מצבי התחברות/הרשמה
  switchAuthMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
  }
  
  // ביצוע התחברות - שולח userName + password ל-POST /api/Users/login
  onLogin() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      
      // UserLoginDTO מצפה ל-userName (האימייל) ו-password
      const loginData = {
        userName: this.loginForm.value.email,
        password: this.loginForm.value.password
      };
      
      this.authService.login(loginData).subscribe({
        next: (user) => {
          console.log('התחברות הצליחה:', user);
          const dest = user.isAdmin ? '/admin' : '/design';
          this.router.navigate([dest]);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('שגיאת התחברות:', error);
          if (error.status === 401) {
            this.errorMessage = 'שם משתמש או סיסמה לא נכונים';
          } else {
            this.errorMessage = 'שגיאה בהתחברות. נסה שוב.';
          }
          this.isLoading = false;
        }
      });
    }
  }
  
  // ביצוע הרשמה - שולח UserDTO ל-POST /api/Users
  onRegister() {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      
      // UserDTO: userId=0 (DB ייצור), userName=אימייל, שם פרטי+משפחה, סיסמה
      const registerData = {
        userId: 0,
        userName: this.registerForm.value.email,
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        password: this.registerForm.value.password,
        address: this.registerForm.value.address || '',
        phone: this.registerForm.value.phone || '',
        isAdmin: false
      };
      
      this.authService.register(registerData).subscribe({
        next: (user) => {
          console.log('הרשמה הצליחה:', user);
          this.router.navigate(['/design']);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('שגיאת הרשמה:', error);
          if (error.status === 400) {
            this.errorMessage = 'שגיאה בנתונים - בדוק שכל השדות תקינים';
          } else {
            this.errorMessage = 'שגיאה בהרשמה. נסה שוב.';
          }
          this.isLoading = false;
        }
      });
    }
  }
  
  // חישוב חוזק סיסמה כמו ב-wwwroot
  private calculatePasswordStrength(password: string): number {
    if (!password) return 0;
    
    let strength = 0;
    
    // קריטריונים לחוזק סיסמה
    if (password.length >= 6) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++; // תווים מיוחדים
    
    return Math.min(strength, 4);
  }

  // תווית חוזק סיסמה
  get passwordStrengthLabel(): string {
    if (this.passwordStrength <= 1) return 'חלשה';
    if (this.passwordStrength === 2) return 'בינונית';
    if (this.passwordStrength === 3) return 'טובה';
    return 'חזקה מאוד';
  }
}
