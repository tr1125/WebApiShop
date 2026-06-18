from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from groq import Groq
from dotenv import load_dotenv
import os
import numpy as np
import httpx
import certifi
import ssl

load_dotenv()

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"]
)

# Python 3.14 added VERIFY_X509_STRICT which rejects NetFree's CA certificate
# because it lacks the Key Usage extension. We use the Windows system store
# (which includes NetFree's installed CA) and disable only that strict check.
_ssl_ctx = ssl.create_default_context()
_ssl_ctx.verify_flags &= ~ssl.VERIFY_X509_STRICT

client = Groq(
    api_key=os.getenv("GROQ_API_KEY"),
    http_client=httpx.Client(verify=_ssl_ctx)
)

# ── SYSTEM PROMPT ─────────────────────────────────────────────
# זה הלב של הסוכן — ערוך את זה לפי הצורך
SYSTEM_PROMPT = """
You are an interior design shopping assistant for DesignRoom Studio.
We sell home-decor products including sofas, tables, lamps, carpets, curtains, clocks, and more.
You help customers find the perfect items for their rooms.

Rules you must ALWAYS follow:
- Always ask about the customer's budget before recommending products.
- Ask about the room style (modern, classic, minimalist, etc.) if not mentioned.
- Keep answers to 3-4 sentences maximum.
- Only recommend products from our catalog.
- If you don't know something, say so honestly.
- Be warm, helpful, and design-savvy.

Example:
User: I need a lamp for my living room.
Assistant: I'd love to help you find the perfect lamp! Could you tell me your budget range and the style of your living room — modern, classic, or something else? That way I can point you to the best options we have.
"""

# ── DATA MODELS ───────────────────────────────────────────────
class Message(BaseModel):
    role: str
    content: str

class ChatRequest(BaseModel):
    message: str
    history: list[Message] = []
    products: list = []

class SearchRequest(BaseModel):
    query: str
    products: list = []
    top_k: int = 5

# ── CHAT ENDPOINT ─────────────────────────────────────────────
@app.post("/chat")
async def chat(req: ChatRequest):
    if req.products:
        catalog_lines = []
        for p in req.products:
            stock = "in stock" if p.get("inStock") else "out of stock"
            line = f"- {p['name']} (${p['price']}) [{stock}]: {p.get('description', '')}"
            catalog_lines.append(line)
        catalog = "\n".join(catalog_lines)
        full_prompt = SYSTEM_PROMPT + f"\n\nAvailable products:\n{catalog}\n\nOnly recommend products from this list."
    else:
        full_prompt = SYSTEM_PROMPT

    messages = [{"role": "system", "content": full_prompt}]
    for m in req.history:
        messages.append({"role": m.role, "content": m.content})
    messages.append({"role": "user", "content": req.message})

    try:
        response = client.chat.completions.create(
            model="llama-3.3-70b-versatile",
            messages=messages,
            max_tokens=400,
            temperature=0.6
        )
        return {"reply": response.choices[0].message.content}
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"AI service error: {str(e)}")

# ── SEMANTIC SEARCH ───────────────────────────────────────────
# NOTE: Groq לא מספק embeddings — משתמשים ב-sentence-transformers במקום (חינמי, מקומי)
# להתקנה: pip install sentence-transformers
# אם לא רוצים את זה, אפשר להשאיר את ה-/search endpoint ולא להשתמש בו

_model = None

def get_embedding_model():
    global _model
    if _model is None:
        from sentence_transformers import SentenceTransformer
        _model = SentenceTransformer("all-MiniLM-L6-v2")
    return _model

def cosine_similarity(a, b):
    a, b = np.array(a), np.array(b)
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b)))

@app.post("/search")
async def search(req: SearchRequest):
    if not req.products:
        return {"results": []}

    model = get_embedding_model()

    query_embedding = model.encode(req.query).tolist()

    scored = []
    for p in req.products:
        product_text = f"{p.get('name', '')} {p.get('description', '')}"
        product_embedding = model.encode(product_text).tolist()
        score = cosine_similarity(query_embedding, product_embedding)
        scored.append({**p, "score": round(score, 3)})

    results = sorted(scored, key=lambda x: x["score"], reverse=True)
    return {"results": results[:req.top_k]}
