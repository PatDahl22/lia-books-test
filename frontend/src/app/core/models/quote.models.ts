export interface Quote {
  id: number;
  text: string;
  author: string;
  source: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface SaveQuoteRequest {
  text: string;
  author: string;
  source: string | null;
}
