// <copyright file="RemoteValueConverter.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json.Converters.Polymorphic;

// https://github.com/dotnet/runtime/issues/72604
internal class RemoteValueConverter : JsonConverter<RemoteValue>
{
    public override RemoteValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);

        if (jsonDocument.RootElement.ValueKind == JsonValueKind.String)
        {
            return new RemoteValue.String(jsonDocument.RootElement.GetString()!);
        }

        return jsonDocument.RootElement.GetProperty("type").ToString() switch
        {
            "number" => jsonDocument.Deserialize<RemoteValue.Number>(options),
            "boolean" => jsonDocument.Deserialize<RemoteValue.Boolean>(options),
            "string" => jsonDocument.Deserialize<RemoteValue.String>(options),
            "null" => jsonDocument.Deserialize<RemoteValue.Null>(options),
            "undefined" => jsonDocument.Deserialize<RemoteValue.Undefined>(options),
            "symbol" => jsonDocument.Deserialize<RemoteValue.Symbol>(options),
            "array" => jsonDocument.Deserialize<RemoteValue.Array>(options),
            "object" => jsonDocument.Deserialize<RemoteValue.Object>(options),
            "function" => jsonDocument.Deserialize<RemoteValue.Function>(options),
            "regexp" => jsonDocument.Deserialize<RemoteValue.RegExp>(options),
            "date" => jsonDocument.Deserialize<RemoteValue.Date>(options),
            "map" => jsonDocument.Deserialize<RemoteValue.Map>(options),
            "set" => jsonDocument.Deserialize<RemoteValue.Set>(options),
            "weakmap" => jsonDocument.Deserialize<RemoteValue.WeakMap>(options),
            "weakset" => jsonDocument.Deserialize<RemoteValue.WeakSet>(options),
            "generator" => jsonDocument.Deserialize<RemoteValue.Generator>(options),
            "error" => jsonDocument.Deserialize<RemoteValue.Error>(options),
            "proxy" => jsonDocument.Deserialize<RemoteValue.Proxy>(options),
            "promise" => jsonDocument.Deserialize<RemoteValue.Promise>(options),
            "typedarray" => jsonDocument.Deserialize<RemoteValue.TypedArray>(options),
            "arraybuffer" => jsonDocument.Deserialize<RemoteValue.ArrayBuffer>(options),
            "nodelist" => jsonDocument.Deserialize<RemoteValue.NodeList>(options),
            "htmlcollection" => jsonDocument.Deserialize<RemoteValue.HtmlCollection>(options),
            "node" => jsonDocument.Deserialize<RemoteValue.Node>(options),
            "window" => jsonDocument.Deserialize<RemoteValue.WindowProxy>(options),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
