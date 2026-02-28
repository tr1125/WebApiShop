import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ProductService } from '../../services/product';
import { Product as ServiceProduct, Category } from '../../models/product.model';

// ממשק לפריט רהיט בחדר
interface FurnitureItem {
  id: number;
  name: string;
  type: string;
  width: number;
  height: number;
  x: number;
  y: number;
  color: string;
  price: number;
  description?: string;
}

// קומפוננט עיצוב החדר - גרירה ושחרור של רהיטים
@Component({
  selector: 'app-design-room',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule],
  templateUrl: './design-room.html',
  styleUrls: ['./design-room.scss']
})
export class DesignRoomComponent implements OnInit {
  // רשימת רהיטים זמינים לגרירה
  availableFurniture: FurnitureItem[] = [];
  
  // רהיטים שנוספו לחדר
  roomFurniture: FurnitureItem[] = [];
  
  // מידות החדר בפיקסלים
  roomWidth = 600;
  roomHeight = 400;
  
  // מצב הפריט הנבחר לעריכה
  selectedItem: FurnitureItem | null = null;
  
  // מאפיינים נוספים שנדרשים בHTML
  isLoading = false;
  availableProducts: ServiceProduct[] = [];
  message = '';
  cartQuantity = 1;
  
  ngOnInit() {
    console.log('Design Room Component loaded');
    this.loadProducts();
  }
  
  constructor(private productService: ProductService) {}
  
  // טעינת מוצרים
  loadProducts() {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.availableProducts = products;
        this.isLoading = false;
      },
      error: () => {
        this.message = 'שגיאה בטעינת מוצרים';
        this.isLoading = false;
      }
    });
  }
  
  // קבלת סוג המוצר
  getProductType(productName: string): string {
    // מיפוי פשוט לפי שם המוצר
    if (productName.includes('כורסא')) return 'chair';
    if (productName.includes('שולחן')) return 'table';
    if (productName.includes('ספה')) return 'sofa';
    if (productName.includes('ארון')) return 'wardrobe';
    return 'furniture';
  }
  
  // טיפול בשחרור פריט בחדר
  drop(event: CdkDragDrop<FurnitureItem[]>) {
    if (event.previousContainer === event.container) {
      // העברת פריט בתוך אותו מיכל
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // העברת פריט מרשימת הפריטים הזמינים לחדר
      // שימוש ב-Data שמוצמד לאלמנט הגרירה
      const sourceItem: any = event.item.data;
      const item: any = { ...sourceItem };
      
      item.id = Date.now(); // מזהה ייחודי לכל פריט בחדר
      
      // התאמת שמות שדות אם חסרים (כגון שם המוצר)
      if (!item.name && item.productName) {
        item.name = item.productName;
      }
      
      // וודא שהמחיר הוא מספר
      if (item.price) {
        item.price = +item.price;
      }

      // שמירת תיאור
      if (!item.description && sourceItem.description) {
        item.description = sourceItem.description;
      }
      
      // הגדרת מידות ברירת מחדל אם חסרות
      if (!item.width) item.width = 100;
      if (!item.height) item.height = 100;
      
      // חישוב מיקום הפריט בחדר לפי מיקום השחרור
      const dropPosition = event.dropPoint;
      // שימוש בקואורדינטות קבועות להתחלה או חישוב יחסי אם אפשרי
      // במקרה פשוט נשים במרכז או בפינה אם החישוב מורכב מדי כרגע ללא אלמנט ה-DOM
      item.x = 50; 
      item.y = 50;
      
      event.container.data.push(item);
    }
  }
  
  // בחירת פריט לעריכה
  selectItem(item: FurnitureItem) {
    this.selectedItem = item;
  }
  
  // הסרת פריט מהחדר
  removeItem(item: FurnitureItem) {
    const index = this.roomFurniture.indexOf(item);
    if (index > -1) {
      this.roomFurniture.splice(index, 1);
    }
    if (this.selectedItem === item) {
      this.selectedItem = null;
    }
  }
  
  // שמירת עיצוב החדר
  saveDesign() {
    console.log('Saving room design:', this.roomFurniture);
    // כאן נוסיף שמירה לשרת
  }
  
  // ניקוי החדר מכל הרהיטים
  clearRoom() {
    this.roomFurniture = [];
    this.selectedItem = null;
  }
  
  // חישוב מחיר כל העיצוב
  getTotalPrice(): number {
    return this.roomFurniture.reduce((total, item) => total + (+item.price || 0), 0);
  }
  
  // קבלת אייקון לפי סוג הרהיט
  getFurnitureIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'chair': '🪑',
      'table': '🪤', 
      'sofa': '🛋️',
      'wardrobe': '🚪',
      'bed': '🛏️',
      'desk': '🏢'
    };
    return icons[type] || '📦';
  }
  
  getImageUrl(relativePath: string): string {
    return this.productService.getImageUrl(relativePath);
  }

  trackByProductId(index: number, product: ServiceProduct): number {
    return product.productId;
  }

  trackByFurnitureId(index: number, item: FurnitureItem): number {
    return item.id;
  }

  addToCart(quantity: number) {
    if (this.selectedItem) {
      console.log(`Added ${quantity}x ${this.selectedItem.name} to cart`);
      this.message = `${quantity}x ${this.selectedItem.name} נוסף לסל`;
      this.selectedItem = null;
      setTimeout(() => this.message = '', 3000);
    }
  }
}