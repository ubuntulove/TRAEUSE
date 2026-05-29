import { useEffect, useState } from 'react';
import { useGameStore } from '@/stores/gameStore';
import { Cpu, Play, ShoppingCart, Map, Volume2, VolumeX } from 'lucide-react';

export default function TitleScreen() {
  const { setPhase, loadSave, initNewGame } = useGameStore();
  const [showContent, setShowContent] = useState(false);
  const [highScore, setHighScore] = useState<number>(0);
  const [soundEnabled, setSoundEnabled] = useState(true);

  useEffect(() => {
    const save = loadSave();
    if (save?.highScore) {
      setHighScore(save.highScore);
    }
    setTimeout(() => setShowContent(true), 500);
  }, [loadSave]);

  const handleStart = () => {
    initNewGame();
    setPhase('hacking');
  };

  return (
    <div className="min-h-screen bg-space-black flex flex-col items-center justify-center relative overflow-hidden">
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-b from-cyber-blue/5 via-transparent to-neon-pink/5" />
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-cyber-blue/10 via-transparent to-transparent" />
        
        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-cyber-blue/30 to-transparent animate-pulse" />
        <div className="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-neon-pink/30 to-transparent animate-pulse" />
        
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-cyber-blue/5 rounded-full blur-3xl animate-pulse" />
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-neon-pink/5 rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }} />
        
        <svg className="absolute inset-0 w-full h-full opacity-10">
          <defs>
            <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
              <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#00d4ff" strokeWidth="0.5" />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#grid)" />
        </svg>
      </div>

      <div 
        className={`relative z-10 flex flex-col items-center transition-all duration-1000 ${
          showContent ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-10'
        }`}
      >
        <div className="relative mb-4">
          <div className="absolute -inset-4 bg-cyber-blue/20 blur-xl rounded-full animate-pulse" />
          <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-cyber-blue to-neon-pink flex items-center justify-center relative">
            <Cpu className="w-12 h-12 text-white animate-pulse-glow" />
          </div>
        </div>

        <h1 className="font-orbitron text-6xl md:text-8xl font-bold text-white mb-2 tracking-wider">
          <span className="bg-clip-text text-transparent bg-gradient-to-r from-cyber-blue via-white to-neon-pink animate-pulse">
            NODE
          </span>
          <br />
          <span className="bg-clip-text text-transparent bg-gradient-to-r from-neon-pink via-white to-cyber-blue">
            BREAKER
          </span>
        </h1>

        <p className="font-share-tech text-cyber-blue/80 text-lg mb-8 tracking-widest">
          HACK THE MATRIX
        </p>

        {highScore > 0 && (
          <div className="mb-8 px-6 py-2 rounded-lg bg-space-black/50 border border-cyber-blue/30 backdrop-blur-sm">
            <p className="font-share-tech text-matrix-green text-sm">
              HIGH SCORE: {highScore.toLocaleString()}
            </p>
          </div>
        )}

        <div className="flex flex-col gap-4 mb-12">
          <button
            onClick={handleStart}
            className="group relative px-12 py-4 bg-gradient-to-r from-cyber-blue to-neon-pink rounded-lg overflow-hidden transition-all duration-300 hover:scale-105 hover:shadow-[0_0_30px_rgba(0,212,255,0.5)]"
          >
            <div className="absolute inset-0 bg-gradient-to-r from-neon-pink to-cyber-blue opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
            <div className="relative flex items-center gap-3">
              <Play className="w-6 h-6" />
              <span className="font-orbitron text-xl font-bold text-white tracking-wider">
                START MISSION
              </span>
            </div>
          </button>

          <div className="flex gap-4">
            <button
              onClick={() => setPhase('select')}
              className="px-6 py-3 bg-space-black/80 border border-cyber-blue/50 rounded-lg hover:bg-cyber-blue/20 transition-all duration-300 flex items-center gap-2"
            >
              <Map className="w-5 h-5 text-cyber-blue" />
              <span className="font-share-tech text-cyber-blue text-sm">SECTORS</span>
            </button>

            <button
              onClick={() => setPhase('shop')}
              className="px-6 py-3 bg-space-black/80 border border-neon-pink/50 rounded-lg hover:bg-neon-pink/20 transition-all duration-300 flex items-center gap-2"
            >
              <ShoppingCart className="w-5 h-5 text-neon-pink" />
              <span className="font-share-tech text-neon-pink text-sm">UPGRADES</span>
            </button>
          </div>
        </div>

        <div className="max-w-md text-center">
          <h3 className="font-orbitron text-cyber-blue text-sm mb-4 tracking-widest">
            HOW TO PLAY
          </h3>
          <div className="font-share-tech text-white/60 text-xs space-y-2">
            <p>
              <span className="text-matrix-green">HACKING:</span> Click and drag to connect nodes from START to END
            </p>
            <p>
              <span className="text-neon-pink">COMBAT:</span> WASD to move, Mouse to aim and shoot
            </p>
            <p>
              <span className="text-cyber-blue">GOAL:</span> Disable enemies with hacking, survive the sector
            </p>
          </div>
        </div>

        <button
          onClick={() => setSoundEnabled(!soundEnabled)}
          className="absolute top-6 right-6 p-3 rounded-full bg-space-black/50 border border-white/20 hover:bg-white/10 transition-all duration-300"
        >
          {soundEnabled ? (
            <Volume2 className="w-6 h-6 text-white/60" />
          ) : (
            <VolumeX className="w-6 h-6 text-white/40" />
          )}
        </button>

        <div className="absolute bottom-6 left-1/2 -translate-x-1/2 font-share-tech text-white/30 text-xs tracking-widest">
          v1.0 • CAPCOM-STYLE HACKING ACTION
        </div>
      </div>

      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 left-0 w-full h-2 bg-gradient-to-b from-black/50 to-transparent" />
        <div className="absolute bottom-0 left-0 w-full h-2 bg-gradient-to-t from-black/50 to-transparent" />
        <div className="absolute top-0 left-0 w-2 h-full bg-gradient-to-r from-black/50 to-transparent" />
        <div className="absolute top-0 right-0 w-2 h-full bg-gradient-to-l from-black/50 to-transparent" />
      </div>
    </div>
  );
}
