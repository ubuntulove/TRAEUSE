import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Menu from "@/pages/Menu";
import Gameplay from "@/pages/Gameplay";

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Menu />} />
        <Route path="/game" element={<Gameplay />} />
      </Routes>
    </Router>
  );
}
