import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Quote, SaveQuoteRequest } from '../models/quote.models';

@Injectable({ providedIn: 'root' })
export class QuoteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/quotes';

  getAll() {
    return this.http.get<Quote[]>(this.baseUrl);
  }

  getById(id: number) {
    return this.http.get<Quote>(`${this.baseUrl}/${id}`);
  }

  create(request: SaveQuoteRequest) {
    return this.http.post<Quote>(this.baseUrl, request);
  }

  update(id: number, request: SaveQuoteRequest) {
    return this.http.put<Quote>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
