import React, { useState, useCallback, useEffect, useRef } from 'react';

export interface UIScreen {
  id: string;
  name: string;
  type: ScreenType;
  visible: boolean;
  priority: number;
  transition?: ScreenTransition;
  children: UIElement[];
  onShow?: () => void;
  onHide?: () => void;
}

export type ScreenType =
  | 'main_menu'
  | 'settings'
  | 'pause'
  | 'loading'
  | 'game_over'
  | 'victory'
  | 'mission_briefing'
  | 'options'
  | 'keybindings'
  | 'audio_settings'
  | 'video_settings'
  | 'controls_settings'
  | 'accessibility'
  | 'credits'
  | 'encyclopedia'
  | 'tech_tree'
  | 'unit_viewer'
  | 'save_load'
  | 'replay_menu'
  | 'multiplayer_lobby'
  | 'chat'
  | 'notification'
  | 'tooltip'
  | 'custom';

export interface ScreenTransition {
  type: TransitionType;
  duration: number;
  easing: EasingFunction;
  properties: TransitionProperties;
}

export type TransitionType =
  | 'fade'
  | 'slide'
  | 'scale'
  | 'flip'
  | 'cube'
  | 'none';

export type EasingFunction =
  | 'linear'
  | 'easeIn'
  | 'easeOut'
  | 'easeInOut'
  | 'easeInQuad'
  | 'easeOutQuad'
  | 'easeInOutQuad'
  | 'easeInCubic'
  | 'easeOutCubic'
  | 'easeInOutCubic'
  | 'easeInElastic'
  | 'easeOutElastic'
  | 'easeInBounce';

export interface TransitionProperties {
  opacity?: number;
  translateX?: number;
  translateY?: number;
  scale?: number;
  rotateX?: number;
  rotateY?: number;
}

export interface UIElement {
  id: string;
  type: ElementType;
  position: Position;
  size: Size;
  style?: UIStyle;
  properties: Record<string, unknown>;
  children?: UIElement[];
  visible: boolean;
  enabled: boolean;
  interactions?: UIInteraction[];
  animations?: UIAnimation[];
  states?: UIState[];
  currentState?: string;
}

export type ElementType =
  | 'button'
  | 'text'
  | 'image'
  | 'slider'
  | 'checkbox'
  | 'dropdown'
  | 'input'
  | 'panel'
  | 'scroll_view'
  | 'list'
  | 'grid'
  | 'progress_bar'
  | 'tooltip'
  | 'modal'
  | 'context_menu'
  | 'tree_view'
  | 'table'
  | 'custom';

export interface Position {
  x: number;
  y: number;
  z?: number;
  anchor?: AnchorPoint;
  offset?: { x: number; y: number };
}

export interface Size {
  width: number | string;
  height: number | string;
  minWidth?: number;
  minHeight?: number;
  maxWidth?: number;
  maxHeight?: number;
}

export type AnchorPoint =
  | 'top_left'
  | 'top'
  | 'top_right'
  | 'left'
  | 'center'
  | 'right'
  | 'bottom_left'
  | 'bottom'
  | 'bottom_right'
  | 'stretch';

export interface UIStyle {
  background?: BackgroundStyle;
  border?: BorderStyle;
  text?: TextStyle;
  shadow?: ShadowStyle;
  transform?: TransformStyle;
  transition?: TransitionStyle;
  animation?: AnimationStyle;
  filter?: FilterStyle;
}

export interface BackgroundStyle {
  color?: string;
  gradient?: GradientStyle;
  image?: string;
  imageRepeat?: 'repeat' | 'no-repeat' | 'round' | 'space';
  imageSize?: 'auto' | 'cover' | 'contain' | Size;
  imagePosition?: Position;
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
  style: 'solid' | 'dashed' | 'dotted' | 'double';
  color: string;
  radius?: number;
  topLeftRadius?: number;
  topRightRadius?: number;
  bottomLeftRadius?: number;
  bottomRightRadius?: number;
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
}

export interface ShadowStyle {
  offsetX: number;
  offsetY: number;
  blur: number;
  spread?: number;
  color: string;
  inset?: boolean;
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
}

export interface TransitionStyle {
  property: string;
  duration: number;
  easing: EasingFunction;
  delay?: number;
}

export interface AnimationStyle {
  name: string;
  duration: number;
  iterationCount: number | 'infinite';
  direction?: 'normal' | 'reverse' | 'alternate' | 'alternate-reverse';
  fillMode?: 'none' | 'forwards' | 'backwards' | 'both';
  playState?: 'running' | 'paused';
  keyframes?: Keyframe[];
}

