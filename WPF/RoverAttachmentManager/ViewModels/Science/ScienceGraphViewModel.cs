﻿using Caliburn.Micro;
using Core.Interfaces;
using Core.Models;
using Core.RoveProtocol;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using RoverAttachmentManager.Models.Science;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace RoverAttachmentManager.ViewModels.Science
{
    public class ScienceGraphViewModel : PropertyChangedBase, IRovecommReceiver
    {
        private readonly IRovecomm _rovecomm;
        private readonly IDataIdResolver _idResolver;
        private readonly ILogger _log;

        private readonly ScienceGraphModel _model;

        public PlotModel SpectrometerPlotModel { set; private get; }
        public PlotModel SensorPlotModel { set; private get; }
        public PlotModel MethanePlotModel { set; private get; }
        public PlotModel CO2PlotModel { set; private get; }
        public PlotModel O2PlotModel { set; private get; }
        public OxyPlot.Series.LineSeries SpectrometerSeries;
        public OxyPlot.Series.LineSeries MethaneConcentrationSeries;
        public OxyPlot.Series.LineSeries CO2ConcentrationSeries;
        public OxyPlot.Series.LineSeries O2ConcentrationSeries;

        public ScienceViewModel Science;

        public bool Graphing = false;

        public double[] SiteTimes = new double[12];


        public float MethaneConcentration
        {
            get
            {
                return _model.MethaneConcentration;
            }
            set
            {
                _model.MethaneConcentration = value;
                NotifyOfPropertyChange(() => MethaneConcentration);
            }
        }
        public float MethaneTemperature
        {
            get
            {
                return _model.MethaneTemperature;
            }
            set
            {
                _model.MethaneTemperature = value;
                NotifyOfPropertyChange(() => MethaneTemperature);
            }
        }
        public float CO2Concentration
        {
            get
            {
                return _model.CO2Concentration;
            }
            set
            {
                _model.CO2Concentration = value;
                NotifyOfPropertyChange(() => CO2Concentration);
            }
        }
        public float O2PartialPressure
        {
            get
            {
                return _model.O2PartialPressure;
            }
            set
            {
                _model.O2PartialPressure = value;
                NotifyOfPropertyChange(() => O2PartialPressure);
            }
        }
        public float O2Temperature
        {
            get
            {
                return _model.O2Temperature;
            }
            set
            {
                _model.O2Temperature = value;
                NotifyOfPropertyChange(() => O2Temperature);
            }
        }
        public float O2Concentration
        {
            get
            {
                return _model.O2Concentration;
            }
            set
            {
                _model.O2Concentration = value;
                NotifyOfPropertyChange(() => O2Concentration);
            }
        }
        public float O2BarometricPressure
        {
            get
            {
                return _model.O2BarometricPressure;
            }
            set
            {
                _model.O2BarometricPressure = value;
                NotifyOfPropertyChange(() => O2BarometricPressure);
            }
        }
        public DateTime StartTime
        {
            get
            {
                return _model.StartTime;
            }
            set
            {
                _model.StartTime = value;
                NotifyOfPropertyChange(() => StartTime);
            }
        }
        public int RunCount
        {
            get
            {
                return _model.RunCount;
            }
            set
            {
                _model.RunCount = value;
                NotifyOfPropertyChange(() => RunCount);
            }
        }
        public ushort SpectrometerPortNumber
        {
            get
            {
                return _model.SpectrometerPortNumber;
            }
            set
            {
                _model.SpectrometerPortNumber = value;
                NotifyOfPropertyChange(() => SpectrometerPortNumber);
            }
        }
        public string SpectrometerFilePath
        {
            get
            {
                return _model.SpectrometerFilePath;
            }
            set
            {
                _model.SpectrometerFilePath = value;
                NotifyOfPropertyChange(() => SpectrometerFilePath);
            }
        }
        public System.Net.IPAddress SpectrometerIPAddress
        {
            get
            {
                return _model.SpectrometerIPAddress;
            }
            set
            {
                _model.SpectrometerIPAddress = value;
                NotifyOfPropertyChange(() => SpectrometerIPAddress);
            }
        }

        public SiteManagmentViewModel SiteManagment
        {
            get
            {
                return _model._siteManagment;
            }
            set
            {
                _model._siteManagment = value;
                NotifyOfPropertyChange(() => SiteManagment);
            }
        }

        public ObservableCollection<PlotModel> Plots
        {
            get
            {
                return _model.Plots;

            }
            set
            {
                _model.Plots = value;
                NotifyOfPropertyChange(() => Plots);
            }
        }

        public PlotModel SelectedPlots
        {
            get
            {
                return _model.SelectedPlot;
            }
            set
            {
                _model.SelectedPlot = value;
                NotifyOfPropertyChange(() => SelectedPlots);
            }
        }

        public ScienceGraphViewModel(IRovecomm networkMessenger, IDataIdResolver idResolver, ILogger log, ScienceViewModel parent)
        {
            _model = new ScienceGraphModel();
            _rovecomm = networkMessenger;
            _idResolver = idResolver;
            _log = log;
            Science = parent;

            _rovecomm.NotifyWhenMessageReceived(this, "Methane");
            _rovecomm.NotifyWhenMessageReceived(this, "CO2");
            _rovecomm.NotifyWhenMessageReceived(this, "O2");

            SensorPlotModel = new PlotModel { Title = "Methane, CO2, and O2 Data" };
            MethaneConcentrationSeries = new OxyPlot.Series.LineSeries();
            CO2ConcentrationSeries = new OxyPlot.Series.LineSeries();
            O2ConcentrationSeries = new OxyPlot.Series.LineSeries();
            SensorPlotModel.Series.Add(MethaneConcentrationSeries);
            SensorPlotModel.Series.Add(CO2ConcentrationSeries);
            SensorPlotModel.Series.Add(O2ConcentrationSeries);
            SensorPlotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "mm:ss", Title = "Time" });
            SensorPlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Title = "Parts Per Million" });

            MethanePlotModel = new PlotModel { Title = "Methane" };
            MethaneConcentrationSeries = new OxyPlot.Series.LineSeries();
            MethanePlotModel.Series.Add(MethaneConcentrationSeries);
            MethanePlotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "mm:ss", Title = "Time" });
            MethanePlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Title = "Parts Per Million" });

            CO2PlotModel = new PlotModel { Title = "CO2" };
            CO2ConcentrationSeries = new OxyPlot.Series.LineSeries();
            CO2PlotModel.Series.Add(CO2ConcentrationSeries);
            CO2PlotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "mm:ss", Title = "Time" });
            CO2PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Title = "Parts Per Million" });

            O2PlotModel = new PlotModel { Title = "O2" };
            O2ConcentrationSeries = new OxyPlot.Series.LineSeries();
            O2PlotModel.Series.Add(O2ConcentrationSeries);
            O2PlotModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "mm:ss", Title = "Time" });
            O2PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Title = "Parts Per Million" });

            Plots = new ObservableCollection<PlotModel>() { SensorPlotModel, MethanePlotModel, CO2PlotModel, O2PlotModel };
            SelectedPlots = SensorPlotModel;
        }


        public void CreateSiteAnnotation()
        {
            SelectedPlots.Annotations.Add(new OxyPlot.Annotations.RectangleAnnotation
            {
                MinimumX = SiteTimes[(Science.SiteNumber - 1) * 2],
                MaximumX = SiteTimes[((Science.SiteNumber - 1) * 2) + 1],
                Text = "Site " + Science.SiteNumber,
                Fill = OxyColor.FromAColor(50, OxyColors.DarkOrange),

            });
        }

        public void UpdateSensorGraphs()
        {
            if (!Graphing) { return; }

            TimeSpan nowSpan = DateTime.UtcNow.Subtract(StartTime);
            DateTime now = new DateTime(nowSpan.Ticks);

            MethaneConcentrationSeries.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(now), MethaneConcentration));
            CO2ConcentrationSeries.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(now), CO2Concentration));
            O2ConcentrationSeries.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(now), O2Concentration));
            SelectedPlots.InvalidatePlot(true);
        }


        public void AddSiteAnnotation(double x, string text)
        {
            SelectedPlots.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = LineAnnotationType.Vertical, X = x, Color = OxyColors.Green, Text = text });
        }


        public void StartSensorGraphs()
        {
            StartTime = DateTime.UtcNow;
            Graphing = true;
            ClearSensorGraphs();
        }


        public void ClearSensorGraphs()
        {
            MethaneConcentrationSeries.Points.Clear();
            CO2ConcentrationSeries.Points.Clear();
            O2ConcentrationSeries.Points.Clear();
        }

        public void ExportGraph(PlotModel model, string filename, int height)
        {
            var pngExporter = new PngExporter { Width = 600, Height = height, Background = OxyColors.White };
            pngExporter.ExportToFile(model, filename);
        }


        public double AverageValueForSeries(OxyPlot.Series.LineSeries series, string plotTitle, string unitsTitle, int plotMax, string filename)
        {
            List<DataPoint> points = series.Points;

            Predicate<DataPoint> isInRange = dataPoint => dataPoint.X >= SiteTimes[Science.SiteNumber * 2] && dataPoint.X <= SiteTimes[(Science.SiteNumber * 2) + 1];

            points = points.FindAll(isInRange);

            PlotModel tempModel = new PlotModel { Title = plotTitle };
            OxyPlot.Series.LineSeries tempSeries = new OxyPlot.Series.LineSeries();
            tempModel.Series.Add(tempSeries);
            tempModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "mm:ss", Title = "Time since task start (minutes:seconds)" });
            tempModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Title = unitsTitle, Minimum = 0, Maximum = plotMax });

            tempSeries.Points.AddRange(points);

            ExportGraph(tempModel, filename, 300);

            double tally = 0;
            foreach (DataPoint dataPoint in points)
            {
                tally += dataPoint.Y;
            }

            return tally / points.Count;
        }

        private DateTime GetTimeDiff()
        {
            TimeSpan nowSpan = DateTime.UtcNow.Subtract(StartTime);
            return new DateTime(nowSpan.Ticks);
        }

        public void ReceivedRovecommMessageCallback(Packet packet, bool reliable)
        {
            switch (packet.Name)
            {
                case "Methane":
                    float[] MethaneData = packet.GetDataArray<float>();
                    MethaneConcentration = (float)(MethaneData[0]);
                    MethaneTemperature = (float)(MethaneData[1]);
                    UpdateSensorGraphs();
                    break;

                case "CO2":
                    CO2Concentration = (float)(packet.GetData<float>());
                    UpdateSensorGraphs();
                    break;

                case "O2":
                    float[] O2Data = packet.GetDataArray<float>();
                    O2PartialPressure = (float)(O2Data[0]);
                    O2Temperature = (float)(O2Data[1]);
                    O2Concentration = (float)(O2Data[2]);
                    O2BarometricPressure = (float)(O2Data[3]);
                    UpdateSensorGraphs();
                    break;

                default:
                    break;
            }
        }

    }
}
