import { useGameStore } from '@/stores/gameStore';
import { ArrowLeft, Zap, Crosshair, Shield, Cpu, Check } from 'lucide-react';

export default function UpgradeShop() {
  const {
    setPhase,
    totalDataCollected,
    weaponsUnlocked,
    unlockWeapon,
    activeWeapon,
    setActiveWeapon,
    sector,
    score,
  } = useGameStore();

  const weapons = [
    {
      id: 'blaster',
      name: 'BLASTER',
      description: 'Standard issue pulse weapon',
      cost: 0,
      icon: Crosshair,
      color: 'cyber-blue',
    },
    {
      id: 'plasma',
      name: 'PLASMA CASTER',
      description: 'High damage energy weapon',
      cost: 500,
      icon: Zap,
      color: 'neon-pink',
    },
    {
      id: 'laser',
      name: 'PRECISION LASER',
      description: 'Fast firing accurate shots',
      cost: 1000,
      icon: Cpu,
      color: 'matrix-green',
    },
    {
      id: 'shield',
      name: 'ENERGY SHIELD',
      description: 'Reduces incoming damage',
      cost: 1500,
      icon: Shield,
      color: 'warning-amber',
    },
  ];

  const upgrades = [
    {
      id: 'health',
      name: 'HEALTH BOOST',
      description: '+25 Max Health',
      cost: 300,
      color: 'neon-pink',
    },
    {
      id: 'speed',
      name: 'THRUSTERS',
      description: '+20% Movement Speed',
      cost: 400,
      color: 'cyber-blue',
    },
    {
      id: 'damage',
      name: 'OVERCLOCK',
      description: '+15% Damage Output',
      cost: 600,
      color: 'warning-amber',
    },
  ];

  const handlePurchase = (item: { id: string; cost: number }) => {
    if (totalDataCollected >= item.cost && !weaponsUnlocked.includes(item.id)) {
      unlockWeapon(item.id);
    }
  };

  return (
    <div className="min-h-screen bg-space-black flex items-center justify-center relative overflow-hidden p-8">
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-gradient-to-br from-neon-pink/5 via-transparent to-cyber-blue/5" />
        <svg className="absolute inset-0 w-full h-full opacity-5">
          <defs>
            <pattern id="gridShop" width="40" height="40" patternUnits="userSpaceOnUse">
              <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#ff0066" strokeWidth="0.5" />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#gridShop)" />
        </svg>
      </div>

      <button
        onClick={() => setPhase('title')}
        className="absolute top-6 left-6 p-3 rounded-lg bg-space-black/50 border border-white/20 hover:bg-white/10 transition-colors flex items-center gap-2 z-20"
      >
        <ArrowLeft className="w-5 h-5 text-white/60" />
        <span className="font-share-tech text-white/60 text-sm">BACK</span>
      </button>

      <div className="relative z-10 max-w-6xl w-full">
        <div className="text-center mb-12">
          <h1 className="font-orbitron text-4xl text-white mb-4 tracking-widest">
            <span className="bg-clip-text text-transparent bg-gradient-to-r from-neon-pink to-cyber-blue">
              UPGRADE TERMINAL
            </span>
          </h1>
          <div className="inline-flex items-center gap-3 px-6 py-3 rounded-lg bg-space-black/50 border border-cyber-blue/30 backdrop-blur-sm">
            <Zap className="w-5 h-5 text-cyber-blue" />
            <span className="font-share-tech text-cyber-blue text-lg">
              {totalDataCollected.toLocaleString()} DATA
            </span>
          </div>
        </div>

        <div className="grid md:grid-cols-2 gap-12 mb-12">
          <div>
            <h2 className="font-orbitron text-xl text-cyber-blue mb-6 tracking-wider">
              WEAPONS
            </h2>
            <div className="space-y-4">
              {weapons.map((weapon) => {
                const Icon = weapon.icon;
                const isUnlocked = weaponsUnlocked.includes(weapon.id);
                const canAfford = totalDataCollected >= weapon.cost;
                const isEquipped = activeWeapon === weapon.id;

                return (
                  <div
                    key={weapon.id}
                    className={`relative p-6 rounded-lg border transition-all duration-300 ${
                      isUnlocked
                        ? 'bg-space-black/80 border-' + weapon.color + '/50'
                        : 'bg-space-black/40 border-white/10'
                    } ${isEquipped ? 'ring-2 ring-matrix-green' : ''}`}
                    style={{
                      borderColor: isUnlocked ? `var(--tw-ring-color, rgba(0,212,255,0.5))` : undefined,
                    }}
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex items-start gap-4">
                        <div className={`p-3 rounded-lg bg-${weapon.color}/20`}>
                          <Icon className={`w-6 h-6 text-${weapon.color}`} />
                        </div>
                        <div>
                          <h3 className="font-orbitron text-white text-lg mb-1">
                            {weapon.name}
                          </h3>
                          <p className="font-share-tech text-white/60 text-sm">
                            {weapon.description}
                          </p>
                        </div>
                      </div>
                      
                      <div className="flex flex-col items-end gap-2">
                        {isUnlocked ? (
                          <>
                            {isEquipped && (
                              <span className="px-3 py-1 rounded bg-matrix-green/20 text-matrix-green text-xs font-share-tech">
                                EQUIPPED
                              </span>
                            )}
                            {!isEquipped && (
                              <button
                                onClick={() => setActiveWeapon(weapon.id)}
                                className="px-3 py-1 rounded bg-cyber-blue/20 text-cyber-blue text-xs font-share-tech hover:bg-cyber-blue/30 transition-colors"
                              >
                                EQUIP
                              </button>
                            )}
                          </>
                        ) : (
                          <button
                            onClick={() => handlePurchase(weapon)}
                            disabled={!canAfford}
                            className={`px-4 py-2 rounded font-share-tech text-sm ${
                              canAfford
                                ? 'bg-neon-pink/20 text-neon-pink hover:bg-neon-pink/30 transition-colors'
                                : 'bg-white/10 text-white/30 cursor-not-allowed'
                            }`}
                          >
                            {weapon.cost === 0 ? 'FREE' : `${weapon.cost} DATA`}
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div>
            <h2 className="font-orbitron text-xl text-neon-pink mb-6 tracking-wider">
              SYSTEM UPGRADES
            </h2>
            <div className="space-y-4">
              {upgrades.map((upgrade) => {
                const isUnlocked = weaponsUnlocked.includes(upgrade.id);
                const canAfford = totalDataCollected >= upgrade.cost;

                return (
                  <div
                    key={upgrade.id}
                    className={`p-6 rounded-lg border transition-all duration-300 ${
                      isUnlocked
                        ? 'bg-space-black/80 border-matrix-green/50'
                        : 'bg-space-black/40 border-white/10'
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h3 className="font-orbitron text-white text-lg mb-1">
                          {upgrade.name}
                        </h3>
                        <p className="font-share-tech text-white/60 text-sm">
                          {upgrade.description}
                        </p>
                      </div>
                      
                      {isUnlocked ? (
                        <div className="flex items-center gap-2 text-matrix-green">
                          <Check className="w-5 h-5" />
                          <span className="font-share-tech text-sm">OWNED</span>
                        </div>
                      ) : (
                        <button
                          onClick={() => handlePurchase(upgrade)}
                          disabled={!canAfford}
                          className={`px-4 py-2 rounded font-share-tech text-sm ${
                            canAfford
                              ? 'bg-cyber-blue/20 text-cyber-blue hover:bg-cyber-blue/30 transition-colors'
                              : 'bg-white/10 text-white/30 cursor-not-allowed'
                          }`}
                        >
                          {upgrade.cost} DATA
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>

            <div className="mt-8 p-6 rounded-lg bg-cyber-blue/10 border border-cyber-blue/30">
              <h3 className="font-orbitron text-cyber-blue text-sm mb-4 tracking-wider">
                PROGRESS
              </h3>
              <div className="space-y-2 font-share-tech text-sm">
                <div className="flex justify-between text-white/60">
                  <span>Current Sector:</span>
                  <span className="text-cyber-blue">{sector}</span>
                </div>
                <div className="flex justify-between text-white/60">
                  <span>High Score:</span>
                  <span className="text-matrix-green">{score.toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 left-0 w-full h-2 bg-gradient-to-b from-space-black to-transparent" />
        <div className="absolute bottom-0 left-0 w-full h-2 bg-gradient-to-t from-space-black to-transparent" />
      </div>
    </div>
  );
}
