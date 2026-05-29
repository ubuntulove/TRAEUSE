import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { useGameStore } from "@/stores/gameStore";
import TitleScreen from "@/components/TitleScreen";
import GameScreen from "@/components/GameScreen";
import UpgradeShop from "@/components/UpgradeShop";
import LevelSelect from "@/components/LevelSelect";

function AppContent() {
  const phase = useGameStore((state) => state.phase);

  switch (phase) {
    case 'title':
      return <TitleScreen />;
    case 'hacking':
    case 'combat':
      return <GameScreen />;
    case 'shop':
      return <UpgradeShop />;
    case 'select':
      return <LevelSelect />;
    case 'victory':
      return <TitleScreen />;
    case 'gameover':
      return <GameScreen />;
    default:
      return <TitleScreen />;
  }
}

export default function App() {
  return (
    <div className="min-h-screen bg-space-black">
      <AppContent />
    </div>
  );
}
