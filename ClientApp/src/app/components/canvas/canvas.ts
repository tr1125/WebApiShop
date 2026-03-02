import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  HostListener, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDrag, CdkDragEnd } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';
import { DesignRoomStateService } from '../../services/design-room-state';
import { CanvasItem, ResizeSession, StaticEntry } from '../../models/canvas.model';
import { ProductService } from '../../services/product';
import { Product, Category, ProductFilter } from '../../models/product.model';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-canvas',
  standalone: true,
  imports: [CommonModule, FormsModule, CdkDrag],
  templateUrl: './canvas.html',
  styleUrls: ['./canvas.scss'],
})
export class CanvasComponent implements OnInit, OnDestroy {

  @ViewChild('canvasEl') canvasEl!: ElementRef<HTMLDivElement>;

  // — Canvas state —
  items: CanvasItem[] = [];
  selectedId: string | null = null;
  dragResets = new Map<string, { x: number; y: number }>();
  canvasWidth  = 800;
  canvasHeight = 600;
  private resizeSession: ResizeSession | null = null;

  // — Catalog (product list in left sidebar) —
  catalogProducts: Product[] = [];
  allProducts: Product[] = [];   // full list used for price lookup
  categories: Category[] = [];
  currentPage = 1;
  hasMoreProducts = false;
  readonly PAGE_SIZE = 10;

  // — Static palette: wall + floor —
  readonly staticPalette: StaticEntry[] = [];

  // — Filters (right sidebar) —
  selectedCategoryIds: number[] = [];
  filterMinPrice: number | null = null;
  filterMaxPrice: number | null = null;
  filterName  = '';
  filterColor = '';
  filterDesc  = '';

  // — Product detail modal —
  selectedCatalogProduct: Product | null = null;
  isProductModalOpen = false;
  modalQuantity = 1;

  currentFloorImage: string | null = null;
  currentWallImage: string | null = null;

  private subs = new Subscription();

  constructor(
    private stateService: DesignRoomStateService,
    private productService: ProductService,
    private authService: AuthService,
    public  router: Router,
    private ngZone: NgZone
  ) {}

  // —
  //  Lifecycle
  // —

