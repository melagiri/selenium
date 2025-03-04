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

package org.openqa.selenium.bidi;

import com.google.common.collect.ImmutableMap;

import org.openqa.selenium.internal.Require;
import org.openqa.selenium.json.JsonInput;

import java.lang.reflect.Type;
import java.util.Map;
import java.util.function.Function;

public class Command<X> {

  private final String method;
  private final Map<String, Object> params;
  private final Function<JsonInput, X> mapper;

  public Command(String method, Map<String, Object> params) {
    this(method, params, Object.class);
  }

  public Command(String method, Map<String, Object> params, Type typeOfX) {
    this(method, params, input -> input.read(Require.nonNull("Type to convert to", typeOfX)));
  }

  public Command(String method, Map<String, Object> params, Function<JsonInput, X> mapper) {
    this.method = Require.nonNull("Method name", method);
    this.params = ImmutableMap.copyOf(Require.nonNull("Command parameters", params));
    this.mapper = Require.nonNull("Mapper for result", mapper);
  }

  public String getMethod() {
    return method;
  }

  public Map<String, Object> getParams() {
    return params;
  }

  Function<JsonInput, X> getMapper() {
    return mapper;
  }
}
