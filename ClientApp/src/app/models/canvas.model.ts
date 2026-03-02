// -------------------------------------------------------------------
// CanvasItem - represents any item placed on the room canvas.
// 'wall' and 'floor' are special background-layer types (no drag/resize).
// -------------------------------------------------------------------
export interface CanvasItem {
  id: string;
  type: string;
  x: number;       // px from left of canvas
  y: number;       // px from top of canvas
  width: number;   // px
  height: number;  // px
  label: string;
  productId?: number;
  price?: number;
  color?: string;
  imageURL?: string;
}

// -------------------------------------------------------------------
// ResizeSession - tracks active resize operation
// -------------------------------------------------------------------
export interface ResizeSession {
  itemId: string;
  startMouseX: number;
  startMouseY: number;
  startWidth: number;
  startHeight: number;
}

// -------------------------------------------------------------------
// StaticEntry - wall / floor palette entry (not from API)
// -------------------------------------------------------------------
export interface StaticEntry {
  type: CanvasItem['type'];
  label: string;
  defaultWidth: number;
  defaultHeight: number;
}
