import { Component } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import ControlCenter from "./RED/ControlCenter";
import "./App.css";

interface IProps {}

interface IState {}

class App extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = {};
  }

  render() {
    return (
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<ControlCenter />} />
        </Routes>
      </BrowserRouter>
    );
  }
}

export default App;