export interface Keyframe {
  offset: number;
  properties: Record<string, unknown>;
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

export interface UIInteraction {
  type: InteractionType;
  callback: (event: UIEvent) => void;
  condition?: () => boolean;
  throttle?: number;
  debounce?: number;
}

export type InteractionType =
  | 'click'
  | 'double_click'
  | 'hover'
  | 'mouse_down'
  | 'mouse_up'
  | 'mouse_enter'
  | 'mouse_leave'
  | 'focus'
  | 'blur'
  | 'key_down'
  | 'key_up'
  | 'drag_start'
  | 'drag'
  | 'drag_end'
  | 'scroll'
  | 'change'
  | 'submit';

export interface UIEvent {
  type: string;
  target: UIElement;
  currentTarget: UIElement;
  timestamp: number;
  data?: Record<string, unknown>;
  mouse?: MouseEventData;
  keyboard?: KeyboardEventData;
  touch?: TouchEventData;
}

export interface MouseEventData {
  x: number;
  y: number;
  button: number;
  buttons: number;
  wheelDelta: number;
}

export interface KeyboardEventData {
  key: string;
  code: string;
  altKey: boolean;
  ctrlKey: boolean;
  shiftKey: boolean;
  metaKey: boolean;
}

export interface TouchEventData {
  touches: { x: number; y: number; id: number }[];
  changedTouches: { x: number; y: number; id: number }[];
}

export interface UIAnimation {
  name: string;
  type: AnimationType;
  duration: number;
  loop: boolean;
  autoPlay: boolean;
  properties: Record<string, unknown>;
  onStart?: () => void;
  onComplete?: () => void;
  onIteration?: () => void;
}

export type AnimationType =
  | 'tween'
  | 'shake'
  | 'pulse'
  | 'bounce'
  | 'fade'
  | 'slide'
  | 'rotate'
  | 'scale'
  | 'color';

export interface UIState {
  name: string;
  style?: Partial<UIStyle>;
  properties?: Record<string, unknown>;
}

export interface SliderConfig {
  min: number;
  max: number;
  step: number;
  value: number;
  showValue: boolean;
  valueFormat?: (value: number) => string;
  trackStyle?: Partial<UIStyle>;
  thumbStyle?: Partial<UIStyle>;
  onChange?: (value: number) => void;
}

export interface ButtonConfig {
  text?: string;
  icon?: string;
  image?: string;
  variant?: ButtonVariant;
  size?: ButtonSize;
  disabled?: boolean;
  loading?: boolean;
  badge?: string | number;
  tooltip?: string;
  onClick?: () => void;
}

export type ButtonVariant =
  | 'primary'
  | 'secondary'
  | 'success'
  | 'danger'
  | 'warning'
  | 'info'
  | 'link'
  | 'outline';

export type ButtonSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

export interface DropdownConfig {
  options: DropdownOption[];
  value?: string;
  placeholder?: string;
  searchable?: boolean;
  multiple?: boolean;
  clearable?: boolean;
  onChange?: (value: string | string[]) => void;
}

export interface DropdownOption {
  value: string;
  label: string;
  disabled?: boolean;
  icon?: string;
  group?: string;
}

export interface CheckboxConfig {
  label?: string;
  checked: boolean;
  indeterminate?: boolean;
  disabled?: boolean;
  onChange?: (checked: boolean) => void;
}

export interface InputConfig {
  type?: InputType;
  placeholder?: string;
  value?: string;
  defaultValue?: string;
  disabled?: boolean;
  readonly?: boolean;
  maxLength?: number;
  min?: number;
  max?: number;
  step?: number;
  pattern?: string;
  autocomplete?: string;
  autocorrect?: string;
  spellcheck?: boolean;
  onChange?: (value: string) => void;
  onFocus?: () => void;
  onBlur?: () => void;
  onSubmit?: (value: string) => void;
}

export type InputType =
  | 'text'
  | 'password'
  | 'email'
  | 'number'
  | 'tel'
  | 'url'
  | 'search'
  | 'date'
  | 'time'
  | 'datetime'
  | 'color';

export interface ModalConfig {
  title?: string;
  content?: string | React.ReactNode;
  closable?: boolean;
  maskClosable?: boolean;
  footer?: React.ReactNode;
  size?: ModalSize;
  onClose?: () => void;
}

export type ModalSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';

export interface NotificationConfig {
  type?: NotificationType;
  title?: string;
  message: string;
  duration?: number;
  closable?: boolean;
  icon?: string;
  actions?: NotificationAction[];
  onClose?: () => void;
}

export type NotificationType =
  | 'info'
  | 'success'
  | 'warning'
  | 'error'
  | 'loading';

export interface NotificationAction {
  text: string;
  onClick?: () => void;
}

export interface TooltipConfig {
  content: string | React.ReactNode;
  position?: TooltipPosition;
  trigger?: TooltipTrigger;
  delay?: number;
  maxWidth?: number;
  arrow?: boolean;
}

export type TooltipPosition =
  | 'top'
  | 'bottom'
  | 'left'
  | 'right'
  | 'top-left'
  | 'top-right'
  | 'bottom-left'
  | 'bottom-right';

export type TooltipTrigger = 'hover' | 'click' | 'focus' | 'manual';

export interface ContextMenuConfig {
  items: ContextMenuItem[];
  position: Position;
  onClose?: () => void;
}

export interface ContextMenuItem {
  id: string;
  label?: string;
  icon?: string;
  disabled?: boolean;
  divider?: boolean;
  submenu?: ContextMenuItem[];
  onClick?: () => void;
}

export interface ToastConfig {
  message: string;
  type?: ToastType;
  duration?: number;
  position?: ToastPosition;
}

export type ToastType = 'default' | 'success' | 'error' | 'warning' | 'info';
export type ToastPosition =
  | 'top'
  | 'bottom'
  | 'left'
  | 'right'
  | 'top-left'
  | 'top-right'
  | 'bottom-left'
  | 'bottom-right'
  | 'center';

export interface LoadingConfig {
  type?: LoadingType;
  text?: string;
  progress?: number;
  fullscreen?: boolean;
}

export type LoadingType = 'spinner' | 'dots' | 'pulse' | 'bars' | 'circle';

export interface ProgressBarConfig {
  value: number;
  min?: number;
  max?: number;
  showLabel?: boolean;
  labelFormat?: (value: number, min: number, max: number) => string;
  animated?: boolean;
  striped?: boolean;
  variant?: ProgressVariant;
}

export type ProgressVariant = 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';

export class UIScreenManager {
  private screens: Map<string, UIScreen> = new Map();
  private activeScreens: Set<string> = new Set();
  private screenHistory: string[] = [];
  private transitionQueue: UIScreen[] = [];
  private isTransitioning: boolean = false;
  private callbacks: Map<string, (screen: UIScreen) => void> = new Map();
  private defaultTransition: ScreenTransition = {
    type: 'fade',
    duration: 300,
    easing: 'easeInOut',
    properties: { opacity: 0 }
  };

  constructor() {
    this.initializeDefaultScreens();
  }

