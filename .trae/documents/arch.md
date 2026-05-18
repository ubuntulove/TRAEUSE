
## 1. Architecture Design
```mermaid
graph TD
    A[Frontend: React + TypeScript] --> B[State Management: Zustand]
    A --> C[Routing: React Router DOM]
    A --> D[UI: Tailwind CSS + Lucide Icons]
    A --> E[Canvas/SVG: Fabric.js / Native SVG]
    F[Storage: LocalStorage] --> A
```

## 2. Technology Description
- Frontend: React@18 + TypeScript + Vite
- Initialization Tool: vite-init with react-ts template
- Backend: None (local storage only)
- State Management: Zustand for app state
- Routing: React Router DOM for navigation
- UI: Tailwind CSS for styling, Lucide React for icons
- Canvas Library: Fabric.js for photo editor
- SVG: Native SVG for vector editor
- Storage: LocalStorage for project persistence

## 3. Route Definitions
| Route | Purpose |
|-------|---------|
| / | Home dashboard |
| /pixelcraft | Photo editor |
| /vectorverse | Vector editor |

## 4. API Definitions (if backend exists)
Not applicable (no backend).

## 5. Server Architecture Diagram (if backend exists)
Not applicable (no backend).

## 6. Data Model (if applicable)
### 6.1 Data Model Definition
```mermaid
erDiagram
    PROJECT {
        string id
        string name
        string toolType
        string thumbnail
        json data
        date createdAt
        date updatedAt
    }
```

### 6.2 Data Definition Language
Not applicable (using LocalStorage).
