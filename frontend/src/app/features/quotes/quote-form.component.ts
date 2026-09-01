import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { SaveQuoteRequest } from '../../core/models/quote.models';
import { QuoteService } from '../../core/services/quote.service';

@Component({
  selector: 'app-quote-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './quote-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteFormComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly quoteService = inject(QuoteService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly quoteId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly form = this.formBuilder.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(4000)]],
    author: ['', [Validators.required, Validators.maxLength(150)]],
    source: ['', [Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    const idValue = this.route.snapshot.paramMap.get('id');
    if (!idValue) {
      return;
    }

    const id = Number(idValue);
    if (!Number.isInteger(id) || id <= 0) {
      void this.router.navigate(['/quotes']);
      return;
    }

    this.quoteId.set(id);
    this.loading.set(true);
    this.quoteService
      .getById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (quote) =>
          this.form.patchValue({ text: quote.text, author: quote.author, source: quote.source ?? '' }),
        error: () => this.errorMessage.set('Citatet kunde inte hämtas.'),
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    const request: SaveQuoteRequest = {
      text: values.text.trim(),
      author: values.author.trim(),
      source: values.source.trim() || null,
    };
    const id = this.quoteId();
    const saveRequest = id
      ? this.quoteService.update(id, request)
      : this.quoteService.create(request);

    this.submitting.set(true);
    this.errorMessage.set(null);
    saveRequest.pipe(finalize(() => this.submitting.set(false))).subscribe({
      next: () => void this.router.navigate(['/quotes']),
      error: () => this.errorMessage.set('Citatet kunde inte sparas. Kontrollera fälten och försök igen.'),
    });
  }
}
