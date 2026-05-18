import { COLORS } from '../constants/gameConfig';

interface HealthBarProps {
  current: number;
  max: number;
  name: string;
  color: string;
  isLeft: boolean;
}

export function HealthBar({ current, max, name, color, isLeft }: HealthBarProps) {
  const percentage = (current / max) * 100;
  const barColor = percentage > 60 ? COLORS.HP_HIGH : percentage > 30 ? COLORS.HP_MEDIUM : COLORS.HP_LOW;

  return (
    <div className={`flex ${isLeft ? 'flex-row' : 'flex-row-reverse'} items-center gap-3`}>
      <div className="flex flex-col items-center">
        <span className={`font-bold text-lg ${isLeft ? 'text-left' : 'text-right'}`} style={{ color }}>
          {name}
        </span>
        <div className="relative w-64 h-8 bg-black/50 border-2 border-white/30">
          <div
            className="h-full transition-all duration-200"
            style={{
              width: `${percentage}%`,
              backgroundColor: barColor,
            }}
          />
          <div className="absolute inset-0 flex items-center justify-center">
            <span className="font-bold text-white text-sm drop-shadow-lg">
              {current} / {max}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
