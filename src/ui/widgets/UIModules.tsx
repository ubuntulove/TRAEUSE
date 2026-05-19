import React, { useState } from 'react';

export interface MenuItem {
  id: string;
  label: string;
  icon?: string;
  hotkey?: string;
  enabled?: boolean;
  children?: MenuItem[];
}

export interface MenuBarProps {
  items: MenuItem[];
  onSelect: (itemId: string) => void;
}

export const MenuBar: React.FC<MenuBarProps> = ({ items, onSelect }) => {
  const [activeMenu, setActiveMenu] = useState<string | null>(null);

  return (
    <div className="flex items-center bg-gradient-to-r from-gray-900 via-gray-800 to-gray-900 border-b-2 border-yellow-600 p-1">
      {items.map((item) => (
        <div key={item.id} className="relative">
          <button
            onClick={() => setActiveMenu(activeMenu === item.id ? null : item.id)}
            onMouseEnter={() => activeMenu && setActiveMenu(item.id)}
            className={`
              px-4 py-2 text-sm font-medium transition-all
              ${activeMenu === item.id
                ? 'bg-yellow-600 text-black'
                : 'text-gray-300 hover:bg-gray-700 hover:text-white'
              }
            `}
          >
            {item.label}
          </button>

          {activeMenu === item.id && item.children && (
            <div className="absolute top-full left-0 mt-1 min-w-48 bg-gray-900 border border-gray-700 rounded-lg shadow-xl z-50">
              {item.children.map((child) => (
                <button
                  key={child.id}
                  disabled={child.enabled === false}
                  onClick={() => {
                    onSelect(child.id);
                    setActiveMenu(null);
                  }}
                  className={`
                    w-full px-4 py-2 text-left text-sm flex items-center justify-between
                    ${child.enabled !== false
                      ? 'text-gray-300 hover:bg-gray-700 hover:text-white'
                      : 'text-gray-600 cursor-not-allowed'
                    }
                  `}
                >
                  <div className="flex items-center gap-2">
                    {child.icon && <span>{child.icon}</span>}
                    <span>{child.label}</span>
                  </div>
                  {child.hotkey && (
                    <span className="text-xs text-gray-500">{child.hotkey}</span>
                  )}
                </button>
              ))}
            </div>
          )}
        </div>
      ))}

      {activeMenu && (
        <div
          className="fixed inset-0 z-40"
          onClick={() => setActiveMenu(null)}
        />
      )}
    </div>
  );
};

export const ContextMenu: React.FC<{
  items: MenuItem[];
  position: { x: number; y: number };
  onSelect: (itemId: string) => void;
  onClose: () => void;
}> = ({ items, position, onSelect, onClose }) => {
  return (
    <>
      <div
        className="fixed inset-0 z-40"
        onClick={onClose}
        onContextMenu={(e) => {
          e.preventDefault();
          onClose();
        }}
      />
      <div
        className="fixed bg-gray-900 border border-gray-700 rounded-lg shadow-xl z-50 min-w-48 py-1"
        style={{ left: position.x, top: position.y }}
      >
        {items.map((item) => (
          <button
            key={item.id}
            disabled={item.enabled === false}
            onClick={() => {
              onSelect(item.id);
              onClose();
            }}
            className={`
              w-full px-4 py-2 text-left text-sm flex items-center justify-between
              ${item.enabled !== false
                ? 'text-gray-300 hover:bg-gray-700 hover:text-white'
                : 'text-gray-600 cursor-not-allowed'
              }
            `}
          >
            <div className="flex items-center gap-2">
              {item.icon && <span>{item.icon}</span>}
              <span>{item.label}</span>
            </div>
            {item.hotkey && (
              <span className="text-xs text-gray-500">{item.hotkey}</span>
            )}
          </button>
        ))}
      </div>
    </>
  );
};