  private initializeDefaultScreens(): void {
    this.createScreen({
      id: 'main_menu',
      name: 'Main Menu',
      type: 'main_menu',
      visible: false,
      priority: 0,
      onShow: () => {},
      onHide: () => {}
    });

    this.createScreen({
      id: 'settings',
      name: 'Settings',
      type: 'settings',
      visible: false,
      priority: 10,
      onShow: () => {},
      onHide: () => {}
    });

    this.createScreen({
      id: 'pause',
      name: 'Pause Menu',
      type: 'pause',
      visible: false,
      priority: 50,
      onShow: () => {},
      onHide: () => {}
    });

    this.createScreen({
      id: 'loading',
      name: 'Loading',
      type: 'loading',
      visible: false,
      priority: 100,
      onShow: () => {},
      onHide: () => {}
    });

    this.createScreen({
      id: 'game_over',
      name: 'Game Over',
      type: 'game_over',
      visible: false,
      priority: 60,
      onShow: () => {},
      onHide: () => {}
    });

    this.createScreen({
      id: 'victory',
      name: 'Victory',
      type: 'victory',
      visible: false,
      priority: 60,
      onShow: () => {},
      onHide: () => {}
    });
  }

  createScreen(config: Partial<UIScreen> & { id: string; name: string }): UIScreen {
    const screen: UIScreen = {
      id: config.id,
      name: config.name,
      type: config.type || 'custom',
      visible: config.visible ?? false,
      priority: config.priority ?? 0,
      transition: config.transition || { ...this.defaultTransition },
      children: config.children || [],
      onShow: config.onShow,
      onHide: config.onHide
    };

    this.screens.set(screen.id, screen);
    return screen;
  }

  getScreen(id: string): UIScreen | undefined {
    return this.screens.get(id);
  }

  getAllScreens(): UIScreen[] {
    return Array.from(this.screens.values());
  }

  showScreen(screenId: string, hideOthers: boolean = false, transition?: ScreenTransition): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    if (hideOthers) {
      this.hideAllScreens();
    }

    if (this.isTransitioning) {
      this.transitionQueue.push(screen);
      return true;
    }

    this.isTransitioning = true;
    const effectiveTransition = transition || screen.transition || this.defaultTransition;

    this.screenHistory.push(screenId);
    screen.visible = true;
    this.activeScreens.add(screenId);

    if (screen.onShow) {
      screen.onShow();
    }

    setTimeout(() => {
      this.isTransitioning = false;

      if (this.transitionQueue.length > 0) {
        const nextScreen = this.transitionQueue.shift()!;
        this.showScreen(nextScreen.id);
      }
    }, effectiveTransition.duration);

    this.notifyScreenChange(screen);

    return true;
  }

  hideScreen(screenId: string, transition?: ScreenTransition): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    const effectiveTransition = transition || screen.transition || this.defaultTransition;
    screen.visible = false;
    this.activeScreens.delete(screenId);

    if (screen.onHide) {
      screen.onHide();
    }

    setTimeout(() => {
      this.notifyScreenChange(screen);
    }, effectiveTransition.duration);

    return true;
  }

  hideAllScreens(): void {
    this.activeScreens.forEach(screenId => {
      this.hideScreen(screenId);
    });
    this.activeScreens.clear();
  }

  toggleScreen(screenId: string): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    if (screen.visible) {
      return this.hideScreen(screenId);
    } else {
      return this.showScreen(screenId);
    }
  }

  getActiveScreens(): UIScreen[] {
    return Array.from(this.activeScreens)
      .map(id => this.screens.get(id))
      .filter((s): s is UIScreen => s !== undefined)
      .sort((a, b) => b.priority - a.priority);
  }

  isScreenActive(screenId: string): boolean {
    return this.activeScreens.has(screenId);
  }

  onScreenChange(callback: (screen: UIScreen) => void): () => void {
    const id = `callback_${Date.now()}`;
    this.callbacks.set(id, callback);
    return () => this.callbacks.delete(id);
  }

  private notifyScreenChange(screen: UIScreen): void {
    this.callbacks.forEach(callback => {
      callback(screen);
    });
  }

  addElementToScreen(screenId: string, element: UIElement): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    screen.children.push(element);
    return true;
  }

  removeElementFromScreen(screenId: string, elementId: string): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    const index = screen.children.findIndex(e => e.id === elementId);
    if (index !== -1) {
      screen.children.splice(index, 1);
      return true;
    }

    return false;
  }

  getScreenHistory(): string[] {
    return [...this.screenHistory];
  }

  goBack(): boolean {
    if (this.screenHistory.length <= 1) return false;

    const currentScreenId = this.screenHistory.pop();
    if (currentScreenId) {
      this.hideScreen(currentScreenId);
    }

    const previousScreenId = this.screenHistory[this.screenHistory.length - 1];
    if (previousScreenId) {
      this.showScreen(previousScreenId);
      return true;
    }

    return false;
  }

  clearHistory(): void {
    this.screenHistory = [];
  }

  destroyScreen(screenId: string): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    this.hideScreen(screenId);
    this.screens.delete(screenId);

    return true;
  }

  setScreenPriority(screenId: string, priority: number): boolean {
    const screen = this.screens.get(screenId);
    if (!screen) return false;

    screen.priority = priority;
    return true;
  }

  serialize(): UIScreenManagerState {
    return {
      screens: Array.from(this.screens.values()).map(screen => ({
        ...screen,
        onShow: undefined,
        onHide: undefined
      })),
      activeScreens: Array.from(this.activeScreens),
      screenHistory: this.screenHistory,
      defaultTransition: this.defaultTransition
    };
  }

  deserialize(state: UIScreenManagerState): void {
    this.screens.clear();
    this.activeScreens.clear();
    this.screenHistory = [];

    state.screens.forEach(screenData => {
      this.createScreen(screenData as UIScreen);
    });

    state.activeScreens.forEach(screenId => {
      this.showScreen(screenId, false);
    });

    this.screenHistory = state.screenHistory;
  }
}

export const uiScreenManager = new UIScreenManager();

