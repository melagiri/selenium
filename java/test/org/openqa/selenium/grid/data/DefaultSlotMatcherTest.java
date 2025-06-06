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

package org.openqa.selenium.grid.data;

import static org.assertj.core.api.Assertions.assertThat;

import org.junit.jupiter.api.Test;
import org.openqa.selenium.Capabilities;
import org.openqa.selenium.ImmutableCapabilities;
import org.openqa.selenium.Platform;
import org.openqa.selenium.remote.CapabilityType;

class DefaultSlotMatcherTest {

  private final DefaultSlotMatcher slotMatcher = new DefaultSlotMatcher();
  private final SemanticVersionComparator comparator = new SemanticVersionComparator();

  @Test
  void testBrowserVersionMatch() {
    assertThat(comparator.compare("", "")).isEqualTo(0);
    assertThat(comparator.compare("", "130.0")).isEqualTo(1);
    assertThat(comparator.compare("131.0.6778.85", "131")).isEqualTo(0);
    assertThat(comparator.compare("131.0.6778.85", "131.0")).isEqualTo(0);
    assertThat(comparator.compare("131.0.6778.85", "131.0.6778")).isEqualTo(0);
    assertThat(comparator.compare("131.0.6778.85", "131.0.6778.95")).isEqualTo(-1);
    assertThat(comparator.compare("130.0", "130.0")).isEqualTo(0);
    assertThat(comparator.compare("130.0", "130")).isEqualTo(0);
    assertThat(comparator.compare("130.0.1", "130")).isEqualTo(0);
    assertThat(comparator.compare("130.0.1", "130.0.1")).isEqualTo(0);
    assertThat(comparator.compare("133.0a1", "133")).isEqualTo(0);
    assertThat(comparator.compare("133", "133.0a1")).isEqualTo(1);
    assertThat(comparator.compare("dev", "Dev")).isEqualTo(0);
    assertThat(comparator.compare("Beta", "beta")).isEqualTo(0);
    assertThat(comparator.compare("130.0.1", "130.0.2")).isEqualTo(-1);
    assertThat(comparator.compare("130.1", "130.0")).isEqualTo(1);
    assertThat(comparator.compare("131.0", "130.0")).isEqualTo(1);
    assertThat(comparator.compare("130.0", "131")).isEqualTo(-1);
  }

