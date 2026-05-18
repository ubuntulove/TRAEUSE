import { COLORS } from '../constants/gameConfig';

interface EnergyBarProps {
  current: number;
  max: number;
  isLeft: boolean;
}

export function EnergyBar({ current, max, isLeft }: EnergyBarProps) {
  const percentage = (current / max) * 100;

  return (
    <div className={`flex ${isLeft ? 'flex-row' : 'flex-row-reverse'} items-center gap-3`}>
      <div className="w-48 h-4 bg-black/50 border border-white/20">
        <div
          className="h-full transition-all duration-300"
          style={{
            width: `${percentage}%`,
            backgroundColor: COLORS.ENERGY,
          }}
        />
        <div className="relative -mt-4 flex justify-center">
          <span className="text-xs text-white/80 drop-shadow">能量</span>
        </div>
      </div>
    </div>
  );
}
