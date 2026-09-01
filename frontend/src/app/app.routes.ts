import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login.component').then((component) => component.LoginComponent),
    title: 'Logga in | LIA Books',
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register.component').then((component) => component.RegisterComponent),
    title: 'Skapa konto | LIA Books',
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/books/book-list.component').then((component) => component.BookListComponent),
    title: 'Böcker | LIA Books',
  },
  {
    path: 'books/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/books/book-form.component').then((component) => component.BookFormComponent),
    title: 'Lägg till bok | LIA Books',
  },
  {
    path: 'books/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/books/book-form.component').then((component) => component.BookFormComponent),
    title: 'Redigera bok | LIA Books',
  },
  {
    path: 'quotes',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/quotes/quote-list.component').then((component) => component.QuoteListComponent),
    title: 'Mina citat | LIA Books',
  },
  {
    path: 'quotes/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/quotes/quote-form.component').then((component) => component.QuoteFormComponent),
    title: 'Lägg till citat | LIA Books',
  },
  {
    path: 'quotes/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/quotes/quote-form.component').then((component) => component.QuoteFormComponent),
    title: 'Redigera citat | LIA Books',
  },
  { path: '', pathMatch: 'full', redirectTo: 'books' },
  { path: '**', redirectTo: 'books' },
];
