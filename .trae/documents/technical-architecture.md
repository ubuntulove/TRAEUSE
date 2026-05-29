# Node Breaker - Technical Architecture

## 1. Architecture Design

```
┌─────────────────────────────────────────┐
│            Frontend Layer               │
├─────────────────────────────────────────┤
│  React@18 + Vite + TailwindCSS         │
│  ├── Game Engine (Canvas/HTML5)         │
│  ├── State Management (React Context)   │
│  ├── Audio System (Howler.js)           │
│  └── Particle Effects System            │
└─────────────────────────────────────────┘
```

## 2. Technology Stack

- **Framework**: React@18 with Vite
- **Styling**: TailwindCSS@3 + Custom CSS
- **Build Tool**: Vite
- **Animation**: Framer Motion + CSS Animations
- **Audio**: Howler.js for sound effects
- **Storage**: LocalStorage for save data
- **No Backend**: Self-contained single-page game

## 3. Route Definitions

| Route | Purpose |
|-------|---------|
| / | Title screen with animated intro |
| /game | Main gameplay screen |
| /shop | Upgrade and customization |
| /select | Level/sector selection |

## 4. Core Systems

### 4.1 Game Engine Architecture
```typescript
interface GameState {
  phase: 'title' | 'hacking' | 'combat' | 'victory' | 'gameover';
  player: { x: number; y: number; health: number; };
  enemies: Enemy[];
  hackingNodes: Node[];
  hackingConnections: Connection[];
  score: number;
  sector: number;
}
```

### 4.2 Hacking System
- Canvas-based rendering for node matrix
- Path-finding validation for connections
- Time-limited challenge mechanics
- Progressive difficulty (more nodes, less time)

### 4.3 Combat System
- Top-down 2D shooter mechanics
- WASD movement + mouse aim/shoot
- Weapon types: Primary (infinite), Special (limited)
- Enemy AI: Patrol, Chase, Attack patterns

### 4.4 Data Persistence
```typescript
interface SaveData {
  sectorsUnlocked: number;
  highScore: number;
  weaponsUnlocked: string[];
  upgradesPurchased: Upgrade[];
  totalDataCollected: number;
}
```

## 5. Component Structure

```
App
├── TitleScreen
├── GameScreen
│   ├── HUD
│   │   ├── HealthBar
│   │   ├── ScoreDisplay
│   │   ├── SectorIndicator
│   │   └── WeaponIndicator
│   ├── GameCanvas
│   │   ├── Player
│   │   ├── Enemies
│   │   ├── Projectiles
│   │   └── Particles
│   └── HackingOverlay
│       ├── NodeMatrix
│       ├── ConnectionLine
│       └── TimerBar
├── UpgradeShop
│   ├── WeaponCard
│   └── UpgradeSlot
└── LevelSelect
    └── SectorMap
```

## 6. Performance Targets

- **FPS**: 60fps on modern browsers
- **Load Time**: < 3 seconds initial load
- **Mobile**: Touch-responsive with 30+ FPS minimum
- **Bundle Size**: < 500KB gzipped
