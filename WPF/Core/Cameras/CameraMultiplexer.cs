﻿using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media.Imaging;
using System.IO;

namespace Core.Cameras {
	public static class CameraMultiplexer {
		private static readonly List<string> BASE_ADDRESSES = new List<string>()
		{
			"http://192.168.1.50:8080",
			"http://192.168.1.51:8080",
            "http://192.168.1.139:8081",
            "http://192.168.1.139:8082"
        };

		private static bool initialized = false;
		static List<FeedInfo> feeds = new List<FeedInfo>();
		static Timer watchdog = new Timer();
		static int watchdogSkip = 0;

		/// <summary>
		/// Returns the total number of connected cameras.
		/// </summary>
		public static int TotalCameraFeeds { get; private set; } = 16;

		/// <summary>
		/// Initialize the multiplexer. Must be called before any feeds are accessed.
		/// </summary>
		public static void Initialize() {
			if(initialized) {
				CommonLog.Instance.Log("Warning: Camera multiplexer already initialized.");
				return;
			}
			
			for(int i = 1; i <= TotalCameraFeeds; i++) {
				Uri uri = ConstructAddress(i);
				//CommonLog.Instance.Log("Created URL {0}", uri);

				MjpegDecoder add = new MjpegDecoder();
				add.ParseStream(uri);
				add.FrameReady += Decoder_FrameReady;
				add.Error += Decoder_Error;

				feeds.Add(new FeedInfo() {
					URI = uri,
					Index = i,
					Decoder = add,
					RenderSurfaces = new List<Image>(),
					LastFrameTime = DateTime.Now,
					LastFrame = new BitmapImage()
				});
			}

			watchdog.Interval = 1000;
			watchdog.Elapsed += Watchdog_Elapsed;
			watchdog.Start();

			initialized = true;
		}

		private static Uri ConstructAddress(int camera)
		{
            //cameras 1-4 should be index 0, cameras 5-8 should be index 2, etc.
            //subtracting 1, dividing by 4, and truncating the decimal achieves this
            int index = (camera-1) / 4;

            //Then camera should be a number between 1 and 4. This weird equation is the result of
            //numbering 1-4 instead of 0-3. But 4-1=3, 3%4=3, 3+1 = 4 and 5-1=4, 4%4 = 0, 0+1 = 1
            camera = ((camera-1) % 4)+1;

			string addr = BASE_ADDRESSES[index];

			return new Uri($"{addr}/{camera}/stream");
		}

		/// <summary>
		/// Maps a MjpegDecoder object to it's index. Should only be called by event handlers attached to a MjpegDecoder object.
		/// </summary>
		/// <param name="sender"></param>
		/// <returns></returns>
		private static int DecoderToIndex(MjpegDecoder sender) {
			try {
				using (IEnumerator<FeedInfo> e = feeds.Where(x => x.Decoder.UUID == sender.UUID).GetEnumerator()) {
					e.MoveNext();
					return e.Current.Index;
				}
			} catch(NullReferenceException ex) {
				throw new ArgumentOutOfRangeException("Passed MjpegDecoder object does not have an index. This is an internal error in the multiplexer class and needs to be reported to the signals team.", ex);
			}
		}

		/// <summary>
		/// Adds a rendering surface to display the output from a camera.
		/// </summary>
		/// <param name="camera">Camera feed to display.</param>
		/// <param name="surface">Name of an Image to display on.</param>
		public static void AddSurface(Camera camera, Image surface) {
			int index = (int)camera;

			if (index > feeds.Count) throw new ArgumentOutOfRangeException("Passed camera index is not valid");
			feeds[index - 1].RenderSurfaces.Add(surface);
		}

		public static void RemoveSurface(Image surface) {
			for(int i = 0; i <= TotalCameraFeeds - 1; i++) {
				Image match = feeds[i].RenderSurfaces.Find(new Predicate<Image>(x => x.Name == surface.Name));
				if(match != null) feeds[i].RenderSurfaces.Remove(match);
			}
		}

