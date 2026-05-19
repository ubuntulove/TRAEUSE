import React, { useState, useEffect, useRef, useCallback } from 'react';

export interface GameHUD {
  id: string;
  type: HUDType;
  position: HUDPosition;
  elements: HUDElement[];
  visible: boolean;
  locked: boolean;
  opacity: number;
  scale: number;
  animations: HUDAnimation[];
  state: HUDState;
  layout: HUDLayout;
  themes: HUDTheme[];
  activeTheme: string;
  responsive: boolean;
  breakpoints: Breakpoint[];
}

export type HUDType =
  | 'classic'
  | 'modern'
  | 'minimal'
  | 'tactical'
  | 'immersive'
  | 'competitive'
  | 'custom';

export type HUDPosition =
  | 'top'
  | 'bottom'
  | 'left'
  | 'right'
  | 'center'
  | 'corners'
  | 'fullscreen';

export interface HUDElement {
  id: string;
  type: ElementType;
  component: React.ComponentType<ElementProps>;
  position: Position;
  size: Size;
  visible: boolean;
  interactive: boolean;
  children?: HUDElement[];
  data?: unknown;
  styling: ElementStyling;
  animations: ElementAnimation[];
  state: ElementState;
  accessibility: ElementAccessibility;
}

export type ElementType =
  | 'resource_bar'
  | 'unit_panel'
  | 'building_panel'
  | 'minimap'
  | 'command_bar'
  | 'production_queue'
  | 'tech_tree'
  | 'stats_panel'
  | 'alert_system'
  | 'chat_box'
  | 'team_panel'
  | 'score_board'
  | 'timer'
  | 'compass'
  | 'radar'
  | 'health_bar'
  | 'ability_bar'
  | 'inventory'
  | 'quest_log'
  | 'buff_debuff'
  | 'notification_center'
  | 'tooltip'
  | 'context_menu'
  | 'loading_bar'
  | 'progress_ring';

export interface Position {
  x: number;
  y: number;
  anchor: AnchorPoint;
  offset: { x: number; y: number };
  layer: number;
  zIndex: number;
}

export type AnchorPoint =
  | 'top-left'
  | 'top'
  | 'top-right'
  | 'left'
  | 'center'
  | 'right'
  | 'bottom-left'
  | 'bottom'
  | 'bottom-right'
  | 'stretch';

export interface Size {
  width: number | string;
  height: number | string;
  minWidth?: number;
  minHeight?: number;
  maxWidth?: number;
  maxHeight?: number;
  aspectRatio?: number;
}

export interface ElementStyling {
  background: BackgroundStyle;
  border: BorderStyle;
  shadow: ShadowStyle;
  text: TextStyle;
  spacing: SpacingStyle;
  transitions: TransitionStyle[];
  filters: FilterStyle[];
  transforms: TransformStyle[];
  animations: AnimationKeyframes[];
}

export interface BackgroundStyle {
  color?: string;
  gradient?: GradientStyle;
  image?: string;
  imageRepeat?: 'repeat' | 'no-repeat' | 'round' | 'space';
  imageSize?: 'auto' | 'cover' | 'contain' | Size;
  blendMode?: 'normal' | 'multiply' | 'screen' | 'overlay' | 'darken' | 'lighten';
  opacity?: number;
  blur?: number;
}

export interface GradientStyle {
  type: 'linear' | 'radial' | 'conic';
  angle?: number;
  stops: GradientStop[];
}

export interface GradientStop {
  offset: number;
  color: string;
  opacity?: number;
}

export interface BorderStyle {
  width: number;
  style: 'solid' | 'dashed' | 'dotted' | 'double' | 'groove' | 'ridge' | 'inset' | 'outset';
  color: string;
  radius?: number;
  topLeftRadius?: number;
  topRightRadius?: number;
  bottomLeftRadius?: number;
  bottomRightRadius?: number;
  image?: string;
  imageSlice?: number;
}

export interface ShadowStyle {
  enabled: boolean;
  offsetX: number;
  offsetY: number;
  blur: number;
  spread?: number;
  color: string;
  inset?: boolean;
  layers?: ShadowLayer[];
}

export interface ShadowLayer {
  offsetX: number;
  offsetY: number;
  blur: number;
  spread: number;
  color: string;
}

export interface TextStyle {
  fontFamily?: string;
  fontSize?: number;
  fontWeight?: number | string;
  fontStyle?: 'normal' | 'italic' | 'oblique';
  textAlign?: 'left' | 'center' | 'right' | 'justify';
  textDecoration?: 'none' | 'underline' | 'overline' | 'line-through';
  textColor?: string;
  textShadow?: ShadowStyle;
  textOverflow?: 'clip' | 'ellipsis' | 'fade';
  textWrap?: 'normal' | 'nowrap' | 'pre' | 'pre-wrap';
  lineHeight?: number;
  letterSpacing?: number;
  wordSpacing?: number;
  textTransform?: 'none' | 'uppercase' | 'lowercase' | 'capitalize';
}

export interface SpacingStyle {
  padding: SpacingValues;
  margin: SpacingValues;
  gap: number;
}

