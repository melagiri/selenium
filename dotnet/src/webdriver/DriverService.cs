// <copyright file="DriverService.cs" company="Selenium Committers">
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

using OpenQA.Selenium.Remote;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Exposes the service provided by a native WebDriver server executable.
    /// </summary>
    public abstract class DriverService : ICommandServer
    {
        private bool isDisposed;
        private Process? driverServiceProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverService"/> class.
        /// </summary>
        /// <param name="servicePath">The full path to the directory containing the executable providing the service to drive the browser.</param>
        /// <param name="port">The port on which the driver executable should listen.</param>
        /// <param name="driverServiceExecutableName">The file name of the driver service executable.</param>
        /// <exception cref="ArgumentException">
        /// If the path specified is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="DriverServiceNotFoundException">
        /// If the specified driver service executable does not exist in the specified directory.
        /// </exception>
        protected DriverService(string? servicePath, int port, string? driverServiceExecutableName)
        {
            this.DriverServicePath = servicePath;
            this.DriverServiceExecutableName = driverServiceExecutableName;
            this.Port = port;
        }

        /// <summary>
        /// Occurs when the driver process is starting.
        /// </summary>
        public event EventHandler<DriverProcessStartingEventArgs>? DriverProcessStarting;

        /// <summary>
        /// Occurs when the driver process has completely started.
        /// </summary>
        public event EventHandler<DriverProcessStartedEventArgs>? DriverProcessStarted;

        /// <summary>
        /// Gets the Uri of the service.
        /// </summary>
        public Uri ServiceUrl
        {
            get
            {
                string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}", this.HostName, this.Port);
                return new Uri(url);
            }
        }

        /// <summary>
        /// Gets or sets the host name of the service. Defaults to "localhost."
        /// </summary>
        /// <remarks>
        /// Most driver service executables do not allow connections from remote
        /// (non-local) machines. This property can be used as a workaround so
        /// that an IP address (like "127.0.0.1" or "::1") can be used instead.
        /// </remarks>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the service.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the initial diagnostic information is suppressed
        /// when starting the driver server executable. Defaults to <see langword="false"/>, meaning
        /// diagnostic information should be shown by the driver server executable.
        /// </summary>
        public bool SuppressInitialDiagnosticInformation { get; set; }

        /// <summary>
        /// Gets a value indicating whether the service is running.
        /// </summary>
        [MemberNotNullWhen(true, nameof(driverServiceProcess))]
        public bool IsRunning => this.driverServiceProcess != null && !this.driverServiceProcess.HasExited;

        /// <summary>
        /// Gets or sets a value indicating whether the command prompt window of the service should be hidden.
        /// </summary>
        public bool HideCommandPromptWindow { get; set; }

        /// <summary>
        /// Gets the process ID of the running driver service executable. Returns 0 if the process is not running.
        /// </summary>
        public int ProcessId
        {
            get
            {
                if (this.IsRunning)
                {
                    // There's a slight chance that the Process object is running,
                    // but does not have an ID set. This should be rare, but we
                    // definitely don't want to throw an exception.
                    try
                    {
                        return this.driverServiceProcess.Id;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the time to wait for an initial connection before timing out.
        /// </summary>
        public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromSeconds(20);

        /// <summary>
        /// Gets or sets the executable file name of the driver service.
        /// </summary>
        public string? DriverServiceExecutableName { get; set; }

        /// <summary>
        /// Gets or sets the path of the driver service.
        /// </summary>
        public string? DriverServicePath { get; set; }

        /// <summary>
        /// Gets the command-line arguments for the driver service.
        /// </summary>
        protected virtual string CommandLineArguments => string.Format(CultureInfo.InvariantCulture, "--port={0}", this.Port);

        /// <summary>
        /// Gets a value indicating the time to wait for the service to terminate before forcing it to terminate.
        /// </summary>
        protected virtual TimeSpan TerminationTimeout => TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets a value indicating whether the service has a shutdown API that can be called to terminate
        /// it gracefully before forcing a termination.
        /// </summary>
        protected virtual bool HasShutdown => true;

        /// <summary>
        /// Gets a value indicating whether the service is responding to HTTP requests.
        /// </summary>
        protected virtual bool IsInitialized
        {
            get
            {
                bool isInitialized = false;

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.ConnectionClose = true;
                        httpClient.Timeout = TimeSpan.FromSeconds(5);

                        Uri serviceHealthUri = new Uri(this.ServiceUrl, new Uri(DriverCommand.Status, UriKind.Relative));
                        using (var response = Task.Run(async () => await httpClient.GetAsync(serviceHealthUri)).GetAwaiter().GetResult())
                        {
                            // Checking the response from the 'status' end point. Note that we are simply checking
                            // that the HTTP status returned is a 200 status, and that the resposne has the correct
                            // Content-Type header. A more sophisticated check would parse the JSON response and
                            // validate its values. At the moment we do not do this more sophisticated check.
                            isInitialized = response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType is { MediaType: string mediaType } && mediaType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    // Do nothing. The exception is expected, meaning driver service is not initialized.
                }

                return isInitialized;
            }
        }

        /// <summary>
        /// Releases all resources associated with this <see cref="DriverService"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the DriverService if it is not already running.
        /// </summary>
        [MemberNotNull(nameof(driverServiceProcess))]
        public void Start()
        {
            if (this.driverServiceProcess != null)
            {
                return;
            }

            this.driverServiceProcess = new Process();

            if (this.DriverServicePath != null)
            {
                if (this.DriverServiceExecutableName is null)
                {
                    throw new InvalidOperationException("If the driver service path is specified, the driver service executable name must be as well");
                }

                this.driverServiceProcess.StartInfo.FileName = Path.Combine(this.DriverServicePath, this.DriverServiceExecutableName);
            }
            else
            {
                this.driverServiceProcess.StartInfo.FileName = new DriverFinder(this.GetDefaultDriverOptions()).GetDriverPath();
            }

            this.driverServiceProcess.StartInfo.Arguments = this.CommandLineArguments;
            this.driverServiceProcess.StartInfo.UseShellExecute = false;
            this.driverServiceProcess.StartInfo.CreateNoWindow = this.HideCommandPromptWindow;

            DriverProcessStartingEventArgs eventArgs = new DriverProcessStartingEventArgs(this.driverServiceProcess.StartInfo);
            this.OnDriverProcessStarting(eventArgs);

            this.driverServiceProcess.Start();
            bool serviceAvailable = this.WaitForServiceInitialization();
            DriverProcessStartedEventArgs processStartedEventArgs = new DriverProcessStartedEventArgs(this.driverServiceProcess);
            this.OnDriverProcessStarted(processStartedEventArgs);

            if (!serviceAvailable)
            {
                throw new WebDriverException($"Cannot start the driver service on {this.ServiceUrl}");
            }
        }

        /// <summary>
        /// The browser options instance that corresponds to the driver service
        /// </summary>
        /// <returns></returns>
        protected abstract DriverOptions GetDefaultDriverOptions();

        /// <summary>
        /// Releases all resources associated with this <see cref="DriverService"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if the Dispose method was explicitly called; otherwise, <see langword="false"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.Stop();
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="DriverProcessStarting"/> event.
        /// </summary>
        /// <param name="eventArgs">A <see cref="DriverProcessStartingEventArgs"/> that contains the event data.</param>
        protected void OnDriverProcessStarting(DriverProcessStartingEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException(nameof(eventArgs), "eventArgs must not be null");
            }

            this.DriverProcessStarting?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="DriverProcessStarted"/> event.
        /// </summary>
        /// <param name="eventArgs">A <see cref="DriverProcessStartedEventArgs"/> that contains the event data.</param>
        protected void OnDriverProcessStarted(DriverProcessStartedEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException(nameof(eventArgs), "eventArgs must not be null");
            }

            this.DriverProcessStarted?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Stops the DriverService.
        /// </summary>
        private void Stop()
        {
            if (this.IsRunning)
            {
                if (this.HasShutdown)
                {
                    Uri shutdownUrl = new Uri(this.ServiceUrl, "/shutdown");
                    DateTime timeout = DateTime.Now.Add(this.TerminationTimeout);
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.ConnectionClose = true;

                        while (this.IsRunning && DateTime.Now < timeout)
                        {
                            try
                            {
                                // Issue the shutdown HTTP request, then wait a short while for
                                // the process to have exited. If the process hasn't yet exited,
                                // we'll retry. We wait for exit here, since catching the exception
                                // for a failed HTTP request due to a closed socket is particularly
                                // expensive.
                                using (var response = Task.Run(async () => await httpClient.GetAsync(shutdownUrl)).GetAwaiter().GetResult())
                                {

                                }

                                this.driverServiceProcess.WaitForExit(3000);
                            }
                            catch (Exception ex) when (ex is HttpRequestException || ex is TimeoutException)
                            {
                            }
                        }
                    }
                }

                // If at this point, the process still hasn't exited, wait for one
                // last-ditch time, then, if it still hasn't exited, kill it. Note
                // that falling into this branch of code should be exceedingly rare.
                if (this.IsRunning)
                {
                    this.driverServiceProcess.WaitForExit(Convert.ToInt32(this.TerminationTimeout.TotalMilliseconds));
                    if (!this.driverServiceProcess.HasExited)
                    {
                        this.driverServiceProcess.Kill();
                    }
                }

                this.driverServiceProcess.Dispose();
                this.driverServiceProcess = null;
            }
        }

        /// <summary>
        /// Waits until a the service is initialized, or the timeout set
        /// by the <see cref="InitializationTimeout"/> property is reached.
        /// </summary>
        /// <returns><see langword="true"/> if the service is properly started and receiving HTTP requests;
        /// otherwise; <see langword="false"/>.</returns>
        private bool WaitForServiceInitialization()
        {
            bool isInitialized = false;
            DateTime timeout = DateTime.Now.Add(this.InitializationTimeout);
            while (!isInitialized && DateTime.Now < timeout)
            {
                // If the driver service process has exited, we can exit early.
                if (!this.IsRunning)
                {
                    break;
                }

                isInitialized = this.IsInitialized;
            }

            return isInitialized;
        }
    }
}
