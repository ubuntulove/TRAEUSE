export interface Technology {
  id: string;
  name: string;
  description: string;
  cost: { credits: number; power: number; ore: number; gems?: number };
  time: number;
  prerequisites: string[];
  unlocks: {
    buildings?: string[];
    units?: string[];
    upgrades?: string[];
    abilities?: string[];
  };
  researchPoints: number;
  maxLevel: number;
  icon: string;
  category: 'offense' | 'defense' | 'economy' | 'support' | 'special';
}

export interface ResearchProgress {
  technologyId: string;
  progress: number;
  startTime: number;
  assignedScientists: number;
}

export class TechTree {
  private technologies: Map<string, Technology> = new Map();
  private unlockedTechnologies: Set<string> = new Set();
  private inProgressResearch: Map<string, ResearchProgress> = new Map();
  private currentResearch: string | null = null;
  private researchSpeed: number = 1.0;
  private scientists: number = 1;
  private listeners: Map<string, Array<(data: any) => void>> = new Map();

  constructor() {
    this.initializeTechnologies();
  }

  private initializeTechnologies(): void {
    this.registerTechnology({
      id: 'advanced-power',
      name: 'Advanced Power Generation',
      description: 'Increases power plant output by 50%',
      cost: { credits: 500, power: 0, ore: 200 },
      time: 60,
      prerequisites: [],
      unlocks: {
        upgrades: ['power-boost'],
      },
      researchPoints: 100,
      maxLevel: 3,
      icon: '⚡',
      category: 'economy',
    });

    this.registerTechnology({
      id: 'advanced-weapons',
      name: 'Advanced Weapons',
      description: 'Increases unit damage by 25%',
      cost: { credits: 800, power: 0, ore: 300 },
      time: 90,
      prerequisites: [],
      unlocks: {
        upgrades: ['damage-boost'],
      },
      researchPoints: 150,
      maxLevel: 3,
      icon: '🔫',
      category: 'offense',
    });

    this.registerTechnology({
      id: 'fortification',
      name: 'Fortification',
      description: 'Increases building armor by 30%',
      cost: { credits: 600, power: 0, ore: 250 },
      time: 75,
      prerequisites: [],
      unlocks: {
        upgrades: ['armor-boost'],
      },
      researchPoints: 120,
      maxLevel: 3,
      icon: '🏰',
      category: 'defense',
    });

    this.registerTechnology({
      id: 'speed-boost',
      name: 'Speed Boost',
      description: 'Increases unit movement speed by 20%',
      cost: { credits: 400, power: 0, ore: 150 },
      time: 45,
      prerequisites: [],
      unlocks: {
        upgrades: ['speed-boost'],
      },
      researchPoints: 80,
      maxLevel: 3,
      icon: '💨',
      category: 'support',
    });

    this.registerTechnology({
      id: 'radar',
      name: 'Advanced Radar',
      description: 'Increases sight range by 50%',
      cost: { credits: 700, power: 0, ore: 300 },
      time: 80,
      prerequisites: [],
      unlocks: {
        upgrades: ['sight-boost'],
      },
      researchPoints: 140,
      maxLevel: 3,
      icon: '📡',
      category: 'support',
    });

    this.registerTechnology({
      id: 'heavy-tanks',
      name: 'Heavy Tanks',
      description: 'Unlocks production of heavy tanks',
      cost: { credits: 1500, power: 0, ore: 500 },
      time: 120,
      prerequisites: ['advanced-weapons', 'fortification'],
      unlocks: {
        units: ['heavy-tank'],
      },
      researchPoints: 200,
      maxLevel: 1,
      icon: '🎖️',
      category: 'offense',
    });

    this.registerTechnology({
      id: 'aircraft-carrier',
      name: 'Aircraft Technology',
      description: 'Unlocks production of aircraft units',
      cost: { credits: 1200, power: 0, ore: 400 },
      time: 100,
      prerequisites: ['advanced-power'],
      unlocks: {
        units: ['helicopter', 'bomber'],
      },
      researchPoints: 180,
      maxLevel: 1,
      icon: '✈️',
      category: 'offense',
    });

    this.registerTechnology({
      id: 'super-weapon',
      name: 'Super Weapon',
      description: 'Unlocks the devastating super weapon',
      cost: { credits: 5000, power: 0, ore: 2000, gems: 100 },
      time: 300,
      prerequisites: ['heavy-tanks', 'aircraft-carrier'],
      unlocks: {
        buildings: ['super-weapon'],
      },
      researchPoints: 500,
      maxLevel: 1,
      icon: '☄️',
      category: 'special',
    });

    this.registerTechnology({
      id: ' cloaking',
      name: 'Cloaking Technology',
      description: 'Units become invisible to enemies',
      cost: { credits: 2000, power: 0, ore: 800, gems: 50 },
      time: 150,
      prerequisites: ['radar'],
      unlocks: {
        upgrades: ['cloak'],
      },
      researchPoints: 250,
      maxLevel: 1,
      icon: '👻',
      category: 'special',
    });

    this.registerTechnology({
      id: 'nanobots',
      name: 'Nanobot Repair',
      description: 'Buildings repair themselves over time',
      cost: { credits: 1800, power: 0, ore: 600 },
      time: 130,
      prerequisites: ['fortification'],
      unlocks: {
        upgrades: ['self-repair'],
      },
      researchPoints: 220,
      maxLevel: 1,
      icon: '🤖',
      category: 'defense',
    });
  }