export const useUIScreen = () => {
  const [activeScreens, setActiveScreens] = useState<UIScreen[]>([]);

  useEffect(() => {
    const updateActiveScreens = () => {
      setActiveScreens(uiScreenManager.getActiveScreens());
    };

    updateActiveScreens();

    const unsubscribe = uiScreenManager.onScreenChange(updateActiveScreens);

    return () => {
      unsubscribe();
    };
  }, []);

  const showScreen = useCallback((screenId: string, hideOthers: boolean = false) => {
    return uiScreenManager.showScreen(screenId, hideOthers);
  }, []);

  const hideScreen = useCallback((screenId: string) => {
    return uiScreenManager.hideScreen(screenId);
  }, []);

  const toggleScreen = useCallback((screenId: string) => {
    return uiScreenManager.toggleScreen(screenId);
  }, []);

  const isActive = useCallback((screenId: string) => {
    return uiScreenManager.isScreenActive(screenId);
  }, []);

  return {
    activeScreens,
    showScreen,
    hideScreen,
    toggleScreen,
    isActive
  };
};

export const Button: React.FC<ButtonProps> = ({
  children,
  variant = 'primary',
  size = 'md',
  disabled = false,
  loading = false,
  icon,
  badge,
  tooltip,
  onClick,
  className = '',
  ...props
}) => {
  const baseClasses = 'inline-flex items-center justify-center font-medium rounded-md transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2';

  const variantClasses: Record<ButtonVariant, string> = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
    secondary: 'bg-gray-600 text-white hover:bg-gray-700 focus:ring-gray-500',
    success: 'bg-green-600 text-white hover:bg-green-700 focus:ring-green-500',
    danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
    warning: 'bg-yellow-600 text-white hover:bg-yellow-700 focus:ring-yellow-500',
    info: 'bg-cyan-600 text-white hover:bg-cyan-700 focus:ring-cyan-500',
    link: 'bg-transparent text-blue-600 hover:text-blue-800 focus:ring-blue-500',
    outline: 'border-2 border-blue-600 text-blue-600 hover:bg-blue-50 focus:ring-blue-500'
  };

  const sizeClasses: Record<ButtonSize, string> = {
    xs: 'px-2 py-1 text-xs',
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-base',
    lg: 'px-6 py-3 text-lg',
    xl: 'px-8 py-4 text-xl'
  };

  const disabledClasses = disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer';

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${disabledClasses} ${className}`}
      disabled={disabled || loading}
      onClick={onClick}
      title={tooltip}
      {...props}
    >
      {loading && (
        <svg className="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
      )}
      {icon && <span className="mr-2">{icon}</span>}
      {children}
      {badge && (
        <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-white bg-opacity-20">
          {badge}
        </span>
      )}
    </button>
  );
};

export interface ButtonProps {
  children?: React.ReactNode;
  variant?: ButtonVariant;
  size?: ButtonSize;
  disabled?: boolean;
  loading?: boolean;
  icon?: React.ReactNode;
  badge?: string | number;
  tooltip?: string;
  onClick?: () => void;
  className?: string;
}

export const Slider: React.FC<SliderProps> = ({
  min = 0,
  max = 100,
  step = 1,
  value,
  onChange,
  showValue = true,
  valueFormat,
  disabled = false,
  label,
  className = ''
}) => {
  const [localValue, setLocalValue] = useState(value || min);
  const currentValue = value !== undefined ? value : localValue;
  const percentage = ((currentValue - min) / (max - min)) * 100;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = parseFloat(e.target.value);
    setLocalValue(newValue);
    if (onChange) {
      onChange(newValue);
    }
  };

  const displayValue = valueFormat ? valueFormat(currentValue) : currentValue.toFixed(step < 1 ? 1 : 0);

  return (
    <div className={`w-full ${className}`}>
      {(label || showValue) && (
        <div className="flex justify-between items-center mb-2">
          {label && <label className="text-sm font-medium text-gray-700">{label}</label>}
          {showValue && <span className="text-sm font-mono text-gray-600">{displayValue}</span>}
        </div>
      )}
      <div className="relative w-full h-2 bg-gray-200 rounded-full">
        <div
          className="absolute h-full bg-blue-600 rounded-full"
          style={{ width: `${percentage}%` }}
        />
        <input
          type="range"
          min={min}
          max={max}
          step={step}
          value={currentValue}
          onChange={handleChange}
          disabled={disabled}
          className="absolute w-full h-2 opacity-0 cursor-pointer"
        />
        <div
          className="absolute w-4 h-4 bg-white border-2 border-blue-600 rounded-full top-1/2 -translate-y-1/2 -translate-x-1/2 pointer-events-none"
          style={{ left: `${percentage}%` }}
        />
      </div>
    </div>
  );
};

export interface SliderProps {
  min?: number;
  max?: number;
  step?: number;
  value?: number;
  onChange?: (value: number) => void;
  showValue?: boolean;
  valueFormat?: (value: number) => string;
  disabled?: boolean;
  label?: string;
  className?: string;
}

export const Checkbox: React.FC<CheckboxProps> = ({
  checked,
  onChange,
  label,
  disabled = false,
  indeterminate = false,
  className = ''
}) => {
  const [localChecked, setLocalChecked] = useState(checked || false);
  const isChecked = checked !== undefined ? checked : localChecked;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalChecked(e.target.checked);
    if (onChange) {
      onChange(e.target.checked);
    }
  };

  return (
    <label className={`inline-flex items-center cursor-pointer ${disabled ? 'opacity-50 cursor-not-allowed' : ''} ${className}`}>
      <div className="relative">
        <input
          type="checkbox"
          checked={isChecked}
          onChange={handleChange}
          disabled={disabled}
          className="sr-only"
        />
        <div className={`w-5 h-5 border-2 rounded transition-all duration-200 ${
          isChecked
            ? 'bg-blue-600 border-blue-600'
            : 'bg-white border-gray-300'
        }`}>
          {(isChecked || indeterminate) && (
            <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              {indeterminate ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              )}
            </svg>
          )}
        </div>
      </div>
      {label && <span className="ml-2 text-sm text-gray-700">{label}</span>}
    </label>
  );
};

export interface CheckboxProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  indeterminate?: boolean;
  className?: string;
}

export const Dropdown: React.FC<DropdownProps> = ({
  options,
  value,
  onChange,
  placeholder = 'Select...',
  disabled = false,
  searchable = false,
  className = ''
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [localValue, setLocalValue] = useState(value || '');
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedValue = value !== undefined ? value : localValue;
  const selectedOption = options.find(opt => opt.value === selectedValue);

  const filteredOptions = searchable && searchTerm
    ? options.filter(opt => opt.label.toLowerCase().includes(searchTerm.toLowerCase()))
    : options;

  const handleSelect = (optionValue: string) => {
    setLocalValue(optionValue);
    setIsOpen(false);
    if (onChange) {
      onChange(optionValue);
    }
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div ref={dropdownRef} className={`relative ${className}`}>
      <button
        type="button"
        onClick={() => !disabled && setIsOpen(!isOpen)}
        className={`w-full px-4 py-2 text-left bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
        }`}
        disabled={disabled}
      >
        <span className={selectedOption ? 'text-gray-900' : 'text-gray-500'}>
          {selectedOption?.label || placeholder}
        </span>
        <span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
          <svg className="w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </span>
      </button>

      {isOpen && (
        <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
          {searchable && (
            <div className="p-2 border-b border-gray-200">
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search..."
                className="w-full px-3 py-1 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500"
                autoFocus
              />
            </div>
          )}
          <ul className="py-1">
            {filteredOptions.map((option) => (
              <li
                key={option.value}
                onClick={() => handleSelect(option.value)}
                className={`px-4 py-2 cursor-pointer hover:bg-blue-50 ${
                  option.value === selectedValue ? 'bg-blue-100 text-blue-900' : 'text-gray-900'
                } ${option.disabled ? 'opacity-50 cursor-not-allowed' : ''}`}
              >
                {option.label}
              </li>
            ))}
            {filteredOptions.length === 0 && (
              <li className="px-4 py-2 text-gray-500">No options found</li>
            )}
          </ul>
        </div>
      )}
    </div>
  );
};

export interface DropdownProps {
  options: DropdownOption[];
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  searchable?: boolean;
  className?: string;
}

export const Input: React.FC<InputProps> = ({
  type = 'text',
  value,
  onChange,
  placeholder,
  disabled = false,
  readonly = false,
  maxLength,
  label,
  error,
  icon,
  className = ''
}) => {
  const [localValue, setLocalValue] = useState(value || '');
  const currentValue = value !== undefined ? value : localValue;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalValue(e.target.value);
    if (onChange) {
      onChange(e.target.value);
    }
  };

  return (
    <div className={`w-full ${className}`}>
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-1">
          {label}
        </label>
      )}
      <div className="relative">
        {icon && (
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            {icon}
          </div>
        )}
        <input
          type={type}
          value={currentValue}
          onChange={handleChange}
          placeholder={placeholder}
          disabled={disabled}
          readOnly={readonly}
          maxLength={maxLength}
          className={`w-full px-4 py-2 border rounded-md focus:outline-none focus:ring-2 ${
            error
              ? 'border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:ring-blue-500'
          } ${icon ? 'pl-10' : ''} ${disabled ? 'bg-gray-100 cursor-not-allowed' : ''}`}
        />
      </div>
      {error && (
        <p className="mt-1 text-sm text-red-600">{error}</p>
      )}
    </div>
  );
};

export interface InputProps {
  type?: InputType;
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  readonly?: boolean;
  maxLength?: number;
  label?: string;
  error?: string;
  icon?: React.ReactNode;
  className?: string;
}

export const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  footer,
  closable = true,
  size = 'md'
}) => {
  const sizeClasses: Record<ModalSize, string> = {
    xs: 'max-w-xs',
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    fullscreen: 'max-w-full h-full'
  };

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && closable) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, closable, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 transition-opacity bg-black bg-opacity-50"
          onClick={closable ? onClose : undefined}
        />

        <div className={`inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle ${sizeClasses[size]} w-full`}>
          {title && (
            <div className="px-6 py-4 border-b border-gray-200">
              <h3 className="text-lg font-medium text-gray-900">{title}</h3>
            </div>
          )}

          <div className="px-6 py-4">
            {children}
          </div>

          {footer && (
            <div className="px-6 py-4 border-t border-gray-200 bg-gray-50">
              {footer}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children?: React.ReactNode;
  footer?: React.ReactNode;
  closable?: boolean;
  size?: ModalSize;
}

export const Toast: React.FC<ToastProps> = ({
  message,
  type = 'default',
  duration = 3000,
  onClose
}) => {
  const [isVisible, setIsVisible] = useState(true);

  useEffect(() => {
    if (duration > 0) {
      const timer = setTimeout(() => {
        setIsVisible(false);
        if (onClose) {
          setTimeout(onClose, 300);
        }
      }, duration);

      return () => clearTimeout(timer);
    }
  }, [duration, onClose]);

  const typeClasses: Record<ToastType, string> = {
    default: 'bg-gray-800 text-white',
    success: 'bg-green-600 text-white',
    error: 'bg-red-600 text-white',
    warning: 'bg-yellow-600 text-white',
    info: 'bg-blue-600 text-white'
  };

  if (!isVisible) return null;

  return (
    <div className={`fixed bottom-4 right-4 px-6 py-3 rounded-lg shadow-lg transform transition-all duration-300 ${
      isVisible ? 'translate-y-0 opacity-100' : 'translate-y-2 opacity-0'
    } ${typeClasses[type]}`}>
      <div className="flex items-center">
        <span>{message}</span>
        {onClose && (
          <button
            onClick={() => {
              setIsVisible(false);
              setTimeout(onClose, 300);
            }}
            className="ml-4 text-white hover:text-gray-200"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
};

export interface ToastProps {
  message: string;
  type?: ToastType;
  duration?: number;
  onClose?: () => void;
}

export const Loading: React.FC<LoadingProps> = ({
  type = 'spinner',
  text = 'Loading...',
  fullscreen = false
}) => {
  const spinner = (
    <svg className="animate-spin h-8 w-8 text-blue-600" fill="none" viewBox="0 0 24 24">
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
    </svg>
  );

  const dots = (
    <div className="flex space-x-2">
      {[0, 1, 2].map((i) => (
        <div
          key={i}
          className="w-3 h-3 bg-blue-600 rounded-full animate-bounce"
          style={{ animationDelay: `${i * 0.15}s` }}
        />
      ))}
    </div>
  );

  const pulse = (
    <div className="w-8 h-8 bg-blue-600 rounded-full animate-pulse" />
  );

  const bars = (
    <div className="flex space-x-1">
      {[0, 1, 2, 3, 4].map((i) => (
        <div
          key={i}
          className="w-1 bg-blue-600 rounded animate-pulse"
          style={{
            height: `${Math.sin(i * 0.5) * 10 + 16}px`,
            animationDelay: `${i * 0.1}s`
          }}
        />
      ))}
    </div>
  );

  const circle = (
    <div className="relative w-8 h-8">
      <div className="absolute inset-0 border-4 border-blue-200 rounded-full" />
      <div className="absolute inset-0 border-4 border-blue-600 rounded-full border-t-transparent animate-spin" />
    </div>
  );

  const loadingElements: Record<LoadingType, React.ReactNode> = {
    spinner,
    dots,
    pulse,
    bars,
    circle
  };

  const content = (
    <div className="flex flex-col items-center space-y-4">
      {loadingElements[type]}
      {text && <p className="text-gray-600">{text}</p>}
    </div>
  );

  if (fullscreen) {
    return (
      <div className="fixed inset-0 flex items-center justify-center bg-white bg-opacity-90 z-50">
        {content}
      </div>
    );
  }

  return <div className="flex items-center justify-center py-4">{content}</div>;
};

export interface LoadingProps {
  type?: LoadingType;
  text?: string;
  fullscreen?: boolean;
}

export const ProgressBar: React.FC<ProgressBarProps> = ({
  value,
  min = 0,
  max = 100,
  showLabel = false,
  labelFormat,
  animated = false,
  striped = false,
  variant = 'primary',
  className = ''
}) => {
  const percentage = Math.min(100, Math.max(0, ((value - min) / (max - min)) * 100));

  const variantClasses: Record<ProgressVariant, string> = {
    primary: 'bg-blue-600',
    secondary: 'bg-gray-600',
    success: 'bg-green-600',
    danger: 'bg-red-600',
    warning: 'bg-yellow-600',
    info: 'bg-cyan-600'
  };

  const labelText = labelFormat
    ? labelFormat(value, min, max)
    : `${Math.round(percentage)}%`;

  return (
    <div className={`w-full ${className}`}>
      <div className={`h-2 bg-gray-200 rounded-full overflow-hidden ${striped ? 'bg-repeat' : ''}`}
        style={striped ? { backgroundImage: 'repeating-linear-gradient(45deg, transparent, transparent 5px, rgba(255,255,255,0.1) 5px, rgba(255,255,255,0.1) 10px)' } : {}}
      >
        <div
          className={`h-full rounded-full transition-all duration-300 ${variantClasses[variant]} ${
            animated ? 'animate-stripe' : ''
          }`}
          style={{ width: `${percentage}%` }}
        />
      </div>
      {showLabel && (
        <p className="mt-1 text-sm text-gray-600 text-right">{labelText}</p>
      )}
    </div>
  );
};

export interface ProgressBarProps {
  value: number;
  min?: number;
  max?: number;
  showLabel?: boolean;
  labelFormat?: (value: number, min: number, max: number) => string;
  animated?: boolean;
  striped?: boolean;
  variant?: ProgressVariant;
  className?: string;
}

export const Tooltip: React.FC<TooltipProps> = ({
  children,
  content,
  position = 'top',
  delay = 200
}) => {
  const [isVisible, setIsVisible] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout>();

  const showTooltip = () => {
    timeoutRef.current = setTimeout(() => {
      setIsVisible(true);
    }, delay);
  };

  const hideTooltip = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    setIsVisible(false);
  };

  const positionClasses: Record<TooltipPosition, string> = {
    top: 'bottom-full left-1/2 -translate-x-1/2 mb-2',
    bottom: 'top-full left-1/2 -translate-x-1/2 mt-2',
    left: 'right-full top-1/2 -translate-y-1/2 mr-2',
    right: 'left-full top-1/2 -translate-y-1/2 ml-2',
    'top-left': 'bottom-full left-0 mb-2',
    'top-right': 'bottom-full right-0 mb-2',
    'bottom-left': 'top-full left-0 mt-2',
    'bottom-right': 'top-full right-0 mt-2',
    center: 'top-full left-1/2 -translate-x-1/2 mt-2'
  };

  return (
    <div className="relative inline-block">
      <div
        onMouseEnter={showTooltip}
        onMouseLeave={hideTooltip}
        onFocus={showTooltip}
        onBlur={hideTooltip}
      >
        {children}
      </div>
      {isVisible && (
        <div
          className={`absolute z-50 px-3 py-2 text-sm text-white bg-gray-900 rounded shadow-lg whitespace-nowrap ${positionClasses[position]}`}
          role="tooltip"
        >
          {content}
          <div className="absolute left-1/2 -translate-x-1/2 border-4 border-transparent border-t-gray-900"
            style={{
              top: position.includes('top') ? '100%' : 'auto',
              bottom: position.includes('bottom') ? '100%' : 'auto',
            }}
          />
        </div>
      )}
    </div>
  );
};

export interface TooltipProps {
  children: React.ReactNode;
  content: string | React.ReactNode;
  position?: TooltipPosition;
  delay?: number;
}

export const Card: React.FC<CardProps> = ({
  children,
  title,
  subtitle,
  footer,
  image,
  className = ''
}) => {
  return (
    <div className={`bg-white rounded-lg shadow-md overflow-hidden ${className}`}>
      {image && (
        <div className="w-full h-48 bg-gray-200">
          <img src={image} alt="" className="w-full h-full object-cover" />
        </div>
      )}
      {(title || subtitle) && (
        <div className="px-6 py-4 border-b border-gray-200">
          {title && <h3 className="text-lg font-medium text-gray-900">{title}</h3>}
          {subtitle && <p className="mt-1 text-sm text-gray-500">{subtitle}</p>}
        </div>
      )}
      <div className="px-6 py-4">
        {children}
      </div>
      {footer && (
        <div className="px-6 py-4 border-t border-gray-200 bg-gray-50">
          {footer}
        </div>
      )}
    </div>
  );
};

export interface CardProps {
  children?: React.ReactNode;
  title?: string;
  subtitle?: string;
  footer?: React.ReactNode;
  image?: string;
  className?: string;
}

export const Tab: React.FC<TabProps> = ({
  tabs,
  activeTab,
  onChange,
  variant = 'underline',
  className = ''
}) => {
  const underline = (index: number) => (
    <div
      className="absolute bottom-0 h-0.5 bg-blue-600 transition-all duration-300"
      style={{
        left: `${index * (100 / tabs.length)}%`,
        width: `${100 / tabs.length}%`
      }}
    />
  );

  const pills = (index: number) => (
    <button
      onClick={() => onChange(tabs[index].id)}
      className={`px-4 py-2 rounded-full transition-all duration-200 ${
        activeTab === tabs[index].id
          ? 'bg-blue-600 text-white'
          : 'text-gray-600 hover:bg-gray-100'
      }`}
    >
      {tabs[index].label}
    </button>
  );

  const standard = (index: number) => (
    <button
      onClick={() => onChange(tabs[index].id)}
      className={`px-4 py-2 border-b-2 transition-all duration-200 ${
        activeTab === tabs[index].id
          ? 'border-blue-600 text-blue-600'
          : 'border-transparent text-gray-600 hover:text-gray-900'
      }`}
    >
      {tabs[index].label}
    </button>
  );

  const renderTab = (index: number) => {
    switch (variant) {
      case 'pills':
        return pills(index);
      case 'underline':
        return standard(index);
      default:
        return standard(index);
    }
  };

  return (
    <div className={`${className}`}>
      <div className="relative flex border-b border-gray-200">
        {tabs.map((tab, index) => (
          <div key={tab.id} className="relative">
            {renderTab(index)}
          </div>
        ))}
        {variant === 'underline' && (
          <div className="absolute bottom-0 h-0.5 bg-gray-200 w-full -z-10" />
        )}
        {variant === 'underline' && (
          <div
            className="absolute bottom-0 h-0.5 bg-blue-600 transition-all duration-300"
            style={{
              left: `${tabs.findIndex(t => t.id === activeTab) * (100 / tabs.length)}%`,
              width: `${100 / tabs.length}%`
            }}
          />
        )}
      </div>
      <div className="py-4">
        {tabs.find(t => t.id === activeTab)?.content}
      </div>
    </div>
  );
};

export interface TabProps {
  tabs: TabItem[];
  activeTab: string;
  onChange: (tabId: string) => void;
  variant?: 'underline' | 'pills';
  className?: string;
}

export interface TabItem {
  id: string;
  label: string;
  content?: React.ReactNode;
  disabled?: boolean;
}

export const Accordion: React.FC<AccordionProps> = ({
  items,
  allowMultiple = false,
  className = ''
}) => {
  const [openItems, setOpenItems] = useState<Set<string>>(new Set());

  const toggleItem = (id: string) => {
    setOpenItems(prev => {
      const newSet = new Set(prev);
      if (newSet.has(id)) {
        newSet.delete(id);
      } else {
        if (!allowMultiple) {
          newSet.clear();
        }
        newSet.add(id);
      }
      return newSet;
    });
  };

  return (
    <div className={`space-y-2 ${className}`}>
      {items.map((item) => (
        <div key={item.id} className="border border-gray-200 rounded-lg overflow-hidden">
          <button
            onClick={() => toggleItem(item.id)}
            className="w-full px-4 py-3 flex items-center justify-between bg-gray-50 hover:bg-gray-100 transition-colors"
          >
            <span className="font-medium text-gray-900">{item.title}</span>
            <svg
              className={`w-5 h-5 text-gray-500 transition-transform ${
                openItems.has(item.id) ? 'rotate-180' : ''
              }`}
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          {openItems.has(item.id) && (
            <div className="px-4 py-3 bg-white">
              {item.content}
            </div>
          )}
        </div>
      ))}
    </div>
  );
};

export interface AccordionProps {
  items: AccordionItem[];
  allowMultiple?: boolean;
  className?: string;
}

export interface AccordionItem {
  id: string;
  title: string;
  content: React.ReactNode;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  showFirstLast = true,
  showPrevNext = true,
  siblingCount = 1,
  className = ''
}) => {
  const getPageNumbers = (): (number | 'ellipsis')[] => {
    const pages: (number | 'ellipsis')[] = [];
    const totalVisible = siblingCount * 2 + 5;

    if (totalPages <= totalVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      if (currentPage > siblingCount + 2) {
        pages.push('ellipsis');
      }

      const start = Math.max(2, currentPage - siblingCount);
      const end = Math.min(totalPages - 1, currentPage + siblingCount);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - siblingCount - 1) {
        pages.push('ellipsis');
      }

      pages.push(totalPages);
    }

    return pages;
  };

  const PageButton: React.FC<{ page: number; isActive?: boolean }> = ({ page, isActive }) => (
    <button
      onClick={() => onPageChange(page)}
      className={`min-w-[40px] h-10 flex items-center justify-center rounded-md transition-colors ${
        isActive
          ? 'bg-blue-600 text-white'
          : 'text-gray-700 hover:bg-gray-100'
      }`}
    >
      {page}
    </button>
  );

  const Ellipsis = () => (
    <span className="min-w-[40px] h-10 flex items-center justify-center text-gray-500">
      ...
    </span>
  );

  return (
    <div className={`flex items-center space-x-2 ${className}`}>
      {showFirstLast && currentPage > 1 && (
        <button
          onClick={() => onPageChange(1)}
          className="h-10 px-3 text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        >
          First
        </button>
      )}

      {showPrevNext && currentPage > 1 && (
        <button
          onClick={() => onPageChange(currentPage - 1)}
          className="h-10 px-3 text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        >
          Previous
        </button>
      )}

      <div className="flex items-center space-x-1">
        {getPageNumbers().map((page, index) => (
          page === 'ellipsis' ? (
            <Ellipsis key={`ellipsis-${index}`} />
          ) : (
            <PageButton key={page} page={page} isActive={page === currentPage} />
          )
        ))}
      </div>

      {showPrevNext && currentPage < totalPages && (
        <button
          onClick={() => onPageChange(currentPage + 1)}
          className="h-10 px-3 text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        >
          Next
        </button>
      )}

      {showFirstLast && currentPage < totalPages && (
        <button
          onClick={() => onPageChange(totalPages)}
          className="h-10 px-3 text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        >
          Last
        </button>
      )}
    </div>
  );
};

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  showFirstLast?: boolean;
  showPrevNext?: boolean;
  siblingCount?: number;
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'primary',
  size = 'md',
  dot = false,
  className = ''
}) => {
  const variantClasses: Record<string, string> = {
    primary: 'bg-blue-100 text-blue-800',
    secondary: 'bg-gray-100 text-gray-800',
    success: 'bg-green-100 text-green-800',
    danger: 'bg-red-100 text-red-800',
    warning: 'bg-yellow-100 text-yellow-800',
    info: 'bg-cyan-100 text-cyan-800'
  };

  const sizeClasses: Record<string, string> = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-0.5 text-sm',
    lg: 'px-3 py-1 text-base'
  };

  return (
    <span className={`inline-flex items-center font-medium rounded-full ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}>
      {dot && (
        <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${
          variant.includes('success') ? 'bg-green-500' :
          variant.includes('danger') ? 'bg-red-500' :
          variant.includes('warning') ? 'bg-yellow-500' :
          'bg-current'
        }`} />
      )}
      {children}
    </span>
  );
};

export interface BadgeProps {
  children: React.ReactNode;
  variant?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  size?: 'sm' | 'md' | 'lg';
  dot?: boolean;
  className?: string;
}

export const Avatar: React.FC<AvatarProps> = ({
  src,
  alt = '',
  name,
  size = 'md',
  shape = 'circle',
  status,
  className = ''
}) => {
  const sizeClasses: Record<string, string> = {
    xs: 'w-6 h-6 text-xs',
    sm: 'w-8 h-8 text-sm',
    md: 'w-10 h-10 text-base',
    lg: 'w-12 h-12 text-lg',
    xl: 'w-16 h-16 text-xl'
  };

  const shapeClasses = shape === 'circle' ? 'rounded-full' : 'rounded-lg';

  const getInitials = (name: string): string => {
    return name
      .split(' ')
      .map(part => part[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const statusColors: Record<string, string> = {
    online: 'bg-green-500',
    offline: 'bg-gray-500',
    away: 'bg-yellow-500',
    busy: 'bg-red-500'
  };

  return (
    <div className={`relative inline-block ${className}`}>
      <div className={`${sizeClasses[size]} ${shapeClasses} bg-blue-600 text-white flex items-center justify-center font-medium overflow-hidden`}>
        {src ? (
          <img src={src} alt={alt} className="w-full h-full object-cover" />
        ) : name ? (
          getInitials(name)
        ) : (
          '?'
        )}
      </div>
      {status && (
        <span className={`absolute bottom-0 right-0 w-3 h-3 ${statusColors[status]} border-2 border-white rounded-full`} />
      )}
    </div>
  );
};

export interface AvatarProps {
  src?: string;
  alt?: string;
  name?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  shape?: 'circle' | 'square';
  status?: 'online' | 'offline' | 'away' | 'busy';
  className?: string;
}

export const Divider: React.FC<DividerProps> = ({
  orientation = 'horizontal',
  type = 'solid',
  color = 'gray',
  className = ''
}) => {
  const orientationClasses = orientation === 'horizontal' ? 'w-full h-px' : 'h-full w-px';
  const colorClasses = {
    gray: 'bg-gray-200',
    blue: 'bg-blue-600',
    red: 'bg-red-600',
    green: 'bg-green-600',
    yellow: 'bg-yellow-600'
  };

  const typeStyles = type === 'dashed' ? 'border-dashed' : '';

  return (
    <div className={`${orientationClasses} ${colorClasses[color]} ${typeStyles} ${className}`} />
  );
};

export interface DividerProps {
  orientation?: 'horizontal' | 'vertical';
  type?: 'solid' | 'dashed';
  color?: 'gray' | 'blue' | 'red' | 'green' | 'yellow';
  className?: string;
}

export const Spinner: React.FC<SpinnerProps> = ({
  size = 'md',
  color = 'primary',
  className = ''
}) => {
  const sizeClasses: Record<string, string> = {
    xs: 'w-3 h-3',
    sm: 'w-4 h-4',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
    xl: 'w-12 h-12'
  };

  const colorClasses: Record<string, string> = {
    primary: 'text-blue-600',
    white: 'text-white',
    gray: 'text-gray-600'
  };

  return (
    <svg
      className={`animate-spin ${sizeClasses[size]} ${colorClasses[color]} ${className}`}
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
    </svg>
  );
};

export interface SpinnerProps {
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  color?: 'primary' | 'white' | 'gray';
  className?: string;
}
