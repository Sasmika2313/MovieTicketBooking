import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  FormsModule,
  NgForm
} from '@angular/forms';

import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  private authService = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  password = '';

  errorMessage = '';
  successMessage = '';
  loading = false;

  onRegister(form: NgForm): void {

    if (form.invalid) return;

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register({
      fullName: this.fullName,
      email: this.email,
      password: this.password
    }).subscribe({

      next: () => {

        this.loading = false;

        this.successMessage =
          'Registration successful! Please login.';

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1200);

      },

      error: (error) => {

        this.loading = false;

        this.errorMessage =
          error.error?.message ||
          'Registration failed. Please try again.';

      }

    });
  }
}