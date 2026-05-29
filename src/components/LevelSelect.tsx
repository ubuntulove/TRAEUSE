import { useGameStore } from '@/stores/gameStore';
import { ArrowLeft, MapPin, Lock, Zap, Cpu, Shield, Play } from 'lucide-react';

export default function LevelSelect() {
  const { setPhase, sector, setPlayerPosition, resetGame } = useGameStore();

  const sectors = [
    {
      id: 1,
      name: 'SECTOR A-1',
      subtitle: 'Research Wing',
      description: 'Training grounds. Basic enemy types.',
      icon: MapPin,
      color: 'cyber-blue',
      enemies: 3,
      difficulty: 1,
    },
    {
      id: 2,
      name: 'SECTOR A-2',
      subtitle: 'Security Level',
      description: 'Increased patrols. Armored units appear.',
      icon: Shield,
      color: 'warning-amber',
      enemies: 4,
      difficulty: 2,
    },
    {
      id: 3,
      name: 'SECTOR B-1',
      subtitle: 'Server Room',
      description: 'Dense network. Fast enemy types.',
      icon: Cpu,
      color: 'neon-pink',
      enemies: 5,
      difficulty: 3,
    },
    {
      id: 4,
      name: 'SECTOR B-2',
      subtitle: 'Command Center',
      description: 'Heavy resistance. Mixed enemy types.',
      icon: Zap,
      color: 'neon-pink',
      enemies: 6,
      difficulty: 4,
    },
    {
      id: 5,
      name: 'SECTOR C-1',
      subtitle: 'Core Reactor',
      description: 'Maximum alert. Boss-level enemies.',
      icon: Zap,
      color: 'neon-pink',
      enemies: 7,
      difficulty: 5,
    },
  ];

  const handleSelectSector = (selectedSector: number) => {
    if (selectedSector <= sector) {
      const store = useGameStore.getState();
      store.sector = selectedSector;
      store.wave = 1;
      store.initNewGame();
      setPlayerPosition(400, 400);
      setPhase('hacking');
    }
  };

  return (
    <div className="min-h-screen bg-space-black flex items-center justify-center relative overflow-hidden p-8">
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-br from-cyber-blue/5 via-transparent to-neon-pink/5" />
        <svg className="absolute inset-0 w-full h-full opacity-5">
          <defs>
            <pattern id="gridSelect" width="60" height="60" patternUnits="userSpaceOnUse">
              <path d="M 60 0 L 0 0 0 60" fill="none" stroke="#00d4ff" strokeWidth="1" />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#gridSelect)" />
        </svg>
      </div>

      <button
        onClick={() => setPhase('title')}
        className="absolute top-6 left-6 p-3 rounded-lg bg-space-black/50 border border-white/20 hover:bg-white/10 transition-colors flex items-center gap-2 z-20"
      >
        <ArrowLeft className="w-5 h-5 text-white/60" />
        <span className="font-share-tech text-white/60 text-sm">BACK</span>
      </button>

      <div className="relative z-10 max-w-5xl w-full">
        <div className="text-center mb-12">
          <h1 className="font-orbitron text-4xl text-white mb-4 tracking-widest">
            <span className="bg-clip-text text-transparent bg-gradient-to-r from-cyber-blue to-neon-pink">
              SELECT SECTOR
            </span>
          </h1>
          <p className="font-share-tech text-white/60">
            Choose your mission. Complete sectors to unlock new areas.
          </p>
        </div>

        <div className="space-y-4">
          {sectors.map((sec, index) => {
            const Icon = sec.icon;
            const isUnlocked = sec.id <= sector;
            const isCurrent = sec.id === sector;
            const prevSector = sectors[index - 1];
            const isPrevUnlocked = !prevSector || prevSector.id < sector;

            return (
              <div key={sec.id} className="relative">
                {index > 0 && (
                  <div className={`absolute left-12 -top-4 w-0.5 h-4 ${
                    isPrevUnlocked ? 'bg-cyber-blue/50' : 'bg-white/10'
                  }`} />
                )}
                
                <button
                  onClick={() => handleSelectSector(sec.id)}
                  disabled={!isUnlocked}
                  className={`w-full p-6 rounded-lg border transition-all duration-300 text-left ${
                    isUnlocked
                      ? 'bg-space-black/80 border-' + sec.color + '/50 hover:bg-space-black hover:border-' + sec.color + ' hover:scale-[1.02] cursor-pointer'
                      : 'bg-space-black/40 border-white/10 cursor-not-allowed opacity-50'
                  }`}
                  style={{
                    borderColor: isUnlocked ? sec.color + '50' : undefined,
                  }}
                >
                  <div className="flex items-center gap-6">
                    <div className={`relative`}>
                      <div className={`w-16 h-16 rounded-lg flex items-center justify-center ${
                        isUnlocked ? 'bg-' + sec.color + '/20' : 'bg-white/10'
                      }`}>
                        {isUnlocked ? (
                          <Icon className={`w-8 h-8 text-${sec.color}`} />
                        ) : (
                          <Lock className="w-8 h-8 text-white/30" />
                        )}
                      </div>
                      {isCurrent && (
                        <div className="absolute -top-1 -right-1 w-3 h-3 rounded-full bg-matrix-green animate-pulse" />
                      )}
                    </div>

                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-1">
                        <h3 className="font-orbitron text-lg text-white">
                          {sec.name}
                        </h3>
                        {isCurrent && (
                          <span className="px-2 py-0.5 rounded text-xs bg-matrix-green/20 text-matrix-green font-share-tech">
                            CURRENT
                          </span>
                        )}
                        {isUnlocked && !isCurrent && isPrevUnlocked && (
                          <span className="px-2 py-0.5 rounded text-xs bg-cyber-blue/20 text-cyber-blue font-share-tech">
                            AVAILABLE
                          </span>
                        )}
                      </div>
                      <p className="font-share-tech text-sm text-white/40 mb-2">
                        {sec.subtitle}
                      </p>
                      <p className="font-share-tech text-xs text-white/60">
                        {sec.description}
                      </p>
                    </div>

                    <div className="text-right">
                      <div className="font-share-tech text-sm text-white/60 mb-1">
                        {sec.enemies} ENEMIES
                      </div>
                      <div className="flex gap-1 justify-end">
                        {Array.from({ length: 5 }).map((_, i) => (
                          <div
                            key={i}
                            className={`w-2 h-2 rounded-full ${
                              i < sec.difficulty ? sec.color : 'bg-white/20'
                            }`}
                          />
                        ))}
                      </div>
                    </div>

                    {isUnlocked && isPrevUnlocked && (
                      <div className="ml-4">
                        <div className="w-10 h-10 rounded-lg bg-cyber-blue/20 flex items-center justify-center">
                          <Play className="w-5 h-5 text-cyber-blue" />
                        </div>
                      </div>
                    )}
                  </div>
                </button>
              </div>
            );
          })}
        </div>

        <div className="mt-12 text-center">
          <p className="font-share-tech text-white/40 text-sm">
            {sector >= 5 ? (
              <span className="text-matrix-green">ALL SECTORS UNLOCKED</span>
            ) : (
              <>Complete <span className="text-cyber-blue">Sector {sector}</span> to unlock the next area</>
            )}
          </p>
        </div>
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
