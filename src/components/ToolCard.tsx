
import { ReactNode } from "react";
import { Link } from "react-router-dom";

interface ToolCardProps {
  title: string;
  description: string;
  icon: ReactNode;
  to: string;
  color: string;
}

export default function ToolCard({
  title,
  description,
  icon,
  to,
  color,
}: ToolCardProps) {
  return (
    <Link
      to={to}
      className="group relative overflow-hidden rounded-2xl border border-slate-700 bg-slate-800/50 p-6 transition-all duration-300 hover:-translate-y-2 hover:border-cyan-400/50 hover:shadow-xl hover:shadow-cyan-500/10"
    >
      <div
        className="mb-4 inline-flex h-16 w-16 items-center justify-center rounded-xl"
        style={{ backgroundColor: color }}
      >
        {icon}
      </div>
      <h3 className="mb-2 text-xl font-semibold text-white">{title}</h3>
      <p className="text-slate-400">{description}</p>
      <div className="pointer-events-none absolute -bottom-10 -right-10 h-32 w-32 rounded-full opacity-0 transition-opacity duration-300 group-hover:opacity-20" style={{ backgroundColor: color }} />
    </Link>
  );
}
