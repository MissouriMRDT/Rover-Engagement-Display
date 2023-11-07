import { Component } from "react";

interface IProps {
  onCoordsChange: (lat: number, lon: number) => void;
}

interface IState {
  currentLat: number;
  currentLon: number;
  satelliteCount: number;
  // pitch: number;
  yaw: number;
  // roll: number;
  horizontalAccur: number;
  verticalAccur: number;
  headingAccur: number;
  // distance: number;
  // quality: number;
}

class GPS extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = {
      currentLat: 0,
      currentLon: 0,
      satelliteCount: 0,
      // pitch: 0,
      yaw: 0,
      // roll: 0,
      horizontalAccur: 0,
      verticalAccur: 0,
      headingAccur: 0,
      // distance: 0,
      // quality: 0,
    };
  }

  accurData(data: number[]): void {
    this.setState({
      horizontalAccur: data[0],
      verticalAccur: data[1],
      headingAccur: data[2],
    });
  }

  GPSLatLon(data: number[]) {
    const currentLat = data[0];
    const currentLon = data[1];
    this.setState({
      currentLat,
      currentLon,
    });
    this.props.onCoordsChange(currentLat, currentLon);
  }

  SatelliteCountData(data: number[]) {
    this.setState({
      satelliteCount: data[0],
    });
  }

  IMUData(data: number[]) {
    this.setState({
      // pitch: data[0],
      yaw: data[1],
      // roll: data[2],
    });
  }

  render() {
    return (
      <div className="GPS">
        <div className="card">
          <div className="card-body">
            <h4 className="card-title pg-override">GPS</h4>
            <hr/>
            {[
              {
                title: "Current Lat.",
                value: this.state.currentLat.toFixed(7),
              },
              {
                title: "Current Lon.",
                value: this.state.currentLon.toFixed(7),
              },
              {
                title: "Satellite Count",
                value: this.state.satelliteCount.toFixed(0),
              },
              // { title: 'Distance', value: this.state.distance.toFixed(3) },
              // { title: 'Quality', value: this.state.quality.toFixed(3) },
              // { title: 'Pitch', value: this.state.pitch.toFixed(3) },
              { title: "Yaw", value: this.state.yaw.toFixed(3) },
              // { title: 'Roll', value: this.state.roll.toFixed(3) },
              {
                title: "Pos. Acc",
                value: this.state.horizontalAccur.toFixed(3),
              },
              { title: "Alt. Acc", value: this.state.verticalAccur.toFixed(3) },
              { title: "Comp. Acc", value: this.state.headingAccur.toFixed(3) },
            ].map((datum) => {
              const { title, value } = datum;
              return (
                <div key={title}>
                  <p>
                    {title}: {value}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    );
  }
}

export default GPS;