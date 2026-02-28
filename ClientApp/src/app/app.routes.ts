import { Routes } from '@angular/router';
import { AuthComponent } from './components/auth/auth';
import { CanvasComponent } from './components/canvas/canvas';
import { AdminComponent } from './components/admin/admin';
import { ProfileComponent } from './components/profile/profile';
import { PersonalAreaComponent } from './components/personal-area/personal-area';

export const routes: Routes = [
  { path: '', redirectTo: 'auth', pathMatch: 'full' },
  { path: 'auth', component: AuthComponent },
  { path: 'design', component: CanvasComponent },
  { path: 'admin', component: AdminComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'personal-area', component: PersonalAreaComponent },
  { path: '**', redirectTo: 'auth' }
];
