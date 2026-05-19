export interface Faction {
  id: string;
  name: string;
  description: string;
  color: string;
  emblem: string;
  abilities: FactionAbility[];
  bonuses: FactionBonus[];
  weaknesses: FactionWeakness[];
  startingUnits: string[];
  startingBuildings: string[];
  techTreeModifier: number;
  resourceModifier: number;
  buildSpeedModifier: number;
  unitHealthModifier: number;
  buildingArmorModifier: number;
  preferredStrategy: 'aggressive' | 'defensive' | 'economic' | 'balanced';
}

export interface FactionAbility {
  id: string;
  name: string;
  description: string;
  cooldown: number;
  effect: string;
  icon: string;
}

export interface FactionBonus {
  id: string;
  name: string;
  description: string;
  multiplier: number;
}

export interface FactionWeakness {
  id: string;
  name: string;
  description: string;
  multiplier: number;
}

export interface FactionRelations {
  factionA: string;
  factionB: string;
  relation: 'allied' | 'neutral' | 'hostile' | 'war';
}

export class FactionManager {
  private factions: Map<string, Faction> = new Map();
  private relations: FactionRelations[] = [];
  private playerFaction: string | null = null;
  private listeners: Map<string, Array<(data: any) => void>> = new Map();

  constructor() {
    this.initializeFactions();
  }

  private initializeFactions(): void {
    this.registerFaction({
      id: 'allied',
      name: 'Allied Nations',
      description: 'Advanced technology and superior firepower',
      color: '#4a90d9',
      emblem: '🛡️',
      abilities: [
        {
          id: 'time-stop',
          name: 'Chrono Beam',
          description: 'Advanced chronoton technology',
          cooldown: 300,
          effect: 'damage',
          icon: '⏰',
        },
        {
          id: 'spy-satellite',
          name: 'Spy Satellite',
          description: 'Reveal entire map',
          cooldown: 180,
          effect: 'reveal',
          icon: '🛰️',
        },
      ],
      bonuses: [
        {
          id: 'tech-advance',
          name: 'Technological Advantage',
          description: 'Research speed increased',
          multiplier: 1.25,
        },
      ],
      weaknesses: [
        {
          id: 'expensive',
          name: 'High Tech Cost',
          description: 'Unit costs increased',
          multiplier: 1.2,
        },
      ],
      startingUnits: ['soldier', 'scout', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'power-plant'],
      techTreeModifier: 1.25,
      resourceModifier: 1.0,
      buildSpeedModifier: 1.0,
      unitHealthModifier: 1.0,
      buildingArmorModifier: 1.0,
      preferredStrategy: 'balanced',
    });

    this.registerFaction({
      id: 'soviet',
      name: 'Soviet Empire',
      description: 'Massive army and overwhelming force',
      color: '#cc0000',
      emblem: '☭',
      abilities: [
        {
          id: 'iron-curtain',
          name: 'Iron Curtain',
          description: 'Make units invulnerable temporarily',
          cooldown: 240,
          effect: 'invulnerable',
          icon: '🛡️',
        },
        {
          id: 'atomic-bomb',
          name: 'Atomic Bomb',
          description: 'Devastating area damage',
          cooldown: 600,
          effect: 'nuke',
          icon: '☢️',
        },
      ],
      bonuses: [
        {
          id: 'mass-production',
          name: 'Mass Production',
          description: 'Unit production speed increased',
          multiplier: 1.3,
        },
      ],
      weaknesses: [
        {
          id: 'slow-tech',
          name: 'Tech Lag',
          description: 'Research speed decreased',
          multiplier: 0.8,
        },
      ],
      startingUnits: ['soldier', 'tank', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'factory', 'power-plant'],
      techTreeModifier: 0.8,
      resourceModifier: 1.1,
      buildSpeedModifier: 1.3,
      unitHealthModifier: 1.2,
      buildingArmorModifier: 1.1,
      preferredStrategy: 'aggressive',
    });

    this.registerFaction({
      id: 'yuri',
      name: 'Yuri\'s Army',
      description: 'Psychic powers and genetic manipulation',
      color: '#00ff00',
      emblem: '🧬',
      abilities: [
        {
          id: 'psychic-dominance',
          name: 'Psychic Dominance',
          description: 'Mind control enemy units',
          cooldown: 360,
          effect: 'mind-control',
          icon: '🧠',
        },
        {
          id: 'gravity-beams',
          name: 'Gravity Beams',
          description: 'Devastating psychic attack',
          cooldown: 300,
          effect: 'damage',
          icon: '🌌',
        },
      ],
      bonuses: [
        {
          id: 'cheap-units',
          name: 'Genetic Efficiency',
          description: 'Unit costs decreased',
          multiplier: 0.8,
        },
      ],
      weaknesses: [
        {
          id: 'weak-defense',
          name: 'Vulnerable Bases',
          description: 'Building armor decreased',
          multiplier: 0.8,
        },
      ],
      startingUnits: ['soldier', 'scout', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'refinery', 'power-plant'],
      techTreeModifier: 1.0,
      resourceModifier: 1.2,
      buildSpeedModifier: 1.0,
      unitHealthModifier: 0.9,
      buildingArmorModifier: 0.8,
      preferredStrategy: 'economic',
    });

    this.registerFaction({
      id: 'empire',
      name: 'Empire of the Rising Sun',
      description: 'Naval superiority and ninja warriors',
      color: '#ff6600',
      emblem: '⛩️',
      abilities: [
        {
          id: 'bamboo-strike',
          name: 'Bamboo Strike',
          description: 'Instant ninja ambush',
          cooldown: 200,
          effect: 'damage',
          icon: '🎋',
        },
        {
          id: 'wave-force',
          name: 'Wave Force',
          description: 'Naval artillery barrage',
          cooldown: 280,
          effect: 'damage',
          icon: '🌊',
        },
      ],
      bonuses: [
        {
          id: 'naval-power',
          name: 'Naval Mastery',
          description: 'Naval units stronger',
          multiplier: 1.4,
        },
      ],
      weaknesses: [
        {
          id: 'air-weakness',
          name: 'Air Vulnerability',
          description: 'Air defense weakened',
          multiplier: 0.7,
        },
      ],
      startingUnits: ['soldier', 'scout', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'naval-yard', 'power-plant'],
      techTreeModifier: 1.0,
      resourceModifier: 1.0,
      buildSpeedModifier: 1.1,
      unitHealthModifier: 1.0,
      buildingArmorModifier: 1.0,
      preferredStrategy: 'defensive',
    });
  }