export const Modal: React.FC<{
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  showClose?: boolean;
}> = ({ isOpen, onClose, title, children, size = 'md', showClose = true }) => {
  if (!isOpen) return null;

  const sizeClasses = {
    sm: 'max-w-md',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl',
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div
        className="absolute inset-0 bg-black bg-opacity-75"
        onClick={onClose}
      />
      <div
        className={`
          relative bg-gradient-to-br from-gray-900 to-gray-800 rounded-xl
          border-2 border-yellow-600 shadow-2xl
          ${sizeClasses[size]}
          w-full mx-4
        `}
      >
        <div className="flex items-center justify-between p-4 border-b border-gray-700">
          <h2 className="text-xl font-bold text-white">{title}</h2>
          {showClose && (
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-white transition-colors text-2xl"
            >
              ×
            </button>
          )}
        </div>
        <div className="p-6">{children}</div>
      </div>
    </div>
  );
};

export const Button: React.FC<{
  children: React.ReactNode;
  onClick?: () => void;
  variant?: 'primary' | 'secondary' | 'danger' | 'success';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  className?: string;
  icon?: React.ReactNode;
}> = ({
  children,
  onClick,
  variant = 'primary',
  size = 'md',
  disabled = false,
  className = '',
  icon,
}) => {
  const variantClasses = {
    primary: 'bg-gradient-to-r from-yellow-600 to-yellow-700 hover:from-yellow-500 hover:to-yellow-600 text-black',
    secondary: 'bg-gradient-to-r from-gray-600 to-gray-700 hover:from-gray-500 hover:to-gray-600 text-white',
    danger: 'bg-gradient-to-r from-red-600 to-red-700 hover:from-red-500 hover:to-red-600 text-white',
    success: 'bg-gradient-to-r from-green-600 to-green-700 hover:from-green-500 hover:to-green-600 text-white',
  };

  const sizeClasses = {
    sm: 'px-3 py-1 text-sm',
    md: 'px-6 py-2 text-base',
    lg: 'px-8 py-3 text-lg',
  };

  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`
        ${variantClasses[variant]}
        ${sizeClasses[size]}
        ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
        font-bold rounded-lg transition-all duration-200
        transform hover:scale-105 active:scale-95
        flex items-center justify-center gap-2
        ${className}
      `}
    >
      {icon && <span>{icon}</span>}
      {children}
    </button>
  );
};

export const Tabs: React.FC<{
  tabs: Array<{ id: string; label: string; icon?: string }>;
  activeTab: string;
  onTabChange: (tabId: string) => void;
}> = ({ tabs, activeTab, onTabChange }) => {
  return (
    <div className="flex border-b border-gray-700">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          onClick={() => onTabChange(tab.id)}
          className={`
            px-6 py-3 font-medium transition-all
            ${activeTab === tab.id
              ? 'text-yellow-500 border-b-2 border-yellow-500'
              : 'text-gray-400 hover:text-white'
            }
          `}
        >
          {tab.icon && <span className="mr-2">{tab.icon}</span>}
          {tab.label}
        </button>
      ))}
    </div>
  );
};

export const ProgressBar: React.FC<{
  value: number;
  max: number;
  label?: string;
  showPercentage?: boolean;
  color?: 'yellow' | 'green' | 'red' | 'blue';
  size?: 'sm' | 'md' | 'lg';
}> = ({
  value,
  max,
  label,
  showPercentage = false,
  color = 'yellow',
  size = 'md',
}) => {
  const percentage = (value / max) * 100;

  const colorClasses = {
    yellow: 'bg-gradient-to-r from-yellow-500 to-yellow-600',
    green: 'bg-gradient-to-r from-green-500 to-green-600',
    red: 'bg-gradient-to-r from-red-500 to-red-600',
    blue: 'bg-gradient-to-r from-blue-500 to-blue-600',
  };

  const sizeClasses = {
    sm: 'h-2',
    md: 'h-4',
    lg: 'h-6',
  };

  return (
    <div className="w-full">
      {(label || showPercentage) && (
        <div className="flex justify-between mb-1 text-sm">
          {label && <span className="text-gray-300">{label}</span>}
          {showPercentage && (
            <span className="text-gray-400">{Math.floor(percentage)}%</span>
          )}
        </div>
      )}
      <div className={`w-full bg-gray-700 rounded-full overflow-hidden ${sizeClasses[size]}`}>
        <div
          className={`h-full ${colorClasses[color]} transition-all duration-300`}
          style={{ width: `${Math.min(100, percentage)}%` }}
        />
      </div>
    </div>
  );
};

