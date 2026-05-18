
interface CanvasProps {
  canvasRef: React.RefObject<HTMLCanvasElement>;
}

export function Canvas({ canvasRef }: CanvasProps) {
  return (
    <div className="flex justify-center p-4">
      <div className="relative rounded-2xl overflow-hidden shadow-2xl shadow-cyan-500/20 border border-cyan-500/30">
        <canvas
          ref={canvasRef}
          width={800}
          height={500}
          className="bg-slate-900"
        />
      </div>
    </div>
  );
}
