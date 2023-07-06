import { createContext, useState } from "react";
import "./App.css";
import { BrowserRouter, Route, Routes, Link } from "react-router-dom";

const UserContext = createContext(null);

function App() {
  const [ctx, setCxt] = useState(false);
  return (
    <BrowserRouter>
      <div className="App App-pg">
        <img src="/swoosh.png" height="50px" alt="Logo Unavailable" />
        <h1 className="pg-override">Rover Engagement Display</h1>
        <button className="btn btn-primary">Wow</button>
        <button className="btn btn-outline-secondary">Wuh</button>
      </div>
    </BrowserRouter>
  );
}

export default App;
