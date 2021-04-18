﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsAppCamera
{
    public partial class Form1 : Form
    {
        // a new thread function that does the core work, 
        // this is so we don't use the UI thread for the work which would make the UI sluggish
        private void WorkerThreadFunc()
        {
            var dtDronesStart = DateTime.Now;
            var dtLastDroneSpotted = DateTime.Now;

            bool fDronesIncoming = false;
            int showDroneText = 0;
            int showNoDronesSeenText = 0;

            // text that goes into the image and the rectangles inwhich they reside
            Font imageFont = new Font("Tahoma", 14);

            WriteLog("Worker thread start");

            SetSkillTimer();

            uint traceCounter = 0;

            while (!_fKillThreads)
            {
                // using camera
                if (_fUsingLiveScreen)
                {
                    bool tracing = false;

                    // See if we need to dump traces
                    if (DateTime.Now - _startTraceTimer <= _maxTraceTime)
                    {
                        tracing = true;
                        var img = pictCamera.Image;
                        var date = DateTime.Now;
                        var dtFormat = date.ToString("MMdd-HH-mm-ss");
                        img.Save(_sLogFilePath + @"\Trace\Div" + dtFormat + "-" + traceCounter.ToString("D6") + ".bmp");
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
                        WriteLog("Last drone seen: " + tSpan.TotalSeconds.ToString("N2") + "s ago");
                        TriggerArduino("E");
                        TriggerArduino("T");

                        dtLastDroneSpotted = DateTime.Now; // HACK! This is to stop an infite set of msgs

                        showNoDronesSeenText = 30;

                        // Send a SMS message
                        if (_smsAlert != null)
                            if (!_smsAlert.RaiseAlert($"Drones not detected on {_smsAlert.MachineName}"))
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

                        Int32 elapsed = (Int32)(_elapseBetweenDrones.TotalSeconds - elapsedTime.TotalSeconds);
                        droneCooldown = "Drone check: " + elapsed.ToString("N0") + "s";
                    }

                    // get the image from the camera
                    var bmp = _camera.GetBitmap();
                    Graphics gd = Graphics.FromImage(bmp);

                    // define a rectangle for the text
                    gd.FillRectangle(Brushes.DarkBlue, 2, 480 - 120, 640 / 3, 480 - 2);
                    gd.SmoothingMode = SmoothingMode.HighSpeed;

                    // Get amount of red/green/blue in the drone hitbox
                    RGBTotal rbgDroneHitboxTotal = new RGBTotal();
                    GetRGBInRange(bmp, _xDroneHitBoxStart, _yDroneHitBoxStart, _widthDroneHitBox, _heightDroneHitBox, ref rbgDroneHitboxTotal);

                    // convert RGB to HSB on the average
                    float h=0, s=0, l=0;
                    RgbToHsb.RGBtoHSB(
                        (int)rbgDroneHitboxTotal.R, 
                        (int)rbgDroneHitboxTotal.G, 
                        (int)rbgDroneHitboxTotal.B, 
                        ref h, ref s, ref l);

                    RgbToHsb.Color hitboxColor = RgbToHsb.GetColorFromRgbHsb(
                        (int)rbgDroneHitboxTotal.R,
                        (int)rbgDroneHitboxTotal.G,
                        (int)rbgDroneHitboxTotal.B,
                        h, s, l);

                    const int xOffset = 4;

                    // write predominant color
                    string c = $"Color: {hitboxColor}";
                    gd.DrawString(c, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 94, bmp.Width, 24));

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
                    gd.DrawString(droneCooldown, imageFont, _colorInfo, new Rectangle(xOffset, bmp.Height - 118, bmp.Width, 24));

                    // if drones spotted and not on drone-check-cooldown then trigger the Arduino to hold EMP pulse
                    // start the countdown for displaying the "incoming" text
                    if (!fDronesIncoming && DronesSpotted(ref rbgDroneHitboxTotal))
                    {
                        dtLastDroneSpotted = DateTime.Now;

                        // send out the EMP
                        WriteLog("Drones detected -> EMP");
                        TriggerArduino("E");

                        dtDronesStart = DateTime.Now;
                        fDronesIncoming = true;
                        showDroneText = _maxIncomingFrames; // display the drone text for a small number of frames

                        // we have seen a drone, so kill the SMS cooldown
                        _smsAlert?.ResetCooldown();
                    }

                    // if this is max frames less 2 of the 'drones detected' screen capture (only one frame so the log info is entered once)
                    // and the hit region is now blue - this means we have seen the EMP pulse
                    if (showDroneText == _maxIncomingFrames - 2 && hitboxColor == RgbToHsb.Color.Blue) 
                    {
                        WriteLog("EMP Pulse detected");
                    }

                    // Display a '!' which shows there's been no drones spotted
                    if (showNoDronesSeenText > 0)
                    {
                        Rectangle rectDronesNotSeen = new Rectangle(310, 10, 50, 220);
                        gd.DrawString("!", new Font("Courier", 120, FontStyle.Bold), Brushes.Firebrick, rectDronesNotSeen);

                        showNoDronesSeenText--;
                    }

                    // display the "Incoming text" - this is written to the image
                    if (showDroneText > 0)
                    {
                        Rectangle rect = new Rectangle(180, bmp.Height - 100, bmp.Width, 100);
                        gd.DrawString("Drones Incoming", new Font("Tahoma", 30), Brushes.Firebrick, rect);

                        showDroneText--;
                    }

                    // dislpay Tracing 'T' if tracing
                    if (tracing == true)
                    {
                        const int boxsize = 30;
                        Rectangle rect = new Rectangle(640 - boxsize, bmp.Height - boxsize, boxsize, boxsize);
                        gd.DrawString("T", new Font("Tahoma", 14), Brushes.WhiteSmoke, rect);
                    }

                    // displays a small heart at the bottom right of the screen when the heartbeat is sent
                    if (_heartBeatSent > 0)
                    {
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

                Thread.Sleep(210);
            }

            KillSkillTimer();
        }
    }
}
