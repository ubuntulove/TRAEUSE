import { Hero } from "../components/Hero";
import { Canvas } from "../components/Canvas";
import { useGameEngine } from "../hooks/useGameEngine";

export default function Home() {
  const { canvasRef, isRunning, handleStart, handleStop, handleReset } = useGameEngine();
  
  return (
    <div className="min-h-screen bg-slate-950">
      <Hero
        isRunning={isRunning}
        onStart={handleStart}
        onStop={handleStop}
        onReset={handleReset}
      />
      <Canvas canvasRef={canvasRef} />
      <section className="py-12 px-4">
        <div className="max-w-4xl mx-auto grid md:grid-cols-3 gap-6">
          <div className="bg-slate-900 rounded-xl p-6 border border-slate-800">
            <h3 className="text-xl font-bold text-cyan-400 mb-3">Physics</h3>
            <p className="text-slate-400">
              Realistic gravity, friction, and collision detection for smooth object movement.
            </p>
          </div>
          <div className="bg-slate-900 rounded-xl p-6 border border-slate-800">
            <h3 className="text-xl font-bold text-purple-400 mb-3">Input</h3>
            <p className="text-slate-400">
              Keyboard and mouse handling for interactive game controls.
            </p>
          </div>
          <div className="bg-slate-900 rounded-xl p-6 border border-slate-800">
            <h3 className="text-xl font-bold text-pink-400 mb-3">Scenes</h3>
            <p className="text-slate-400">
              Simple scene management to organize and update game objects.
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}