// <copyright file="ContinueRequestCommand.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Communication;
using System.Collections.Generic;

namespace OpenQA.Selenium.BiDi.Modules.Network;

internal class ContinueRequestCommand(ContinueRequestCommandParameters @params)
    : Command<ContinueRequestCommandParameters>(@params, "network.continueRequest");

internal record ContinueRequestCommandParameters(Request Request, BytesValue? Body, IEnumerable<CookieHeader>? Cookies, IEnumerable<Header>? Headers, string? Method, string? Url) : CommandParameters;

public record ContinueRequestOptions : CommandOptions
{
    public BytesValue? Body { get; set; }

    public IEnumerable<CookieHeader>? Cookies { get; set; }

    public IEnumerable<Header>? Headers { get; set; }

    public string? Method { get; set; }

    public string? Url { get; set; }
}
