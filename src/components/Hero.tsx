
import { Play, Pause, RotateCcw } from "lucide-react";

interface HeroProps {
  isRunning: boolean;
  onStart: () => void;
  onStop: () => void;
  onReset: () => void;
}

export function Hero({ isRunning, onStart, onStop, onReset }: HeroProps) {
  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-slate-900 via-indigo-900 to-slate-900 text-white py-20 px-4">
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-40 -left-40 w-80 h-80 bg-cyan-500/20 rounded-full blur-3xl animate-pulse"></div>
        <div className="absolute -bottom-40 -right-40 w-80 h-80 bg-purple-500/20 rounded-full blur-3xl animate-pulse" style={{ animationDelay: "1s" }}></div>
      </div>
      <div className="relative max-w-4xl mx-auto text-center">
        <h1 className="text-5xl md:text-6xl font-bold mb-6 bg-gradient-to-r from-cyan-400 to-purple-400 bg-clip-text text-transparent">
          Neon Engine
        </h1>
        <p className="text-lg md:text-xl text-slate-300 mb-10 max-w-2xl mx-auto">
          A lightweight, browser-based 2D game engine with physics, input handling, and scene management.
        </p>
        <div className="flex flex-wrap justify-center gap-4">
          {!isRunning ? (
            <button
              onClick={onStart}
              className="flex items-center gap-2 px-8 py-4 bg-cyan-500 hover:bg-cyan-400 text-slate-900 font-bold rounded-xl shadow-lg shadow-cyan-500/50 transition-all hover:scale-105"
            >
              <Play size={24} />
              Start Demo
            </button>
          ) : (
            <button
              onClick={onStop}
              className="flex items-center gap-2 px-8 py-4 bg-orange-500 hover:bg-orange-400 text-slate-900 font-bold rounded-xl shadow-lg shadow-orange-500/50 transition-all hover:scale-105"
            >
              <Pause size={24} />
              Pause
            </button>
          )}
          <button
            onClick={onReset}
            className="flex items-center gap-2 px-8 py-4 bg-slate-700 hover:bg-slate-600 text-white font-bold rounded-xl shadow-lg transition-all hover:scale-105"
          >
            <RotateCcw size={24} />
            Reset
          </button>
        </div>
        <div className="mt-12 text-sm text-slate-400">
          Use <kbd className="px-2 py-1 bg-slate-800 rounded text-cyan-400">WASD</kbd> or <kbd className="px-2 py-1 bg-slate-800 rounded text-cyan-400">Arrow Keys</kbd> to move the cyan box
        </div>
      </div>
    </section>
  );
}