  @Test
  void fullMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void fullMatchExtensionCaps() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "firefox",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "se:downloadsEnabled",
            true);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "firefox",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "se:downloadsEnabled",
            true);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void fullMatchWithTestMetadata() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", CapabilityType.PLATFORM_NAME, Platform.LINUX);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.PLATFORM_NAME,
            Platform.LINUX,
            "se:name",
            "testName");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void fullMatchWithVideoRecording() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.PLATFORM_NAME,
            Platform.LINUX,
            "se:noVncPort",
            7900,
            "se:vncEnabled",
            true,
            "se:containerName",
            "my-container");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.PLATFORM_NAME,
            Platform.LINUX,
            "se:recordVideo",
            true,
            "se:screenResolution",
            "1920x1080");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void notMatchWithTestMetadata() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.PLATFORM_NAME,
            Platform.LINUX,
            "myApp:version",
            "beta");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.PLATFORM_NAME,
            Platform.LINUX,
            "se:recordVideo",
            true,
            "se:name",
            "testName",
            "myApp:version",
            "stable");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void matchesBrowserAndVersion() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void matchesBrowser() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities = new ImmutableCapabilities(CapabilityType.BROWSER_NAME, "chrome");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void matchDownloadsForRegularTestMatchingAgainstADownloadAwareNode() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", "se:downloadsEnabled", true);
    Capabilities capabilities = new ImmutableCapabilities(CapabilityType.BROWSER_NAME, "chrome");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void matchDownloadsForAutoDownloadTestMatchingAgainstADownloadAwareNode() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", "se:downloadsEnabled", true);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", "se:downloadsEnabled", true);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void ensureNoMatchFOrDownloadAwareTestMatchingAgainstOrdinaryNode() {
    Capabilities stereotype = new ImmutableCapabilities(CapabilityType.BROWSER_NAME, "chrome");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", "se:downloadsEnabled", true);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void matchesEmptyBrowser() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void matchesPrefixedPlatformVersion() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "10");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "10");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void prefixedPlatformVersionDoesNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "10");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "11");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void matchesWhenPrefixedPlatformVersionIsNotRequested() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "10");
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void prefixedPlatformVersionDoesNotMatchWhenNotPresentInStereotype() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "80",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:platformVersion",
            "10");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void platformDoesNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.MAC);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void browserDoesNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "firefox",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void browserVersionDoesNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "84",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void requestedPlatformDoesNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "80",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void shouldNotMatchIfRequestedBrowserVersionIsMissingFromStereotype() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome", CapabilityType.PLATFORM_NAME, Platform.WINDOWS);

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "84",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void shouldNotMatchCapabilitiesThatAreDifferentButDoNotContainCommonCapabilityNames() {
    Capabilities stereotype = new ImmutableCapabilities("acceptInsecureCerts", "true");
    Capabilities capabilities = new ImmutableCapabilities("acceptInsecureCerts", "false");

    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void shouldMatchCapabilitiesThatAreTheSameButDoNotContainCommonCapabilityNames() {
    Capabilities stereotype = new ImmutableCapabilities("strictFileInteractability", "true");
    Capabilities capabilities = new ImmutableCapabilities("strictFileInteractability", "true");

    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void extensionPrefixedCapabilitiesMatches() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void extensionPrefixedCapabilitiesMatchesWhenNotPresentInStereotype() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "chrome",
            CapabilityType.BROWSER_VERSION, "84",
            CapabilityType.PLATFORM_NAME, Platform.WINDOWS);

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void extensionPrefixedCapabilitiesDoNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "gouda");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void multipleExtensionPrefixedCapabilitiesMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam",
            "prefixed:fruit",
            "mango");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam",
            "prefixed:fruit",
            "mango");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void multipleExtensionPrefixedCapabilitiesDoNotMatchWhenOneIsDifferent() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam",
            "prefixed:fruit",
            "mango");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "prefixed:cheese",
            "amsterdam",
            "prefixed:fruit",
            "orange");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void vendorExtensionPrefixedCapabilitiesAreIgnoredForMatching() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "goog:cheese",
            "amsterdam",
            "ms:fruit",
            "mango");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME,
            "chrome",
            CapabilityType.BROWSER_VERSION,
            "84",
            CapabilityType.PLATFORM_NAME,
            Platform.WINDOWS,
            "goog:cheese",
            "gouda",
            "ms:fruit",
            "orange");
    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }

  @Test
  void emptyCapabilitiesDoNotMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.BROWSER_NAME, "firefox",
            CapabilityType.BROWSER_VERSION, "98",
            CapabilityType.PLATFORM_NAME, Platform.MAC);

    Capabilities capabilities = new ImmutableCapabilities();
    assertThat(slotMatcher.matches(stereotype, capabilities)).isFalse();
  }

  @Test
  void extensionCapsAlsoMatch() {
    Capabilities stereotype =
        new ImmutableCapabilities(
            CapabilityType.PLATFORM_NAME,
            Platform.IOS,
            "appium:platformVersion",
            "15.5",
            "appium:automationName",
            "XCUITest",
            "appium:deviceName",
            "iPhone 13");

    Capabilities capabilities =
        new ImmutableCapabilities(
            CapabilityType.PLATFORM_NAME,
            Platform.IOS,
            "appium:platformVersion",
            "15.5",
            "appium:automationName",
            "XCUITest",
            "appium:noReset",
            true,
            "appium:deviceName",
            "iPhone 13");

    assertThat(slotMatcher.matches(stereotype, capabilities)).isTrue();
  }
}
