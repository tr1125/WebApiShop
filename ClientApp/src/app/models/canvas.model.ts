// -------------------------------------------------------------------
// CanvasItem - represents any item placed on the room canvas.
// 'wall' and 'floor' are special background-layer types (no drag/resize).
// -------------------------------------------------------------------
export interface CanvasItem {
  id: string;
  type: 'sofa' | 'chair' | 'table' | 'rug' | 'wardrobe' | 'desk' | 'bed' | 'wall' | 'floor';
  x: number;       // px from left of canvas
  y: number;       // px from top of canvas
  width: number;   // px
  height: number;  // px
  label: string;
  emoji: string;
  productId?: number;
  price?: number;
  color?: string;
  imageURL?: string;
}
