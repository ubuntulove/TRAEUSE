
import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface Project {
  id: string;
  name: string;
  toolType: "pixelcraft" | "vectorverse";
  thumbnail?: string;
  data: any;
  createdAt: Date;
  updatedAt: Date;
}

interface ProjectStore {
  projects: Project[];
  currentProject: Project | null;
  addProject: (project: Omit<Project, "id" | "createdAt" | "updatedAt">) => void;
  updateProject: (id: string, project: Partial<Project>) => void;
  deleteProject: (id: string) => void;
  setCurrentProject: (project: Project | null) => void;
}

export const useProjectStore = create<ProjectStore>()(
  persist(
    (set) => ({
      projects: [],
      currentProject: null,
      addProject: (project) =>
        set((state) => ({
          projects: [
            ...state.projects,
            {
              ...project,
              id: Date.now().toString(),
              createdAt: new Date(),
              updatedAt: new Date(),
            },
          ],
        })),
      updateProject: (id, project) =>
        set((state) => ({
          projects: state.projects.map((p) =>
            p.id === id ? { ...p, ...project, updatedAt: new Date() } : p
          ),
          currentProject:
            state.currentProject?.id === id
              ? { ...state.currentProject, ...project, updatedAt: new Date() }
              : state.currentProject,
        })),
      deleteProject: (id) =>
        set((state) => ({
          projects: state.projects.filter((p) => p.id !== id),
          currentProject:
            state.currentProject?.id === id ? null : state.currentProject,
        })),
      setCurrentProject: (project) => set({ currentProject: project }),
    }),
    {
      name: "creative-suite-storage",
    }
  )
);
