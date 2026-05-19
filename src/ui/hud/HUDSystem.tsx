import React, { useState, useEffect } from 'react';
import { useGameStore } from '../../store/useGameStore';

export interface HUDConfig {
  showResources: boolean;
  showMinimap: boolean;
  showBuildMenu: boolean;
  showUnitInfo: boolean;
  showTechTree: boolean;
  showMissions: boolean;
  position: 'top' | 'bottom' | 'left' | 'right';
}

export interface HUDResource {
  type: 'credits' | 'power' | 'ore' | 'gems' | 'energy';
  value: number;
  max?: number;
  icon: string;
  color: string;
}

export interface HUDAction {
  id: string;
  name: string;
  icon: string;
  hotkey: string;
  enabled: boolean;
  callback: () => void;
}

const defaultHUDConfig: HUDConfig = {
  showResources: true,
  showMinimap: true,
  showBuildMenu: true,
  showUnitInfo: true,
  showTechTree: false,
  showMissions: true,
  position: 'top',
};

export class HUDManager {
  private config: HUDConfig;
  private visible: boolean = true;
  private listeners: Map<string, Array<(data: any) => void>> = new Map();

  constructor(config: Partial<HUDConfig> = {}) {
    this.config = { ...defaultHUDConfig, ...config };
  }

  public show(): void {
    this.visible = true;
    this.emit('visibility-changed', { visible: true });
  }

  public hide(): void {
    this.visible = false;
    this.emit('visibility-changed', { visible: false });
  }

  public toggle(): void {
    if (this.visible) {
      this.hide();
    } else {
      this.show();
    }
  }

  public isVisible(): boolean {
    return this.visible;
  }

  public setConfig(config: Partial<HUDConfig>): void {
    this.config = { ...this.config, ...config };
    this.emit('config-changed', { config: this.config });
  }

  public getConfig(): HUDConfig {
    return { ...this.config };
  }

  public on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, []);
    }
    this.listeners.get(event)!.push(callback);
  }

  public off(event: string, callback: (data: any) => void): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  private emit(event: string, data: any): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      for (const callback of callbacks) {
        callback(data);
      }
    }
  }
}