export interface SpacingValues {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export interface TransitionStyle {
  property: string;
  duration: number;
  easing: 'linear' | 'ease' | 'ease-in' | 'ease-out' | 'ease-in-out' | 'step-start' | 'step-end';
  delay?: number;
}

export interface FilterStyle {
  blur?: number;
  brightness?: number;
  contrast?: number;
  grayscale?: number;
  hueRotate?: number;
  invert?: number;
  opacity?: number;
  saturate?: number;
  sepia?: number;
  dropShadow?: ShadowStyle;
}

export interface TransformStyle {
  translateX?: number;
  translateY?: number;
  translateZ?: number;
  rotate?: number;
  rotateX?: number;
  rotateY?: number;
  rotateZ?: number;
  scale?: number;
  scaleX?: number;
  scaleY?: number;
  scaleZ?: number;
  skewX?: number;
  skewY?: number;
  perspective?: number;
  transformOrigin?: string;
  transformStyle?: 'flat' | 'preserve-3d';
}

export interface AnimationKeyframes {
  name: string;
  duration: number;
  iterationCount: number | 'infinite';
  direction?: 'normal' | 'reverse' | 'alternate' | 'alternate-reverse';
  fillMode?: 'none' | 'forwards' | 'backwards' | 'both';
  playState?: 'running' | 'paused';
  delay?: number;
  keyframes: Keyframe[];
}

export interface Keyframe {
  offset: number;
  properties: Record<string, unknown>;
}

export interface ElementAnimation {
  name: string;
  type: AnimationType;
  trigger: AnimationTrigger;
  duration: number;
  loop: boolean;
  autoPlay: boolean;
  properties: Record<string, unknown>;
  onStart?: () => void;
  onComplete?: () => void;
  onIteration?: () => void;
}

export type AnimationType =
  | 'fade'
  | 'slide'
  | 'scale'
  | 'rotate'
  | 'bounce'
  | 'shake'
  | 'pulse'
  | 'glow'
  | 'flip'
  | 'hinge'
  | 'zoom'
  | 'slideUp'
  | 'slideDown'
  | 'slideLeft'
  | 'slideRight';

export type AnimationTrigger =
  | 'mount'
  | 'hover'
  | 'click'
  | 'focus'
  | 'active'
  | 'transition'
  | 'custom'
  | 'loop';

export interface ElementState {
  default: Partial<ElementStyling>;
  hover?: Partial<ElementStyling>;
  active?: Partial<ElementStyling>;
  disabled?: Partial<ElementStyling>;
  focused?: Partial<ElementStyling>;
  expanded?: Partial<ElementStyling>;
  collapsed?: Partial<ElementStyling>;
  loading?: Partial<ElementStyling>;
  error?: Partial<ElementStyling>;
  success?: Partial<ElementStyling>;
  warning?: Partial<ElementStyling>;
}

export interface ElementAccessibility {
  ariaLabel?: string;
  ariaDescription?: string;
  role?: string;
  tabIndex?: number;
  focusable?: boolean;
  screenReaderOnly?: boolean;
  keyboardShortcuts?: string[];
}

export interface ElementProps {
  element: HUDElement;
  data?: unknown;
  onAction?: (action: HUDAction) => void;
  style?: React.CSSProperties;
  className?: string;
}

export interface HUDAction {
  type: ActionType;
  elementId: string;
  payload?: unknown;
}

export type ActionType =
  | 'click'
  | 'hover'
  | 'focus'
  | 'change'
  | 'drag'
  | 'resize'
  | 'minimize'
  | 'maximize'
  | 'close'
  | 'custom';

export interface HUDAnimation {
  name: string;
  type: AnimationType;
  trigger: AnimationTrigger;
  duration: number;
  loop: boolean;
  autoPlay: boolean;
}

export interface HUDState {
  mode: HUDMode;
  theme: string;
  scale: number;
  locked: boolean;
  visible: boolean;
  editing: boolean;
  customizations: Record<string, unknown>;
}

export type HUDMode =
  | 'gameplay'
  | 'menu'
  | 'settings'
  | 'inventory'
  | 'map'
  | 'spectator'
  | 'debug';

export interface HUDLayout {
  grid: GridLayout;
  regions: LayoutRegion[];
  widgets: WidgetPlacement[];
}

export interface GridLayout {
  enabled: boolean;
  columns: number;
  rows: number;
  cellSize: Size;
  gap: number;
  snapToGrid: boolean;
  showGrid: boolean;
}

export interface LayoutRegion {
  id: string;
  name: string;
  position: Position;
  size: Size;
  elements: string[];
  collapsible: boolean;
  collapsed: boolean;
}

export interface WidgetPlacement {
  widgetId: string;
  regionId: string;
  position: Position;
  size: Size;
  order: number;
  visible: boolean;
}

export interface HUDTheme {
  id: string;
  name: string;
  colors: ThemeColors;
  fonts: ThemeFonts;
  spacing: ThemeSpacing;
  effects: ThemeEffects;
  animations: ThemeAnimations;
}

export interface ThemeColors {
  primary: string;
  secondary: string;
  accent: string;
  background: string;
  backgroundAlt: string;
  foreground: string;
  text: string;
  textSecondary: string;
  border: string;
  success: string;
  warning: string;
  error: string;
  info: string;
  resource1: string;
  resource2: string;
  resource3: string;
  team1: string;
  team2: string;
  team3: string;
  team4: string;
}

export interface ThemeFonts {
  primary: string;
  secondary: string;
  monospace: string;
  sizes: FontSizes;
}

export interface FontSizes {
  xs: number;
  sm: number;
  md: number;
  lg: number;
  xl: number;
  xxl: number;
}

export interface ThemeSpacing {
  xs: number;
  sm: number;
  md: number;
  lg: number;
  xl: number;
}

export interface ThemeEffects {
  shadows: boolean;
  gradients: boolean;
  animations: boolean;
  transitions: boolean;
  blur: boolean;
  glow: boolean;
}

export interface ThemeAnimations {
  speed: 'slow' | 'normal' | 'fast' | 'instant';
  easing: string;
  reducedMotion: boolean;
}

export interface Breakpoint {
  name: string;
  minWidth: number;
  maxWidth: number;
  scale: number;
  layout: Partial<HUDLayout>;
}

export interface ResourceData {
  credits: number;
  maxCredits: number;
  ore: number;
  maxOre: number;
  power: number;
  maxPower: number;
  Tiberium?: number;
  alloy?: number;
  energy?: number;
}

export interface UnitData {
  id: string;
  name: string;
  type: string;
  health: number;
  maxHealth: number;
  armor: number;
  speed: number;
  range: number;
  damage: number;
  abilities: Ability[];
  status: UnitStatus;
  selected: boolean;
  owned: boolean;
  team: number;
  position: { x: number; y: number };
}

export interface UnitStatus {
  moving: boolean;
  attacking: boolean;
  harvesting: boolean;
  constructing: boolean;
  idle: boolean;
  stunned: boolean;
  cloaked: boolean;
  loaded: boolean;
  garrisoned: boolean;
}

export interface Ability {
  id: string;
  name: string;
  icon: string;
  hotkey: string;
  cooldown: number;
  remainingCooldown: number;
  energyCost: number;
  available: boolean;
}

export interface BuildingData {
  id: string;
  name: string;
  type: string;
  health: number;
  maxHealth: number;
  power: number;
  producing: string | null;
  productionProgress: number;
  queue: ProductionItem[];
  rallyPoint?: { x: number; y: number };
  active: boolean;
  powered: boolean;
  damaged: boolean;
}

export interface ProductionItem {
  id: string;
  type: string;
  name: string;
  progress: number;
  cost: number;
  timeRemaining: number;
}

export interface AlertData {
  id: string;
  type: AlertType;
  title: string;
  message: string;
  icon?: string;
  timestamp: number;
  duration?: number;
  dismissible: boolean;
  action?: AlertAction;
  priority: number;
  category: AlertCategory;
}

export type AlertType =
  | 'info'
  | 'success'
  | 'warning'
  | 'error'
  | 'critical';

export type AlertCategory =
  | 'combat'
  | 'economy'
  | 'production'
  | 'technology'
  | 'social'
  | 'system';

export interface AlertAction {
  label: string;
  onClick: () => void;
  icon?: string;
}

export interface MinimapData {
  width: number;
  height: number;
  zoom: number;
  centerX: number;
  centerY: number;
  revealedTiles: boolean[][];
  units: MinimapUnit[];
  buildings: MinimapBuilding[];
  terrain: MinimapTerrain[];
  fogOfWar: boolean;
  showGrid: boolean;
  showPaths: boolean;
}

export interface MinimapUnit {
  id: string;
  x: number;
  y: number;
  type: 'infantry' | 'vehicle' | 'aircraft' | 'naval';
  team: number;
  selected: boolean;
}

export interface MinimapBuilding {
  id: string;
  x: number;
  y: number;
  type: string;
  team: number;
  damaged: boolean;
}

export interface MinimapTerrain {
  x: number;
  y: number;
  type: 'land' | 'water' | 'impassable';
}

export class HUDManager {
  private hud: GameHUD;
  private elementRefs: Map<string, React.RefObject<HTMLElement>> = new Map();
  private listeners: Map<string, Set<Function>> = new Map();
  private state: Map<string, unknown> = new Map();
  private contextMenu: ContextMenuState | null = null;
  private tooltip: TooltipState | null = null;
  private dragState: DragState | null = null;
  private resizeState: ResizeState | null = null;
  private history: HUDHistory = { past: [], future: [] };
  private maxHistorySize: number = 50;
  private activeBreakpoint: Breakpoint | null = null;