export const Tooltip: React.FC<{
  content: string;
  children: React.ReactNode;
  position?: 'top' | 'bottom' | 'left' | 'right';
}> = ({ content, children, position = 'top' }) => {
  const [visible, setVisible] = useState(false);

  const positionClasses = {
    top: 'bottom-full left-1/2 transform -translate-x-1/2 mb-2',
    bottom: 'top-full left-1/2 transform -translate-x-1/2 mt-2',
    left: 'right-full top-1/2 transform -translate-y-1/2 mr-2',
    right: 'left-full top-1/2 transform -translate-y-1/2 ml-2',
  };

  return (
    <div
      className="relative inline-block"
      onMouseEnter={() => setVisible(true)}
      onMouseLeave={() => setVisible(false)}
    >
      {children}
      {visible && (
        <div
          className={`
            absolute ${positionClasses[position]}
            px-3 py-2 bg-gray-900 border border-gray-700 rounded-lg
            text-sm text-white whitespace-nowrap shadow-xl z-50
          `}
        >
          {content}
        </div>
      )}
    </div>
  );
};

export const Slider: React.FC<{
  value: number;
  min: number;
  max: number;
  step?: number;
  label?: string;
  onChange: (value: number) => void;
}> = ({ value, min, max, step = 1, label, onChange }) => {
  const percentage = ((value - min) / (max - min)) * 100;

  return (
    <div className="w-full">
      {label && (
        <div className="flex justify-between mb-2 text-sm text-gray-300">
          <span>{label}</span>
          <span>{Math.floor(value)}</span>
        </div>
      )}
      <div className="relative h-3 bg-gray-700 rounded-full">
        <div
          className="absolute h-full bg-gradient-to-r from-yellow-500 to-yellow-600 rounded-full"
          style={{ width: `${percentage}%` }}
        />
        <input
          type="range"
          min={min}
          max={max}
          step={step}
          value={value}
          onChange={(e) => onChange(parseFloat(e.target.value))}
          className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
        />
        <div
          className="absolute top-1/2 transform -translate-y-1/2 w-5 h-5 bg-white rounded-full shadow-lg border-2 border-yellow-600"
          style={{ left: `calc(${percentage}% - 10px)` }}
        />
      </div>
    </div>
  );
};

export const Checkbox: React.FC<{
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
}> = ({ checked, onChange, label }) => {
  return (
    <label className="flex items-center gap-3 cursor-pointer">
      <div
        onClick={() => onChange(!checked)}
        className={`
          w-6 h-6 rounded border-2 flex items-center justify-center
          transition-all duration-200
          ${checked
            ? 'bg-yellow-600 border-yellow-600'
            : 'bg-gray-800 border-gray-600 hover:border-gray-500'
          }
        `}
      >
        {checked && <span className="text-white text-lg">✓</span>}
      </div>
      {label && <span className="text-gray-300">{label}</span>}
    </label>
  );
};

export const Select: React.FC<{
  value: string;
  options: Array<{ value: string; label: string }>;
  onChange: (value: string) => void;
  label?: string;
}> = ({ value, options, onChange, label }) => {
  return (
    <div className="w-full">
      {label && <label className="block text-sm text-gray-300 mb-2">{label}</label>}
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-4 py-2 bg-gray-800 border border-gray-700 rounded-lg text-white focus:outline-none focus:border-yellow-600"
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
};

export const LoadingSpinner: React.FC<{
  size?: 'sm' | 'md' | 'lg';
  color?: string;
}> = ({ size = 'md', color = '#fbbf24' }) => {
  const sizeMap = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
  };

  return (
    <div
      className={`${sizeMap[size]} border-4 border-gray-700 border-t-transparent rounded-full animate-spin`}
      style={{ borderTopColor: color }}
    />
  );
};
