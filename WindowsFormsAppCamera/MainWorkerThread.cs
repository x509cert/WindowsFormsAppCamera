using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace WindowsFormsAppCamera
{
    public partial class Form1
    {
        // a new thread function that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
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
            SolidBrush[] colDronesIncomingFade = new SolidBrush[MaxIncomingFrames];
            const float ratio = 255 / (float)MaxIncomingFrames;
            for (int i=0; i < MaxIncomingFrames; i++)
                colDronesIncomingFade[MaxIncomingFrames - i - 1] = new SolidBrush(Color.FromArgb((int)(255 - (ratio * i)), 0, 0));

            // settings on the pen used to draw the hitbox
            _penHitBox.Width = 1;
            _penHitBox.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            WriteLog("Worker thread start");

            SetSkillTimer();
            
            // give each screen shot image a unique number
            uint traceCounter = 0;

            // memory-mapped IO for sending details to the camera app
            _mmio = new MMIo();
            _mmio?.Write(txtName.Text);

            while (!_fKillThreads)
            {
                Trace.TraceInformation("Main thread loop start");
                Trace.Indent();

                // using camera
                if (_fUsingLiveScreen)
                {
                    // TODO remove tracing - it's not used anymore
                    bool tracing = false;

                    // See if we need to dump traces
                    if (DateTime.Now - _startTraceTimer <= _maxTraceTime)
                    {
                        Trace.TraceInformation("Dump image");

                        tracing = true;
                        var img = pictCamera.Image;
                        var date = DateTime.Now;
                        var dtFormat = date.ToString("MMdd-HH-mm-ss");
                        img.Save(_sLogFilePath + @"\Trace\Div" + dtFormat + "-" + traceCounter.ToString("D6") + ".jpg",ImageFormat.Jpeg);
                        traceCounter++;
                    }
                    else
                    {
                        btnTrace.Enabled = true;
                    }

                    // need to check that if drones have not been spotted for a while then
                    // throw out the EMP and deploy the turret
                    // this is an emergency measure
                    // sends an SMS alert if one is configured
                    TimeSpan tSpan = DateTime.Now - dtLastDroneSpotted;
                    if (tSpan > _longestTimeBetweenDrones)
                    {
                        Trace.TraceInformation($"Drone last seen {tSpan.TotalSeconds}s");

                        WriteLog("Last drone seen: " + tSpan.TotalSeconds.ToString("N2") + "s ago");
                        TriggerArduino("E");
                        TriggerArduino("T");
                        WriteLog("Emergency EMP and Turret deployed");

                        // This is to stop an infite set of msgs
                        dtLastDroneSpotted = DateTime.Now; 

                        showNoDronesSeenText = 30;

                        // Send a SMS message
                        Trace.TraceInformation("Send Emergency SMS");
                        if (_smsAlert != null    && 
                            chkSmsAlerts.Checked && 
                            !_smsAlert.RaiseAlert($"Drones not detected on {_smsAlert.MachineName} [Time:{DateTime.Now}]"))
                            WriteLog("SMS alert failed");
                    }

                    string droneCooldown = "Drone check: Ready";

                    // this stops the code from checking for drones constantly right after drones are spotted and EMP sent out
                    if (fDronesIncoming)
                    {
                        TimeSpan elapsedTime = DateTime.Now - dtDronesStart;
                        if (elapsedTime > _elapseBetweenDrones)
                        {
                            WriteLog("Ready for next drone scan");
                            fDronesIncoming = false;
                        }

                        var elapsed = (Int32)(_elapseBetweenDrones.TotalSeconds - elapsedTime.TotalSeconds);
                        droneCooldown = $"Drone check: {elapsed:N0}s";
                    }

                    // get the image from the camera
                    Trace.TraceInformation("Getting camera image");
                    Trace.Indent();

                    var bmp = _camera.GetBitmap();
                    Graphics gd = Graphics.FromImage(bmp);

                    // define a rectangle for the text
                    gd.FillRectangle(Brushes.DarkBlue, 2, 480 - 94, 640 / 3, 480 - 2);
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

                    // draw the RGB charts
                    _chartR.Draw(_arrR, (byte)rbgDroneHitboxTotal.R, (byte)_cfg.LastCalibratedR); pictR.Image = _chartR.Bmp;
                    _chartG.Draw(_arrG, (byte)rbgDroneHitboxTotal.G, (byte)_cfg.LastCalibratedG); pictG.Image = _chartG.Bmp;
                    _chartB.Draw(_arrB, (byte)rbgDroneHitboxTotal.B, (byte)_cfg.LastCalibratedB); pictB.Image = _chartB.Bmp;

                    // if drones spotted and not on drone-check-cooldown then trigger the Arduino to hold EMP pulse
                    // start the countdown for displaying the "incoming" text
                    if (!fDronesIncoming && DronesSpotted(ref rbgDroneHitboxTotal))
                    {
                        Trace.TraceInformation("Drone Spotted");

                        dtLastDroneSpotted = DateTime.Now;

                        // send out the EMP
                        WriteLog("Drones detected -> EMP");
                        TriggerArduino("E");

                        dtDronesStart = DateTime.Now;
                        fDronesIncoming = true;
                        showDroneText = MaxIncomingFrames; // display the drone text for a small number of frames

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
                        var col = colDronesIncomingFade[showDroneText-1];
                        gd.DrawString("Drones Incoming!", new Font("Tahoma", 30), col, rect);

                        showDroneText--;
                    }

                    // display 'T' if tracing
                    if (tracing)
                    {
                        const int boxsize = 30;
                        Rectangle rect = new Rectangle(640 - boxsize, bmp.Height - boxsize, boxsize, boxsize);
                        gd.DrawString("T", new Font("Tahoma", 14), Brushes.WhiteSmoke, rect);
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