  constructor(type: HUDType = 'modern') {
    this.hud = this.createDefaultHUD(type);
    this.setupDefaultElements();
  }

  private createDefaultHUD(type: HUDType): GameHUD {
    return {
      id: `hud_${Date.now()}`,
      type,
      position: 'corners',
      elements: [],
      visible: true,
      locked: false,
      opacity: 1,
      scale: 1,
      animations: [],
      state: {
        mode: 'gameplay',
        theme: 'default',
        scale: 1,
        locked: false,
        visible: true,
        editing: false,
        customizations: {}
      },
      layout: {
        grid: {
          enabled: false,
          columns: 12,
          rows: 8,
          cellSize: { width: 50, height: 50 },
          gap: 10,
          snapToGrid: true,
          showGrid: false
        },
        regions: [],
        widgets: []
      },
      themes: [],
      activeTheme: 'default',
      responsive: true,
      breakpoints: [
        { name: 'mobile', minWidth: 0, maxWidth: 767, scale: 0.7, layout: {} },
        { name: 'tablet', minWidth: 768, maxWidth: 1023, scale: 0.85, layout: {} },
        { name: 'desktop', minWidth: 1024, maxWidth: Infinity, scale: 1, layout: {} }
      ]
    };
  }

  private setupDefaultElements(): void {
    this.addElement({
      id: 'resource_bar',
      type: 'resource_bar',
      component: ResourceBar,
      position: { x: 0, y: 0, anchor: 'top-left', offset: { x: 0, y: 0 }, layer: 1, zIndex: 100 },
      size: { width: 300, height: 60 },
      visible: true,
      interactive: false,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: {
        default: {},
        loading: { background: { color: '#333' } },
        error: { background: { color: '#550000' } }
      },
      accessibility: { ariaLabel: 'Resource Bar' }
    });

    this.addElement({
      id: 'minimap',
      type: 'minimap',
      component: Minimap,
      position: { x: 0, y: 0, anchor: 'bottom-right', offset: { x: 20, y: 20 }, layer: 1, zIndex: 100 },
      size: { width: 250, height: 250 },
      visible: true,
      interactive: true,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Minimap' }
    });

    this.addElement({
      id: 'command_bar',
      type: 'command_bar',
      component: CommandBar,
      position: { x: 0, y: 0, anchor: 'bottom', offset: { x: 0, y: -80 }, layer: 1, zIndex: 100 },
      size: { width: 600, height: 70 },
      visible: true,
      interactive: true,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Command Bar' }
    });

    this.addElement({
      id: 'unit_panel',
      type: 'unit_panel',
      component: UnitPanel,
      position: { x: 0, y: 0, anchor: 'bottom-left', offset: { x: 20, y: -150 }, layer: 1, zIndex: 100 },
      size: { width: 200, height: 120 },
      visible: true,
      interactive: true,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Unit Information Panel' }
    });

    this.addElement({
      id: 'alert_system',
      type: 'alert_system',
      component: AlertSystem,
      position: { x: 0, y: 0, anchor: 'top-right', offset: { x: 20, y: 80 }, layer: 2, zIndex: 200 },
      size: { width: 350, height: 'auto' },
      visible: true,
      interactive: false,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Alert Notifications' }
    });

    this.addElement({
      id: 'timer',
      type: 'timer',
      component: GameTimer,
      position: { x: 0, y: 0, anchor: 'top', offset: { x: 0, y: 0 }, layer: 1, zIndex: 100 },
      size: { width: 150, height: 40 },
      visible: true,
      interactive: false,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Game Timer' }
    });

    this.addElement({
      id: 'score_board',
      type: 'score_board',
      component: ScoreBoard,
      position: { x: 0, y: 0, anchor: 'top', offset: { x: 0, y: 0 }, layer: 1, zIndex: 100 },
      size: { width: 400, height: 50 },
      visible: false,
      interactive: true,
      styling: this.getDefaultElementStyling(),
      animations: [],
      state: { default: {} },
      accessibility: { ariaLabel: 'Score Board' }
    });
  }

