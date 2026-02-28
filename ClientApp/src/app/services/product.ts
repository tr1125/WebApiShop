import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError, BehaviorSubject, tap } from 'rxjs';
import { Product, Category, ProductFilter } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'http://localhost:5013/api';

  private productsSubject = new BehaviorSubject<Product[]>([]);
  private categoriesSubject = new BehaviorSubject<Category[]>([]);

  public products$ = this.productsSubject.asObservable();
  public categories$ = this.categoriesSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadProducts();
    this.loadCategories();
  }

  getProducts(filter?: ProductFilter): Observable<Product[]> {
    let params = new HttpParams();
    if (filter) {
      if (filter.categoryIds?.length) {
        filter.categoryIds.forEach(id => params = params.append('categoryIds', id.toString()));
      }
      if (filter.minPrice !== undefined)  params = params.set('minPrice', filter.minPrice.toString());
      if (filter.maxPrice !== undefined)  params = params.set('maxPrice', filter.maxPrice.toString());
      if (filter.name)                   params = params.set('name', filter.name);
      if (filter.color)                  params = params.set('color', filter.color);
      if (filter.desc)                   params = params.set('desc', filter.desc);
      if (filter.position)               params = params.set('position', filter.position.toString());
      if (filter.limit)                  params = params.set('skip', filter.limit.toString());
    }
    return this.http.get<Product[]>(`${this.apiUrl}/Products`, { params })
      .pipe(catchError(err => { console.error('getProducts error:', err); return throwError(() => err); }));
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/Products/${id}`)
      .pipe(catchError(err => { console.error('getProductById error:', err); return throwError(() => err); }));
  }

  createProduct(product: Omit<Product, 'productId'>): Observable<Product> {
    return this.http.post<Product>(`${this.apiUrl}/Products`, product)
      .pipe(catchError(err => { console.error('createProduct error:', err); return throwError(() => err); }));
  }

  updateProduct(id: number, product: Partial<Product>): Observable<Product> {
    const { productId, ...productData } = product;
    return this.http.put<Product>(`${this.apiUrl}/Products/${id}`, productData)
      .pipe(
        tap(response => console.log('updateProduct success:', response)),
        catchError(err => {
          console.error('updateProduct error:', err);
          console.error('Error status:', err.status);
          console.error('Error message:', err.error);
          return throwError(() => err);
        })
      );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Products/${id}`)
      .pipe(catchError(err => { console.error('deleteProduct error:', err); return throwError(() => err); }));
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.apiUrl}/Categories`)
      .pipe(catchError(err => { console.error('getCategories error:', err); return throwError(() => err); }));
  }

  private loadProducts(): void {
    this.getProducts().subscribe({
      next: p => this.productsSubject.next(p),
      error: err => console.error('loadProducts error:', err)
    });
  }

  private loadCategories(): void {
    this.getCategories().subscribe({
      next: c => this.categoriesSubject.next(c),
      error: err => console.error('loadCategories error:', err)
    });
  }

  refreshProducts(filter?: ProductFilter): void {
    this.getProducts(filter).subscribe({
      next: p => this.productsSubject.next(p),
      error: err => console.error('refreshProducts error:', err)
    });
  }

  getImageUrl(relativePath: string): string {
    if (!relativePath) return '';
    return `http://localhost:4200/${relativePath}`;
  }

}
