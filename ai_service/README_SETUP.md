# AI Shopping Agent — הוראות הטמעה

## מבנה הקבצים

```
ai_service/          ← שרת Python חדש (ליצור לצד הפרויקט הקיים)
  chat_service.py
  requirements.txt
  .env

dotnet/              ← להוסיף לפרויקט WebApiShop הקיים
  ChatController.cs
  SearchController.cs

angular/             ← להוסיף לפרויקט Angular הקיים
  chat.service.ts
  chat.component.ts
  chat.component.html
  chat.component.css
```

---

## שלב 1 — Python Service

```bash
cd ai_service

# התקן תלויות
pip install -r requirements.txt

# ערוך את .env והכנס את ה-API KEY שלך:
# GROQ_API_KEY=gsk_xxxxxxxxxxxx

# הרץ את השרת
uvicorn chat_service:app --port 8001 --reload
```

בדוק שעובד: http://localhost:8001/docs

---

## שלב 2 — .NET

1. העתק `ChatController.cs` ו-`SearchController.cs` לתיקיית `Controllers/` של `WebApiShop`

2. **שים לב:** בשני הcontrollers יש קריאה ל-`_productService.GetAllProductsAsync()`
   — ודא שזה שם המתודה הנכון אצלך (בדוק ב-`IProductService`)

3. הוסף ל-`Program.cs` (אם לא קיים כבר):
```csharp
builder.Services.AddHttpClient();
```

---

## שלב 3 — Angular

1. העתק את 4 קבצי Angular לתיקייה חדשה: `src/app/chat/`

2. הוסף לתחתית `app.component.html`:
```html
<app-chat></app-chat>
```
 
3. ייבא את `ChatComponent` ב-`app.component.ts`:
```typescript
import { ChatComponent } from './chat/chat.component';

@Component({
  imports: [..., ChatComponent],
})
```

---

## שלב 4 — בדיקה

1. הרץ את Python: `uvicorn chat_service:app --port 8001 --reload`
2. הרץ את .NET: `dotnet run`
3. הרץ את Angular: `npm start`
4. פתח את האתר — אמור להופיע בועת 💬 בפינה הימנית התחתונה

---

## שם המתודה ב-IProductService

בדוק ב-`Services/IProductService.cs` מה שם המתודה שמחזירה את כל המוצרים.
אם שמה שונה (למשל `GetProductsAsync` או `GetAllAsync`), עדכן בשני ה-Controllers.