  private getDefaultElementStyling(): ElementStyling {
    return {
      background: {
        color: 'rgba(20, 20, 20, 0.85)',
        gradient: {
          type: 'linear',
          angle: 180,
          stops: [
            { offset: 0, color: 'rgba(40, 40, 40, 0.9)' },
            { offset: 100, color: 'rgba(20, 20, 20, 0.85)' }
          ]
        }
      },
      border: {
        width: 1,
        style: 'solid',
        color: 'rgba(255, 255, 255, 0.1)',
        radius: 4
      },
      shadow: {
        enabled: true,
        offsetX: 0,
        offsetY: 2,
        blur: 8,
        color: 'rgba(0, 0, 0, 0.5)'
      },
      text: {
        fontFamily: 'Roboto, sans-serif',
        fontSize: 14,
        textColor: '#ffffff',
        textAlign: 'left'
      },
      spacing: {
        padding: { top: 8, right: 12, bottom: 8, left: 12 },
        margin: { top: 0, right: 0, bottom: 0, left: 0 },
        gap: 8
      },
      transitions: [
        { property: 'all', duration: 200, easing: 'ease-out' }
      ],
      filters: [],
      transforms: [],
      animations: []
    };
  }

  addElement(element: HUDElement): void {
    this.hud.elements.push(element);
    this.emit('element_added', { element });
  }

  removeElement(elementId: string): boolean {
    const index = this.hud.elements.findIndex(e => e.id === elementId);
    if (index !== -1) {
      const element = this.hud.elements[index];
      this.hud.elements.splice(index, 1);
      this.elementRefs.delete(elementId);
      this.emit('element_removed', { elementId });
      return true;
    }
    return false;
  }

  getElement(elementId: string): HUDElement | undefined {
    return this.hud.elements.find(e => e.id === elementId);
  }

  getAllElements(): HUDElement[] {
    return [...this.hud.elements];
  }

  updateElement(elementId: string, updates: Partial<HUDElement>): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    Object.assign(element, updates);
    this.emit('element_updated', { elementId, updates });
    return true;
  }

