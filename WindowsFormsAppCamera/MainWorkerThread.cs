using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;

namespace WindowsFormsAppCamera
{
    public class AutoAdjustMean
    {
        private List<long> _valueR = new List<long>();
        private const int _maxCount = 100;

        public int AddAndCheckMean(long val)
        {
            double ret = 0;

            _valueR.Add(val);
            if (_valueR.Count > _maxCount)
            {
                _valueR.Sort();
                ret = CalculateFilteredMean();
                _valueR.Clear();
            }

            return (int)ret;
        }
        private static double Quantile(List<long> sortedData, double percentile)
        {
            double position = (sortedData.Count + 1) * percentile;
            double leftNumber, rightNumber;

            double fraction = position - Math.Floor(position);

            if (position >= 1)
            {
                leftNumber = sortedData[(int)Math.Floor(position) - 1];
                rightNumber = sortedData[(int)Math.Floor(position)];
            }
            else
            {
                return sortedData[0];
            }

            if (leftNumber != rightNumber)
            {
                return leftNumber + (rightNumber - leftNumber) * fraction;
            }
            else
            {
                return leftNumber;
            }
        }
        private double CalculateFilteredMean()
        {
            // Assuming data is already sorted
            double q1 = Quantile(_valueR, 0.25);
            double q3 = Quantile(_valueR, 0.75);

            // Calculate IQR
            double iqr = q3 - q1;

            // Define outlier thresholds
            double lowerBound = q1 - 1.5 * iqr;
            double upperBound = q3 + 1.5 * iqr;

            // Filter out outliers
            var filteredData = _valueR.Where(x => x >= lowerBound && x <= upperBound).ToList();

            // Calculate the mean of the filtered data
            return filteredData.Count != 0 ? filteredData.Average() : 0;
        }
    }

    public partial class Form1
    {
        // a new thread function that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
            _udpBroadcast = new UdpBroadcast(_udpBroadcastPort);

            Trace.TraceInformation("Main WorkerThreadStart");
            Trace.Indent();

            Thread.Sleep(_threadStartDelay);

            var dtDronesStart = DateTime.Now;
            var dtLastDroneSpotted = DateTime.Now;

            bool fDronesIncoming = false;
            int showDroneText = 0;
            int showNoDronesSeenText = 0;

            // text that goes into the image and the rectangles in which they reside
            Font imageFont = new Font("Tahoma", 14);

            // sets the darkening red for "Drones Incoming"
            var colDronesIncomingFade = new SolidBrush[_maxIncomingDronesFrames];
            const float ratio = 255 / (float)_maxIncomingDronesFrames;
            for (int i=0; i < _maxIncomingDronesFrames; i++)
                colDronesIncomingFade[_maxIncomingDronesFrames - i - 1] = new SolidBrush(Color.FromArgb((int)(255 - (ratio * i)), 0, 0));

            // settings on the pen used to draw the hitbox
            _penHitBox.Width = 1;
            _penHitBox.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            WriteLog("Worker thread start");

            SetSkillTimer();

            // memory-mapped IO for sending details to the camera app
            _mmio = new MMIo();
            _mmio?.Write(txtName.Text);

            AutoAdjustMean adjustMean = new AutoAdjustMean();

