// <copyright file="HttpResponseData.cs" company="Selenium Committers">
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Represents the response data for an intercepted HTTP call.
    /// </summary>
    public class HttpResponseData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseData"/> type.
        /// </summary>
        public HttpResponseData()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the request that generated this response.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the URL of the HTTP response.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the numeric status code of the HTTP response.
        /// </summary>
        public long StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the body of the HTTP response.
        /// </summary>
        [DisallowNull]
        public string? Body
        {
            get => this.Content?.ReadAsString();
            set => this.Content = new HttpResponseContent(value);
        }

        /// <summary>
        /// Gets or sets the content of the HTTP response.
        /// </summary>
        public HttpResponseContent? Content { get; set; }

        /// <summary>
        /// Gets or sets the type of resource for this response.
        /// </summary>
        public string? ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the reason for an error response.
        /// </summary>
        public string? ErrorReason { get; set; }

        /// <summary>
        /// Gets the headers of the HTTP response.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the cookie headers of the HTTP response.
        /// </summary>
        public List<string> CookieHeaders { get; } = new List<string>();
    }
}