  setElementVisibility(elementId: string, visible: boolean): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    element.visible = visible;
    this.emit('visibility_changed', { elementId, visible });
    return true;
  }

  setElementPosition(elementId: string, position: Position): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    element.position = position;
    this.emit('position_changed', { elementId, position });
    return true;
  }

  setElementSize(elementId: string, size: Size): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    element.size = size;
    this.emit('size_changed', { elementId, size });
    return true;
  }

  setElementStyling(elementId: string, styling: Partial<ElementStyling>): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    element.styling = { ...element.styling, ...styling };
    this.emit('styling_changed', { elementId, styling });
    return true;
  }

  setElementData(elementId: string, data: unknown): boolean {
    const element = this.getElement(elementId);
    if (!element) return false;

    element.data = data;
    this.emit('data_changed', { elementId, data });
    return true;
  }

  showContextMenu(x: number, y: number, items: ContextMenuItem[]): void {
    this.contextMenu = {
      x,
      y,
      items,
      visible: true
    };
    this.emit('context_menu_show', { x, y, items });
  }

  hideContextMenu(): void {
    if (this.contextMenu) {
      this.contextMenu.visible = false;
      this.emit('context_menu_hide', {});
      this.contextMenu = null;
    }
  }

  showTooltip(x: number, y: number, content: TooltipContent): void {
    this.tooltip = {
      x,
      y,
      content,
      visible: true
    };
    this.emit('tooltip_show', { x, y, content });
  }

  hideTooltip(): void {
    if (this.tooltip) {
      this.tooltip.visible = false;
      this.emit('tooltip_hide', {});
      this.tooltip = null;
    }
  }

  startDrag(elementId: string, startX: number, startY: number): void {
    const element = this.getElement(elementId);
    if (!element || !element.interactive) return;

    this.dragState = {
      elementId,
      startX,
      startY,
      currentX: startX,
      currentY: startY,
      offsetX: startX - element.position.x,
      offsetY: startY - element.position.y,
      active: true
    };

    this.emit('drag_start', { elementId, startX, startY });
  }

  updateDrag(currentX: number, currentY: number): void {
    if (!this.dragState || !this.dragState.active) return;

    this.dragState.currentX = currentX;
    this.dragState.currentY = currentY;

    const element = this.getElement(this.dragState.elementId);
    if (element) {
      element.position.x = currentX - this.dragState.offsetX;
      element.position.y = currentY - this.dragState.offsetY;
    }

    this.emit('drag_update', {
      elementId: this.dragState.elementId,
      currentX,
      currentY
    });
  }

  endDrag(): void {
    if (!this.dragState) return;

    this.emit('drag_end', {
      elementId: this.dragState.elementId,
      finalX: this.dragState.currentX,
      finalY: this.dragState.currentY
    });

    this.dragState = null;
  }

  startResize(elementId: string, handle: ResizeHandle, startX: number, startY: number): void {
    const element = this.getElement(elementId);
    if (!element || !element.interactive) return;

    this.resizeState = {
      elementId,
      handle,
      startX,
      startY,
      startWidth: typeof element.size.width === 'number' ? element.size.width : 100,
      startHeight: typeof element.size.height === 'number' ? element.size.height : 100,
      currentWidth: 0,
      currentHeight: 0,
      active: true
    };

    this.emit('resize_start', { elementId, handle, startX, startY });
  }

  updateResize(currentX: number, currentY: number): void {
    if (!this.resizeState || !this.resizeState.active) return;

    const deltaX = currentX - this.resizeState.startX;
    const deltaY = currentY - this.resizeState.startY;

    const element = this.getElement(this.resizeState.elementId);
    if (!element) return;

    switch (this.resizeState.handle) {
      case 'e':
        element.size.width = this.resizeState.startWidth + deltaX;
        break;
      case 'w':
        element.size.width = this.resizeState.startWidth - deltaX;
        break;
      case 's':
        element.size.height = this.resizeState.startHeight + deltaY;
        break;
      case 'n':
        element.size.height = this.resizeState.startHeight - deltaY;
        break;
      case 'se':
        element.size.width = this.resizeState.startWidth + deltaX;
        element.size.height = this.resizeState.startHeight + deltaY;
        break;
      case 'sw':
        element.size.width = this.resizeState.startWidth - deltaX;
        element.size.height = this.resizeState.startHeight + deltaY;
        break;
      case 'ne':
        element.size.width = this.resizeState.startWidth + deltaX;
        element.size.height = this.resizeState.startHeight - deltaY;
        break;
      case 'nw':
        element.size.width = this.resizeState.startWidth - deltaX;
        element.size.height = this.resizeState.startHeight - deltaY;
        break;
    }

    this.emit('resize_update', {
      elementId: this.resizeState.elementId,
      currentWidth: element.size.width,
      currentHeight: element.size.height
    });
  }

  endResize(): void {
    if (!this.resizeState) return;

    this.emit('resize_end', {
      elementId: this.resizeState.elementId,
      finalWidth: this.resizeState.currentWidth,
      finalHeight: this.resizeState.currentHeight
    });

    this.resizeState = null;
  }

  undo(): void {
    if (this.history.past.length === 0) return;

    const previousState = this.history.past.pop()!;
    this.history.future.push(this.cloneHUDState());

    this.applyHUDState(previousState);
    this.emit('undo', { previousState });
  }

  redo(): void {
    if (this.history.future.length === 0) return;

    const nextState = this.history.future.pop()!;
    this.history.past.push(this.cloneHUDState());

    this.applyHUDState(nextState);
    this.emit('redo', { nextState });
  }

  private saveToHistory(): void {
    this.history.past.push(this.cloneHUDState());
    this.history.future = [];

    if (this.history.past.length > this.maxHistorySize) {
      this.history.past.shift();
    }
  }

  private cloneHUDState(): GameHUD {
    return JSON.parse(JSON.stringify(this.hud));
  }

  private applyHUDState(state: GameHUD): void {
    this.hud = state;
    this.emit('state_applied', {});
  }

  setMode(mode: HUDMode): void {
    if (this.hud.state.mode === mode) return;

    this.saveToHistory();
    this.hud.state.mode = mode;

    this.hud.elements.forEach(element => {
      switch (mode) {
        case 'gameplay':
          element.visible = element.id !== 'score_board';
          break;
        case 'menu':
          element.visible = false;
          break;
        case 'inventory':
          element.visible = ['inventory', 'resource_bar', 'alert_system'].includes(element.id);
          break;
        case 'map':
          element.visible = ['minimap', 'resource_bar'].includes(element.id);
          break;
        case 'spectator':
          element.visible = !['command_bar', 'unit_panel'].includes(element.id);
          break;
      }
    });

    this.emit('mode_changed', { mode });
  }

  toggleEditMode(): void {
    this.hud.state.editing = !this.hud.state.editing;
    this.emit('edit_mode_toggled', { editing: this.hud.state.editing });
  }

  lock(): void {
    this.hud.locked = true;
    this.emit('hud_locked', {});
  }

  unlock(): void {
    this.hud.locked = false;
    this.emit('hud_unlocked', {});
  }

  setScale(scale: number): void {
    this.hud.scale = Math.max(0.5, Math.min(2, scale));
    this.hud.state.scale = this.hud.scale;
    this.emit('scale_changed', { scale: this.hud.scale });
  }

  setOpacity(opacity: number): void {
    this.hud.opacity = Math.max(0, Math.min(1, opacity));
    this.emit('opacity_changed', { opacity: this.hud.opacity });
  }

  show(): void {
    this.hud.visible = true;
    this.hud.state.visible = true;
    this.emit('hud_shown', {});
  }

  hide(): void {
    this.hud.visible = false;
    this.hud.state.visible = false;
    this.emit('hud_hidden', {});
  }

  toggle(): void {
    if (this.hud.visible) {
      this.hide();
    } else {
      this.show();
    }
  }

  addTheme(theme: HUDTheme): void {
    this.hud.themes.push(theme);
    this.emit('theme_added', { theme });
  }

  removeTheme(themeId: string): boolean {
    const index = this.hud.themes.findIndex(t => t.id === themeId);
    if (index !== -1) {
      this.hud.themes.splice(index, 1);
      this.emit('theme_removed', { themeId });
      return true;
    }
    return false;
  }

  applyTheme(themeId: string): boolean {
    const theme = this.hud.themes.find(t => t.id === themeId);
    if (!theme) return false;

    this.hud.activeTheme = themeId;
    this.hud.state.theme = themeId;

    this.hud.elements.forEach(element => {
      if (theme.colors) {
        element.styling.background.color = theme.colors.background;
        element.styling.text.textColor = theme.colors.text;
        element.styling.border.color = theme.colors.border;
      }
    });

    this.emit('theme_applied', { themeId });
    return true;
  }

  addBreakpoint(breakpoint: Breakpoint): void {
    this.hud.breakpoints.push(breakpoint);
    this.emit('breakpoint_added', { breakpoint });
  }

  updateBreakpoints(): void {
    const width = window.innerWidth;
    let matching: Breakpoint | null = null;

    for (const breakpoint of this.hud.breakpoints) {
      if (width >= breakpoint.minWidth && width <= breakpoint.maxWidth) {
        matching = breakpoint;
        break;
      }
    }

    if (matching && matching !== this.activeBreakpoint) {
      const previousBreakpoint = this.activeBreakpoint;
      this.activeBreakpoint = matching;

      if (matching.scale !== undefined) {
        this.setScale(matching.scale);
      }

      this.emit('breakpoint_changed', {
        previous: previousBreakpoint?.name,
        current: matching.name
      });
    }
  }

  getState(key: string): unknown {
    return this.state.get(key);
  }

  setState(key: string, value: unknown): void {
    this.state.set(key, value);
    this.emit('state_changed', { key, value });
  }

  registerElementRef(elementId: string, ref: React.RefObject<HTMLElement>): void {
    this.elementRefs.set(elementId, ref);
  }

  getElementRef(elementId: string): React.RefObject<HTMLElement> | undefined {
    return this.elementRefs.get(elementId);
  }

  triggerAction(action: HUDAction): void {
    this.emit('action_triggered', action);
  }

  getHUD(): GameHUD {
    return this.hud;
  }

  exportLayout(): string {
    return JSON.stringify(this.hud, null, 2);
  }

  importLayout(layoutJson: string): boolean {
    try {
      const layout = JSON.parse(layoutJson);
      this.applyHUDState(layout);
      this.emit('layout_imported', {});
      return true;
    } catch (error) {
      console.error('Failed to import layout:', error);
      return false;
    }
  }

  resetToDefault(type?: HUDType): void {
    this.hud = this.createDefaultHUD(type || this.hud.type);
    this.setupDefaultElements();
    this.emit('reset_to_default', { type: type || this.hud.type });
  }

  on(event: string, callback: Function): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function): void {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: unknown): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(cb => cb(data));
    }
  }
}

