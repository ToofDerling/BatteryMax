using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Windows.Graphics.Display;

/*
 * ==============================================================
 * @ID       $Id: MainForm.cs 971 2010-09-30 16:09:30Z ww $
 * @created  2008-07-31
 * ==============================================================
 *
 * The official license for this file is shown next.
 * Unofficially, consider this e-postcardware as well:
 * if you find this module useful, let us know via e-mail, along with
 * where in the world you are and (if applicable) your website address.
 */

/* ***** BEGIN LICENSE BLOCK *****
 * Version: MIT License
 *
 * Copyright (c) 2010 Michael Sorens http://www.simple-talk.com/author/michael-sorens/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * ***** END LICENSE BLOCK *****
 */

namespace BatteryMax
{
    /// <summary>
    /// Framework for running application as a tray app.
    /// </summary>
    /// <remarks>
    /// Tray app code adapted from "Creating Applications with NotifyIcon in Windows Forms", Jessica Fosler,
    /// http://windowsclient.net/articles/notifyiconapplications.aspx
    /// </remarks>
    public class CustomApplicationContext : ApplicationContext
    {
        private BatteryIconManager batteryIconManager;

        private DetailsForm detailsForm;

        private void ShowDetailsForm()
        {
            if (detailsForm == null)
            {
                detailsForm = new DetailsForm { BatteryIconManager = batteryIconManager };
                detailsForm.Closed += (s, e) => detailsForm = null; // Null out the form so we know to create a new one.
                detailsForm.Show();
            }
            else
            {
                detailsForm.Activate();
            }
        }

        private IContainer components;	// A list of components to dispose when the context is disposed
        private NotifyIcon notifyIcon;	// The icon that sits in the system tray

        public async Task InitializeContextAsync(BatteryData testBatteryData = null)
        {
            components = new Container();
            notifyIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
            };

            notifyIcon.DoubleClick += (s, e) => ShowDetailsForm();

            var detailsItem = new ToolStripMenuItem
            {
                Text = "Show &Details"
            };
            detailsItem.Click += (s, e) => ShowDetailsForm();
            notifyIcon.ContextMenuStrip.Items.Add(detailsItem);

            var restartItem = new ToolStripMenuItem
            {
                Text = "R&estart"
            };
            restartItem.Click += (s, e) => Restart();
            notifyIcon.ContextMenuStrip.Items.Add(restartItem);

            var exitItem = new ToolStripMenuItem
            {
                Text = "E&xit"
            };
            exitItem.Click += (s, e) => ExitThread();
            notifyIcon.ContextMenuStrip.Items.Add(exitItem);

            batteryIconManager = new BatteryIconManager();
            await batteryIconManager.InitializeDataAsync(testBatteryData);

            // Handle initial update here. Setting notifyicon visible from BatteryManager thread causes the contextmenu to hang.
            UpdateIcon();
            notifyIcon.Visible = true;
            // ShowBalloonTip only works when notifyicon is visble so any initial message (like battery not found) must run here.
            ShowBalloon();

            screenBounds = Screen.PrimaryScreen.Bounds;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            batteryIconManager.BatteryChanged += (s, e) => UpdateIcon();
            batteryIconManager.Start();
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            // Neither SystemInformation.SmallIconSize (or DisplayProperties.ResolutionScale) changes when
            // user changes the ui scaling. But screen bounds does. So the easiest way to handle it is to
            // listen to DisplaySettingsChanged and restart the app if changed.
            if (screenBounds != Screen.PrimaryScreen.Bounds)
            {
                Restart();
            }
        }

        private Rectangle screenBounds;


        private void Restart()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            ExitThread();

            var dll = Assembly.GetExecutingAssembly().Location;
            var exe = Path.ChangeExtension(dll, ".exe");

            var process = new Process
            {
                StartInfo =
                {
                    FileName = exe,
                    UseShellExecute = true,
                }
            };
            process.Start();
        }

        private void ShowBalloon()
        {
            if (notifyIcon.Visible && batteryIconManager.WarningText != null)
            {
                notifyIcon.ShowBalloonTip(10000, null, batteryIconManager.WarningText, ToolTipIcon.None);
            }
        }

        private Icon currentIcon = null;

        private void UpdateIcon()
        {
            if (batteryIconManager.UpdateText != null)
            {
                notifyIcon.Text = batteryIconManager.UpdateText;
            }

            if (batteryIconManager.UpdateIcon != null)
            {
                notifyIcon.Icon = batteryIconManager.UpdateIcon;

                if (currentIcon != null)
                {
                    DestroyIcon(currentIcon.Handle);
                }
                else
                {
                    notifyIcon.Visible = true;
                }

                currentIcon = batteryIconManager.UpdateIcon;
            }

            ShowBalloon();
        }

        // BatteryIconManager uses Icon.FromHandle to create icons. MSDN: "When using this method, you must dispose of the original icon 
        // by using the DestroyIcon method in the Windows API to ensure that the resources are released."
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        /// <summary>
		/// When the application context is disposed, dispose things like the notify icon.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                if (batteryIconManager != null)
                {
                    batteryIconManager.Stop();
                }
                if (currentIcon != null)
                {
                    DestroyIcon(currentIcon.Handle);
                }
                components.Dispose();
            }
        }

        /// <summary>
        /// If we are presently showing a form, clean it up.
        /// </summary>
        protected override void ExitThreadCore()
        {
            // before we exit, let forms clean themselves up.
            if (detailsForm != null)
            {
                detailsForm.Close();
            }

            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }
    }
}
