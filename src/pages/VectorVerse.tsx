
import { useEffect, useRef, useState } from "react";
import { Home, Save, Trash2, PenTool, Square, Circle, Type, Undo2, Redo2 } from "lucide-react";
import { Link, useSearchParams } from "react-router-dom";
import { useProjectStore } from "@/store/projectStore";

interface VectorObject {
  id: string;
  type: "rect" | "circle" | "text" | "path";
  x: number;
  y: number;
  width?: number;
  height?: number;
  radius?: number;
  fill: string;
  text?: string;
}

export default function VectorVerse() {
  const svgRef = useRef<SVGSVGElement>(null);
  const [objects, setObjects] = useState<VectorObject[]>([]);
  const [searchParams] = useSearchParams();
  const { projects, addProject, updateProject, setCurrentProject } = useProjectStore();
  const [projectName, setProjectName] = useState("Untitled Vector Project");

  useEffect(() => {
    const projectId = searchParams.get("project");
    if (projectId) {
      const project = projects.find((p) => p.id === projectId);
      if (project) {
        setProjectName(project.name);
        setCurrentProject(project);
        if (project.data) {
          setObjects(project.data);
        }
      }
    }
  }, [searchParams, projects, setCurrentProject]);

  const handleSave = () => {
    const projectId = searchParams.get("project");
    const thumbnail = svgRef.current?.toDataURL("image/png");
    if (projectId) {
      updateProject(projectId, { name: projectName, data: objects, thumbnail });
    } else {
      addProject({
        name: projectName,
        toolType: "vectorverse",
        data: objects,
        thumbnail,
      });
    }
    alert("Project saved!");
  };

  const addShape = (type: "rect" | "circle") => {
    const newObj: VectorObject = {
      id: Date.now().toString(),
      type,
      x: 100,
      y: 100,
      fill: type === "rect" ? "#22D3EE" : "#8B5CF6",
      ...(type === "rect" ? { width: 100, height: 100 } : { radius: 50 }),
    };
    setObjects([...objects, newObj]);
  };

  const addText = () => {
    const newObj: VectorObject = {
      id: Date.now().toString(),
      type: "text",
      x: 100,
      y: 100,
      fill: "#0F172A",
      text: "Hello Vector!",
    };
    setObjects([...objects, newObj]);
  };

  return (
    <div className="flex h-screen bg-slate-900 text-white">
      <div className="w-16 flex flex-col items-center gap-4 border-r border-slate-700 bg-slate-800 py-6">
        <Link to="/" className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Home className="h-6 w-6 text-slate-300" />
        </Link>
        <button onClick={handleSave} className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Save className="h-6 w-6 text-slate-300" />
        </button>
        <button className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Undo2 className="h-6 w-6 text-slate-300" />
        </button>
        <button className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Redo2 className="h-6 w-6 text-slate-300" />
        </button>
        <div className="flex-1" />
        <button className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Trash2 className="h-6 w-6 text-slate-300" />
        </button>
      </div>

      <div className="flex-1 flex flex-col">
        <div className="flex items-center gap-4 border-b border-slate-700 bg-slate-800 px-6 py-3">
          <input
            type="text"
            value={projectName}
            onChange={(e) => setProjectName(e.target.value)}
            className="bg-transparent border-none outline-none text-xl font-semibold w-64"
          />
          <div className="flex items-center gap-2 ml-auto">
            <button className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition">
              <PenTool className="h-5 w-5" />
              <span>Pen</span>
            </button>
            <button onClick={() => addShape("rect")} className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition">
              <Square className="h-5 w-5" />
              <span>Rectangle</span>
            </button>
            <button onClick={() => addShape("circle")} className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition">
              <Circle className="h-5 w-5" />
              <span>Circle</span>
            </button>
            <button onClick={addText} className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition">
              <Type className="h-5 w-5" />
              <span>Text</span>
            </button>
          </div>
        </div>

        <div className="flex-1 flex items-center justify-center bg-slate-950 p-8">
          <div className="bg-white shadow-2xl">
            <svg
              ref={svgRef}
              width="800"
              height="600"
              xmlns="http://www.w3.org/2000/svg"
            >
              {objects.map((obj) => {
                switch (obj.type) {
                  case "rect":
                    return (
                      <rect
                        key={obj.id}
                        x={obj.x}
                        y={obj.y}
                        width={obj.width}
                        height={obj.height}
                        fill={obj.fill}
                      />
                    );
                  case "circle":
                    return (
                      <circle
                        key={obj.id}
                        cx={obj.x}
                        cy={obj.y}
                        r={obj.radius}
                        fill={obj.fill}
                      />
                    );
                  case "text":
                    return (
                      <text
                        key={obj.id}
                        x={obj.x}
                        y={obj.y}
                        fill={obj.fill}
                        fontSize="30"
                      >
                        {obj.text}
                      </text>
                    );
                  default:
                    return null;
                }
              })}
            </svg>
          </div>
        </div>
      </div>

      <div className="w-64 border-l border-slate-700 bg-slate-800 p-4">
        <h3 className="mb-4 text-lg font-semibold">Properties</h3>
        <div className="space-y-4">
          <p className="text-slate-400 text-sm">Select an object to edit its properties</p>
        </div>
      </div>
    </div>
  );
}