            while (!_fKillThreads)
            {
                Trace.TraceInformation("Main thread loop start");
                Trace.Indent();

                // using camera
                if (_fUsingLiveScreen)
                {
                    // need to check that if drones have not been spotted for a while then
                    // throw out the EMP and deploy the turret
                    // this is an emergency measure
                    // sends an SMS alert if one is configured
                    TimeSpan tSpan = DateTime.Now - dtLastDroneSpotted;
                    if (tSpan > _longestTimeBetweenDrones)
                    {
                        _udpBroadcast?.SendMessage("Drones not seen");
                        _dronesNotSeenCount++;

                        Trace.TraceInformation($"Drone last seen over {tSpan.TotalSeconds}s ago, not seen count is {_dronesNotSeenCount}");
                        WriteLog($"Last drone seen over {tSpan.TotalSeconds:N2}s ago, not seen count is {_dronesNotSeenCount}");
 
                        if (_dronesNotSeenCount <= _dronesNotSeenCountThreshold)
                        {
                            TriggerArduino("E");
                            TriggerArduino("T");
                            WriteLog("Emergency EMP and Turret deployed");
                            _udpBroadcast?.SendMessage("Emergency EMP/Turret");
                        }

                        // shutdown threashold is hit, but Arduino not stopped yet
                        if (_dronesNotSeenCount > _dronesNotSeenCountThreshold && _fStopArduino == false)
                        {
                            Trace.TraceInformation("Drones not seen for a while, stopping Arduino");
                            WriteLog("Drones not seen for a while, stopping Arduino");
                            _udpBroadcast?.SendMessage("Drones not seen. Halting.");

                            _fStopArduino = true;

                            if (chkSmsAlerts.Checked &&                            
                                _smsAlert != null && 
                                !_smsAlert.RaiseAlert($"Shutting down Arduino, {_smsAlert.MachineName} [Time:{DateTime.Now}]"))
                                    WriteLog("SMS alert failed");

                            // send an instruction to the Arduino so it does not go into fail safe mode
                            TriggerArduino("H");
                        }

                        // This is to stop an infite set of msgs
                        dtLastDroneSpotted = DateTime.Now; 
                        showNoDronesSeenText = 30;

                        // Send a SMS message
                        Trace.TraceInformation("Send Emergency SMS");
                        if (_fStopArduino == false &&
                            _smsAlert != null      && 
                            chkSmsAlerts.Checked   && 
                            !_smsAlert.RaiseAlert($"Drones not detected {_dronesNotSeenCount} on {_smsAlert.MachineName} {DateTime.Now}"))
                                WriteLog("SMS alert failed");
                    }

                    string droneCooldown = "Drone check: Ready";
                    bool coolDownComplete = true;

                    // this stops the code from checking for drones constantly right after drones are spotted and EMP sent out
                    if (fDronesIncoming)
                    {
                        TimeSpan elapsedTime = DateTime.Now - dtDronesStart;
                        if (elapsedTime > _elapseBetweenDrones)
                        {
                            WriteLog("Ready for next drone scan");
                            _udpBroadcast?.SendMessage("Ready");

                            fDronesIncoming = false;
                        }

                        int elapsed = (int)(_elapseBetweenDrones.TotalSeconds - elapsedTime.TotalSeconds);
                        coolDownComplete = elapsed == 0;
                        droneCooldown = $"Drone check: {elapsed:N0}s";
                    }

                    // get the image from the camera
                    Trace.TraceInformation("Getting camera image");
                    Trace.Indent();

                    Bitmap bmp = _camera.GetBitmap();
                    Graphics gd = Graphics.FromImage(bmp);

                    // define a rectangle for the text
                    // yes, it's a series of magic numbers, but it works
                    gd.FillRectangle(
                        coolDownComplete ? Brushes.DarkGreen : Brushes.DarkBlue, 
                        2, 
                        480 - 94, 
                        640 / 3, 
                        480 - 2);
                    
                    gd.SmoothingMode = SmoothingMode.HighSpeed;

                    // Get amount of red/green/blue in the drone hitbox
                    RgbTotal rbgDroneHitboxTotal = new RgbTotal();
                    Color mainColor = Color.Transparent;
                    GetRgbInRange(bmp, 
                        XDroneHitBoxStart, 
                        YDroneHitBoxStart, 
                        WidthDroneHitBox, 
                        HeightDroneHitBox, 
                        ref rbgDroneHitboxTotal,
                        ref mainColor);

                    Trace.TraceInformation("Write info to bitmap");

                    // text offset in the main drawing rectangle
                    const int xOffset = 4;

                    // calculate current RGB as discrete values and percentages and write into the bmp
                    int percentChange = (int)(rbgDroneHitboxTotal.R / (float)_cfg.LastCalibratedR * 100);
                    string r = $"R: {rbgDroneHitboxTotal.R:N0} ({percentChange}%)";
                    gd.DrawString(r, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 70, bmp.Width, 24));

                    percentChange = (int)(rbgDroneHitboxTotal.G / (float)_cfg.LastCalibratedG * 100);
                    string g = $"G: {rbgDroneHitboxTotal.G:N0} ({percentChange}%)";
                    gd.DrawString(g, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 48, bmp.Width, 24));

                    percentChange = (int)(rbgDroneHitboxTotal.B / (float)_cfg.LastCalibratedB * 100);
                    string b = $"B: {rbgDroneHitboxTotal.B:N0} ({percentChange}%)";
                    gd.DrawString(b, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 24, bmp.Width, 24));

