
import React, { useEffect, useRef, useState } from 'react';
import { useGameStore, Building, Unit } from '@/store/useGameStore';
import { useNavigate } from 'react-router-dom';

const TILE_SIZE = 40;
const MAP_WIDTH = 30;
const MAP_HEIGHT = 20;
const CANVAS_WIDTH = MAP_WIDTH * TILE_SIZE;
const CANVAS_HEIGHT = MAP_HEIGHT * TILE_SIZE;

const BUILDING_COSTS: Record<string, { credits: number; power: number; ore: number }> = {
  'refinery': { credits: 500, power: 0, ore: 0 },
  'power-plant': { credits: 300, power: 100, ore: 0 },
  'barracks': { credits: 400, power: 0, ore: 0 },
  'factory': { credits: 800, power: 0, ore: 0 },
};

const UNIT_COSTS: Record<string, { credits: number; power: number; ore: number }> = {
  'miner': { credits: 300, power: 0, ore: 0 },
  'soldier': { credits: 100, power: 0, ore: 0 },
  'tank': { credits: 600, power: 0, ore: 0 },
};

const Gameplay: React.FC = () => {
  const navigate = useNavigate();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [selectedBuilding, setSelectedBuilding] = useState<string | null>(null);
  const [selectedUnit, setSelectedUnit] = useState<string | null>(null);
  const {
    credits,
    power,
    ore,
    buildings,
    units,
    selectedUnits,
    addBuilding,
    addUnit,
    updateUnit,
    selectUnit,
    updateResources,
  } = useGameStore();

  // Initialize game state
  useEffect(() => {
    // Add initial buildings and units
    addBuilding({
      id: 'hq-1',
      type: 'refinery',
      x: 5,
      y: 10,
      health: 1000,
      maxHealth: 1000,
      owner: 'player',
    });
    
    addBuilding({
      id: 'power-1',
      type: 'power-plant',
      x: 3,
      y: 10,
      health: 800,
      maxHealth: 800,
      owner: 'player',
    });

    addUnit({
      id: 'miner-1',
      type: 'miner',
      x: 7 * TILE_SIZE,
      y: 10 * TILE_SIZE,
      health: 200,
      maxHealth: 200,
      owner: 'player',
      speed: 1,
      damage: 0,
      attackCooldown: 0,
      lastAttackTime: 0,
    });

    addUnit({
      id: 'soldier-1',
      type: 'soldier',
      x: 8 * TILE_SIZE,
      y: 9 * TILE_SIZE,
      health: 100,
      maxHealth: 100,
      owner: 'player',
      speed: 2,
      damage: 10,
      attackCooldown: 1000,
      lastAttackTime: 0,
    });

    addUnit({
      id: 'soldier-2',
      type: 'soldier',
      x: 8 * TILE_SIZE,
      y: 11 * TILE_SIZE,
      health: 100,
      maxHealth: 100,
      owner: 'player',
      speed: 2,
      damage: 10,
      attackCooldown: 1000,
      lastAttackTime: 0,
    });

    // Enemy buildings and units
    addBuilding({
      id: 'enemy-hq',
      type: 'refinery',
      x: 24,
      y: 10,
      health: 1000,
      maxHealth: 1000,
      owner: 'enemy',
    });

    addUnit({
      id: 'enemy-tank-1',
      type: 'tank',
      x: 22 * TILE_SIZE,
      y: 10 * TILE_SIZE,
      health: 400,
      maxHealth: 400,
      owner: 'enemy',
      speed: 1.5,
      damage: 30,
      attackCooldown: 1500,
      lastAttackTime: 0,
    });
  }, [addBuilding, addUnit]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animationId: number;
    let lastTime = Date.now();

    const gameLoop = () => {
      const now = Date.now();
      const delta = now - lastTime;
      lastTime = now;

      updateGame(delta);
      render(ctx);

      animationId = requestAnimationFrame(gameLoop);
    };

    animationId = requestAnimationFrame(gameLoop);

    return () => cancelAnimationFrame(animationId);
  }, [buildings, units, selectedUnits]);

  const updateGame = (delta: number) => {
    units.forEach(unit => {
      if (unit.targetX !== undefined && unit.targetY !== undefined) {
        const dx = unit.targetX - unit.x;
        const dy = unit.targetY - unit.y;
        const dist = Math.sqrt(dx * dx + dy * dy);
        
        if (dist > 5) {
          const moveX = (dx / dist) * unit.speed;
          const moveY = (dy / dist) * unit.speed;
          updateUnit(unit.id, { x: unit.x + moveX, y: unit.y + moveY });
        } else {
          updateUnit(unit.id, { targetX: undefined, targetY: undefined });
        }
      }

      if (unit.type !== 'miner') {
        const now = Date.now();
        if (now - unit.lastAttackTime > unit.attackCooldown) {
          const enemy = units.find(u => 
            u.owner !== unit.owner && 
            Math.sqrt((u.x - unit.x)**2 + (u.y - unit.y)**2) < 100
          );
          
          if (enemy) {
            const newHealth = Math.max(0, enemy.health - unit.damage);
            updateUnit(enemy.id, { health: newHealth });
            updateUnit(unit.id, { lastAttackTime: now });
          }
        }
      }
    });
  };

  const render = (ctx: CanvasRenderingContext2D) => {
    // Clear canvas
    ctx.fillStyle = '#1a2b3a';
    ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

    // Draw grid
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.05)';
    ctx.lineWidth = 1;
    for (let x = 0; x <= MAP_WIDTH; x++) {
      ctx.beginPath();
      ctx.moveTo(x * TILE_SIZE, 0);
      ctx.lineTo(x * TILE_SIZE, CANVAS_HEIGHT);
      ctx.stroke();
    }
    for (let y = 0; y <= MAP_HEIGHT; y++) {
      ctx.beginPath();
      ctx.moveTo(0, y * TILE_SIZE);
      ctx.lineTo(CANVAS_WIDTH, y * TILE_SIZE);
      ctx.stroke();
    }

    // Draw ore deposits
    ctx.fillStyle = '#4a9eff';
    for (let i = 0; i < 5; i++) {
      ctx.beginPath();
      ctx.arc((8 + i * 4) * TILE_SIZE, 5 * TILE_SIZE, 25, 0, Math.PI * 2);
      ctx.fill();
      ctx.beginPath();
      ctx.arc((8 + i * 4) * TILE_SIZE, 15 * TILE_SIZE, 25, 0, Math.PI * 2);
      ctx.fill();
    }

    // Draw buildings
    buildings.forEach(building => {
      const x = building.x * TILE_SIZE;
      const y = building.y * TILE_SIZE;
      const w = TILE_SIZE * 2;
      const h = TILE_SIZE * 2;

      ctx.fillStyle = building.owner === 'player' ? '#2c5f2d' : '#8b0000';
      ctx.fillRect(x, y, w, h);

      ctx.strokeStyle = building.owner === 'player' ? '#4CAF50' : '#ff4444';
      ctx.lineWidth = 3;
      ctx.strokeRect(x, y, w, h);

      // Health bar
      const healthPercent = building.health / building.maxHealth;
      ctx.fillStyle = '#333';
      ctx.fillRect(x, y - 10, w, 5);
      ctx.fillStyle = healthPercent > 0.5 ? '#4CAF50' : healthPercent > 0.25 ? '#ff9800' : '#ff4444';
      ctx.fillRect(x, y - 10, w * healthPercent, 5);
    });

    // Draw units
    units.forEach(unit => {
      const isSelected = selectedUnits.includes(unit.id);

      ctx.beginPath();
      ctx.fillStyle = unit.owner === 'player' ? '#00bcd4' : '#f44336';
      ctx.arc(unit.x, unit.y, unit.type === 'tank' ? 15 : unit.type === 'miner' ? 12 : 8, 0, Math.PI * 2);
      ctx.fill();

      if (isSelected) {
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 3;
        ctx.stroke();
      }

      // Health bar
      const healthPercent = unit.health / unit.maxHealth;
      ctx.fillStyle = '#333';
      ctx.fillRect(unit.x - 15, unit.y - 25, 30, 4);
      ctx.fillStyle = healthPercent > 0.5 ? '#4CAF50' : healthPercent > 0.25 ? '#ff9800' : '#ff4444';
      ctx.fillRect(unit.x - 15, unit.y - 25, 30 * healthPercent, 4);
    });
  };

  const handleCanvasClick = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    if (selectedBuilding) {
      const tileX = Math.floor(x / TILE_SIZE);
      const tileY = Math.floor(y / TILE_SIZE);
      const cost = BUILDING_COSTS[selectedBuilding];
      
      if (credits >= cost.credits) {
        addBuilding({
          id: `${selectedBuilding}-${Date.now()}`,
          type: selectedBuilding as any,
          x: tileX,
          y: tileY,
          health: 1000,
          maxHealth: 1000,
          owner: 'player',
        });
        updateResources(-cost.credits);
        setSelectedBuilding(null);
      }
    } else {
      const clickedUnit = units.find(u => {
        const dx = u.x - x;
        const dy = u.y - y;
        return Math.sqrt(dx * dx + dy * dy) < 20;
      });

      if (clickedUnit) {
        selectUnit(clickedUnit.id, e.shiftKey);
      } else {
        if (selectedUnits.length > 0) {
          selectedUnits.forEach(id => {
            updateUnit(id, { targetX: x, targetY: y });
          });
        }
      }
    }
  };

  const handleProduceUnit = (type: string) => {
    const cost = UNIT_COSTS[type];
    if (credits >= cost.credits) {
      addUnit({
        id: `${type}-${Date.now()}`,
        type: type as any,
        x: 300,
        y: 400,
        health: type === 'tank' ? 400 : type === 'miner' ? 200 : 100,
        maxHealth: type === 'tank' ? 400 : type === 'miner' ? 200 : 100,
        owner: 'player',
        speed: type === 'soldier' ? 2 : type === 'tank' ? 1.5 : 1,
        damage: type === 'tank' ? 30 : type === 'miner' ? 0 : 10,
        attackCooldown: type === 'tank' ? 1500 : 1000,
        lastAttackTime: 0,
      });
      updateResources(-cost.credits);
    }
  };

  return (
    <div className="min-h-screen bg-[#0d1b2a]">
      {/* Top HUD - Resources */}
      <div className="bg-[#1a2b3a] border-b border-gray-700 p-4 flex justify-between items-center">
        <div className="flex gap-8">
          <div className="flex items-center gap-2">
            <span className="text-yellow-400 text-2xl">💰</span>
            <span className="text-xl font-bold text-yellow-400">{credits}</span>
            <span className="text-gray-400">Credits</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-blue-400 text-2xl">⚡</span>
            <span className="text-xl font-bold text-blue-400">{power}</span>
            <span className="text-gray-400">Power</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-cyan-400 text-2xl">💎</span>
            <span className="text-xl font-bold text-cyan-400">{ore}</span>
            <span className="text-gray-400">Ore</span>
          </div>
        </div>
        <button
          onClick={() => navigate('/')}
          className="btn-industrial px-4 py-2"
        >
          Menu
        </button>
      </div>

      {/* Game Canvas */}
      <div className="flex">
        <div className="flex-1 p-4">
          <canvas
            ref={canvasRef}
            width={CANVAS_WIDTH}
            height={CANVAS_HEIGHT}
            onClick={handleCanvasClick}
            className="border-2 border-gray-700 bg-[#1a2b3a] cursor-crosshair"
          />
        </div>

        {/* Side Panel - Build Menu */}
        <div className="w-64 bg-[#1a2b3a] border-l border-gray-700 p-4">
          <h3 className="text-lg font-bold text-red-500 mb-4 border-b border-gray-700 pb-2">
            BUILDINGS
          </h3>
          <div className="grid grid-cols-2 gap-2 mb-6">
            {[
              { type: 'refinery', label: 'Refinery', cost: 500 },
              { type: 'power-plant', label: 'Power Plant', cost: 300 },
              { type: 'barracks', label: 'Barracks', cost: 400 },
              { type: 'factory', label: 'Factory', cost: 800 },
            ].map(item => (
              <button
                key={item.type}
                onClick={() => setSelectedBuilding(item.type)}
                className={`btn-industrial p-2 text-sm ${
                  selectedBuilding === item.type ? 'border-red-500' : ''
                }`}
              >
                <div>{item.label}</div>
                <div className="text-yellow-400 text-xs">{item.cost} 💰</div>
              </button>
            ))}
          </div>

          <h3 className="text-lg font-bold text-red-500 mb-4 border-b border-gray-700 pb-2">
            UNITS
          </h3>
          <div className="grid grid-cols-2 gap-2">
            {[
              { type: 'miner', label: 'Miner', cost: 300 },
              { type: 'soldier', label: 'Soldier', cost: 100 },
              { type: 'tank', label: 'Tank', cost: 600 },
            ].map(item => (
              <button
                key={item.type}
                onClick={() => handleProduceUnit(item.type)}
                className="btn-industrial p-2 text-sm"
              >
                <div>{item.label}</div>
                <div className="text-yellow-400 text-xs">{item.cost} 💰</div>
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Gameplay;
