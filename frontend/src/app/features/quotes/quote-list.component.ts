import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Quote } from '../../core/models/quote.models';
import { QuoteService } from '../../core/services/quote.service';

@Component({
  selector: 'app-quote-list',
  imports: [RouterLink],
  templateUrl: './quote-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteListComponent implements OnInit {
  private readonly quoteService = inject(QuoteService);

  readonly quotes = signal<Quote[]>([]);
  readonly loading = signal(true);
  readonly deletingId = signal<number | null>(null);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.quoteService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (quotes) => this.quotes.set(quotes),
        error: () => this.errorMessage.set('Citaten kunde inte hämtas. Försök igen.'),
      });
  }

  deleteQuote(quote: Quote): void {
    if (!window.confirm(`Vill du radera citatet av ${quote.author}?`)) {
      return;
    }

    this.deletingId.set(quote.id);
    this.errorMessage.set(null);
    this.quoteService
      .delete(quote.id)
      .pipe(finalize(() => this.deletingId.set(null)))
      .subscribe({
        next: () => this.quotes.update((items) => items.filter((item) => item.id !== quote.id)),
        error: () => this.errorMessage.set('Citatet kunde inte raderas.'),
      });
  }
}
