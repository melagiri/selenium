// <copyright file="SourceActions.cs" company="Selenium Committers">
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
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Modules.Input;

public abstract record SourceActions
{
    public string Id { get; } = Guid.NewGuid().ToString();
}

public interface ISourceAction;

public record SourceActions<T> : SourceActions, IEnumerable<ISourceAction> where T : ISourceAction
{
    public IList<ISourceAction> Actions { get; set; } = [];

    public IEnumerator<ISourceAction> GetEnumerator() => Actions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Actions.GetEnumerator();

    public void Add(ISourceAction action) => Actions.Add(action);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Pause), "pause")]
[JsonDerivedType(typeof(Key.Down), "keyDown")]
[JsonDerivedType(typeof(Key.Up), "keyUp")]
public interface IKeySourceAction : ISourceAction;

public record KeyActions : SourceActions<IKeySourceAction>
{
    public KeyActions Type(string text)
    {
        foreach (var character in text)
        {
            Add(new Key.Down(character));
            Add(new Key.Up(character));
        }

        return this;
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Pause), "pause")]
[JsonDerivedType(typeof(Pointer.Down), "pointerDown")]
[JsonDerivedType(typeof(Pointer.Up), "pointerUp")]
[JsonDerivedType(typeof(Pointer.Move), "pointerMove")]
public interface IPointerSourceAction : ISourceAction;

public record PointerActions : SourceActions<IPointerSourceAction>
{
    public PointerParameters? Options { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Pause), "pause")]
[JsonDerivedType(typeof(Wheel.Scroll), "scroll")]
public interface IWheelSourceAction : ISourceAction;

public record WheelActions : SourceActions<IWheelSourceAction>;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Pause), "pause")]
public interface INoneSourceAction : ISourceAction;

public record NoneActions : SourceActions<None>;

public abstract partial record Key : IKeySourceAction
{
    public record Down(char Value) : Key;

    public record Up(char Value) : Key;
}

public abstract record Pointer : IPointerSourceAction
{
    public record Down(int Button) : Pointer, IPointerCommonProperties
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? Pressure { get; set; }
        public double? TangentialPressure { get; set; }
        public int? Twist { get; set; }
        public double? AltitudeAngle { get; set; }
        public double? AzimuthAngle { get; set; }
    }

    public record Up(int Button) : Pointer;

    public record Move(int X, int Y) : Pointer, IPointerCommonProperties
    {
        public int? Duration { get; set; }

        public Origin? Origin { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? Pressure { get; set; }
        public double? TangentialPressure { get; set; }
        public int? Twist { get; set; }
        public double? AltitudeAngle { get; set; }
        public double? AzimuthAngle { get; set; }
    }
}

public abstract record Wheel : IWheelSourceAction
{
    public record Scroll(int X, int Y, int DeltaX, int DeltaY) : Wheel
    {
        public int? Duration { get; set; }

        public Origin? Origin { get; set; }
    }
}

public abstract record None : INoneSourceAction;

public record Pause : ISourceAction, IKeySourceAction, IPointerSourceAction, IWheelSourceAction, INoneSourceAction
{
    public long? Duration { get; set; }
}

public record PointerParameters
{
    public PointerType? PointerType { get; set; }
}

public enum PointerType
{
    Mouse,
    Pen,
    Touch
}

public interface IPointerCommonProperties
{
    public int? Width { get; set; }

    public int? Height { get; set; }

    public double? Pressure { get; set; }

    public double? TangentialPressure { get; set; }

    public int? Twist { get; set; }

    public double? AltitudeAngle { get; set; }

    public double? AzimuthAngle { get; set; }
}
