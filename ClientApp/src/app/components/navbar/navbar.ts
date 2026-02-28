import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

// ניווט ראשי של האפליקציה - תפריט עליון לניווט בין דפים
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss']
})
export class NavbarComponent implements OnInit {
  isAuthPage = false;

  constructor(private router: Router) {}

  ngOnInit() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      // Only show navbar on auth page
      this.isAuthPage = event.urlAfterRedirects.includes('/auth');
    });
  }

  // תצוגת שם האפליקציה
  appName = 'Room Design Studio';
  
  // רשימת קישורי ניווט לדפים השונים
  navLinks = [
    { path: '/design', label: 'עיצוב חדרים', icon: '🏠' },
    { path: '/admin', label: 'אזור מנהל', icon: '⚙️' },
    { path: '/auth', label: 'התחברות', icon: '👤' }
  ];
}
