import { useEffect, useRef, useCallback, useState } from 'react';
import { useGameStore, Node } from '@/stores/gameStore';

interface HackingSystemProps {
  onComplete: (success: boolean) => void;
}

export default function HackingSystem({ onComplete }: HackingSystemProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [mousePos, setMousePos] = useState({ x: 0, y: 0 });
  
  const {
    nodes,
    setNodes,
    connections,
    addConnection,
    currentPath,
    addToPath,
    clearPath,
    hackingTimeLeft,
    maxHackingTime,
    setHackingTime,
    enemies,
    hackEnemy,
    sector,
    wave,
  } = useGameStore();

  const generateNodes = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const newNodes: Node[] = [];
    const padding = 80;
    const width = canvas.width;
    const height = canvas.height;

    const startNode: Node = {
      id: 'start',
      x: padding + Math.random() * 60,
      y: height / 2 + (Math.random() - 0.5) * 100,
      type: 'start',
      connected: false,
    };
    newNodes.push(startNode);

    const numNodes = Math.min(3 + sector + Math.floor(wave / 2), 8);
    for (let i = 0; i < numNodes; i++) {
      const node: Node = {
        id: `node-${i}`,
        x: padding + 80 + (i * (width - padding * 2 - 80)) / (numNodes + 1) + (Math.random() - 0.5) * 60,
        y: height / 2 + (Math.random() - 0.5) * (height * 0.5),
        type: 'normal',
        connected: false,
      };
      newNodes.push(node);
    }

    const endNode: Node = {
      id: 'end',
      x: width - padding - Math.random() * 60,
      y: height / 2 + (Math.random() - 0.5) * 100,
      type: 'end',
      connected: false,
    };
    newNodes.push(endNode);

    setNodes(newNodes);
  }, [sector, wave, setNodes]);

  useEffect(() => {
    generateNodes();
  }, [generateNodes]);

  useEffect(() => {
    const timer = setInterval(() => {
      setHackingTime(hackingTimeLeft - 0.1);
      if (hackingTimeLeft <= 0) {
        onComplete(false);
      }
    }, 100);
    return () => clearInterval(timer);
  }, [hackingTimeLeft, setHackingTime, onComplete]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.strokeStyle = 'rgba(0, 212, 255, 0.1)';
    ctx.lineWidth = 1;
    const gridSize = 40;
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

    connections.forEach((conn) => {
      const fromNode = nodes.find((n) => n.id === conn.from);
      const toNode = nodes.find((n) => n.id === conn.to);
      if (fromNode && toNode) {
        const gradient = ctx.createLinearGradient(fromNode.x, fromNode.y, toNode.x, toNode.y);
        gradient.addColorStop(0, '#00d4ff');
        gradient.addColorStop(1, '#00ff88');
        
        ctx.strokeStyle = gradient;
        ctx.lineWidth = 4;
        ctx.shadowColor = '#00d4ff';
        ctx.shadowBlur = 15;
        ctx.beginPath();
        ctx.moveTo(fromNode.x, fromNode.y);
        ctx.lineTo(toNode.x, toNode.y);
        ctx.stroke();
        ctx.shadowBlur = 0;
      }
    });

    if (currentPath.length > 1) {
      ctx.strokeStyle = 'rgba(0, 212, 255, 0.6)';
      ctx.lineWidth = 3;
      ctx.setLineDash([5, 5]);
      ctx.shadowColor = '#00d4ff';
      ctx.shadowBlur = 10;
      ctx.beginPath();
      for (let i = 0; i < currentPath.length; i++) {
        const node = nodes.find((n) => n.id === currentPath[i]);
        if (node) {
          if (i === 0) {
            ctx.moveTo(node.x, node.y);
          } else {
            ctx.lineTo(node.x, node.y);
          }
        }
      }
      ctx.stroke();
      ctx.setLineDash([]);
      ctx.shadowBlur = 0;
    }

    if (isDrawing && currentPath.length > 0) {
      const lastNode = nodes.find((n) => n.id === currentPath[currentPath.length - 1]);
      if (lastNode) {
        ctx.strokeStyle = 'rgba(0, 212, 255, 0.8)';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(lastNode.x, lastNode.y);
        ctx.lineTo(mousePos.x, mousePos.y);
        ctx.stroke();
      }
    }

    nodes.forEach((node) => {
      const isInPath = currentPath.includes(node.id);
      const size = node.type === 'start' ? 24 : node.type === 'end' ? 24 : 18;
      
      let color = '#666';
      if (node.type === 'start') color = '#00d4ff';
      else if (node.type === 'end') color = '#00ff88';
      else if (isInPath) color = '#ff0066';

      ctx.shadowColor = color;
      ctx.shadowBlur = isInPath ? 20 : 10;
      
      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.arc(node.x, node.y, size, 0, Math.PI * 2);
      ctx.fill();

      ctx.fillStyle = node.type === 'end' ? '#000' : '#fff';
      ctx.beginPath();
      ctx.arc(node.x, node.y, size * 0.5, 0, Math.PI * 2);
      ctx.fill();

      ctx.shadowBlur = 0;

      ctx.fillStyle = '#fff';
      ctx.font = '10px "Share Tech Mono"';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      
      let label = '';
      if (node.type === 'start') label = 'S';
      else if (node.type === 'end') label = 'E';
      else label = String(currentPath.indexOf(node.id) + 1);
      
      ctx.fillText(label, node.x, node.y);
    });

  }, [nodes, connections, currentPath, isDrawing, mousePos, hackingTimeLeft, maxHackingTime]);

  const handleMouseDown = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;
    
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    
    const clickedNode = nodes.find(
      (node) => Math.hypot(node.x - x, node.y - y) < 30
    );
    
    if (clickedNode && clickedNode.type === 'start') {
      setIsDrawing(true);
      clearPath();
      addToPath(clickedNode.id);
    }
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;
    
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    
    setMousePos({ x, y });

    if (!isDrawing) return;

    const hoverNode = nodes.find(
      (node) => Math.hypot(node.x - x, node.y - y) < 30
    );

    if (hoverNode && !currentPath.includes(hoverNode.id)) {
      const lastNodeId = currentPath[currentPath.length - 1];
      addConnection(lastNodeId, hoverNode.id);
      addToPath(hoverNode.id);

      if (hoverNode.type === 'end') {
        setTimeout(() => {
          const targetEnemy = enemies.find(e => !e.hacked);
          if (targetEnemy) {
            hackEnemy(targetEnemy.id);
          }
          onComplete(true);
        }, 300);
      }
    }
  };

  const handleMouseUp = () => {
    setIsDrawing(false);
  };

  const handleTouchStart = (e: React.TouchEvent<HTMLCanvasElement>) => {
    e.preventDefault();
    const touch = e.touches[0];
    const mouseEvent = new MouseEvent('mousedown', {
      clientX: touch.clientX,
      clientY: touch.clientY,
    });
    handleMouseDown(mouseEvent as unknown as React.MouseEvent<HTMLCanvasElement>);
  };

  const handleTouchMove = (e: React.TouchEvent<HTMLCanvasElement>) => {
    e.preventDefault();
    const touch = e.touches[0];
    const mouseEvent = new MouseEvent('mousemove', {
      clientX: touch.clientX,
      clientY: touch.clientY,
    });
    handleMouseMove(mouseEvent as unknown as React.MouseEvent<HTMLCanvasElement>);
  };

  const handleTouchEnd = () => {
    handleMouseUp();
  };

  const timePercent = (hackingTimeLeft / maxHackingTime) * 100;
  const timeColor = timePercent > 50 ? '#00ff88' : timePercent > 25 ? '#ffaa00' : '#ff0066';

  return (
    <div className="absolute inset-0 flex items-center justify-center bg-space-black/95 z-50">
      <div className="absolute inset-0 bg-gradient-to-br from-cyber-blue/10 via-transparent to-neon-pink/10" />
      
      <div className="absolute top-0 left-0 w-full h-1">
        <div 
          className="h-full transition-all duration-100"
          style={{ 
            width: `${timePercent}%`, 
            backgroundColor: timeColor,
            boxShadow: `0 0 20px ${timeColor}`
          }}
        />
      </div>

      <div className="absolute top-4 left-1/2 -translate-x-1/2 text-center z-10">
        <h2 className="font-orbitron text-2xl text-cyber-blue mb-2 tracking-widest">
          INFILTRATING SYSTEM
        </h2>
        <div className="flex items-center justify-center gap-4 font-share-tech text-sm">
          <span className="text-white/60">TIME:</span>
          <span style={{ color: timeColor }}>{hackingTimeLeft.toFixed(1)}s</span>
          <span className="text-white/40">|</span>
          <span className="text-white/60">SECTOR {sector}</span>
          <span className="text-white/40">|</span>
          <span className="text-white/60">WAVE {wave}</span>
        </div>
      </div>

      <canvas
        ref={canvasRef}
        width={800}
        height={500}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        className="max-w-full max-h-full cursor-crosshair"
      />

      <div className="absolute bottom-6 left-1/2 -translate-x-1/2 font-share-tech text-white/40 text-sm text-center">
        CONNECT NODES FROM <span className="text-cyber-blue">S</span> TO <span className="text-matrix-green">E</span>
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
