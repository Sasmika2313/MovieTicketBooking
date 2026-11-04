import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  FormsModule,
  NgForm
} from '@angular/forms';

import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';

  errorMessage = '';
  loading = false;

  onLogin(form: NgForm): void {

    if (form.invalid) return;

    this.loading = true;
    this.errorMessage = '';

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({

      next: (response) => {

        this.authService.saveLogin(response);

        this.loading = false;

        this.router.navigate(['/dashboard']);
      },

      error: (error) => {

        this.loading = false;

        this.errorMessage =
          error.error?.message ||
          'Login failed. Please check your credentials.';

      }

    });
  }
}