  public registerFaction(faction: Faction): void {
    this.factions.set(faction.id, faction);
  }

  public getFaction(id: string): Faction | undefined {
    return this.factions.get(id);
  }

  public getAllFactions(): Faction[] {
    return Array.from(this.factions.values());
  }

  public getPlayerFaction(): Faction | null {
    return this.playerFaction ? this.factions.get(this.playerFaction) || null : null;
  }

  public setPlayerFaction(factionId: string): boolean {
    if (!this.factions.has(factionId)) {
      return false;
    }
    this.playerFaction = factionId;
    this.emit('player-faction-changed', { faction: this.factions.get(factionId) });
    return true;
  }

  public setRelation(factionA: string, factionB: string, relation: FactionRelations['relation']): void {
    const existingIndex = this.relations.findIndex(
      r => (r.factionA === factionA && r.factionB === factionB) ||
           (r.factionA === factionB && r.factionB === factionA)
    );

    if (existingIndex !== -1) {
      this.relations[existingIndex].relation = relation;
    } else {
      this.relations.push({ factionA, factionB, relation });
    }

    this.emit('relation-changed', { factionA, factionB, relation });
  }

  public getRelation(factionA: string, factionB: string): FactionRelations['relation'] {
    const rel = this.relations.find(
      r => (r.factionA === factionA && r.factionB === factionB) ||
           (r.factionA === factionB && r.factionB === factionA)
    );
    return rel?.relation || 'neutral';
  }

  public areAllied(factionA: string, factionB: string): boolean {
    return this.getRelation(factionA, factionB) === 'allied';
  }

  public areEnemies(factionA: string, factionB: string): boolean {
    const rel = this.getRelation(factionA, factionB);
    return rel === 'hostile' || rel === 'war';
  }