  public registerTechnology(tech: Technology): void {
    this.technologies.set(tech.id, tech);
  }

  public getTechnology(id: string): Technology | undefined {
    return this.technologies.get(id);
  }

  public getAllTechnologies(): Technology[] {
    return Array.from(this.technologies.values());
  }

  public isUnlocked(id: string): boolean {
    return this.unlockedTechnologies.has(id);
  }

  public getUnlockedTechnologies(): Technology[] {
    return Array.from(this.unlockedTechnologies).map(id => this.technologies.get(id)!);
  }

  public canResearch(id: string): boolean {
    const tech = this.technologies.get(id);
    if (!tech) return false;

    if (this.unlockedTechnologies.has(id)) return false;

    for (const prereq of tech.prerequisites) {
      if (!this.unlockedTechnologies.has(prereq)) {
        return false;
      }
    }

    return true;
  }

  public startResearch(id: string): boolean {
    if (!this.canResearch(id)) {
      return false;
    }

    if (this.currentResearch) {
      this.cancelResearch();
    }

    const tech = this.technologies.get(id)!;

    this.currentResearch = id;
    this.inProgressResearch.set(id, {
      technologyId: id,
      progress: 0,
      startTime: Date.now(),
      assignedScientists: this.scientists,
    });

    this.emit('research-started', { technologyId: id });

    return true;
  }

  public cancelResearch(): void {
    if (!this.currentResearch) return;

    this.inProgressResearch.delete(this.currentResearch);
    this.emit('research-cancelled', { technologyId: this.currentResearch });
    this.currentResearch = null;
  }

  public update(deltaTime: number): void {
    if (!this.currentResearch) return;

    const progress = this.inProgressResearch.get(this.currentResearch);
    if (!progress) return;

    const tech = this.technologies.get(this.currentResearch)!;
    const researchTime = tech.time / this.researchSpeed;
    const progressPerSecond = (this.scientists * progress.assignedScientists) / researchTime;

    progress.progress += progressPerSecond * deltaTime;

    if (progress.progress >= 1) {
      this.completeResearch(this.currentResearch);
    }

    this.emit('research-progress', {
      technologyId: this.currentResearch,
      progress: progress.progress,
    });
  }

  private completeResearch(id: string): void {
    const tech = this.technologies.get(id);
    if (!tech) return;

    this.unlockedTechnologies.add(id);
    this.inProgressResearch.delete(id);
    this.currentResearch = null;

    this.emit('research-completed', {
      technologyId: id,
      unlocks: tech.unlocks,
    });
  }

  public getCurrentResearch(): string | null {
    return this.currentResearch;
  }

  public getResearchProgress(id: string): number {
    const progress = this.inProgressResearch.get(id);
    return progress?.progress || 0;
  }

  public getAvailableResearch(): Technology[] {
    return this.getAllTechnologies().filter(tech => this.canResearch(tech.id));
  }

  public getResearchableNow(resources: { credits: number; power: number; ore: number; gems?: number }): Technology[] {
    return this.getAvailableResearch().filter(tech => {
      if (tech.cost.credits > resources.credits) return false;
      if (tech.cost.power > (resources.power || 0)) return false;
      if (tech.cost.ore > resources.ore) return false;
      if ((tech.cost.gems || 0) > (resources.gems || 0)) return false;
      return true;
    });
  }

  public setResearchSpeed(speed: number): void {
    this.researchSpeed = Math.max(0.1, Math.min(10, speed));
  }

  public getResearchSpeed(): number {
    return this.researchSpeed;
  }

  public setScientists(count: number): void {
    this.scientists = Math.max(1, Math.min(10, count));
  }

  public getScientists(): number {
    return this.scientists;
  }

  public assignScientists(id: string, count: number): void {
    const progress = this.inProgressResearch.get(id);
    if (progress) {
      progress.assignedScientists = Math.max(1, Math.min(10, count));
    }
  }

  public getTechnologiesByCategory(category: Technology['category']): Technology[] {
    return this.getAllTechnologies().filter(tech => tech.category === category);
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

  public clear(): void {
    this.unlockedTechnologies.clear();
    this.inProgressResearch.clear();
    this.currentResearch = null;
  }

  public reset(): void {
    this.clear();
  }

  public toJSON(): object {
    return {
      unlockedTechnologies: Array.from(this.unlockedTechnologies),
      currentResearch: this.currentResearch,
      researchSpeed: this.researchSpeed,
      scientists: this.scientists,
    };
  }

  public fromJSON(data: any): void {
    if (data.unlockedTechnologies) {
      this.unlockedTechnologies = new Set(data.unlockedTechnologies);
    }

    if (data.researchSpeed) {
      this.researchSpeed = data.researchSpeed;
    }

    if (data.scientists) {
      this.scientists = data.scientists;
    }
  }
}