                    // Write elapsed time to next drone check
                    gd.DrawString(droneCooldown, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 94, bmp.Width, 24));

                    // Draw the RGB charts
                    _chartR.Draw(_arrR, (byte)rbgDroneHitboxTotal.R, (byte)_cfg.LastCalibratedR); pictR.Image = _chartR.Bmp;
                    _chartG.Draw(_arrG, (byte)rbgDroneHitboxTotal.G, (byte)_cfg.LastCalibratedG); pictG.Image = _chartG.Bmp;
                    _chartB.Draw(_arrB, (byte)rbgDroneHitboxTotal.B, (byte)_cfg.LastCalibratedB); pictB.Image = _chartB.Bmp;

                    // Add a recalibration check here
                    if ((DateTime.Now.Second & 3) == 0)
                    {
                        int newMean = adjustMean.AddAndCheckMean(rbgDroneHitboxTotal.R);

                        var s = $"Changing calibration mean from {_cfg.LastCalibratedR} to {newMean}";
                        Trace.TraceInformation(s);
                        _udpBroadcast?.SendMessage(s);

                        _cfg.LastCalibratedR = newMean; 
                    }

                    // If drones spotted and not on drone-check-cooldown then trigger the Arduino to hold EMP pulse
                    // Also, start the countdown for displaying the "incoming" text
                    if (!fDronesIncoming && DronesSpotted(ref rbgDroneHitboxTotal))
                    {
                        Trace.TraceInformation("Drone Spotted");
                        _udpBroadcast?.SendMessage("Drones Spotted");

                        if (_fStopArduino == true)
                        {
                            Trace.TraceInformation("Restarting the Arduino");
                            if (_smsAlert != null &&
                                chkSmsAlerts.Checked &&
                                !_smsAlert.RaiseAlert($"Restarting on {_smsAlert.MachineName} [Time:{DateTime.Now}]"))
                                WriteLog("SMS alert failed");
                        }

                        _dronesNotSeenCount = 0;
                        _fStopArduino = false;

                        // used to slow the pulse down if there is more than one pulse
                        if (_fDelayEMP == true)
                        {
                            Trace.TraceInformation("EMP Delayed");
                            Thread.Sleep(333);
                        }

                        dtLastDroneSpotted = DateTime.Now;

                        // send out the EMP
                        WriteLog("Drones detected -> EMP");
                        _udpBroadcast?.SendMessage("EMP sent");
                        TriggerArduino("E");

                        dtDronesStart = DateTime.Now;
                        fDronesIncoming = true;
                        showDroneText = _maxIncomingDronesFrames; // display the drone text for a small number of frames

                        // we have seen a drone, so kill the SMS cooldown
                        _smsAlert?.ResetCooldown();
                    }
                    
                    // Display a '!' which shows there's been no drones spotted
                    if (showNoDronesSeenText > 0)
                    {
                        Rectangle rectDronesNotSeen = new Rectangle(310, 10, 50, 220);
                        gd.DrawString("!", new Font("Tahoma", 120, FontStyle.Bold), Brushes.Firebrick, rectDronesNotSeen);

                        showNoDronesSeenText--;
                    }

                    // display the "Incoming text" - this is written to the image
                    if (showDroneText > 0)
                    {
                        Trace.TraceInformation("Drone text");
                        Rectangle rect = new Rectangle(180, 20 + (showDroneText * 4), bmp.Width, 100);
                        SolidBrush col = colDronesIncomingFade[showDroneText-1];
                        gd.DrawString("Drones Incoming!", new Font("Tahoma", 30), col, rect);

                        showDroneText--;
                    }

                    // displays a small heart at the bottom right of the screen when the heartbeat is sent
                    if (_heartBeatSent > 0)
                    {
                        Trace.TraceInformation("Heartbeat");
                        const int boxsize = 36;
                        Rectangle rect = new Rectangle(640 - boxsize, bmp.Height - boxsize, boxsize, boxsize);
                        gd.DrawString("Y", new Font("Webdings", 24), Brushes.Red, rect);

                        _heartBeatSent--;
                    }

                    // write the camera image + text etc to the UI
                    DrawTargetRange(bmp);
                    pictCamera.Image = bmp;
                }
                else
                {
                    // when running in no camera mode (ie; timer), uses a black screen
                    Bitmap bmp = new Bitmap(640, 480);
                    Graphics gr = Graphics.FromImage(bmp);
                    gr.FillRectangle(Brushes.Black, 0, 0, 640, 480);
                    pictCamera.Image = bmp;
                }

                Trace.Unindent();
                Trace.TraceInformation("Graphics loop completed");

                Trace.Unindent();
                Trace.TraceInformation("Main thread loop end");

                Thread.Sleep(_loopDelay);
            }

            Trace.Unindent();

            KillSkillTimer();

            _mmio?.Close();
        }
    }
}