  public declareWar(factionA: string, factionB: string): void {
    this.setRelation(factionA, factionB, 'war');
  }

  public formAlliance(factionA: string, factionB: string): void {
    this.setRelation(factionA, factionB, 'allied');
  }

  public breakAlliance(factionA: string, factionB: string): void {
    this.setRelation(factionA, factionB, 'neutral');
  }

  public getFactionsByRelation(faction: string, relation: FactionRelations['relation']): Faction[] {
    return this.getAllFactions().filter(f => {
      if (f.id === faction) return false;
      return this.getRelation(faction, f.id) === relation;
    });
  }

  public getFactionModifier(factionId: string, modifierType: 'tech' | 'resource' | 'buildSpeed' | 'unitHealth' | 'buildingArmor'): number {
    const faction = this.factions.get(factionId);
    if (!faction) return 1.0;

    switch (modifierType) {
      case 'tech':
        return faction.techTreeModifier;
      case 'resource':
        return faction.resourceModifier;
      case 'buildSpeed':
        return faction.buildSpeedModifier;
      case 'unitHealth':
        return faction.unitHealthModifier;
      case 'buildingArmor':
        return faction.buildingArmorModifier;
      default:
        return 1.0;
    }
  }

  public applyFactionModifiers(factionId: string, baseValue: number, modifierType: 'tech' | 'resource' | 'buildSpeed' | 'unitHealth' | 'buildingArmor'): number {
    return baseValue * this.getFactionModifier(factionId, modifierType);
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

  public clearRelations(): void {
    this.relations = [];
  }

  public toJSON(): object {
    return {
      playerFaction: this.playerFaction,
      relations: this.relations,
    };
  }

  public fromJSON(data: any): void {
    if (data.playerFaction) {
      this.playerFaction = data.playerFaction;
    }

    if (data.relations) {
      this.relations = data.relations;
    }
  }
}

export class FactionGenerator {
  private factionManager: FactionManager;

  constructor(factionManager: FactionManager) {
    this.factionManager = factionManager;
  }

  public generateRandomFaction(): Faction {
    const colors = ['#ff0000', '#00ff00', '#0000ff', '#ffff00', '#ff00ff', '#00ffff', '#ff8800', '#88ff00'];
    const emblems = ['⚔️', '🏹', '🔱', '⚜️', '🗡️', '🛡️', '🎖️', '👑'];

    const strategies: Array<'aggressive' | 'defensive' | 'economic' | 'balanced'> = ['aggressive', 'defensive', 'economic', 'balanced'];

    const factionId = `custom-${Date.now()}`;
    const name = `Custom Faction ${Math.floor(Math.random() * 1000)}`;
    const color = colors[Math.floor(Math.random() * colors.length)];
    const emblem = emblems[Math.floor(Math.random() * emblems.length)];

    const faction: Faction = {
      id: factionId,
      name,
      description: 'A custom generated faction',
      color,
      emblem,
      abilities: [],
      bonuses: [],
      weaknesses: [],
      startingUnits: ['soldier', 'scout', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'power-plant'],
      techTreeModifier: 0.8 + Math.random() * 0.4,
      resourceModifier: 0.8 + Math.random() * 0.4,
      buildSpeedModifier: 0.8 + Math.random() * 0.4,
      unitHealthModifier: 0.8 + Math.random() * 0.4,
      buildingArmorModifier: 0.8 + Math.random() * 0.4,
      preferredStrategy: strategies[Math.floor(Math.random() * strategies.length)],
    };

    return faction;
  }

  public generateBalancedFaction(): Faction {
    return {
      id: `balanced-${Date.now()}`,
      name: 'Balanced Force',
      description: 'A faction with balanced stats',
      color: '#888888',
      emblem: '⚖️',
      abilities: [],
      bonuses: [],
      weaknesses: [],
      startingUnits: ['soldier', 'scout', 'miner'],
      startingBuildings: ['command-center', 'barracks', 'power-plant'],
      techTreeModifier: 1.0,
      resourceModifier: 1.0,
      buildSpeedModifier: 1.0,
      unitHealthModifier: 1.0,
      buildingArmorModifier: 1.0,
      preferredStrategy: 'balanced',
    };
  }
}
