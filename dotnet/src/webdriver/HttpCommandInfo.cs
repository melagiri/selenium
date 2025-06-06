// <copyright file="HttpCommandInfo.cs" company="Selenium Committers">
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
using System.Globalization;

namespace OpenQA.Selenium
{
    /// <summary>
    /// Provides the execution information for a <see cref="DriverCommand"/>.
    /// </summary>
    public class HttpCommandInfo : CommandInfo
    {
        /// <summary>
        /// POST verb for the command info
        /// </summary>
        public const string PostCommand = "POST";

        /// <summary>
        /// GET verb for the command info
        /// </summary>
        public const string GetCommand = "GET";

        /// <summary>
        /// DELETE verb for the command info
        /// </summary>
        public const string DeleteCommand = "DELETE";

        private const string SessionIdPropertyName = "sessionId";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCommandInfo"/> class
        /// </summary>
        /// <param name="method">Method of the Command</param>
        /// <param name="resourcePath">Relative URL path to the resource used to execute the command</param>
        /// <exception cref="ArgumentNullException">If <paramref name="method"/> or <paramref name="resourcePath"/> are <see langword="null"/>.</exception>
        public HttpCommandInfo(string method, string resourcePath)
        {
            this.ResourcePath = resourcePath ?? throw new ArgumentNullException(nameof(resourcePath));
            this.Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        /// <summary>
        /// Gets the URL representing the path to the resource.
        /// </summary>
        public string ResourcePath { get; }

        /// <summary>
        /// Gets the HTTP method associated with the command.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Gets the unique identifier for this command within the scope of its protocol definition
        /// </summary>
        public override string CommandIdentifier
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.Method, this.ResourcePath); }
        }

        /// <summary>
        /// Creates the full URI associated with this command, substituting command
        /// parameters for tokens in the URI template.
        /// </summary>
        /// <param name="baseUri">The base URI associated with the command.</param>
        /// <param name="commandToExecute">The command containing the parameters with which
        /// to substitute the tokens in the template.</param>
        /// <returns>The full URI for the command, with the parameters of the command
        /// substituted for the tokens in the template.</returns>
        public Uri CreateCommandUri(Uri baseUri, Command commandToExecute)
        {
            string[] urlParts = this.ResourcePath.Split(["/"], StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < urlParts.Length; i++)
            {
                string urlPart = urlParts[i];
                if (urlPart.StartsWith("{", StringComparison.OrdinalIgnoreCase) && urlPart.EndsWith("}", StringComparison.OrdinalIgnoreCase))
                {
                    urlParts[i] = GetCommandPropertyValue(urlPart, commandToExecute);
                }
            }

            string relativeUrlString = string.Join("/", urlParts);
            Uri relativeUri = new Uri(relativeUrlString, UriKind.Relative);
            if (!Uri.TryCreate(baseUri, relativeUri, out Uri? fullUri))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to create URI from base {0} and relative path {1}", baseUri?.ToString(), relativeUrlString));
            }

            return fullUri;
        }

        private static string GetCommandPropertyValue(string propertyName, Command commandToExecute)
        {
            string propertyValue = string.Empty;

            // Strip the curly braces
            propertyName = propertyName.Substring(1, propertyName.Length - 2);

            if (propertyName == SessionIdPropertyName)
            {
                if (commandToExecute.SessionId != null)
                {
                    propertyValue = commandToExecute.SessionId.ToString();
                }
            }
            else if (commandToExecute.Parameters != null && commandToExecute.Parameters.Count > 0)
            {
                // Extract the URL parameter, and remove it from the parameters dictionary
                // so it doesn't get transmitted as a JSON parameter.
                if (commandToExecute.Parameters.TryGetValue(propertyName, out var propertyValueObject))
                {
                    if (propertyValueObject != null)
                    {
                        propertyValue = propertyValueObject.ToString()!;
                        commandToExecute.Parameters.Remove(propertyName);
                    }
                }
            }

            return propertyValue;
        }
    }
}