export interface ContextMenuState {
  x: number;
  y: number;
  items: ContextMenuItem[];
  visible: boolean;
}

export interface ContextMenuItem {
  id: string;
  label: string;
  icon?: string;
  disabled?: boolean;
  divider?: boolean;
  shortcut?: string;
  submenu?: ContextMenuItem[];
  onClick?: () => void;
}

export interface TooltipState {
  x: number;
  y: number;
  content: TooltipContent;
  visible: boolean;
}

export interface TooltipContent {
  title?: string;
  text: string;
  icon?: string;
  image?: string;
  stats?: TooltipStat[];
  actions?: TooltipAction[];
}

export interface TooltipStat {
  label: string;
  value: string;
  color?: string;
}

export interface TooltipAction {
  label: string;
  onClick: () => void;
}

export interface DragState {
  elementId: string;
  startX: number;
  startY: number;
  currentX: number;
  currentY: number;
  offsetX: number;
  offsetY: number;
  active: boolean;
}

export interface ResizeState {
  elementId: string;
  handle: ResizeHandle;
  startX: number;
  startY: number;
  startWidth: number;
  startHeight: number;
  currentWidth: number;
  currentHeight: number;
  active: boolean;
}

export type ResizeHandle = 'n' | 's' | 'e' | 'w' | 'ne' | 'nw' | 'se' | 'sw';

export interface HUDHistory {
  past: GameHUD[];
  future: GameHUD[];
}

