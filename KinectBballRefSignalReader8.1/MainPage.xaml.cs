using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;


namespace KinectBballRefSignalReader8._1
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private KinectSensor kinectSensor = null;
        private Body[] bodies = null;
        private int activeBodyIndex = 0;
        private BodyFrameReader bodyFrameReader = null;
        //private string statusText = null;
       // private KinectBodyView kinectBodyView = null;
        private GestureDetector gestureDetector = null;
        private GestureResultView gestureResultView = null;
        private FrameDescription currentFrameDescription;

        private string statusText = null;

        BodyFrameReader bodyReader;
        VisualGestureBuilderFrameSource gestureSource;
        VisualGestureBuilderFrameReader gestureReader;
        
        public static String RefSignalInput = "";
        public static String PlayerNumber = "";
        public static String FoulType = "";
        public static String DirectionOfPlay = "";

        //Gesture playerNum10;
        //Gesture rightDirOP;
        //Gesture leftDirOP;
        //Gesture holdingFoul;
        //Gesture blockScreenFoul;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            RefSignalInput = "Press the Kinect and setup buttons in the bottom left corner. Then press the record button in the bottom center to start detecting Referee Signals.";

            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // open the sensor
            this.kinectSensor.Open();

            this.InitializeComponent();

            // Initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // Create a gesture detector for each body (6 bodies => 6 detectors)
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView("", i, false, false, 0.0f);
                GestureDetector detector = new GestureDetector(this.kinectSensor, result);
                //result.PropertyChanged += GestureResult_PropertyChanged;
                this.gestureDetectorList.Add(detector);
            }

            //RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
            txtGestureOutput.Text = RefSignalInput;

        }//- End of MainPage()

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.FrameArrived -= this.UpdateKinectFrameData;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (kinectSensor != null && kinectSensor.IsOpen)
            {
                kinectSensor.Close();
            }
        }//- End of MainPage_Unloaded

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null)
            {
                kinectSensor.Open();

                if (kinectSensor.IsOpen)
                {
                    tblKinectStatus.Text = "Kinect successfully acquired and opened!";
                }//- End of inner if
            }//- End of outer if
        }//- End of MainPage_Loaded
        

        //void OnLoadGestureFromDb(object sender, RoutedEventArgs e)
        //{
        //    // we assume that this file exists and will load
        //    VisualGestureBuilderDatabase db = new VisualGestureBuilderDatabase(@"GestureDatabase.gbd");

        //    this.playerNum10 = db.AvailableGestures.Where(g => g.Name == "PlayerNum10").Single();
        //    this.rightDirOP = db.AvailableGestures.Where(g => g.Name == "DirectionOfPlay_Right").Single();
        //    this.leftDirOP = db.AvailableGestures.Where(g => g.Name == "DirectionOfPlay_Left").Single();
        //    this.holdingFoul = db.AvailableGestures.Where(g => g.Name == "HoldingFoul").Single();
        //    this.blockScreenFoul = db.AvailableGestures.Where(g => g.Name == "Blocking_ScreeningFoul").Single();
        //}
        //void OnOpenReaders(object sender, RoutedEventArgs e)
        //{
        //    this.OpenBodyReader();
        //    this.OpenGestureReader();
        //}
        //void OpenBodyReader()
        //{
        //    if (this.bodies == null)
        //    {
        //        this.bodies = new Body[this.kinectSensor.BodyFrameSource.BodyCount];
        //    }
        //    this.bodyReader = this.kinectSensor.BodyFrameSource.OpenReader();
        //    this.bodyReader.FrameArrived += OnBodyFrameArrived;
        //}
        //void OpenGestureReader()
        //{
        //    this.gestureSource = new VisualGestureBuilderFrameSource(this.kinectSensor, 0);

        //    this.gestureSource.AddGesture(this.playerNum10);
        //    this.gestureSource.AddGesture(this.rightDirOP);
        //    this.gestureSource.AddGesture(this.leftDirOP);
        //    this.gestureSource.AddGesture(this.holdingFoul);
        //    this.gestureSource.AddGesture(this.blockScreenFoul);

        //    this.gestureSource.TrackingIdLost += OnTrackingIdLost;

        //    this.gestureReader = this.gestureSource.OpenReader();
        //    this.gestureReader.IsPaused = true;
        //    this.gestureReader.FrameArrived += OnGestureFrameArrived;
        //}
        //void OnCloseReaders(object sender, RoutedEventArgs e)
        //{
        //    if (this.gestureReader != null)
        //    {
        //        this.gestureReader.FrameArrived -= this.OnGestureFrameArrived;
        //        this.gestureReader.Dispose();
        //        this.gestureReader = null;
        //    }
        //    if (this.gestureSource != null)
        //    {
        //        this.gestureSource.TrackingIdLost -= this.OnTrackingIdLost;
        //        this.gestureSource.Dispose();
        //    }
        //    this.bodyReader.Dispose();
        //    this.bodyReader = null;
        //}
        //void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e)
        //{
        //    this.gestureReader.IsPaused = true;
        //}
        //void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        //{
        //    using (var frame = e.FrameReference.AcquireFrame())
        //    {
        //        if (frame != null)
        //        {
        //            frame.GetAndRefreshBodyData(this.bodies);

        //            var trackedBody = this.bodies.Where(b => b.IsTracked).FirstOrDefault();

        //            if (trackedBody != null)
        //            {
        //                if (this.gestureReader.IsPaused)
        //                {
        //                    this.gestureSource.TrackingId = trackedBody.TrackingId;
        //                    this.gestureReader.IsPaused = false;
        //                }
        //            }
        //            else
        //            {
        //                this.OnTrackingIdLost(null, null);
        //            }
        //        }
        //    }
        //}
        //void OnGestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        //{
        //    using (var frame = e.FrameReference.AcquireFrame())
        //    {
        //        if (frame != null)
        //        {
        //            var continuousResults = frame.ContinuousGestureResults;

        //            if ((continuousResults != null) && (continuousResults.ContainsKey(this.playerNum10)))
        //            {
        //                var result = continuousResults[this.playerNum10];

        //                PlayerNumber = "Player #10";
                        
        //            }
        //            if ((continuousResults != null) && (continuousResults.ContainsKey(this.rightDirOP)))
        //            {
        //                var result = continuousResults[this.rightDirOP];

        //                DirectionOfPlay = "Right";

        //            }
        //            if ((continuousResults != null) && (continuousResults.ContainsKey(this.leftDirOP)))
        //            {
        //                var result = continuousResults[this.leftDirOP];

        //                DirectionOfPlay = "Left";

        //            }
        //            if ((continuousResults != null) && (continuousResults.ContainsKey(this.holdingFoul)))
        //            {
        //                var result = continuousResults[this.holdingFoul];

        //                FoulType = "Holding Foul";

        //            }
        //            if ((continuousResults != null) && (continuousResults.ContainsKey(this.blockScreenFoul)))
        //            {
        //                var result = continuousResults[this.blockScreenFoul];

        //                FoulType = "Blocking/Illegal Screening Foul";

        //            }

        //        }

        //        RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
        //    }
        //}

        private void btnKinectStart_Click(object sender, RoutedEventArgs e)
        {
            btnKinectStart.Visibility = Visibility.Collapsed;
            btnKinectStop.Visibility = Visibility.Visible;

            txtGestureOutput.Text = "";
            RefSignalInput = "";
            //PlayerNumber = GestureDetector.PlayerNumber;
            //FoulType = GestureDetector.FoulType;
            //DirectionOfPlay = GestureDetector.DirectionOfPlay;

            this.kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null)
            {
                kinectSensor.Open();
            }//- End of outer if

            if (this.bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived += this.UpdateKinectFrameData;
            }

            // Get the gesture reading/detection working here!!!!!!!!!!!!!!!!

            // Read gesture and generate text for txtGestureOutput in RefSignalInput
            //RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
            txtGestureOutput.Text = "Press 'Stop Recording' button and output will display.";
        }

        void GestureResult_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GestureResultView result = sender as GestureResultView;
            //this.GestureVisual.Opacity = result.Confidence;
            if (result.Confidence > 0.5)
            {
                // If the gesture confidence is greater than .65 then check the gesture name to output the correct string.
                if (result.GestureName.Equals("Player 10", StringComparison.OrdinalIgnoreCase))
                {
                    PlayerNumber = "Player 10";
                    RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
                    txtGestureOutput.Text = RefSignalInput;
                } else if (result.GestureName.Equals("Right", StringComparison.OrdinalIgnoreCase))
                {
                    DirectionOfPlay = "Right";
                    RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
                    txtGestureOutput.Text = RefSignalInput;
                }
                else if (result.GestureName.Equals("Left", StringComparison.OrdinalIgnoreCase))
                {
                    DirectionOfPlay = "Left";
                    RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
                    txtGestureOutput.Text = RefSignalInput;
                }
                else if (result.GestureName.Equals("Holding Foul", StringComparison.OrdinalIgnoreCase))
                {
                    FoulType = "Holding Foul";
                    RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
                    txtGestureOutput.Text = RefSignalInput;
                }
                else if (result.GestureName.Equals("Blocking/Illegal Screening Foul", StringComparison.OrdinalIgnoreCase))
                {
                    FoulType = "Blocking/Illegal Screening Foul";
                    RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
                    txtGestureOutput.Text = RefSignalInput;
                }//- End of inner if/else if
            }//- End of outer if
        }//- End of GestureResult_PropertyChanged

        private void btnKinectStop_Click(object sender, RoutedEventArgs e)
        {
            btnKinectStop.Visibility = Visibility.Collapsed;
            btnKinectStart.Visibility = Visibility.Visible;

            RefSignalInput = PlayerNumber + " committed " + FoulType + ". Play continues to the " + DirectionOfPlay;
            txtGestureOutput.Text = RefSignalInput;

            if (kinectSensor != null && kinectSensor.IsOpen)
            {
                kinectSensor.Close();
            }
        }//- End of btnKinectStop_Click

        private List<GestureDetector> gestureDetectorList = null;
        public bool isTakingScreenshot = false;
        
        private void kinectSensor_IsAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
        {
            this.StatusText = this.kinectSensor.IsAvailable ? "Running" : "Not Available";
        }//- End of kinectSensor_IsAvailableChanged

        private void RegisterGesture(BodyFrame bodyFrame)
        {
            bool dataReceived = false;
            Body[] bodies = null;

            if (bodyFrame != null)
            {
                if (bodies == null)
                {
                    // Creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                    bodies = new Body[bodyFrame.BodyCount];
                }//- End of inner if

                // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                // As long as those body objects are not disposed and not set to null in the array,
                // those body objects will be re-used.
                bodyFrame.GetAndRefreshBodyData(bodies);
                dataReceived = true;
            }//- End of outer if

            if (dataReceived)
            {
                // We may have lost/acquired bodies, so update the corresponding gesture detectors
                if (bodies != null)
                {
                    // Loop through all bodies to see if any of the gesture detectors need to be updated
                    for (int i = 0; i < bodyFrame.BodyCount; ++i)
                    {
                        Body body = bodies[i];
                        ulong trackingId = body.TrackingId;

                        // If the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // If the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // If the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }//- End of if
                    }//- End of for
                }//- End of inner if
            }//- End of outer if
        }//- End of RegisterGesture

        /// <summary>
        /// Gets the first body in the bodies array that is currently tracked by the Kinect sensor
        /// </summary>
        /// <returns>Index of first tracked body, or -1 if no body is tracked</returns>
        private int GetActiveBodyIndex()
        {
            int activeBodyIndex = -1;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                // find the first tracked body and verify it has hands tracking enabled (by default, Kinect will only track handstate for 2 people)
                if (this.bodies[i].IsTracked && (this.bodies[i].HandRightState != HandState.NotTracked || this.bodies[i].HandLeftState != HandState.NotTracked))
                {
                    activeBodyIndex = i;
                    break;
                }
            }

            return activeBodyIndex;
        }

        /// <summary>
        /// Retrieves the latest body frame data from the sensor and updates the associated gesture detector object
        /// </summary>
        /// /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void UpdateKinectFrameData(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;

                    //if (!this.bodies[this.activeBodyIndex].IsTracked)
                    //{
                    //    // we lost tracking of the active body, so update to the first tracked body in the array
                    //    int bodyIndex = this.GetActiveBodyIndex();

                    //    if (bodyIndex > 0)
                    //    {
                    //        this.activeBodyIndex = bodyIndex;
                    //    }
                    //}

                    //dataReceived = true;
                }
            }

            if (dataReceived)
            {
                Body activeBody = this.bodies[this.activeBodyIndex];

                // visualize the new body data
                //this.kinectBodyView.UpdateBodyData(activeBody);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }

                //// visualize the new gesture data
                //if (activeBody.TrackingId != this.gestureDetector.TrackingId)
                //{
                //    // if the tracking ID changed, update the detector with the new value
                //    this.gestureDetector.TrackingId = activeBody.TrackingId;
                //}

                //if (this.gestureDetector.TrackingId == 0)
                //{
                //    // the active body is not tracked, pause the detector and update the UI
                //    this.gestureDetector.IsPaused = true;
                //    //this.gestureResultView.UpdateGestureResult(false, false, false, false, -1.0f);
                //}
                //else
                //{
                //    // the active body is tracked, unpause the detector
                //    this.gestureDetector.IsPaused = false;

                //    // get the latest gesture frame from the sensor and updates the UI with the results
                //    //this.gestureDetector.UpdateGestureData();
                //}
            }
        }

        #region FRAME DESCRIPTION EVENTS
        public event PropertyChangedEventHandler PropertyChanged;

        
        public string StatusText
        {
            get {
                return this.statusText;
            }//- End of get
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }//- End of inner if
                }//- End of outer if
            }//- End of set
        }//- End of StatusText

        public FrameDescription CurrentFrameDescription
        {
            get { return this.currentFrameDescription; }//- End of get
            set
            {
                if (this.currentFrameDescription != value)
                {
                    this.currentFrameDescription = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("CurrentFrameDescription"));
                    }//- End of inner if
                }//- End of outer if
            }//- End of set
        }//- End of CurrentFrameDescription

        #endregion

        #region INFRARED SETUP

        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private WriteableBitmap bitmap = null;

        //Infrared Frame
        private InfraredFrameReader infraredFrameReader = null;
        private ushort[] infraredFrameData = null;
        private byte[] infraredPixels = null;

        /// <summary>
        /// The highest value that can be returned in the InfraredFrame.
        /// It is cast to a float for readability in the visualization code.
        /// </summary>
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /// </summary>
        /// Used to set the lower limit, post processing, of the
        /// infrared data that we will render.
        /// Increasing or decreasing this value sets a brightness
        /// "wall" either closer or further away.
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// The upper limit, post processing, of the
        /// infrared data that will render.
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        /// <summary>
        /// The InfraredSceneValueAverage value specifies the 
        /// average infrared value of the scene. 
        /// This value was selected by analyzing the average
        /// pixel intensity for a given scene.
        /// This could be calculated at runtime to handle different
        /// IR conditions of a scene (outside vs inside).
        /// </summary>
        private const float InfraredSceneValueAverage = 0.08f;

        /// <summary>
        /// The InfraredSceneStandardDeviations value specifies 
        /// the number of standard deviations to apply to
        /// InfraredSceneValueAverage.
        /// This value was selected by analyzing data from a given scene.
        /// This could be calculated at runtime to handle different
        /// IR conditions of a scene (outside vs inside).
        /// </summary>
        private const float InfraredSceneStandardDeviations = 3.0f;

        private void kinectSetup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // get the infraredFrameDescription from the InfraredFrameSource
            FrameDescription infraredFrameDescription =
                this.kinectSensor.InfraredFrameSource.FrameDescription;

            // open the reader for the infrared frames
            this.infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

            // wire handler for frame arrival
            this.infraredFrameReader.FrameArrived += InfraredFrameReader_FrameArrived;

            // allocate space to put the pixels being received and converted
            this.infraredFrameData =
                new ushort[infraredFrameDescription.Width * infraredFrameDescription.Height];
            this.infraredPixels =
                new byte[infraredFrameDescription.Width * infraredFrameDescription.Height * BytesPerPixel];

            // create the bitmap to display
            this.bitmap = new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height);

            // added for propertychanged
            this.CurrentFrameDescription = infraredFrameDescription;


            // set isAvailableChanged event
            this.kinectSensor.IsAvailableChanged += this._kinect_IsAvailableChanged;

            // use the window object as the view model
            this.DataContext = this;

            this.kinectSensor.Open();
        } // end of kinectsetup

        private void _kinect_IsAvailableChanged(KinectSensor sender, IsAvailableChangedEventArgs args)
        {
            this.StatusText = this.kinectSensor.IsAvailable ? "Running" : "Not Available";
        }

        private void InfraredFrameReader_FrameArrived(InfraredFrameReader sender, InfraredFrameArrivedEventArgs args)
        {
            bool infraredFrameProcessed = false;

            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = args.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    FrameDescription infraredFrameDescription =
                                                infraredFrame.FrameDescription;

                    // verify data and write the new infrared frame data
                    // to the display bitmap
                    if (((infraredFrameDescription.Width * infraredFrameDescription.Height)
                                                            == this.infraredFrameData.Length) &&
                        (infraredFrameDescription.Width == this.bitmap.PixelWidth) &&
                        (infraredFrameDescription.Height == this.bitmap.PixelHeight))
                    {
                        // Copy the pixel data from the image to a 
                        // temporary array
                        infraredFrame.CopyFrameDataToArray(this.infraredFrameData);
                        infraredFrameProcessed = true;
                    }
                }
            } // end using

            // we got a frame, convert and render
            if (infraredFrameProcessed)
            {
                ConvertInfraredDataToPixels();
                RenderPixelArray(this.infraredPixels);
            }
        }// end framearrived event

        // Reader_InfraredFrameArrived() before this...
        private void ConvertInfraredDataToPixels()
        {
            // Convert the infrared to RGB
            int colorPixelIndex = 0;
            for (int i = 0; i < this.infraredFrameData.Length; ++i)
            {
                // normalize the incoming infrared data (ushort) to a float ranging 
                // from InfraredOutputValueMinimum to InfraredOutputValueMaximum] by

                // 1. dividing the incoming value by the source maximum value
                float intensityRatio = (float)this.infraredFrameData[i] / InfraredSourceValueMaximum;

                // 2. dividing by the (average scene value * standard deviations)
                intensityRatio /= InfraredSceneValueAverage * InfraredSceneStandardDeviations;

                // 3. limiting the value to InfraredOutputValueMaximum
                intensityRatio = Math.Min(InfraredOutputValueMaximum, intensityRatio);

                // 4. limiting the lower value InfraredOutputValueMinimum
                intensityRatio = Math.Max(InfraredOutputValueMinimum, intensityRatio);

                // 5. converting the normalized value to a byte and using 
                // the result as the RGB components required by the image
                byte intensity = (byte)(intensityRatio * 255.0f);
                this.infraredPixels[colorPixelIndex++] = intensity; //Blue
                this.infraredPixels[colorPixelIndex++] = intensity; //Green
                this.infraredPixels[colorPixelIndex++] = intensity; //Red
                this.infraredPixels[colorPixelIndex++] = 255;       //Alpha           
            }
        }

        // ConvertInfraredDataToPixels() before this...
        private void RenderPixelArray(byte[] pixels)
        {
            pixels.CopyTo(this.bitmap.PixelBuffer);
            this.bitmap.Invalidate();
            FrameDisplayImage.Source = this.bitmap;
        }
        #endregion

        private void kinect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null)
            {
                tblKinectStatus.Text = "Kinect successfully acquired!";
            }
            else
            {
                tblKinectStatus.Text = "Cannot open sensor!";
            }

        }

    }//- End of MainPage
}//- End of KinectBballRefSignalReader8._1
