
import React from 'react';
import { useGameStore } from '@/store/useGameStore';
import { useNavigate } from 'react-router-dom';

const Menu: React.FC = () => {
  const navigate = useNavigate();
  const setGamePhase = useGameStore((state) => state.setGamePhase);

  const handleStartGame = () => {
    setGamePhase('playing');
    navigate('/game');
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center grid-bg">
      <div className="text-center mb-12">
        <h1 className="text-6xl font-black text-red-500 text-glow mb-4 tracking-widest">
          RED ALERT
        </h1>
        <p className="text-xl text-gray-400">WEB EDITION</p>
      </div>

      <div className="flex flex-col gap-4 w-80">
        <button
          onClick={handleStartGame}
          className="btn-industrial py-4 px-8 text-xl font-bold text-white uppercase tracking-wider"
        >
          Start Game
        </button>
        
        <button className="btn-industrial py-4 px-8 text-xl font-bold text-white uppercase tracking-wider">
          Options
        </button>
        
        <button className="btn-industrial py-4 px-8 text-xl font-bold text-white uppercase tracking-wider">
          Credits
        </button>
      </div>

      <footer className="absolute bottom-8 text-gray-600 text-sm">
        &copy; 2024 Web RTS Studio
      </footer>
    </div>
  );
};

export default Menu;
