
import { useState } from "react";
import { Home, Copy } from "lucide-react";
import { Link } from "react-router-dom";

export default function ColorLab() {
  const [color, setColor] = useState("#22D3EE");
  const [copied, setCopied] = useState(false);

  const handleCopy = (hex: string) => {
    navigator.clipboard.writeText(hex);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  const hexToRgb = (hex: string) => {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result
      ? {
          r: parseInt(result[1], 16),
          g: parseInt(result[2], 16),
          b: parseInt(result[3], 16),
        }
      : null;
  };

  const rgb = hexToRgb(color) || { r: 0, g: 0, b: 0 };

  const getComplementary = (hex: string) => {
    const rgb = hexToRgb(hex);
    if (!rgb) return "#000000";
    return `#${(255 - rgb.r).toString(16).padStart(2, "0")}${(255 - rgb.g).toString(16).padStart(2, "0")}${(255 - rgb.b).toString(16).padStart(2, "0")}`;
  };

  const complementary = getComplementary(color);

  return (
    <div className="flex h-screen bg-slate-900 text-white">
      <div className="w-16 flex flex-col items-center gap-4 border-r border-slate-700 bg-slate-800 py-6">
        <Link to="/" className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Home className="h-6 w-6 text-slate-300" />
        </Link>
      </div>

      <div className="flex-1 flex">
        <div className="flex-1 flex flex-col items-center justify-center p-8">
          <div
            className="w-64 h-64 rounded-2xl shadow-2xl mb-8"
            style={{ backgroundColor: color }}
          />
          <div className="flex items-center gap-4">
            <input
              type="color"
              value={color}
              onChange={(e) => setColor(e.target.value)}
              className="w-24 h-24 rounded-lg cursor-pointer"
            />
            <div className="space-y-2">
              <div className="flex items-center gap-2 bg-slate-800 px-4 py-2 rounded-lg border border-slate-700">
                <span className="text-slate-400 w-12">HEX:</span>
                <span className="font-mono">{color}</span>
                <button
                  onClick={() => handleCopy(color)}
                  className="ml-2 text-slate-400 hover:text-white transition"
                >
                  <Copy className="h-4 w-4" />
                </button>
                {copied && <span className="text-xs text-green-400">Copied!</span>}
              </div>
              <div className="flex items-center gap-2 bg-slate-800 px-4 py-2 rounded-lg border border-slate-700">
                <span className="text-slate-400 w-12">RGB:</span>
                <span className="font-mono">{rgb.r}, {rgb.g}, {rgb.b}</span>
              </div>
            </div>
          </div>
        </div>

        <div className="w-80 border-l border-slate-700 bg-slate-800 p-6">
          <h2 className="text-2xl font-semibold mb-6">Color Harmonies</h2>
          <div className="space-y-4">
            <div>
              <h3 className="text-sm font-medium text-slate-400 mb-2">Complementary</h3>
              <div className="flex gap-2">
                <div
                  className="flex-1 h-16 rounded-lg cursor-pointer"
                  style={{ backgroundColor: color }}
                  onClick={() => handleCopy(color)}
                />
                <div
                  className="flex-1 h-16 rounded-lg cursor-pointer"
                  style={{ backgroundColor: complementary }}
                  onClick={() => handleCopy(complementary)}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
