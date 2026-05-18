import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "@/pages/Home";
import PixelCraft from "@/pages/PixelCraft";
import VectorVerse from "@/pages/VectorVerse";
import ColorLab from "@/pages/ColorLab";

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/pixelcraft" element={<PixelCraft />} />
        <Route path="/vectorverse" element={<VectorVerse />} />
        <Route path="/colorlab" element={<ColorLab />} />
      </Routes>
    </Router>
  );
}
