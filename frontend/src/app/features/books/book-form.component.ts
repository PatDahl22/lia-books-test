import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { SaveBookRequest } from '../../core/models/book.models';
import { BookService } from '../../core/services/book.service';

@Component({
  selector: 'app-book-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './book-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookFormComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly bookService = inject(BookService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly bookId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly form = this.formBuilder.group({
    title: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    author: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(150)]),
    publishedYear: this.formBuilder.control<number | null>(null, [Validators.min(1), Validators.max(9999)]),
    description: this.formBuilder.nonNullable.control('', [Validators.maxLength(2000)]),
  });

  ngOnInit(): void {
    const idValue = this.route.snapshot.paramMap.get('id');
    if (!idValue) {
      return;
    }

    const id = Number(idValue);
    if (!Number.isInteger(id) || id <= 0) {
      void this.router.navigate(['/books']);
      return;
    }

    this.bookId.set(id);
    this.loading.set(true);
    this.bookService
      .getById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (book) =>
          this.form.patchValue({
            title: book.title,
            author: book.author,
            publishedYear: book.publishedYear,
            description: book.description ?? '',
          }),
        error: () => this.errorMessage.set('Boken kunde inte hämtas.'),
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    const request: SaveBookRequest = {
      title: values.title.trim(),
      author: values.author.trim(),
      publishedYear: values.publishedYear,
      description: values.description.trim() || null,
    };
    const id = this.bookId();
    const saveRequest = id
      ? this.bookService.update(id, request)
      : this.bookService.create(request);

    this.submitting.set(true);
    this.errorMessage.set(null);
    saveRequest.pipe(finalize(() => this.submitting.set(false))).subscribe({
      next: () => void this.router.navigate(['/books']),
      error: (_error: HttpErrorResponse) => this.errorMessage.set('Boken kunde inte sparas. Kontrollera fälten och försök igen.'),
    });
  }
}
