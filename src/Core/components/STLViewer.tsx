/* eslint-disable react/no-unused-prop-types */
// All props that appear unused in this file are important for the stl renderer
import React, { Component } from 'react';
import { ScaleLoader } from 'halogenium';
import Paint from './Paint';

interface IProps {
  className?: string;
  url?: string;
  width?: number;
  height?: number;
  backgroundColor?: string;
  modelColor?: string;
  rotate?: boolean;
  orbitControls?: boolean;
  cameraX?: number;
  cameraY?: number;
  cameraZ?: number;
  lights?: [number, number, number][];
  lightColor?: string;
  rotation?: number[];
  rotationSpeeds?: number[];
  model?: any;
  zoom?: number;
}

interface IState {}

class STLViewer extends Component<IProps, IState> {
  static defaultProps = {
    className: '',
    url: '',
    backgroundColor: '#EAEAEA',
    modelColor: '#B92C2C',
    height: 400,
    width: 400,
    rotate: true,
    orbitControls: true,
    cameraX: 0,
    cameraY: 0,
    cameraZ: null,
    lights: [
      [0, 1, 5],
      [0, 1, -5],
      [5, 1, 0],
      [-5, 1, 0],
    ],
    lightColor: '#B92C2C',
    rotation: [0, 0, 0],
    rotationSpeeds: [0, 0, 0.02],
    model: undefined,
    zoom: 30,
  };

  paint = new Paint();

  componentDidMount() {
    this.paint.init(this);
  }

  componentDidUpdate() {
    this.paint.init(this);
  }

  componentWillUnmount() {
    this.paint.clean();
  }

  render() {
    const { width, height, modelColor } = this.props;
    return (
      <div
        className={this.props.className}
        style={{
          width,
          height,
          overflow: 'hidden',
        }}
      >
        <div
          style={{
            height: '100%',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
          }}
        >
          <ScaleLoader color={modelColor} size="16px" />
        </div>
      </div>
    );
  }
}

export default STLViewer;
