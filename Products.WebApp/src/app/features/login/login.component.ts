import { AfterViewInit, Component, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import { Client, LoginCommand } from '../../api/client';
import { AuthService } from '../../auth/auth.service';

type LoginForm = {
  username: FormControl<string>;
  password: FormControl<string>;
};

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent implements AfterViewInit {
  @ViewChild('usernameInput') private readonly usernameInput?: ElementRef<HTMLInputElement>;

  private readonly api = inject(Client);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  showPassword = false;
  submitting = false;
  error?: string;

  readonly form = new FormGroup<LoginForm>({
    username: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  ngAfterViewInit(): void {
    if (this.auth.isLoggedIn()) {
      this.router.navigateByUrl('/products');
      return;
    }
    queueMicrotask(() => this.usernameInput?.nativeElement?.focus());
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  submit(): void {
    if (this.submitting) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.submitting = true;
    this.error = undefined;
    const cmd = new LoginCommand({
      username: this.form.controls.username.value,
      password: this.form.controls.password.value,
    });
    this.api
      .login(cmd)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.auth.setLoggedIn(true);
          this.router.navigateByUrl('/products');
        },
        error: (e: any) => {
          const msg = String(e?.message ?? e?.error?.message ?? 'Login failed').trim();
          this.error = msg || 'Login failed';
        },
      });
  }
}
