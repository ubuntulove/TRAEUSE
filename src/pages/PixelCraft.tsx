
import { useEffect, useRef, useState } from "react";
import * as fabric from "fabric";
import {
  Home,
  Save,
  Trash2,
  Image as ImageIcon,
  Square,
  Circle,
  Type,
  Undo2,
  Redo2,
} from "lucide-react";
import { Link, useSearchParams } from "react-router-dom";
import { useProjectStore } from "@/store/projectStore";

export default function PixelCraft() {
  const canvasRef = useRef<HTMLDivElement>(null);
  const [canvas, setCanvas] = useState<fabric.Canvas | null>(null);
  const [searchParams] = useSearchParams();
  const { projects, addProject, updateProject, setCurrentProject } =
    useProjectStore();
  const [projectName, setProjectName] = useState("Untitled Project");

  useEffect(() => {
    if (!canvasRef.current) return;

    const newCanvas = new fabric.Canvas("pixelcraft-canvas", {
      width: 800,
      height: 600,
      backgroundColor: "#ffffff",
    });

    setCanvas(newCanvas);

    const projectId = searchParams.get("project");
    if (projectId) {
      const project = projects.find((p) => p.id === projectId);
      if (project) {
        setProjectName(project.name);
        setCurrentProject(project);
        if (project.data) {
          newCanvas.loadFromJSON(project.data, newCanvas.renderAll.bind(newCanvas));
        }
      }
    }

    return () => {
      newCanvas.dispose();
    };
  }, [searchParams, projects, setCurrentProject]);

  const handleSave = () => {
    if (!canvas) return;
    const json = canvas.toJSON();
    const thumbnail = canvas.toDataURL({
      format: "png",
      multiplier: 0.2,
    });

    const projectId = searchParams.get("project");
    if (projectId) {
      updateProject(projectId, { name: projectName, data: json, thumbnail });
    } else {
      addProject({
        name: projectName,
        toolType: "pixelcraft",
        data: json,
        thumbnail,
      });
    }
    alert("Project saved!");
  };

  const addImage = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!canvas || !e.target.files || e.target.files.length === 0) return;
    const file = e.target.files[0];
    const reader = new FileReader();
    reader.onload = (event) => {
      fabric.Image.fromURL(event.target?.result as string, (img) => {
        img.scaleToWidth(400);
        canvas.add(img).setActiveObject(img);
      });
    };
    reader.readAsDataURL(file);
  };

  const addShape = (type: "rect" | "circle") => {
    if (!canvas) return;
    const shape =
      type === "rect"
        ? new fabric.Rect({
            left: 100,
            top: 100,
            fill: "#22D3EE",
            width: 100,
            height: 100,
          })
        : new fabric.Circle({
            left: 100,
            top: 100,
            fill: "#8B5CF6",
            radius: 50,
          });
    canvas.add(shape).setActiveObject(shape);
  };

  const addText = () => {
    if (!canvas) return;
    const text = new fabric.IText("Hello World!", {
      left: 100,
      top: 100,
      fill: "#0F172A",
      fontSize: 30,
    });
    canvas.add(text).setActiveObject(text);
  };

  return (
    <div className="flex h-screen bg-slate-900 text-white">
      <div className="w-16 flex flex-col items-center gap-4 border-r border-slate-700 bg-slate-800 py-6">
        <Link to="/" className="p-2 rounded-lg hover:bg-slate-700 transition">
          <Home className="h-6 w-6 text-slate-300" />
        </Link>
        <button
          onClick={handleSave}
          className="p-2 rounded-lg hover:bg-slate-700 transition"
        >
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
            <label className="flex items-center gap-2 cursor-pointer px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition">
              <ImageIcon className="h-5 w-5" />
              <span>Add Image</span>
              <input
                type="file"
                accept="image/*"
                className="hidden"
                onChange={addImage}
              />
            </label>
            <button
              onClick={() => addShape("rect")}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition"
            >
              <Square className="h-5 w-5" />
              <span>Rectangle</span>
            </button>
            <button
              onClick={() => addShape("circle")}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition"
            >
              <Circle className="h-5 w-5" />
              <span>Circle</span>
            </button>
            <button
              onClick={addText}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-700 hover:bg-slate-600 transition"
            >
              <Type className="h-5 w-5" />
              <span>Text</span>
            </button>
          </div>
        </div>

        <div className="flex-1 flex items-center justify-center bg-slate-950 p-8">
          <div ref={canvasRef} className="shadow-2xl">
            <canvas id="pixelcraft-canvas" />
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
