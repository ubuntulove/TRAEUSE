
import ToolCard from "@/components/ToolCard";
import { useProjectStore } from "@/store/projectStore";
import { Image, PenTool, Palette } from "lucide-react";
import { Link } from "react-router-dom";

export default function Home() {
  const { projects } = useProjectStore();

  const tools = [
    {
      title: "PixelCraft",
      description: "A powerful photo editor with layers and filters",
      icon: <Image className="h-8 w-8 text-white" />,
      to: "/pixelcraft",
      color: "#06B6D4",
    },
    {
      title: "VectorVerse",
      description: "Create beautiful vector graphics and illustrations",
      icon: <PenTool className="h-8 w-8 text-white" />,
      to: "/vectorverse",
      color: "#8B5CF6",
    },
    {
      title: "ColorLab",
      description: "Advanced color picker with harmony modes",
      icon: <Palette className="h-8 w-8 text-white" />,
      to: "/colorlab",
      color: "#F97316",
    },
  ];

  return (
    <div className="min-h-screen bg-slate-900 text-white">
      <div className="container mx-auto px-6 py-12">
        <div className="mb-12 text-center">
          <h1 className="mb-4 text-5xl font-bold bg-gradient-to-r from-cyan-400 to-purple-500 bg-clip-text text-transparent">
            Creative Suite
          </h1>
          <p className="text-slate-400 text-lg">Unleash your creativity</p>
        </div>

        <div className="mb-16">
          <h2 className="mb-6 text-2xl font-semibold">Tools</h2>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {tools.map((tool, index) => (
              <ToolCard key={index} {...tool} />
            ))}
          </div>
        </div>

        {projects.length > 0 && (
          <div>
            <h2 className="mb-6 text-2xl font-semibold">Recent Projects</h2>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {projects.map((project) => (
                <Link
                  key={project.id}
                  to={`/${project.toolType}?project=${project.id}`}
                  className="group rounded-xl border border-slate-700 bg-slate-800/30 p-4 transition-all hover:border-cyan-500/50 hover:bg-slate-800/50"
                >
                  <div className="mb-3 h-32 rounded-lg bg-slate-700/50 flex items-center justify-center">
                    {project.thumbnail ? (
                      <img
                        src={project.thumbnail}
                        alt={project.name}
                        className="h-full w-full object-cover rounded-lg"
                      />
                    ) : (
                      <div className="text-slate-500">No preview</div>
                    )}
                  </div>
                  <h3 className="font-medium text-white group-hover:text-cyan-400">
                    {project.name}
                  </h3>
                  <p className="text-sm text-slate-500">
                    {new Date(project.updatedAt).toLocaleDateString()}
                  </p>
                </Link>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
