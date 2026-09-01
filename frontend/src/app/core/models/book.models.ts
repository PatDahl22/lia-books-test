export interface Book {
  id: number;
  title: string;
  author: string;
  description: string | null;
  publishedYear: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface SaveBookRequest {
  title: string;
  author: string;
  description: string | null;
  publishedYear: number | null;
}
