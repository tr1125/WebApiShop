import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../services/product';
import { Product, Category } from '../../models/product.model';
import { OrderService } from '../../services/order';
import { OrderDTO, ORDER_STATUSES } from '../../models/order.model';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

// קומפוננט אזור מנהל - ניהול מוצרים וקטגוריות
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin.html',
  styleUrls: ['./admin.scss']
})
export class AdminComponent implements OnInit {
  // טפסים
  productForm: FormGroup;
  categoryForm: FormGroup;
  
  // נתונים
  products: Product[] = [];
  categories: Category[] = [];
  orders: OrderDTO[] = [];
  users: any[] = [];
  orderStatuses = ORDER_STATUSES;
  pendingStatus: Record<number, string> = {};
  
  // מצבים
  isLoading = false;
  isProductsLoading = false;
  isCategoriesLoading = false;
  isOrdersLoading = false;
  isUsersLoading = false;
  showAddProduct = false;
  showAddCategory = false;
  message = '';
  messageType: 'success' | 'error' = 'success';
  editingProductId: number | null = null;
  
  constructor(
    private formBuilder: FormBuilder,
    private productService: ProductService,
    private authService: AuthService,
    private orderService: OrderService,
    private router: Router
  ) {
    // יצירת טופס מוצר
    this.productForm = this.formBuilder.group({
      productName: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(0)]],
      categoryId: ['', Validators.required],
      imageURL: [''],
      color: ['']
    });
    
    // יצירת טופס קטגוריה
    this.categoryForm = this.formBuilder.group({
      categoryName: ['', Validators.required]
    });
  }
  
  ngOnInit() {
    // בדיקת אימות
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth']);
      return;
    }

    // טעינת נתונים ראשונית
    this.loadProducts(true);
    this.loadCategories(true);
    this.loadOrders(true);
    this.loadUsers();
  }
  
  // טעינת קטגוריות
  loadCategories(isInit = false) {
    if (!isInit) {
      this.isCategoriesLoading = true;
    }
    this.productService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
        this.isCategoriesLoading = false;
        console.log('קטגוריות נטענו:', categories);
      },
      error: (error) => {
        this.isCategoriesLoading = false;
        console.error('שגיאה בטעינת קטגוריות:', error);
        this.showMessage('שגיאה בטעינת קטגוריות', 'error');
      }
    });
  }
  
  // טעינת מוצרים
  loadProducts(isInit = false) {
    if (!isInit) {
      this.isProductsLoading = true;
    }
    
    // נגדיר פרמטרים לקבלת כל המוצרים (או כמות גדולה מספיק)
    // ה-limit ממופה ל-skip ב-ProductService, וה-position הוא מס' עמוד
    // אבל אנחנו רוצים להגדיל את כמות המוצרים בדף אחד
    this.productService.getProducts({ limit: 10000 }).subscribe({
      next: (products) => {
        this.products = products;
        this.isProductsLoading = false;
        console.log('מוצרים נטענו (Admin):', products.length);
      },
      error: (error) => {
        this.isProductsLoading = false;
        console.error('שגיאה בטעינת מוצרים:', error);
        this.showMessage('שגיאה בטעינת מוצרים', 'error');
      }
    });
  }
  
  // טעינת הזמנות
  loadOrders(isInit = false) {
    if (!isInit) {
      this.isOrdersLoading = true;
    }
    this.orderService.getAllOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.isOrdersLoading = false;
        orders.forEach(o => { this.pendingStatus[o.orderId] = o.status; });
      },
      error: () => {
        this.isOrdersLoading = false;
        this.showMessage('שגיאה בטעינת הזמנות', 'error');
      }
    });
  }

  // טעינת משתמשים
  loadUsers() {
    this.authService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users.filter(u => !u.isAdmin);
        console.log('משתמשים נטענו:', users);
      },
      error: (error) => {
        console.error('שגיאה בטעינת משתמשים:', error);
        this.showMessage('שגיאה בטעינת משתמשים', 'error');
      }
    });
  }
  
  // העלאת משתמש לתפקיד מנהל
  promoteUser(userId: number) {
    if (confirm('האם אתה בטוח שברצונך להעלות משתמש זה לתפקיד מנהל?')) {
      this.authService.promoteToAdmin(userId).subscribe({
        next: () => {
          this.users = this.users.filter(u => u.userId !== userId);
          this.showMessage('משתמש הועלה לתפקיד מנהל בהצלחה!', 'success');
        },
        error: () => this.showMessage('שגיאה בהעלאת משתמש', 'error')
      });
    }
  }

  // עדכון סטטוס הזמנה
  onUpdateOrderStatus(order: OrderDTO) {
    const newStatus = this.pendingStatus[order.orderId];
    if (!newStatus) return;
    this.orderService.updateOrderStatus(order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        this.showMessage(`הזמנה #${order.orderId} עודכנה ל-${newStatus}`, 'success');
      },
      error: () => this.showMessage('שגיאה בעדכון סטטוס', 'error')
    });
  }

  // הוספת/עדכון מוצר
  onAddProduct() {
    if (this.productForm.valid) {
      this.isLoading = true;
      
      const productData = {
        productName: this.productForm.value.productName,
        description: this.productForm.value.description || "",
        price: Number(this.productForm.value.price),
        categoryId: Number(this.productForm.value.categoryId),
        imageURL: this.productForm.value.imageURL || "",
        color: this.productForm.value.color || "",
        isDeleted: false
      };
      console.log('Sending product data:', productData);
      
      if (this.editingProductId) {
        // עדכון מוצר קיים
        this.productService.updateProduct(this.editingProductId, productData).subscribe({
          next: (product) => {
            const index = this.products.findIndex(p => p.productId === this.editingProductId);
            if (index !== -1) {
              this.products[index] = product;
            }
            this.productForm.reset();
            this.showAddProduct = false;
            this.editingProductId = null;
            this.showMessage('מוצר עודכן בהצלחה!', 'success');
            this.isLoading = false;
          },
          error: (error) => {
            console.error('שגיאה בעדכון מוצר:', error);
            this.showMessage('שגיאה בעדכון המוצר', 'error');
            this.isLoading = false;
          }
        });
      } else {
        // הוספת מוצר חדש
        this.productService.createProduct(productData).subscribe({
          next: (product) => {
            console.log('מוצר נוסף בהצלחה:', product);
            this.products.push(product);
            this.productForm.reset();
            this.showAddProduct = false;
            this.showMessage('מוצר נוסף בהצלחה!', 'success');
            this.isLoading = false;
          },
          error: (error) => {
            console.error('שגיאה בהוספת מוצר:', error);
            this.showMessage('שגיאה בהוספת המוצר', 'error');
            this.isLoading = false;
          }
        });
      }
    } else {
      this.showMessage('נא למלא את כל שדות החובה בצורה תקינה', 'error');
    }
  }
  
  // הוספת קטגוריה חדשה
  onAddCategory() {
    if (this.categoryForm.valid) {
      this.isLoading = true;
      
      // כאן נוסיף קריאה ל-API להוספת קטגוריה
      // לעכשיו רק סימולציה
      setTimeout(() => {
        const newCategory = {
          id: Date.now(),
          ...this.categoryForm.value
        };
        
        this.categories.push(newCategory);
        this.categoryForm.reset();
        this.showAddCategory = false;
        this.showMessage('קטגוריה נוספה בהצלחה!', 'success');
        this.isLoading = false;
      }, 1000);
    }
  }
  
  // מחיקת מוצר
  deleteProduct(productId: number) {
    if (confirm('האם אתה בטוח שברצונך למחוק מוצר זה?')) {
      this.productService.deleteProduct(productId).subscribe({
        next: () => {
          this.products = this.products.filter(p => p.productId !== productId);
          this.showMessage('מוצר נמחק בהצלחה!', 'success');
        },
        error: (error) => {
          console.error('שגיאה במחיקת מוצר:', error);
          this.showMessage('שגיאה במחיקת המוצר', 'error');
        }
      });
    }
  }
  
  // עריכת מוצר
  editProduct(product: Product) {
    this.editingProductId = product.productId;
    this.productForm.patchValue(product);
    this.showAddProduct = true;
  }
  
  // ביטול הוספת מוצר
  cancelAddProduct() {
    this.productForm.reset();
    this.showAddProduct = false;
    this.editingProductId = null;
  }
  
  // ביטול הוספת קטגוריה
  cancelAddCategory() {
    this.categoryForm.reset();
    this.showAddCategory = false;
  }
  
  // הצגת הודעה
  private showMessage(message: string, type: 'success' | 'error') {
    alert(message);
  }
  
  // קבלת מספר מוצרים פעילים
  getActiveProductsCount(): number {
    return this.products.filter(p => !p.isDeleted).length;
  }
  
  // חישוב ערך כללי של המוצרים
  getTotalValue(): number {
    return this.products.reduce((total, product) => total + product.price, 0);
  }
  
  // יצירת ד"ח
  generateReport() {
    const report = {
      totalProducts: this.products.length,
      activeProducts: this.getActiveProductsCount(),
      totalCategories: this.categories.length,
      totalValue: this.getTotalValue(),
      generatedAt: new Date()
    };
    
    console.log('ד"ח מערכת:', report);
    this.showMessage('ד"ח נוצר - ראה קונסול', 'success');
  }

  // חזרה לדף הבית - לדף העיצוב (ולא לדף הראשי שזורק חזרה למנהל)
  backToHome() {
    this.router.navigate(['/design']);
  }
  
  // Track by functions לאופטימיזציה
  trackByProductId(index: number, product: Product): number {
    return product.productId;
  }
  
  trackByCategoryId(index: number, category: Category): number {
    return category.cetegoryId;
  }
}
