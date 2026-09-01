import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Book, SaveBookRequest } from '../models/book.models';

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/books';

  getAll() {
    return this.http.get<Book[]>(this.baseUrl);
  }

  getById(id: number) {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(request: SaveBookRequest) {
    return this.http.post<Book>(this.baseUrl, request);
  }

  update(id: number, request: SaveBookRequest) {
    return this.http.put<Book>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