export const HUDResourceBar: React.FC<{
  resources: HUDResource[];
  showBars?: boolean;
}> = ({ resources, showBars = false }) => {
  return (
    <div className="flex items-center gap-6 p-3 bg-gradient-to-r from-gray-900 via-gray-800 to-gray-900 border-b-2 border-yellow-600 shadow-lg">
      {resources.map((resource) => (
        <div key={resource.type} className="flex items-center gap-2 group">
          <span className="text-2xl" style={{ color: resource.color }}>
            {resource.icon}
          </span>
          <div className="flex flex-col">
            <span className="text-sm font-bold text-white">
              {Math.floor(resource.value).toLocaleString()}
            </span>
            {showBars && resource.max && (
              <div className="w-20 h-1 bg-gray-700 rounded-full overflow-hidden">
                <div
                  className="h-full transition-all duration-300"
                  style={{
                    width: `${(resource.value / resource.max) * 100}%`,
                    backgroundColor: resource.color,
                  }}
                />
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

export const HUDActionBar: React.FC<{
  actions: HUDAction[];
  columns?: number;
}> = ({ actions, columns = 8 }) => {
  return (
    <div
      className="grid gap-1 p-2 bg-gradient-to-t from-gray-900 to-gray-800 border-t-2 border-yellow-600"
      style={{ gridTemplateColumns: `repeat(${columns}, 1fr)` }}
    >
      {actions.map((action) => (
        <button
          key={action.id}
          disabled={!action.enabled}
          onClick={action.callback}
          className={`
            flex flex-col items-center justify-center p-2 rounded
            transition-all duration-200 transform
            ${action.enabled
              ? 'bg-gradient-to-b from-gray-700 to-gray-800 hover:from-yellow-600 hover:to-yellow-700 hover:scale-105 cursor-pointer'
              : 'bg-gray-800 opacity-50 cursor-not-allowed'
            }
          `}
        >
          <span className="text-2xl">{action.icon}</span>
          <span className="text-xs text-gray-400 mt-1">{action.hotkey}</span>
        </button>
      ))}
    </div>
  );
};

export const HUDUnitInfo: React.FC<{
  unit: {
    name: string;
    health: number;
    maxHealth: number;
    type: string;
    icon: string;
    level?: number;
    experience?: number;
    status?: string[];
  } | null;
}> = ({ unit }) => {
  if (!unit) {
    return (
      <div className="w-64 p-4 bg-gray-900 border-l-4 border-yellow-600">
        <p className="text-gray-500 text-sm italic">No unit selected</p>
      </div>
    );
  }

  return (
    <div className="w-64 p-4 bg-gradient-to-br from-gray-900 to-gray-800 border-l-4 border-yellow-600 shadow-xl">
      <div className="flex items-center gap-3 mb-3">
        <span className="text-4xl">{unit.icon}</span>
        <div>
          <h3 className="text-lg font-bold text-white">{unit.name}</h3>
          <p className="text-xs text-gray-400">Level {unit.level || 1}</p>
        </div>
      </div>

      <div className="mb-3">
        <div className="flex justify-between text-sm text-gray-300 mb-1">
          <span>Health</span>
          <span>{Math.floor(unit.health)} / {unit.maxHealth}</span>
        </div>
        <div className="w-full h-3 bg-gray-700 rounded-full overflow-hidden">
          <div
            className="h-full transition-all duration-300 bg-gradient-to-r from-green-600 to-green-400"
            style={{ width: `${(unit.health / unit.maxHealth) * 100}%` }}
          />
        </div>
      </div>

      {unit.experience !== undefined && (
        <div className="mb-3">
          <div className="flex justify-between text-sm text-gray-300 mb-1">
            <span>Experience</span>
            <span>{Math.floor(unit.experience)}</span>
          </div>
          <div className="w-full h-2 bg-gray-700 rounded-full overflow-hidden">
            <div
              className="h-full transition-all duration-300 bg-gradient-to-r from-blue-600 to-blue-400"
              style={{ width: `${(unit.experience % 100)}%` }}
            />
          </div>
        </div>
      )}

      {unit.status && unit.status.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {unit.status.map((status, index) => (
            <span
              key={index}
              className="px-2 py-1 text-xs bg-yellow-600 text-black rounded"
            >
              {status}
            </span>
          ))}
        </div>
      )}
    </div>
  );
};

export const HUDNotifications: React.FC<{
  notifications: Array<{
    id: string;
    message: string;
    type: 'info' | 'warning' | 'success' | 'error';
    duration?: number;
  }>;
  onDismiss?: (id: string) => void;
}> = ({ notifications, onDismiss }) => {
  useEffect(() => {
    const timers: NodeJS.Timeout[] = [];

    notifications.forEach((notification) => {
      if (notification.duration && onDismiss) {
        const timer = setTimeout(() => {
          onDismiss(notification.id);
        }, notification.duration);
        timers.push(timer);
      }
    });

    return () => {
      timers.forEach((timer) => clearTimeout(timer));
    };
  }, [notifications, onDismiss]);

  const getTypeStyles = (type: string) => {
    switch (type) {
      case 'success':
        return 'bg-green-600 border-green-400';
      case 'warning':
        return 'bg-yellow-600 border-yellow-400';
      case 'error':
        return 'bg-red-600 border-red-400';
      default:
        return 'bg-blue-600 border-blue-400';
    }
  };

  return (
    <div className="fixed top-20 right-4 flex flex-col gap-2 z-50">
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={`
            px-4 py-3 rounded-lg border-l-4 shadow-xl
            transform transition-all duration-300
            ${getTypeStyles(notification.type)}
          `}
        >
          <p className="text-white text-sm">{notification.message}</p>
        </div>
      ))}
    </div>
  );
};

export const HUDMinimap: React.FC<{
  entities: Array<{
    id: string;
    position: { x: number; y: number };
    type: 'unit' | 'building';
    team: number;
  }>;
  camera: { x: number; y: number; width: number; height: number };
  mapSize: { width: number; height: number };
  width?: number;
  height?: number;
  onClick?: (x: number, y: number) => void;
}> = ({
  entities,
  camera,
  mapSize,
  width = 200,
  height = 150,
  onClick,
}) => {
  const scaleX = width / mapSize.width;
  const scaleY = height / mapSize.height;

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!onClick) return;

    const rect = e.currentTarget.getBoundingClientRect();
    const x = (e.clientX - rect.left) / scaleX;
    const y = (e.clientY - rect.top) / scaleY;

    onClick(x, y);
  };

  return (
    <div
      className="relative bg-gray-900 border-2 border-yellow-600 cursor-pointer overflow-hidden"
      style={{ width, height }}
      onClick={handleClick}
    >
      {entities.map((entity) => (
        <div
          key={entity.id}
          className={`
            absolute w-2 h-2 rounded-full
            ${entity.team === 0 ? 'bg-cyan-400' : 'bg-red-400'}
          `}
          style={{
            left: entity.position.x * scaleX - 1,
            top: entity.position.y * scaleY - 1,
          }}
        />
      ))}

      <div
        className="absolute border-2 border-white pointer-events-none"
        style={{
          left: camera.x * scaleX,
          top: camera.y * scaleY,
          width: camera.width * scaleX,
          height: camera.height * scaleY,
        }}
      />
    </div>
  );
};

export const HUDBottomPanel: React.FC<{
  selectedUnits: any[];
  buildMenuOpen: boolean;
  onBuildMenuToggle: () => void;
  buildOptions: Array<{
    id: string;
    name: string;
    icon: string;
    cost: number;
    available: boolean;
  }>;
  onBuildSelect: (id: string) => void;
}> = ({ selectedUnits, buildMenuOpen, onBuildMenuToggle, buildOptions, onBuildSelect }) => {
  return (
    <div className="bg-gray-900 border-t-2 border-yellow-600">
      <div className="flex items-center justify-between p-2 border-b border-gray-700">
        <div className="flex items-center gap-4">
          <span className="text-gray-400 text-sm">
            {selectedUnits.length} unit{selectedUnits.length !== 1 ? 's' : ''} selected
          </span>
          <button
            onClick={onBuildMenuToggle}
            className={`
              px-4 py-2 rounded transition-all
              ${buildMenuOpen
                ? 'bg-yellow-600 text-black'
                : 'bg-gray-700 text-white hover:bg-gray-600'
              }
            `}
          >
            Build Menu
          </button>
        </div>
      </div>

      {buildMenuOpen && (
        <div className="p-4">
          <div className="grid grid-cols-6 gap-3">
            {buildOptions.map((option) => (
              <button
                key={option.id}
                disabled={!option.available}
                onClick={() => onBuildSelect(option.id)}
                className={`
                  flex flex-col items-center p-3 rounded-lg transition-all
                  ${option.available
                    ? 'bg-gradient-to-b from-gray-700 to-gray-800 hover:from-yellow-600 hover:to-yellow-700 cursor-pointer'
                    : 'bg-gray-800 opacity-50 cursor-not-allowed'
                  }
                `}
              >
                <span className="text-3xl mb-2">{option.icon}</span>
                <span className="text-xs text-white mb-1">{option.name}</span>
                <span className="text-xs text-yellow-400">{option.cost}</span>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export const HUDTimer: React.FC<{
  time: number;
  isPaused?: boolean;
}> = ({ time, isPaused = false }) => {
  const minutes = Math.floor(time / 60);
  const seconds = Math.floor(time % 60);

  return (
    <div className={`
      px-4 py-2 rounded-lg font-mono text-lg
      ${isPaused ? 'bg-red-600' : 'bg-gray-800'}
      text-white
    `}>
      {isPaused && <span className="mr-2">⏸️</span>}
      {minutes.toString().padStart(2, '0')}:{seconds.toString().padStart(2, '0')}
    </div>
  );
};

export const HUDFullscreen: React.FC<{
  children: React.ReactNode;
  showHUD?: boolean;
}> = ({ children, showHUD = true }) => {
  const [isFullscreen, setIsFullscreen] = useState(false);

  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(!!document.fullscreenElement);
    };

    document.addEventListener('fullscreenchange', handleFullscreenChange);
    return () => document.removeEventListener('fullscreenchange', handleFullscreenChange);
  }, []);

  const toggleFullscreen = async () => {
    if (!document.fullscreenElement) {
      await document.documentElement.requestFullscreen();
    } else {
      await document.exitFullscreen();
    }
  };

  return (
    <div className="fixed inset-0 bg-black">
      {children}

      {showHUD && (
        <button
          onClick={toggleFullscreen}
          className="fixed top-4 right-4 p-2 bg-gray-800 rounded-lg hover:bg-gray-700 transition-colors z-50"
        >
          {isFullscreen ? '⛶' : '⛶'}
        </button>
      )}
    </div>
  );
};

export class HUDUpdater {
  private store: any;
  private hudManager: HUDManager;

  constructor(store: any) {
    this.store = store;
    this.hudManager = new HUDManager();
  }

  public update(): void {
    const state = this.store.getState();

    const resources: HUDResource[] = [
      {
        type: 'credits',
        value: state.credits,
        icon: '💰',
        color: '#ffd700',
      },
      {
        type: 'power',
        value: state.power,
        icon: '⚡',
        color: '#00bfff',
      },
      {
        type: 'ore',
        value: state.ore,
        icon: '💎',
        color: '#888888',
      },
    ];
  }

  public getManager(): HUDManager {
    return this.hudManager;
  }
}
