import React, { Component } from "react";
import GPS from "./components/GPS";

interface IProps {}

interface IState {
  storedWaypoints: any;
  currentCoords: { lat: number; lon: number };
}

class ControlCenter extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = {
      storedWaypoints: {},
      currentCoords: { lat: 0, lon: 0 },
    };
    this.updateWaypoints = this.updateWaypoints.bind(this);
    this.updateCoords = this.updateCoords.bind(this);
  }

  updateWaypoints = (storedWaypoints: any) => {
    this.setState({ storedWaypoints });
  };

  updateCoords(lat: any, lon: any): void {
    this.setState({
      currentCoords: { lat, lon },
    });
  }

  render() {
    return (
      <div className="App">
        <img src="/swoosh.png" height="50px" alt="No Logo?" />
        <h1 className="pg-override">Rover Engagement Display</h1>
        <div className="row">
          <GPS onCoordsChange={this.updateCoords}/>
          
        </div>
      </div>
    );
  }
}

export default ControlCenter;
