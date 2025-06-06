// <copyright file="Window.cs" company="Selenium Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Defines the interface through which the user can manipulate the browser window.
    /// </summary>
    internal sealed class Window : IWindow
    {
        private WebDriver driver;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="driver">Instance of the driver currently in use</param>
        public Window(WebDriver driver)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        /// <summary>
        /// Gets or sets the position of the browser window relative to the upper-left corner of the screen.
        /// </summary>
        /// <remarks>When setting this property, it should act as the JavaScript window.moveTo() method.</remarks>
        public Point Position
        {
            get
            {
                Response commandResponse = this.driver.Execute(DriverCommand.GetWindowRect, null);

                Dictionary<string, object?> rawPosition = (Dictionary<string, object?>)commandResponse.Value!;
                int x = Convert.ToInt32(rawPosition["x"], CultureInfo.InvariantCulture);
                int y = Convert.ToInt32(rawPosition["y"], CultureInfo.InvariantCulture);

                return new Point(x, y);
            }

            set
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("x", value.X);
                parameters.Add("y", value.Y);
                this.driver.Execute(DriverCommand.SetWindowRect, parameters);
            }
        }

        /// <summary>
        /// Gets or sets the size of the outer browser window, including title bars and window borders.
        /// </summary>
        /// <remarks>When setting this property, it should act as the JavaScript window.resizeTo() method.</remarks>
        public Size Size
        {
            get
            {
                Response commandResponse = this.driver.Execute(DriverCommand.GetWindowRect, null);

                Dictionary<string, object?> rawPosition = (Dictionary<string, object?>)commandResponse.Value!;
                int height = Convert.ToInt32(rawPosition["height"], CultureInfo.InvariantCulture);
                int width = Convert.ToInt32(rawPosition["width"], CultureInfo.InvariantCulture);

                return new Size(width, height);
            }

            set
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("width", value.Width);
                parameters.Add("height", value.Height);
                this.driver.Execute(DriverCommand.SetWindowRect, parameters);
            }
        }

        /// <summary>
        /// Maximizes the current window if it is not already maximized.
        /// </summary>
        public void Maximize()
        {
            this.driver.Execute(DriverCommand.MaximizeWindow, null);
        }

        /// <summary>
        /// Minimizes the current window if it is not already minimized.
        /// </summary>
        public void Minimize()
        {
            this.driver.Execute(DriverCommand.MinimizeWindow, null);
        }

        /// <summary>
        /// Sets the current window to full screen if it is not already in that state.
        /// </summary>
        public void FullScreen()
        {
            this.driver.Execute(DriverCommand.FullScreenWindow, null);
        }
    }
}