  ngOnInit(): void {
    // Initialize user-specific canvas storage
    const user = this.authService.getCurrentUser();
    const userId = user?.userId || 'guest';
    const isGuest = !user;
    
    // Use sessionStorage for guest, localStorage for registered users
    const storage = isGuest ? sessionStorage : localStorage;
    const savedFloor = storage.getItem(`floor_${userId}`);
    const savedWall = storage.getItem(`wall_${userId}`);
  
    if (savedFloor) this.currentFloorImage = savedFloor;
    if (savedWall) this.currentWallImage = savedWall;

    this.stateService.setUser(user?.userId ?? null);



    // Subscribe to canvas items
    this.subs.add(
      this.stateService.items$.subscribe(items => {
        this.items = items;
        items.forEach(it => {
          if (!this.dragResets.has(it.id)) {
            this.dragResets.set(it.id, { x: 0, y: 0 });
          }
        });
      })
    );

    // All products (for price lookup)
    this.subs.add(
      this.productService.products$.subscribe(products => {
        this.allProducts = products;
      })
    );

    // Categories for filter checkboxes
    this.subs.add(
      this.productService.categories$.subscribe(cats => {
        this.categories = cats;
      })
    );

    // Initial catalog load (page 1, no filters)
    this.loadCatalog();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // —
  //  Catalog: load / filter / paging
  // —

  loadCatalog(append = false): void {
    const filter: ProductFilter = {
      categoryIds: this.selectedCategoryIds.length ? [...this.selectedCategoryIds] : undefined,
      minPrice:    this.filterMinPrice ?? undefined,
      maxPrice:    this.filterMaxPrice ?? undefined,
      name:        this.filterName.trim()  || undefined,
      color:       this.filterColor.trim() || undefined,
      desc:        this.filterDesc.trim()  || undefined,
      position:    this.currentPage,
    };

    this.productService.getProducts(filter).subscribe({
      next: products => {
        products.forEach(p => {
          console.log('Product:', p.productName, 'ImageURL:', p.imageURL, 'Cleaned:', this.CleanimageUrl(p.imageURL));
        });
        this.catalogProducts = append
          ? [...this.catalogProducts, ...products]
          : products;
        this.hasMoreProducts = products.length >= this.PAGE_SIZE;
      },
      error: err => console.error('loadCatalog error:', err)
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadCatalog(false);
  }

  loadMore(): void {
    this.currentPage++;
    this.loadCatalog(true);
  }

  clearFilters(): void {
    this.selectedCategoryIds = [];
    this.filterMinPrice = null;
    this.filterMaxPrice = null;
    this.filterName  = '';
    this.filterColor = '';
    this.filterDesc  = '';
    this.applyFilters();
  }

  toggleCategory(id: number): void {
    const idx = this.selectedCategoryIds.indexOf(id);
    if (idx === -1) this.selectedCategoryIds.push(id);
    else this.selectedCategoryIds.splice(idx, 1);
    this.applyFilters();
  }

  isCategorySelected(id: number): boolean {
    return this.selectedCategoryIds.includes(id);
  }

  // —
  //  Add items to canvas
  // —

  /** Click on catalog product → select it + add to canvas (cart) */
  selectCatalogProduct(product: Product): void {
    this.selectedCatalogProduct = product;
    this.addItemToCanvas(product);
  }

  /** Add multiple items from modal to canvas */
  addToCanvasFromModal(): void {
    if (!this.selectedCatalogProduct || this.modalQuantity < 1) return;
    
    // Add items in a loop, staggering them slightly
    for (let i = 0; i < this.modalQuantity; i++) {
      // Small random offset so they don't stack perfectly
      const offsetX = i * 20; 
      const offsetY = i * 20;
      this.addItemToCanvas(this.selectedCatalogProduct, offsetX, offsetY);
    }
    
    this.closeProductModal();
  }

  /** Add item helper */
  addItemToCanvas(product: Product, offsetX = 0, offsetY = 0): void {
    const size = this.defaultSizeFor(product.categoryId);
    const type = this.getCategoryName(product.categoryId);
    
    if (type === 'floors') {
      this.currentFloorImage = this.CleanimageUrl(product.imageURL);
      const user = this.authService.getCurrentUser();
      const userId = user?.userId || 'guest';
      const storage = !user ? sessionStorage : localStorage;
      storage.setItem(`floor_${userId}`, this.currentFloorImage); 
      return;
    }

    if (type === 'walls') {
      this.currentWallImage = this.CleanimageUrl(product.imageURL);
      const user = this.authService.getCurrentUser();
      const userId = user?.userId || 'guest';
      const storage = !user ? sessionStorage : localStorage;
      storage.setItem(`wall_${userId}`, this.currentWallImage);
      return;
    }

    const newItem: CanvasItem = {
      id: `item-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
      type,
      x: Math.round(this.canvasWidth  / 2 - size.w / 2 + offsetX),
      y: Math.round(this.canvasHeight / 2 - size.h / 2 + offsetY),
      width:  size.w,
      height: size.h,
      label:  product.productName,
      productId: product.productId,
      price: product.price,
      color: product.color,
      imageURL: product.imageURL,
    };
    
    // Bounds check to ensure item stays mostly visible
    newItem.x = Math.max(0, Math.min(newItem.x, this.canvasWidth - newItem.width));
    newItem.y = Math.max(0, Math.min(newItem.y, this.canvasHeight - newItem.height));

    this.stateService.addItem(newItem);
    this.selectedId = newItem.id;
  }

  // /** Click on wall / floor static entry → add to canvas */
  // addStaticToCanvas(entry: StaticEntry): void {
  //   const newItem: CanvasItem = {
  //     id: `item-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
  //     type:   entry.type,
  //     x:      Math.round(this.canvasWidth  / 2 - entry.defaultWidth  / 2),
  //     y:      Math.round(this.canvasHeight / 2 - entry.defaultHeight / 2),
  //     width:  entry.defaultWidth,
  //     height: entry.defaultHeight,
  //     label:  entry.label,
  //   };
  //   this.stateService.addItem(newItem);
  //   this.selectedId = newItem.id;
  // }

  // —
  //  Product modal
  // —

  openProductModal(): void  { 
    this.modalQuantity = 1;
    this.isProductModalOpen = true;  
  }
  closeProductModal(): void { this.isProductModalOpen = false; }

  // —
  //  Cart / canvas removal
  // —

  /** Remove the currently selected canvas item (= remove from cart) */
  removeSelectedFromCanvas(): void {
    if (this.selectedId) {
      this.stateService.removeItem(this.selectedId);
      this.selectedId = null;
    }
  }

  clearAll(): void {
    this.stateService.clearCanvas();
    this.selectedId = null;
  }

  // —
  //  Auth helpers
  // —

  get currentUser() { return this.authService.getCurrentUser(); }
  get isGuest(): boolean { return !this.authService.isAuthenticated(); }
  get canCheckout(): boolean { return this.authService.isAuthenticated(); }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }

  navigateToPersonalArea(): void {
    this.router.navigate(['/personal-area']);
  }

  // —
  //  Canvas selection
  // —

  selectItem(item: CanvasItem): void { 
    this.selectedId = item.id; 
    
    // Update selectedCatalogProduct based on item.productId
    if (item.productId) {
      // 1. Try to find in loaded products (catalog or allProducts)
      // Check both lists to maximize chances of finding the full product
      const found = this.catalogProducts.find(p => p.productId === item.productId) 
                 || this.allProducts.find(p => p.productId === item.productId);
      
      if (found) {
        this.selectedCatalogProduct = found;
      } else {
        // 2. Fallback: reconstruct from CanvasItem
        // If we really need description, we could fetch it here, but for now we enable the button
        this.selectedCatalogProduct = { 
          productId: item.productId,
          productName: item.label ?? 'Unknown',
          price: item.price ?? 0,
          color: item.color,
          imageURL: item.imageURL,
          // Missing fields
          categoryId: 0,
          isDeleted: false,
          description: '' 
        } as Product;
        
        // Optional: Fetch full details if missing
        this.productService.getProductById(item.productId).subscribe({
          next: (p) => {
            // Only update if still selected
            if (this.selectedCatalogProduct?.productId === p.productId) {
              this.selectedCatalogProduct = p;
            }
          },
          error: () => console.log('Could not fetch full product details for item', item.id)
        });
      }
    } else {
      this.selectedCatalogProduct = null;
    }
  }

  get selectedItem(): CanvasItem | undefined {
    return this.items.find(i => i.id === this.selectedId);
  }

  // —
  //  CDK drag
  // —

  getDragReset(item: CanvasItem): { x: number; y: number } {
    return this.dragResets.get(item.id) ?? { x: 0, y: 0 };
  }

  onDragEnded(event: CdkDragEnd, item: CanvasItem): void {
    const pos = event.source.getFreeDragPosition();
    let newX = Math.max(0, Math.min(item.x + pos.x, this.canvasWidth  - item.width));
    let newY = Math.max(0, Math.min(item.y + pos.y, this.canvasHeight - item.height));
    this.stateService.moveItem(item.id, Math.round(newX), Math.round(newY));
    this.dragResets.set(item.id, { x: 0, y: 0 });
    event.source.reset();
  }

  // —
  //  Resize — corner handle
  // —

  startResize(event: MouseEvent, item: CanvasItem): void {
    event.stopPropagation();
    event.preventDefault();
    this.resizeSession = {
      itemId: item.id,
      startMouseX: event.clientX,
      startMouseY: event.clientY,
      startWidth:  item.width,
      startHeight: item.height,
    };
  }

  @HostListener('document:mousemove', ['$event'])
  onDocumentMouseMove(event: MouseEvent): void {
    if (!this.resizeSession) return;
    const dx = event.clientX - this.resizeSession.startMouseX;
    const dy = event.clientY - this.resizeSession.startMouseY;
    this.ngZone.run(() => {
      this.stateService.resizeItem(
        this.resizeSession!.itemId,
        this.resizeSession!.startWidth  + dx,
        this.resizeSession!.startHeight + dy,
      );
    });
  }

  @HostListener('document:mouseup')
  onDocumentMouseUp(): void { this.resizeSession = null; }

  resizeDimension(dim: 'width' | 'height', event: Event): void {
    if (!this.selectedId || !this.selectedItem) return;
    const raw = (event.target as HTMLInputElement).valueAsNumber;
    const value = isNaN(raw) ? 40 : raw;
    const newWidth  = dim === 'width'  ? value : this.selectedItem.width;
    const newHeight = dim === 'height' ? value : this.selectedItem.height;
    this.stateService.resizeItem(this.selectedId, newWidth, newHeight);
  }

  // —
  //  Canvas layer getters + price
  // —

  get backgroundLayers(): CanvasItem[] {
    return this.items.filter(i => i.type === 'wall' || i.type === 'floor');
  }

  get furnitureItems(): CanvasItem[] {
    return this.items.filter(i => i.type !== 'wall' && i.type !== 'floor');
  }

  get totalPrice(): number {
    return this.furnitureItems.reduce((sum, item) => sum + (item.price ?? 0), 0);
  }

  trackById(_: number, item: CanvasItem): string { return item.id; }

  get groupedProducts(): { category: string, products: Product[] }[] {
    const groups = new Map<string, Product[]>();
    for (const p of this.catalogProducts) {
      // Find category name
      const cat = this.categories.find(c => c.cetegoryId === p.categoryId);
      const catName = cat ? cat.categoryName : 'כללי';
      if (!groups.has(catName)) {
        groups.set(catName, []);
      }
      groups.get(catName)!.push(p);
    }
    return Array.from(groups.entries()).map(([category, products]) => ({ category, products }));
  }
  
  // —
  //  Utility helpers (public so template can call them)
  // —

  imageUrl(url?: string): string {
    if (!url) return '';
    return url.startsWith('http') ? url : `http://localhost:4200/${url}`;
  }

  getIcon(name: string): string {
    const n = (name || '').toLowerCase();
    const base = 'images/icons/';
    
    if (n === 'home') return base + 'home.png';
    if (n === 'user') return base + 'person.png';
    if (n === 'trash') return base + 'delete.png';
    if (n === 'search') return base + 'search.png';
    if (n === 'filter') return base + 'filter.png';
    if (n === 'cart') return base + 'shopping_cart.png';
    if (n === 'add_cart') return base + 'add_shopping_cart.png';
    if (n === 'check') return base + 'check.png';
    if (n === 'orders') return base + 'orders.png';
    
    return base + 'details.png';
  }

  private getCategoryName(categoryId: number): CanvasItem['type'] {
    const cat = this.categories.find(c => c.cetegoryId === categoryId);
    return (cat?.categoryName.toLowerCase() as CanvasItem['type']) || 'unknown';
  }

  private defaultSizeFor(categoryId: number): { w: number; h: number } {
    const cat = this.categories.find(c => c.cetegoryId === categoryId);
    const name = cat?.categoryName.toLowerCase() || '';
    
    const map: Record<string, { w: number; h: number }> = {
      'floors':                { w: 800, h: 600 },
      'walls':                 { w: 800, h: 20  },
      'windows':               { w: 100, h: 120 },
      'sofas & armchairs':     { w: 180, h: 90  },
      'pictures':              { w: 80,  h: 80  },
      'flower pots':           { w: 60,  h: 60  },
      'curtains':              { w: 100, h: 150 },
      'lamps':                 { w: 50,  h: 80  },
      'tables':                { w: 120, h: 80  },
      'clocks':                { w: 60,  h: 60  },
      'carpets':               { w: 200, h: 140 },
    };
    return map[name] ?? { w: 100, h: 100 };
  }

  getZIndex(type: string): number {
    const n = type.toLowerCase();
    
    if (n === 'floors') return 1;
    if (n === 'walls') return 2;
    if (n === 'carpets') return 10;
    if (n === 'clocks' || n === 'windows' || n === 'pictures') return 20;
    if (n === 'curtains') return 30;
    if (n === 'flower pots' || n === 'sofas & armchairs' || n === 'tables' || n === 'lamps') return 40;
    
    return 50;
  }

  shouldHaveShadow(type: string): boolean {
    const shadowTypes = ['sofas & armchairs', 'flower pots', 'tables', 'lamps'];
    return shadowTypes.some(t => type.toLowerCase().includes(t.toLowerCase()));
  }

  getItemCategoryId(item: CanvasItem): number {
    if (!item.productId) return 0;
    const product = this.allProducts.find(p => p.productId === item.productId);
    return product?.categoryId || 0;
  }


  CleanimageUrl(url?: string): string {
    if (!url) return '';
  
  // 1. החלפת לוכסנים הפוכים (\) בלוכסנים ישרים (/)
  // 2. ניקוי תווים לבנים ותווי בקרה נסתרים
  const cleanedUrl = url
    .replace(/\\/g, '/')                      // הופך \ ל- /
    .replace(/[\n\r\t\0\x0B\f]/g, '')         // מנקה תווים מיוחדים כולל \f
    .trim();
  
  return cleanedUrl.startsWith('http') 
    ? cleanedUrl 
    : `http://localhost:4200/${cleanedUrl}`;
  }
}


  
