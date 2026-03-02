import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { User, LoginRequest, RegisterRequest } from '../models/user.model';

// שירות אימות משתמשים - התחברות, הרשמה ומעקב סטטוס
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5013/api'; // כתובת ה-API
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);

  // Observable לרכיבי האפליקציה
  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(private http: HttpClient) {
    // בדיקה אם יש משתמש מחובר מ-localStorage בעת טעינת השירות
    this.checkStoredAuth();
  }

  // בדיקת אימות שמור מ-localStorage
  private checkStoredAuth(): void {
    const token = localStorage.getItem('authToken');
    const userStr = localStorage.getItem('currentUser');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
        this.isLoggedInSubject.next(true);
      } catch (error) {
        console.error('Error parsing stored user data:', error);
        this.logout();
      }
    }
  }

  // התחברות משתמש - שולח ל-POST /api/Users/login ומקבל UserDTO
  login(loginData: LoginRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/Users/login`, loginData)
      .pipe(
        tap(user => {
          // שמירת המשתמש ב-localStorage
          localStorage.setItem('currentUser', JSON.stringify(user));
          localStorage.setItem('authToken', 'token_' + user.userId); // שמירת token פשוט
          this.currentUserSubject.next(user);
          this.isLoggedInSubject.next(true);
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  // הרשמת משתמש חדש - שולח ל-POST /api/Users ומקבל UserDTO
  register(registerData: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/Users`, registerData)
      .pipe(
        tap(user => {
          // שמירת המשתמש ב-localStorage לאחר הרשמה
          localStorage.setItem('currentUser', JSON.stringify(user));
          localStorage.setItem('authToken', 'token_' + user.userId);
          this.currentUserSubject.next(user);
          this.isLoggedInSubject.next(true);
        }),
        catchError(error => {
          console.error('Registration error:', error);
          return throwError(() => error);
        })
      );
  }

  // עדכון פרטי משתמש - שולח PUT /api/Users/{id}
  updateUser(id: number, userData: Partial<User> & { password: string }): Observable<User> {
    console.log('updateUser called with id:', id, 'userData:', userData);
    return this.http.put<User>(`${this.apiUrl}/Users/${id}`, userData)
      .pipe(
        tap((updatedUser) => {
          console.log('updateUser response:', updatedUser);
          // עדכון localStorage אחרי הצלחה
          localStorage.setItem('currentUser', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
        }),
        catchError(error => {
          console.error('Update error:', error);
          return throwError(() => error);
        })
      );
  }

  // קבלת כל המשתמשים
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/Users`)
      .pipe(
        catchError(error => {
          console.error('Get users error:', error);
          return throwError(() => error);
        })
      );
  }

  // עדכון תפקיד משתמש ל-admin
  promoteToAdmin(userId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/Users/${userId}/promote`, {})
      .pipe(
        catchError(error => {
          console.error('Promote error:', error);
          return throwError(() => error);
        })
      );
  }

  // התנתקות משתמש
  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('authToken');
    this.currentUserSubject.next(null);
    this.isLoggedInSubject.next(false);
  }

  // בדיקה אם המשתמש הוא מנהל
  isAdmin(): boolean {
    return this.currentUserSubject.value?.isAdmin === true;
  }

  // קבלת משתמש נוכחי
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  // בדיקה אם המשתמש מחובר
  isAuthenticated(): boolean {
    return this.isLoggedInSubject.value;
  }

}
