# הוראות הטמעה — Emotion Mirror בצ'אט

## מה השתנה

- `chat.component.ts` — עודכן (הוסף לוגיקת מצלמה)
- `chat.component.html` — עודכן (הוסף כפתור 📷 + תצוגת מצלמה)
- `chat.component.css` — עודכן (עוצב רכיב המצלמה)

---

## שלב 1 — הוסף face-api.js ל-index.html

הוסף את השורה הזו ל-`<head>` של `src/index.html`:

```html
<script defer src="https://cdn.jsdelivr.net/npm/face-api.js@0.22.2/dist/face-api.min.js"></script>
```

---

## שלב 2 — הורד את מודלי ה-AI (חובה)

face-api.js צריך קבצי מודל מקומיים. הורד אותם פעם אחת:

```bash
# בתוך תיקיית Angular שלך (ClientApp):
mkdir -p src/assets/face-api-models

# הורד את שני המודלים:
curl -L https://github.com/justadudewhohacks/face-api.js/raw/master/weights/tiny_face_detector_model-weights_manifest.json \
  -o src/assets/face-api-models/tiny_face_detector_model-weights_manifest.json

curl -L https://github.com/justadudewhohacks/face-api.js/raw/master/weights/tiny_face_detector_model-shard1 \
  -o src/assets/face-api-models/tiny_face_detector_model-shard1

curl -L https://github.com/justadudewhohacks/face-api.js/raw/master/weights/face_expression_recognition_model-weights_manifest.json \
  -o src/assets/face-api-models/face_expression_recognition_model-weights_manifest.json

curl -L https://github.com/justadudewhohacks/face-api.js/raw/master/weights/face_expression_recognition_model-shard1 \
  -o src/assets/face-api-models/face_expression_recognition_model-shard1
```

> על Windows — הורד ידנית מ:
> https://github.com/justadudewhohacks/face-api.js/tree/master/weights
> את 4 הקבצים האלה ושמור ב-`src/assets/face-api-models/`

---

## שלב 3 — החלף את הקבצים

העתק את 3 הקבצים המעודכנים לתיקיית `src/app/chat/`

---

## איך זה עובד

1. משתמש לוחץ 📷
2. הדפדפן מבקש הרשאת מצלמה
3. מופיע תצוגה חיה עם נקודה אדומה (SECURITY indicator)
4. face-api.js מנתח את הפנים
5. ברגע שזוהה רגש → האימוג'י מתווסף לטקסט והמצלמה נכבית

## מיפוי רגשות

| רגש | אימוג'י |
|-----|---------|
| happy | 😊 |
| sad | 😢 |
| angry | 😠 |
| surprised | 😲 |
| fearful | 😨 |
| disgusted | 🤢 |
| neutral | 😐 |

---

## נושאי CV שכדאי להדגיש בראיון

- **SECURITY**: בקשת הרשאת מצלמה רק על פעולת משתמש מפורשת, אינדיקטור ויזואלי שהמצלמה פעילה, כיבוי מיידי אחרי הזיהוי, `ngOnDestroy` שמנקה את ה-stream
- **Privacy**: שום תמונה לא נשלחת לשרת — הכל רץ בדפדפן
- **UX**: lazy loading של המודלים רק בלחיצה הראשונה
