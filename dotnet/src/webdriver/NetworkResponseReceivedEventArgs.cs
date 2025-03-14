// <copyright file="NetworkResponseReceivedEventArgs.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium
{
    /// <summary>
    /// Provides data for the NetworkResponseReceived event of an object implementing the <see cref="INetwork"/> interface.
    /// </summary>
    public class NetworkResponseReceivedEventArgs : EventArgs
    {
        private readonly Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkResponseReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="responseData">The <see cref="HttpResponseData"/> that describes the network response.</param>
        public NetworkResponseReceivedEventArgs(HttpResponseData responseData)
        {
            this.RequestId = responseData.RequestId;
            this.ResponseUrl = responseData.Url;
            this.ResponseStatusCode = responseData.StatusCode;
            this.ResponseContent = responseData.Content;
            this.ResponseResourceType = responseData.ResourceType;
            foreach (KeyValuePair<string, string> header in responseData.Headers)
            {
                this.responseHeaders[header.Key] = header.Value;
            }
        }

        /// <summary>
        /// Gets the request ID of the network request that generated this response.
        /// </summary>
        public string? RequestId { get; }

        /// <summary>
        /// Gets the URL of the network response.
        /// </summary>
        public string? ResponseUrl { get; }

        /// <summary>
        /// Gets the HTTP status code of the network response.
        /// </summary>
        public long ResponseStatusCode { get; }

        /// <summary>
        /// Gets the body of the network response.
        /// </summary>
        /// <remarks>
        /// This property is an alias for <see cref="ResponseContent"/>.ReadAsString() to keep backward compatibility.
        /// </remarks>
        public string? ResponseBody => this.ResponseContent?.ReadAsString();

        /// <summary>
        /// Gets the content of the network response, if any.
        /// </summary>
        public HttpResponseContent? ResponseContent { get; }

        /// <summary>
        /// Gets the type of resource of the network response.
        /// </summary>
        public string? ResponseResourceType { get; }

        /// <summary>
        /// Gets the headers associated with this network response.
        /// </summary>
        public IReadOnlyDictionary<string, string> ResponseHeaders => this.responseHeaders;
    }
}
