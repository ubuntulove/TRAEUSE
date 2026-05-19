import { Vector2D } from '../../entities/Entity';

export interface MissionObjective {
  id: string;
  type: 'primary' | 'secondary' | 'bonus';
  name: string;
  description: string;
  target: string;
  targetAmount: number;
  currentAmount: number;
  completed: boolean;
  bonusReward?: { credits?: number; ore?: number; gems?: number };
}

export interface MissionReward {
  credits: number;
  ore: number;
  gems: number;
  experience: number;
  unlocks: string[];
}

export interface Mission {
  id: string;
  name: string;
  description: string;
  objectives: MissionObjective[];
  rewards: MissionReward;
  timeLimit: number;
  mapId: string;
  difficulty: 'easy' | 'normal' | 'hard' | 'expert';
  prerequisites: string[];
  briefing: string;
  debriefing: {
    success: string;
    failure: string;
  };
}

export interface MissionProgress {
  missionId: string;
  started: boolean;
  completed: boolean;
  failed: boolean;
  startTime: number;
  elapsedTime: number;
  objectiveProgress: Map<string, number>;
}

export class MissionManager {
  private missions: Map<string, Mission> = new Map();
  private missionProgress: Map<string, MissionProgress> = new Map();
  private currentMission: Mission | null = null;
  private missionHistory: string[] = [];
  private listeners: Map<string, Array<(data: any) => void>> = new Map();

  constructor() {
    this.initializeMissions();
  }