		/// <summary>
		/// Saves and returns the last displayed frame from a camera.
		/// </summary>
		/// <param name="camera">Camera stream to screenshot</param>
		/// <returns>Bitmap image of the feed</returns>
		public static void Screenshot(Camera camera)
		{
			int index = (int)camera;

			if (index > feeds.Count) throw new ArgumentOutOfRangeException("Passed camera index is not valid");
			System.Drawing.Bitmap img = ConvertBitmapImageToBitmap(feeds[index - 1].LastFrame);

			// If the image width is 1, there was an error converting the image
			if (img.Width == 1) return;

			DateTime now = DateTime.Now;
			string fn = $"camera{Pad(index)}-{Pad(now.Year)}-{Pad(now.Month)}-{Pad(now.Day)}_{Pad(now.Hour)}-{Pad(now.Minute)}-{Pad(now.Second)}.jpg";

			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Rover Screenshots");
			Directory.CreateDirectory(path);
			path = Path.Combine(path, fn);

			img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

			CommonLog.Instance.Log("Screenshot of camera stream {0} saved to {1}", index, path);
		}

		public static string Pad(int raw)
		{
			return raw.ToString().PadLeft(2, '0');
		}

		/// <summary>
		/// Converts a BitmapImage to a Bitmap
		/// </summary>
		/// <param name="bitmapImage">Input BitmapImage to convert</param>
		/// <returns>Converted Bitmap</returns>
		private static System.Drawing.Bitmap ConvertBitmapImageToBitmap(BitmapImage bitmapImage) {
			try {
				using (MemoryStream outStream = new MemoryStream()) {
					BitmapEncoder enc = new BmpBitmapEncoder();
					enc.Frames.Add(BitmapFrame.Create(bitmapImage));
					enc.Save(outStream);
					System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

					return new System.Drawing.Bitmap(bitmap);
				}
			}
			
			catch (InvalidOperationException ex) {
				CommonLog.Instance.Log("Unable to create screenshot. Check if the camera is connected and has displayed at least one frame. Error message: {0}", ex.Message);
				return new System.Drawing.Bitmap(1, 1);
			}
		}

		private static void Watchdog_Elapsed(object sender, ElapsedEventArgs e) {
			// ensures the watchdog doesn't stop a pending reconnection
			if (watchdogSkip > 0) { watchdogSkip--; return; }

			DateTime now = DateTime.Now;

			for(int i = 0; i < feeds.Count; i++) {
				FeedInfo fi = feeds[i];
				
				double downTime = new TimeSpan(now.Ticks - fi.LastFrameTime.Ticks).TotalMilliseconds;

				if (downTime >= 2000) {
					fi.Decoder.StopStream();
					fi.Decoder.ParseStream(fi.URI);
					watchdogSkip = 3;
				}
			}
		}

		private static void Decoder_Error(object sender, ErrorEventArgs e) {
			int index = DecoderToIndex((MjpegDecoder)sender);
		}

		private static void Decoder_FrameReady(object sender, FrameReadyEventArgs e) {
			int index = DecoderToIndex((MjpegDecoder)sender);
			foreach(Image surface in feeds[index - 1].RenderSurfaces) { surface.Source = e.BitmapImage; }

			feeds[index - 1].LastFrame = e.BitmapImage;
			feeds[index - 1].LastFrameTime = DateTime.Now;
		}
	}

	public enum Camera
	{
		// Fixed and always present
		LeftGimbal = 1,
		RightGimbal,
		LeftSuspension,
		RightSuspension,

		// Arm (swappable with science)
		LeftEndEffector,
		RightEndEffector,
		Elbow,

		// Science (swappable with arm)
		Actuation = 5,
		SensorBox = 6,
		Carousel = 7,

		/// <summary>
		/// Placeholder camera for migrating off of hardcoded integer camera indexes
		/// </summary>
		PlaceholderCamera8,

        AutonomyRegular = 9,
        AutonomyDepth = 13

	}
}
