from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from openai import OpenAI
from dotenv import load_dotenv
import os
import json
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

client = OpenAI(
    api_key=os.getenv("OPENAI_API_KEY"),
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

When the user asks you to design a room, suggest a layout, or place items on the canvas,
call the design_room function with a thoughtful arrangement. The canvas is 800×600 px.
Place items so they don't overlap and feel like a real room layout.
Typical sizes: sofa 200×80, armchair 100×80, table 140×70, lamp 60×100, rug 200×150.
CRITICAL: Always use the exact productId values shown as [ID:X] in the catalog. Never invent IDs.

For floors (category: floors) and walls (category: walls):
- Do NOT add them to items[]. Instead, put their imageURL in floorImageURL / wallImageURL fields.
- These set the background texture of the room.

Example:
User: I need a lamp for my living room.
Assistant: I'd love to help you find the perfect lamp! Could you tell me your budget range and the style of your living room — modern, classic, or something else? That way I can point you to the best options we have.
"""

# ── CANVAS TOOL (function calling) ────────────────────────────
CANVAS_TOOL = {
    "type": "function",
    "function": {
        "name": "design_room",
        "description": (
            "Place selected furniture products on the room canvas to design the room. "
            "Call this when the user asks you to design a room, suggest a layout, "
            "or arrange/place items on the canvas."
        ),
        "parameters": {
            "type": "object",
            "properties": {
                "reply": {
                    "type": "string",
                    "description": "Friendly explanation of the design choices shown to the user as a chat message."
                },
                "floorImageURL": {
                    "type": "string",
                    "description": "imageURL of the floor product to set as background (from catalog, category floors). Omit if no floor needed."
                },
                "wallImageURL": {
                    "type": "string",
                    "description": "imageURL of the wall product to set as background (from catalog, category walls). Omit if no wall needed."
                },
                "items": {
                    "type": "array",
                    "description": "Products to place on the 800×600 px canvas.",
                    "items": {
                        "type": "object",
                        "properties": {
                            "productId": {"type": "integer", "description": "The EXACT productId from the catalog (shown as [ID:X]). Never invent an ID."},
                            "name":      {"type": "string"},
                            "x":         {"type": "integer", "description": "Left edge in px (0–720)."},
                            "y":         {"type": "integer", "description": "Top edge in px (0–520)."},
                            "width":     {"type": "integer", "description": "Width in px (60–300)."},
                            "height":    {"type": "integer", "description": "Height in px (50–250)."}
                        },
                        "required": ["productId", "name", "x", "y", "width", "height"]
                    }
                }
            },
            "required": ["reply", "items"]
        }
    }
}

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
            color_str = f", color: {p['color']}" if p.get('color') else ""
            cat = p.get('category', '')
            cat_str = f" [category: {cat}]" if cat else ""
            img_str = f" [imageURL: {p['imageURL']}]" if p.get('imageURL') else ""
            line = f"- [ID:{p['productId']}] {p['name']} (${p['price']}){color_str}{cat_str}{img_str} [{stock}]: {p.get('description', '')}"
            catalog_lines.append(line)
        catalog = "\n".join(catalog_lines)
        full_prompt = SYSTEM_PROMPT + f"\n\nAvailable products:\n{catalog}\n\nOnly recommend products from this list. ALWAYS use the exact productId shown as [ID:X].\nFor floors and walls: use their imageURL directly in floorImageURL / wallImageURL fields (do NOT add them to items[])."
    else:
        full_prompt = SYSTEM_PROMPT

    messages = [{"role": "system", "content": full_prompt}]
    for m in req.history:
        messages.append({"role": m.role, "content": m.content})
    messages.append({"role": "user", "content": req.message})

    try:
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            tools=[CANVAS_TOOL],
            tool_choice="auto",
            max_tokens=600,
            temperature=0.6
        )
        msg = response.choices[0].message

        # ── Function call → design the room ──────────────────
        if msg.tool_calls:
            args = json.loads(msg.tool_calls[0].function.arguments)

            # Build a product lookup by productId
            product_map = {p.get("productId", p.get("id")): p for p in req.products}

            canvas_actions = []
            for item in args.get("items", []):
                pid  = item["productId"]
                prod = product_map.get(pid, {})
                canvas_actions.append({
                    "id":        f"ai-{pid}-{item['x']}-{item['y']}",
                    "type":      "product",
                    "productId": pid,
                    "label":     item["name"],
                    "x":         item["x"],
                    "y":         item["y"],
                    "width":     item["width"],
                    "height":    item["height"],
                    "imageURL":  prod.get("imageURL") or prod.get("imageUrl"),
                    "price":     prod.get("price"),
                    "color":     prod.get("color"),
                })
            return {"reply": args["reply"], "canvasActions": canvas_actions, "floorImageURL": args.get("floorImageURL"), "wallImageURL": args.get("wallImageURL")}

        # ── Normal text reply ─────────────────────────────────
        return {"reply": msg.content, "canvasActions": None}

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