  private initializeMissions(): void {
    this.registerMission({
      id: 'tutorial-01',
      name: 'First Steps',
      description: 'Learn the basics of base building and unit production',
      objectives: [
        {
          id: 'build-barracks',
          type: 'primary',
          name: 'Build a Barracks',
          description: 'Construct your first Barracks building',
          target: 'barracks',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'train-soldier',
          type: 'primary',
          name: 'Train a Soldier',
          description: 'Produce a soldier unit from the barracks',
          target: 'soldier',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
      ],
      rewards: {
        credits: 500,
        ore: 200,
        gems: 0,
        experience: 100,
        unlocks: [],
      },
      timeLimit: 600,
      mapId: 'tutorial-01',
      difficulty: 'easy',
      prerequisites: [],
      briefing: 'Welcome, Commander. This is your first mission. Build a Barracks and train a Soldier to complete your training.',
      debriefing: {
        success: 'Excellent work, Commander. You have completed your first mission.',
        failure: 'Mission failed. Don\'t worry, Commander. Try again.',
      },
    });

    this.registerMission({
      id: 'tutorial-02',
      name: 'Resource Gathering',
      description: 'Learn to gather and manage resources',
      objectives: [
        {
          id: 'build-refinery',
          type: 'primary',
          name: 'Build a Refinery',
          description: 'Construct a Refinery to process ore',
          target: 'refinery',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'train-miners',
          type: 'primary',
          name: 'Train Miners',
          description: 'Produce miner units to gather resources',
          target: 'miner',
          targetAmount: 3,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'collect-ore',
          type: 'primary',
          name: 'Collect Ore',
          description: 'Gather 1000 ore from the field',
          target: 'ore',
          targetAmount: 1000,
          currentAmount: 0,
          completed: false,
          bonusReward: { credits: 200 },
        },
      ],
      rewards: {
        credits: 800,
        ore: 300,
        gems: 10,
        experience: 200,
        unlocks: [],
      },
      timeLimit: 900,
      mapId: 'tutorial-02',
      difficulty: 'easy',
      prerequisites: ['tutorial-01'],
      briefing: 'Commander, your next objective is to establish a resource gathering operation. Build a Refinery and train Miners to collect ore.',
      debriefing: {
        success: 'Well done! Your resource gathering operation is operational.',
        failure: 'Mission failed. Resources are critical to your success.',
      },
    });

    this.registerMission({
      id: 'battle-01',
      name: 'First Strike',
      description: 'Engage and destroy the enemy base',
      objectives: [
        {
          id: 'destroy-enemy-hq',
          type: 'primary',
          name: 'Destroy Enemy HQ',
          description: 'Destroy the enemy Command Center',
          target: 'command-center',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'build-army',
          type: 'primary',
          name: 'Build an Army',
          description: 'Produce at least 5 combat units',
          target: 'combat-unit',
          targetAmount: 5,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'no-buildings-lost',
          type: 'secondary',
          name: 'Defensive Success',
          description: 'Don\'t lose any buildings',
          target: 'buildings',
          targetAmount: 1,
          currentAmount: 1,
          completed: false,
          bonusReward: { credits: 500 },
        },
      ],
      rewards: {
        credits: 2000,
        ore: 1000,
        gems: 50,
        experience: 500,
        unlocks: ['advanced-weapons'],
      },
      timeLimit: 1200,
      mapId: 'battle-01',
      difficulty: 'normal',
      prerequisites: ['tutorial-02'],
      briefing: 'Commander, intelligence reports an enemy base nearby. Destroy their Command Center to secure the area.',
      debriefing: {
        success: 'Victory! The enemy base has been destroyed. Good work, Commander.',
        failure: 'Mission failed. The enemy remains a threat.',
      },
    });

    this.registerMission({
      id: 'battle-02',
      name: 'Holding the Line',
      description: 'Defend your base against enemy waves',
      objectives: [
        {
          id: 'survive-waves',
          type: 'primary',
          name: 'Survive',
          description: 'Survive for 10 minutes',
          target: 'time',
          targetAmount: 600,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'destroy-enemies',
          type: 'primary',
          name: 'Destroy Enemies',
          description: 'Destroy 20 enemy units',
          target: 'enemy-unit',
          targetAmount: 20,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'build-defenses',
          type: 'secondary',
          name: 'Fortify',
          description: 'Build 3 turrets',
          target: 'turret',
          targetAmount: 3,
          currentAmount: 0,
          completed: false,
        },
      ],
      rewards: {
        credits: 3000,
        ore: 1500,
        gems: 100,
        experience: 800,
        unlocks: ['fortification'],
      },
      timeLimit: 900,
      mapId: 'battle-02',
      difficulty: 'hard',
      prerequisites: ['battle-01'],
      briefing: 'Commander, we\'ve detected multiple enemy formations approaching. Prepare your defenses and survive the assault.',
      debriefing: {
        success: 'Outstanding defense, Commander! The enemy assault has been repelled.',
        failure: 'Your base has been overrun. Reinforcements are on the way.',
      },
    });

    this.registerMission({
      id: 'campaign-final',
      name: 'Final Assault',
      description: 'Launch the final assault on the enemy stronghold',
      objectives: [
        {
          id: 'destroy-super-weapon',
          type: 'primary',
          name: 'Super Weapon',
          description: 'Destroy the enemy Super Weapon',
          target: 'super-weapon',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'capture-enemy-hq',
          type: 'primary',
          name: 'Capture HQ',
          description: 'Capture the enemy Command Center',
          target: 'hq',
          targetAmount: 1,
          currentAmount: 0,
          completed: false,
        },
        {
          id: 'all-units-survived',
          type: 'bonus',
          name: 'Perfect Victory',
          description: 'Complete the mission without losing any units',
          target: 'units',
          targetAmount: 1,
          currentAmount: 1,
          completed: false,
          bonusReward: { credits: 5000, gems: 500 },
        },
      ],
      rewards: {
        credits: 10000,
        ore: 5000,
        gems: 500,
        experience: 2000,
        unlocks: ['victory'],
      },
      timeLimit: 1800,
      mapId: 'campaign-final',
      difficulty: 'expert',
      prerequisites: ['battle-02'],
      briefing: 'Commander, this is it. The enemy Super Weapon is nearly operational. Launch a full assault and destroy it before it\'s too late.',
      debriefing: {
        success: 'VICTORY! The enemy has been defeated. You have saved the world, Commander!',
        failure: 'The Super Weapon has been activated. All is lost...',
      },
    });
  }

  public registerMission(mission: Mission): void {
    this.missions.set(mission.id, mission);
  }

  public getMission(id: string): Mission | undefined {
    return this.missions.get(id);
  }

  public getAllMissions(): Mission[] {
    return Array.from(this.missions.values());
  }

  public getCurrentMission(): Mission | null {
    return this.currentMission;
  }

  public startMission(id: string): boolean {
    const mission = this.missions.get(id);
    if (!mission) return false;

    if (mission.prerequisites.length > 0) {
      const completedPrereqs = mission.prerequisites.filter(p => this.isMissionCompleted(p));
      if (completedPrereqs.length !== mission.prerequisites.length) {
        return false;
      }
    }

    this.currentMission = mission;

    const progress: MissionProgress = {
      missionId: id,
      started: true,
      completed: false,
      failed: false,
      startTime: Date.now(),
      elapsedTime: 0,
      objectiveProgress: new Map(),
    };

    for (const objective of mission.objectives) {
      progress.objectiveProgress.set(objective.id, 0);
    }

    this.missionProgress.set(id, progress);
    this.emit('mission-started', { mission });

    return true;
  }

  public update(deltaTime: number): void {
    if (!this.currentMission) return;

    const progress = this.missionProgress.get(this.currentMission.id);
    if (!progress) return;

    progress.elapsedTime += deltaTime;

    for (const objective of this.currentMission.objectives) {
      const current = progress.objectiveProgress.get(objective.id) || 0;
      if (!objective.completed && current >= objective.targetAmount) {
        objective.completed = true;
        this.emit('objective-completed', {
          missionId: this.currentMission.id,
          objective,
        });
      }
    }

    if (this.currentMission.timeLimit > 0 && progress.elapsedTime >= this.currentMission.timeLimit) {
      const allPrimaryCompleted = this.currentMission.objectives
        .filter(o => o.type === 'primary')
        .every(o => o.completed);

      if (!allPrimaryCompleted) {
        this.failMission();
        return;
      }
    }

    const allPrimaryCompleted = this.currentMission.objectives
      .filter(o => o.type === 'primary')
      .every(o => o.completed);

    if (allPrimaryCompleted) {
      this.completeMission();
    }

    this.emit('mission-updated', {
      mission: this.currentMission,
      progress,
    });
  }

  public updateObjective(objectiveId: string, amount: number): void {
    if (!this.currentMission) return;

    const progress = this.missionProgress.get(this.currentMission.id);
    if (!progress) return;

    const objective = this.currentMission.objectives.find(o => o.id === objectiveId);
    if (!objective) return;

    const current = progress.objectiveProgress.get(objectiveId) || 0;
    progress.objectiveProgress.set(objectiveId, current + amount);
    objective.currentAmount = current + amount;
  }

  public completeMission(): void {
    if (!this.currentMission) return;

    const progress = this.missionProgress.get(this.currentMission.id);
    if (!progress) return;

    progress.completed = true;
    progress.failed = false;

    for (const objective of this.currentMission.objectives) {
      objective.completed = true;
    }

    this.missionHistory.push(this.currentMission.id);

    this.emit('mission-completed', {
      mission: this.currentMission,
      rewards: this.currentMission.rewards,
    });

    this.currentMission = null;
  }

  public failMission(): void {
    if (!this.currentMission) return;

    const progress = this.missionProgress.get(this.currentMission.id);
    if (!progress) return;

    progress.completed = false;
    progress.failed = true;

    this.emit('mission-failed', {
      mission: this.currentMission,
    });

    this.currentMission = null;
  }

  public abortMission(): void {
    if (!this.currentMission) return;

    const progress = this.missionProgress.get(this.currentMission.id);
    if (progress) {
      progress.failed = true;
    }

    this.emit('mission-aborted', {
      mission: this.currentMission,
    });

    this.currentMission = null;
  }

  public isMissionCompleted(id: string): boolean {
    const progress = this.missionProgress.get(id);
    return progress?.completed || false;
  }

  public isMissionFailed(id: string): boolean {
    const progress = this.missionProgress.get(id);
    return progress?.failed || false;
  }

  public getMissionProgress(id: string): MissionProgress | undefined {
    return this.missionProgress.get(id);
  }

  public getCompletedMissions(): Mission[] {
    return this.missionHistory.map(id => this.missions.get(id)).filter((m): m is Mission => m !== undefined);
  }

  public getAvailableMissions(): Mission[] {
    return this.getAllMissions().filter(m => {
      if (this.isMissionCompleted(m.id) || this.isMissionFailed(m.id)) return false;
      if (m.prerequisites.length > 0) {
        const completedPrereqs = m.prerequisites.filter(p => this.isMissionCompleted(p));
        return completedPrereqs.length === m.prerequisites.length;
      }
      return true;
    });
  }

  public getMissionsByDifficulty(difficulty: Mission['difficulty']): Mission[] {
    return this.getAllMissions().filter(m => m.difficulty === difficulty);
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
    this.currentMission = null;
    this.missionProgress.clear();
  }

  public toJSON(): object {
    return {
      missionHistory: this.missionHistory,
      missionProgress: Array.from(this.missionProgress.entries()).map(([id, progress]) => ({
        ...progress,
        objectiveProgress: Array.from(progress.objectiveProgress.entries()),
      })),
    };
  }

  public fromJSON(data: any): void {
    if (data.missionHistory) {
      this.missionHistory = data.missionHistory;
    }

    if (data.missionProgress) {
      for (const p of data.missionProgress) {
        const progress: MissionProgress = {
          ...p,
          objectiveProgress: new Map(p.objectiveProgress),
        };
        this.missionProgress.set(p.missionId, progress);
      }
    }
  }
}