const ResourceBar: React.FC<ElementProps> = ({ data }) => {
  const resources = data as ResourceData;

  return (
    <div className="flex items-center gap-4 px-4 py-2 bg-black/80 rounded-lg border border-white/10">
      <div className="flex items-center gap-2">
        <div className="w-4 h-4 bg-yellow-500 rounded" />
        <span className="text-yellow-400 font-mono">{resources.credits}</span>
      </div>
      <div className="flex items-center gap-2">
        <div className="w-4 h-4 bg-amber-700 rounded" />
        <span className="text-amber-600 font-mono">{resources.ore}</span>
      </div>
      <div className="flex items-center gap-2">
        <div className="w-4 h-4 bg-blue-500 rounded" />
        <span className="text-blue-400 font-mono">{resources.power}/{resources.maxPower}</span>
      </div>
    </div>
  );
};

const Minimap: React.FC<ElementProps> = ({ data, onAction }) => {
  const minimap = data as MinimapData;

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * minimap.width;
    const y = ((e.clientY - rect.top) / rect.height) * minimap.height;
    onAction?.({ type: 'click', elementId: 'minimap', payload: { x, y } });
  };

  return (
    <div
      className="relative bg-gray-900 rounded-lg border border-white/10 overflow-hidden cursor-pointer"
      onClick={handleClick}
    >
      <div
        className="absolute inset-0"
        style={{ backgroundSize: 'cover' }}
      />
      {minimap.units.map(unit => (
        <div
          key={unit.id}
          className={`absolute w-2 h-2 rounded-full ${
            unit.team === 1 ? 'bg-blue-500' : 'bg-red-500'
          } ${unit.selected ? 'ring-2 ring-white' : ''}`}
          style={{
            left: `${(unit.x / minimap.width) * 100}%`,
            top: `${(unit.y / minimap.height) * 100}%`
          }}
        />
      ))}
      <div
        className="absolute border-2 border-white/50 pointer-events-none"
        style={{
          left: '25%',
          top: '25%',
          width: '50%',
          height: '50%'
        }}
      />
    </div>
  );
};

const CommandBar: React.FC<ElementProps> = ({ data, onAction }) => {
  const commands = (data?.commands || []) as CommandButton[];

  return (
    <div className="flex items-center gap-2 px-4 py-2 bg-black/80 rounded-lg border border-white/10">
      {commands.map((cmd, index) => (
        <button
          key={cmd.id || index}
          className={`w-12 h-12 flex items-center justify-center rounded border transition-all ${
            cmd.disabled
              ? 'bg-gray-700 border-gray-600 opacity-50 cursor-not-allowed'
              : 'bg-gray-800 border-gray-600 hover:bg-gray-700 hover:border-gray-500'
          }`}
          disabled={cmd.disabled}
          onClick={() => onAction?.({ type: 'click', elementId: 'command_bar', payload: cmd })}
          title={cmd.hotkey ? `${cmd.label} (${cmd.hotkey})` : cmd.label}
        >
          {cmd.icon}
        </button>
      ))}
    </div>
  );
};

interface CommandButton {
  id?: string;
  label: string;
  icon: React.ReactNode;
  hotkey?: string;
  disabled?: boolean;
}

const UnitPanel: React.FC<ElementProps> = ({ data }) => {
  const units = (data?.units || []) as UnitData[];

  if (units.length === 0) {
    return (
      <div className="w-full h-full flex items-center justify-center bg-black/80 rounded-lg border border-white/10">
        <span className="text-gray-500 text-sm">No units selected</span>
      </div>
    );
  }

  const primaryUnit = units[0];

  return (
    <div className="w-full h-full bg-black/80 rounded-lg border border-white/10 p-3">
      <div className="flex items-center gap-3 mb-2">
        <div className="w-16 h-16 bg-gray-700 rounded flex items-center justify-center">
          <span className="text-2xl">{data?.icon || '⚔️'}</span>
        </div>
        <div className="flex-1">
          <h3 className="text-white font-medium">{primaryUnit.name}</h3>
          <p className="text-gray-400 text-xs">{primaryUnit.type}</p>
        </div>
      </div>
      <div className="mb-2">
        <div className="flex justify-between text-xs mb-1">
          <span className="text-gray-400">Health</span>
          <span className="text-white">{primaryUnit.health}/{primaryUnit.maxHealth}</span>
        </div>
        <div className="h-2 bg-gray-700 rounded overflow-hidden">
          <div
            className="h-full bg-green-500 transition-all"
            style={{ width: `${(primaryUnit.health / primaryUnit.maxHealth) * 100}%` }}
          />
        </div>
      </div>
      {units.length > 1 && (
        <div className="text-xs text-gray-400">
          +{units.length - 1} more selected
        </div>
      )}
    </div>
  );
};

