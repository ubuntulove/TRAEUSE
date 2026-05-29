import { useEffect, useRef, useCallback, useState } from 'react';
import { useGameStore, Enemy, Projectile } from '@/stores/gameStore';
import { Zap, Crosshair, Heart } from 'lucide-react';

interface GameCanvasProps {
  onWaveComplete: () => void;
  onGameOver: () => void;
}

export default function GameCanvas({ onWaveComplete, onGameOver }: GameCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const keysPressed = useRef<Set<string>>(new Set());
  const mousePos = useRef({ x: 0, y: 0 });
  const lastShot = useRef(0);
  const [showDamage, setShowDamage] = useState(false);

  const {
    playerX,
    playerY,
    setPlayerPosition,
    playerHealth,
    maxHealth,
    damagePlayer,
    enemies,
    updateEnemies,
    damageEnemy,
    hackEnemy,
    projectiles,
    spawnProjectile,
    updateProjectiles,
    score,
    addScore,
    sector,
    wave,
    specialAmmo,
    useSpecialAmmo,
    activeWeapon,
    setPhase,
  } = useGameStore();

  const spawnEnemies = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const numEnemies = Math.min(2 + sector + Math.floor(wave / 2), 8);
    const newEnemies: Enemy[] = [];

    for (let i = 0; i < numEnemies; i++) {
      const side = Math.floor(Math.random() * 4);
      let x: number, y: number;

      switch (side) {
        case 0: x = Math.random() * canvas.width; y = -50; break;
        case 1: x = canvas.width + 50; y = Math.random() * canvas.height; break;
        case 2: x = Math.random() * canvas.width; y = canvas.height + 50; break;
        default: x = -50; y = Math.random() * canvas.height; break;
      }

      const type = wave > 3 && Math.random() > 0.7 ? 'fast' : wave > 5 && Math.random() > 0.8 ? 'armored' : 'basic';
      const baseHealth = type === 'armored' ? 80 : type === 'fast' ? 30 : 50;

      newEnemies.push({
        id: `enemy-${i}-${Date.now()}`,
        x,
        y,
        health: baseHealth + sector * 10,
        maxHealth: baseHealth + sector * 10,
        hacked: false,
        type,
      });
    }

    updateEnemies(newEnemies);
  }, [sector, wave, updateEnemies]);

  useEffect(() => {
    spawnEnemies();
  }, [spawnEnemies]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      keysPressed.current.add(e.key.toLowerCase());
    };
    const handleKeyUp = (e: KeyboardEvent) => {
      keysPressed.current.delete(e.key.toLowerCase());
    };
    const handleMouseMove = (e: MouseEvent) => {
      const canvas = canvasRef.current;
      if (!canvas) return;
      const rect = canvas.getBoundingClientRect();
      mousePos.current = {
        x: e.clientX - rect.left,
        y: e.clientY - rect.top,
      };
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);
    window.addEventListener('mousemove', handleMouseMove);

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
      window.removeEventListener('mousemove', handleMouseMove);
    };
  }, []);

  useEffect(() => {
    const handleShoot = (e: MouseEvent) => {
      if (e.button !== 0) return;
      const now = Date.now();
      if (now - lastShot.current < 150) return;
      lastShot.current = now;

      const dx = mousePos.current.x - playerX;
      const dy = mousePos.current.y - playerY;
      const dist = Math.hypot(dx, dy);
      const speed = 12;

      if (e.shiftKey && specialAmmo > 0) {
        for (let i = -2; i <= 2; i++) {
          const angle = Math.atan2(dy, dx) + (i * Math.PI) / 16;
          spawnProjectile(
            playerX,
            playerY,
            Math.cos(angle) * speed * 1.5,
            Math.sin(angle) * speed * 1.5,
            true
          );
        }
        useSpecialAmmo();
      } else {
        spawnProjectile(
          playerX,
          playerY,
          (dx / dist) * speed,
          (dy / dist) * speed,
          true
        );
      }
    };

    window.addEventListener('mousedown', handleShoot);
    return () => window.removeEventListener('mousedown', handleShoot);
  }, [playerX, playerY, spawnProjectile, specialAmmo, useSpecialAmmo]);

  useEffect(() => {
    const gameLoop = setInterval(() => {
      const canvas = canvasRef.current;
      if (!canvas) return;
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      let newX = playerX;
      let newY = playerY;
      const speed = 5;

      if (keysPressed.current.has('w') || keysPressed.current.has('arrowup')) newY -= speed;
      if (keysPressed.current.has('s') || keysPressed.current.has('arrowdown')) newY += speed;
      if (keysPressed.current.has('a') || keysPressed.current.has('arrowleft')) newX -= speed;
      if (keysPressed.current.has('d') || keysPressed.current.has('arrowright')) newX += speed;

      newX = Math.max(20, Math.min(canvas.width - 20, newX));
      newY = Math.max(20, Math.min(canvas.height - 20, newY));
      setPlayerPosition(newX, newY);

      let updatedEnemies = enemies.map((enemy) => {
        if (enemy.hacked) return enemy;

        const dx = newX - enemy.x;
        const dy = newY - enemy.y;
        const dist = Math.hypot(dx, dy);
        
        const enemySpeed = enemy.type === 'fast' ? 2.5 : enemy.type === 'armored' ? 1 : 1.5;
        
        return {
          ...enemy,
          x: enemy.x + (dx / dist) * enemySpeed,
          y: enemy.y + (dy / dist) * enemySpeed,
        };
      });

      updatedEnemies.forEach((enemy) => {
        if (!enemy.hacked && Math.hypot(enemy.x - newX, enemy.y - newY) < 35) {
          damagePlayer(10);
          setShowDamage(true);
          setTimeout(() => setShowDamage(false), 100);
        }
      });

      let updatedProjectiles = projectiles
        .map((p) => ({
          ...p,
          x: p.x + p.vx,
          y: p.y + p.vy,
        }))
        .filter(
          (p) => p.x > -10 && p.x < canvas.width + 10 && p.y > -10 && p.y < canvas.height + 10
        );

      updatedProjectiles.forEach((proj) => {
        if (proj.isPlayer) {
          updatedEnemies.forEach((enemy) => {
            if (!enemy.hacked && Math.hypot(proj.x - enemy.x, proj.y - enemy.y) < 30) {
              const damage = activeWeapon === 'blaster' ? 15 : 25;
              damageEnemy(enemy.id, damage);
              updatedProjectiles = updatedProjectiles.filter((p) => p.id !== proj.id);
            }
          });
        }
      });

      updatedEnemies = updatedEnemies.filter((enemy) => {
        if (enemy.health <= 0) {
          const points = enemy.type === 'armored' ? 150 : enemy.type === 'fast' ? 100 : 75;
          addScore(points);
          return false;
        }
        return true;
      });

      updateProjectiles(updatedProjectiles);
      updateEnemies(updatedEnemies);

      if (playerHealth <= 0) {
        onGameOver();
      }

      if (updatedEnemies.length === 0) {
        onWaveComplete();
      }

    }, 1000 / 60);

    return () => clearInterval(gameLoop);
  }, [playerX, playerY, enemies, projectiles, playerHealth, updateProjectiles, updateEnemies, damageEnemy, addScore, setPlayerPosition, damagePlayer, onGameOver, onWaveComplete, activeWeapon]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.fillStyle = '#0a0a0f';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.strokeStyle = 'rgba(0, 212, 255, 0.1)';
    ctx.lineWidth = 1;
    const gridSize = 50;
    for (let x = 0; x < canvas.width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, canvas.height);
      ctx.stroke();
    }
    for (let y = 0; y < canvas.height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(canvas.width, y);
      ctx.stroke();
    }

    ctx.strokeStyle = 'rgba(255, 0, 102, 0.3)';
    ctx.lineWidth = 2;
    ctx.strokeRect(50, 50, canvas.width - 100, canvas.height - 100);

    projectiles.forEach((proj) => {
      const color = proj.isPlayer ? '#00d4ff' : '#ff0066';
      ctx.shadowColor = color;
      ctx.shadowBlur = 10;
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.arc(proj.x, proj.y, proj.isPlayer ? 5 : 8, 0, Math.PI * 2);
      ctx.fill();
      ctx.shadowBlur = 0;
    });

    enemies.forEach((enemy) => {
      const color = enemy.hacked ? '#00ff88' : enemy.type === 'armored' ? '#ff0066' : enemy.type === 'fast' ? '#ffaa00' : '#ff0066';
      const size = enemy.type === 'armored' ? 25 : enemy.type === 'fast' ? 18 : 22;

      ctx.shadowColor = color;
      ctx.shadowBlur = enemy.hacked ? 5 : 15;
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.arc(enemy.x, enemy.y, size, 0, Math.PI * 2);
      ctx.fill();

      if (!enemy.hacked) {
        ctx.fillStyle = '#000';
        ctx.beginPath();
        ctx.arc(enemy.x, enemy.y, size * 0.5, 0, Math.PI * 2);
        ctx.fill();

        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(enemy.x, enemy.y, size * 0.3, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.shadowBlur = 0;

      const healthPercent = enemy.health / enemy.maxHealth;
      ctx.fillStyle = '#333';
      ctx.fillRect(enemy.x - 20, enemy.y - size - 10, 40, 4);
      ctx.fillStyle = color;
      ctx.fillRect(enemy.x - 20, enemy.y - size - 10, 40 * healthPercent, 4);
    });

    const playerAngle = Math.atan2(mousePos.current.y - playerY, mousePos.current.x - playerX);
    
    ctx.shadowColor = '#00d4ff';
    ctx.shadowBlur = 20;
    ctx.fillStyle = '#00d4ff';
    ctx.beginPath();
    ctx.arc(playerX, playerY, 18, 0, Math.PI * 2);
    ctx.fill();
    ctx.shadowBlur = 0;

    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.arc(playerX, playerY, 10, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = '#00d4ff';
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.moveTo(playerX, playerY);
    ctx.lineTo(playerX + Math.cos(playerAngle) * 30, playerY + Math.sin(playerAngle) * 30);
    ctx.stroke();

  }, [playerX, playerY, enemies, projectiles, mousePos]);

  return (
    <div className="relative w-full h-full">
      <canvas
        ref={canvasRef}
        width={800}
        height={600}
        className="w-full h-full cursor-crosshair"
      />

      <div className={`absolute top-4 left-4 font-share-tech text-xs ${showDamage ? 'text-neon-pink' : 'text-white/60'}`}>
        <div className="flex items-center gap-2 mb-2">
          <Heart className="w-4 h-4 text-neon-pink" />
          <span>{playerHealth}/{maxHealth}</span>
        </div>
        <div className="flex items-center gap-2">
          <Crosshair className="w-4 h-4 text-cyber-blue" />
          <span>{score.toLocaleString()}</span>
        </div>
      </div>

      <div className="absolute top-4 right-4 font-share-tech text-xs text-white/60">
        <div className="flex items-center gap-2 mb-1">
          <Zap className="w-4 h-4 text-warning-amber" />
          <span>SPECIAL: {specialAmmo}</span>
        </div>
        <div className="text-cyber-blue">SECTOR {sector} | WAVE {wave}</div>
      </div>

      <div className="absolute bottom-4 left-1/2 -translate-x-1/2 font-share-tech text-xs text-white/40 text-center">
        WASD: Move | CLICK: Shoot | SHIFT+CLICK: Special Attack
      </div>
    </div>
  );
}
