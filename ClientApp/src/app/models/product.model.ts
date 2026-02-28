export interface Product {
  productId: number;
  productName: string;
  price: number;
  categoryId: number;
  description?: string;
  imageURL?: string;
  color?: string;
  isDeleted: boolean;
}

export interface Category {
  cetegoryId: number;
  categoryName: string;
}

export interface ProductFilter {
  categoryIds?: number[];   // multiple category filter
  minPrice?: number;
  maxPrice?: number;
  name?: string;
  color?: string;
  desc?: string;            // search in description
  position?: number;        // page number (1-based, 10 items per page)
  limit?: number;           // max items per page (default 10)
}
