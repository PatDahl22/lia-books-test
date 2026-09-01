import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Book } from '../../core/models/book.models';
import { BookService } from '../../core/services/book.service';

@Component({
  selector: 'app-book-list',
  imports: [RouterLink],
  templateUrl: './book-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookListComponent implements OnInit {
  private readonly bookService = inject(BookService);

  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly deletingId = signal<number | null>(null);
  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadBooks();
  }

  deleteBook(book: Book): void {
    const confirmed = window.confirm(`Vill du radera “${book.title}”?`);
    if (!confirmed) {
      return;
    }

    this.deletingId.set(book.id);
    this.errorMessage.set(null);
    this.bookService
      .delete(book.id)
      .pipe(finalize(() => this.deletingId.set(null)))
      .subscribe({
        next: () => this.books.update((items) => items.filter((item) => item.id !== book.id)),
        error: () => this.errorMessage.set('Boken kunde inte raderas. Försök igen.'),
      });
  }

  private loadBooks(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.bookService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (books) => this.books.set(books),
        error: (_error: HttpErrorResponse) =>
          this.errorMessage.set('Böckerna kunde inte hämtas. Kontrollera anslutningen och försök igen.'),
      });
  }
}
