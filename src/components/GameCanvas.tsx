import { useRef, useEffect, useState, useCallback } from 'react';
import { GAME_CONFIG, MECH_CONFIG, COLORS } from '../constants/gameConfig';
import { GameEngine } from '../game/GameEngine';
import { HealthBar } from './HealthBar';
import { EnergyBar } from './EnergyBar';

interface GameState {
  status: 'menu' | 'playing' | 'paused' | 'gameover';
  winner: 1 | 2 | null;
}

export function GameCanvas() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const engineRef = useRef<GameEngine | null>(null);
  const [gameState, setGameState] = useState<GameState>({ status: 'menu', winner: null });
  const [player1Hp, setPlayer1Hp] = useState(MECH_CONFIG.P1.maxHp);
  const [player2Hp, setPlayer2Hp] = useState(MECH_CONFIG.P2.maxHp);
  const [player1Energy, setPlayer1Energy] = useState(MECH_CONFIG.P1.maxEnergy);
  const [player2Energy, setPlayer2Energy] = useState(MECH_CONFIG.P2.maxEnergy);

  const handleStateChange = useCallback(
    (status: 'menu' | 'playing' | 'paused' | 'gameover', winner: 1 | 2 | null) => {
      setGameState({ status, winner });
    },
    []
  );

  useEffect(() => {
    if (!canvasRef.current) return;

    const engine = new GameEngine(canvasRef.current, handleStateChange);
    engineRef.current = engine;

    const updateUI = setInterval(() => {
      if (engineRef.current) {
        const status = engineRef.current.getStatus();
        if (status.status === 'playing' || status.status === 'gameover') {
          setPlayer1Hp(MECH_CONFIG.P1.maxHp);
          setPlayer2Hp(MECH_CONFIG.P2.maxHp);
          setPlayer1Energy(MECH_CONFIG.P1.maxEnergy);
          setPlayer2Energy(MECH_CONFIG.P2.maxEnergy);
        }
      }
    }, 100);

    return () => {
      clearInterval(updateUI);
      engine.destroy();
    };
  }, [handleStateChange]);

  const handleStartGame = () => {
    if (engineRef.current) {
      engineRef.current.start();
    }
  };

  const handleRestartGame = () => {
    if (engineRef.current) {
      engineRef.current.reset();
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4" style={{ backgroundColor: COLORS.BACKGROUND }}>
      <div className="mb-4">
        <h1 className="text-4xl font-bold text-center" style={{ color: COLORS.TEXT_GOLD, fontFamily: 'monospace' }}>
          ⚔️ PIXEL MECH ARENA ⚔️
        </h1>
      </div>

      <div className="mb-4 flex justify-between w-full max-w-4xl px-4">
        <HealthBar
          current={player1Hp}
          max={MECH_CONFIG.P1.maxHp}
          name={MECH_CONFIG.P1.name}
          color={MECH_CONFIG.P1.color}
          isLeft={true}
        />
        <div className="flex items-center">
          <span className="text-2xl font-bold" style={{ color: COLORS.TEXT_GOLD }}>VS</span>
        </div>
        <HealthBar
          current={player2Hp}
          max={MECH_CONFIG.P2.maxHp}
          name={MECH_CONFIG.P2.name}
          color={MECH_CONFIG.P2.color}
          isLeft={false}
        />
      </div>

      <div className="mb-4 flex justify-between w-full max-w-4xl px-4">
        <EnergyBar current={player1Energy} max={MECH_CONFIG.P1.maxEnergy} isLeft={true} />
        <div />
        <EnergyBar current={player2Energy} max={MECH_CONFIG.P2.maxEnergy} isLeft={false} />
      </div>

      <div className="relative">
        <canvas
          ref={canvasRef}
          width={GAME_CONFIG.CANVAS_WIDTH}
          height={GAME_CONFIG.CANVAS_HEIGHT}
          className="border-4 border-white/20 rounded-lg shadow-2xl"
          style={{ imageRendering: 'pixelated' }}
        />

        {gameState.status === 'menu' && (
          <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/70 rounded-lg">
            <h2 className="text-5xl font-bold mb-8" style={{ color: COLORS.TEXT_GOLD, fontFamily: 'monospace' }}>
              PIXEL MECH ARENA
            </h2>
            <p className="text-white mb-8 text-center max-w-md">
              双人像素风机甲对战游戏<br />
              击败对手的机甲即可获胜！
            </p>
            <button
              onClick={handleStartGame}
              className="px-8 py-4 text-2xl font-bold rounded-lg transition-all transform hover:scale-105 active:scale-95"
              style={{ 
                backgroundColor: COLORS.TEXT_GOLD, 
                color: '#1a1a2e',
                fontFamily: 'monospace',
                boxShadow: `0 0 20px ${COLORS.TEXT_GOLD}`
              }}
            >
              开始游戏
            </button>
            <div className="mt-8 text-sm text-white/70 text-center">
              <p className="mb-2" style={{ color: MECH_CONFIG.P1.color }}>玩家1: WASD移动 | J攻击 | K防御 | L大招</p>
              <p style={{ color: MECH_CONFIG.P2.color }}>玩家2: 方向键移动 | Numpad1攻击 | Numpad2防御 | Numpad3大招</p>
            </div>
          </div>
        )}

        {gameState.status === 'gameover' && (
          <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/70 rounded-lg">
            <h2 className="text-5xl font-bold mb-4" style={{ color: COLORS.TEXT_GOLD, fontFamily: 'monospace' }}>
              {gameState.winner === 1 ? '🔥 烈焰战将 胜利！ 🔥' : '❄️ 寒冰守卫 胜利！ ❄️'}
            </h2>
            <p className="text-white mb-8 text-xl">
              恭喜获胜者！
            </p>
            <button
              onClick={handleRestartGame}
              className="px-8 py-4 text-2xl font-bold rounded-lg transition-all transform hover:scale-105 active:scale-95"
              style={{ 
                backgroundColor: COLORS.TEXT_GOLD, 
                color: '#1a1a2e',
                fontFamily: 'monospace',
                boxShadow: `0 0 20px ${COLORS.TEXT_GOLD}`
              }}
            >
              再来一局
            </button>
          </div>
        )}
      </div>

      <div className="mt-4 flex gap-8 text-sm text-white/60">
        <div style={{ color: MECH_CONFIG.P1.color }}>
          <span className="font-bold">P1 - 烈焰战将</span><br />
          攻击: {MECH_CONFIG.P1.attackDamage[0]}-{MECH_CONFIG.P1.attackDamage[1]}<br />
          大招: {MECH_CONFIG.P1.specialDamage[0]}-{MECH_CONFIG.P1.specialDamage[1]}
        </div>
        <div style={{ color: MECH_CONFIG.P2.color }}>
          <span className="font-bold">P2 - 寒冰守卫</span><br />
          攻击: {MECH_CONFIG.P2.attackDamage[0]}-{MECH_CONFIG.P2.attackDamage[1]}<br />
          大招: {MECH_CONFIG.P2.specialDamage[0]}-{MECH_CONFIG.P2.specialDamage[1]}
        </div>
      </div>

      <div className="mt-4 text-xs text-white/40 text-center">
        提示: 防御姿态可以减少50%伤害，并回复少量能量
      </div>
    </div>
  );
}