const AlertSystem: React.FC<ElementProps> = ({ data }) => {
  const alerts = (data?.alerts || []) as AlertData[];

  if (alerts.length === 0) return null;

  return (
    <div className="flex flex-col gap-2">
      {alerts.slice(0, 5).map(alert => (
        <div
          key={alert.id}
          className={`px-4 py-3 rounded-lg border ${
            alert.type === 'error' || alert.type === 'critical'
              ? 'bg-red-900/80 border-red-700'
              : alert.type === 'warning'
              ? 'bg-yellow-900/80 border-yellow-700'
              : alert.type === 'success'
              ? 'bg-green-900/80 border-green-700'
              : 'bg-blue-900/80 border-blue-700'
          }`}
        >
          <div className="flex items-start gap-2">
            <span className="text-lg">{alert.icon || 'ℹ️'}</span>
            <div className="flex-1">
              <h4 className="text-white font-medium text-sm">{alert.title}</h4>
              <p className="text-gray-300 text-xs">{alert.message}</p>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

const GameTimer: React.FC<ElementProps> = ({ data }) => {
  const time = (data?.time as number) || 0;
  const minutes = Math.floor(time / 60000);
  const seconds = Math.floor((time % 60000) / 1000);

  return (
    <div className="flex items-center justify-center px-4 py-2 bg-black/80 rounded-lg border border-white/10">
      <span className="text-white font-mono text-lg">
        {String(minutes).padStart(2, '0')}:{String(seconds).padStart(2, '0')}
      </span>
    </div>
  );
};

const ScoreBoard: React.FC<ElementProps> = ({ data }) => {
  const scores = (data?.scores || []) as TeamScore[];

  return (
    <div className="flex items-center gap-4 px-4 py-2 bg-black/80 rounded-lg border border-white/10">
      {scores.map((team, index) => (
        <div key={team.teamId} className="flex items-center gap-2">
          <div
            className="w-4 h-4 rounded"
            style={{ backgroundColor: team.color }}
          />
          <span className="text-white font-mono">{team.score}</span>
          {index < scores.length - 1 && (
            <span className="text-gray-500">vs</span>
          )}
        </div>
      ))}
    </div>
  );
};

interface TeamScore {
  teamId: number;
  teamName: string;
  score: number;
  color: string;
}

export const useHUD = (manager: HUDManager) => {
  const [elements, setElements] = useState<HUDElement[]>(manager.getAllElements());
  const [visible, setVisible] = useState(manager.getHUD().visible);
  const [scale, setScale] = useState(manager.getHUD().scale);

  useEffect(() => {
    const updateElements = () => {
      setElements(manager.getAllElements());
      setVisible(manager.getHUD().visible);
      setScale(manager.getHUD().scale);
    };

    manager.on('element_added', updateElements);
    manager.on('element_removed', updateElements);
    manager.on('element_updated', updateElements);
    manager.on('visibility_changed', updateElements);
    manager.on('scale_changed', updateElements);

    return () => {
      manager.off('element_added', updateElements);
      manager.off('element_removed', updateElements);
      manager.off('element_updated', updateElements);
      manager.off('visibility_changed', updateElements);
      manager.off('scale_changed', updateElements);
    };
  }, [manager]);

  return {
    elements,
    visible,
    scale,
    manager
  };
};

export const HUDContextMenu: React.FC<{
  state: ContextMenuState | null;
  onSelect: (item: ContextMenuItem) => void;
  onClose: () => void;
}> = ({ state, onSelect, onClose }) => {
  useEffect(() => {
    const handleClick = () => onClose();
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };

    if (state) {
      document.addEventListener('click', handleClick);
      document.addEventListener('keydown', handleEscape);
    }

    return () => {
      document.removeEventListener('click', handleClick);
      document.removeEventListener('keydown', handleEscape);
    };
  }, [state, onClose]);

  if (!state || !state.visible) return null;

  return (
    <div
      className="fixed bg-gray-900 border border-gray-700 rounded-lg shadow-xl py-1 min-w-[200px] z-50"
      style={{ left: state.x, top: state.y }}
      onClick={e => e.stopPropagation()}
    >
      {state.items.map(item => (
        <React.Fragment key={item.id}>
          {item.divider ? (
            <div className="border-t border-gray-700 my-1" />
          ) : (
            <button
              className={`w-full px-4 py-2 text-left text-sm flex items-center gap-3 ${
                item.disabled
                  ? 'text-gray-500 cursor-not-allowed'
                  : 'text-white hover:bg-gray-800'
              }`}
              disabled={item.disabled}
              onClick={() => !item.disabled && onSelect(item)}
            >
              {item.icon && <span>{item.icon}</span>}
              <span className="flex-1">{item.label}</span>
              {item.shortcut && (
                <span className="text-gray-500 text-xs">{item.shortcut}</span>
              )}
            </button>
          )}
        </React.Fragment>
      ))}
    </div>
  );
};

export const HUDTooltip: React.FC<{
  state: TooltipState | null;
  onClose: () => void;
}> = ({ state, onClose }) => {
  useEffect(() => {
    const handleMouseLeave = () => onClose();
    const timeout = setTimeout(onClose, 5000);

    return () => {
      clearTimeout(timeout);
    };
  }, [state, onClose]);

  if (!state || !state.visible) return null;

  return (
    <div
      className="fixed bg-gray-900 border border-gray-700 rounded-lg shadow-xl p-3 max-w-sm z-50"
      style={{ left: state.x + 10, top: state.y + 10 }}
    >
      {state.content.title && (
        <h4 className="text-white font-medium mb-2">{state.content.title}</h4>
      )}
      <p className="text-gray-300 text-sm">{state.content.text}</p>
      {state.content.stats && (
        <div className="mt-2 pt-2 border-t border-gray-700">
          {state.content.stats.map((stat, i) => (
            <div key={i} className="flex justify-between text-sm">
              <span className="text-gray-400">{stat.label}</span>
              <span style={{ color: stat.color }}>{stat.value}</span>
            </div>
          ))}
        </div>
      )}
      {state.content.actions && (
        <div className="mt-2 pt-2 border-t border-gray-700 flex gap-2">
          {state.content.actions.map((action, i) => (
            <button
              key={i}
              className="px-3 py-1 bg-blue-600 text-white text-xs rounded hover:bg-blue-700"
              onClick={action.onClick}
            >
              {action.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export const hudManager = new HUDManager();
