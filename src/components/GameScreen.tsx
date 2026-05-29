import { useCallback, useState } from 'react';
import { useGameStore } from '@/stores/gameStore';
import GameCanvas from './GameCanvas';
import HackingSystem from './HackingSystem';
import { RotateCcw, Home, Zap } from 'lucide-react';

export default function GameScreen() {
  const [showHacking, setShowHacking] = useState(true);
  const [hackSuccess, setHackSuccess] = useState(false);
  
  const {
    phase,
    setPhase,
    nextWave,
    nextSector,
    addSpecialAmmo,
    resetGame,
    wave,
    sector,
  } = useGameStore();

  const handleHackComplete = useCallback((success: boolean) => {
    setShowHacking(false);
    setHackSuccess(success);
    
    if (success) {
      addSpecialAmmo(1);
    }
  }, [addSpecialAmmo]);

  const handleWaveComplete = useCallback(() => {
    if (wave >= 3) {
      nextSector();
    } else {
      nextWave();
    }
    setShowHacking(true);
    setHackSuccess(false);
  }, [wave, nextWave, nextSector]);

  const handleGameOver = useCallback(() => {
    setPhase('gameover');
  }, [setPhase]);

  const handleRetry = () => {
    resetGame();
    setShowHacking(true);
    setHackSuccess(false);
    setPhase('hacking');
  };

  return (
    <div className="min-h-screen bg-space-black flex items-center justify-center relative overflow-hidden">
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-br from-cyber-blue/5 via-transparent to-neon-pink/5" />
        <svg className="absolute inset-0 w-full h-full opacity-5">
          <defs>
            <pattern id="smallGrid" width="20" height="20" patternUnits="userSpaceOnUse">
              <path d="M 20 0 L 0 0 0 20" fill="none" stroke="#00d4ff" strokeWidth="0.5" />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#smallGrid)" />
        </svg>
      </div>

      {showHacking && phase === 'hacking' ? (
        <HackingSystem onComplete={handleHackComplete} />
      ) : (
        <div className="relative z-10 w-full max-w-4xl aspect-[4/3]">
          <GameCanvas 
            onWaveComplete={handleWaveComplete}
            onGameOver={handleGameOver}
          />

          {hackSuccess && (
            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-matrix-green/20 border border-matrix-green rounded-lg px-6 py-3 backdrop-blur-sm animate-pulse">
              <div className="flex items-center gap-2 font-orbitron text-matrix-green">
                <Zap className="w-5 h-5" />
                <span>SYSTEM BREACHED</span>
              </div>
            </div>
          )}
        </div>
      )}

      {phase === 'gameover' && (
        <div className="absolute inset-0 bg-space-black/90 flex items-center justify-center z-50">
          <div className="text-center">
            <h2 className="font-orbitron text-5xl text-neon-pink mb-4 tracking-widest">
              MISSION FAILED
            </h2>
            <p className="font-share-tech text-white/60 mb-8">
              The AI has terminated your connection
            </p>
            
            <div className="flex gap-4 justify-center">
              <button
                onClick={handleRetry}
                className="px-8 py-4 bg-gradient-to-r from-cyber-blue to-neon-pink rounded-lg font-orbitron text-white hover:scale-105 transition-transform flex items-center gap-2"
              >
                <RotateCcw className="w-5 h-5" />
                RETRY
              </button>
              
              <button
                onClick={() => setPhase('title')}
                className="px-8 py-4 bg-space-black border border-white/20 rounded-lg font-orbitron text-white/60 hover:bg-white/10 transition-colors flex items-center gap-2"
              >
                <Home className="w-5 h-5" />
                MENU
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="absolute top-4 left-4 z-20">
        <button
          onClick={() => setPhase('title')}
          className="p-2 rounded-lg bg-space-black/50 border border-white/20 hover:bg-white/10 transition-colors"
        >
          <Home className="w-5 h-5 text-white/60" />
        </button>
      </div>

      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 left-0 w-full h-2 bg-gradient-to-b from-space-black to-transparent" />
        <div className="absolute bottom-0 left-0 w-full h-2 bg-gradient-to-t from-space-black to-transparent" />
        <div className="absolute top-0 left-0 w-2 h-full bg-gradient-to-r from-space-black to-transparent" />
        <div className="absolute top-0 right-0 w-2 h-full bg-gradient-to-l from-space-black to-transparent" />
      </div>
    </div>
  );
}
