// <copyright file="StorageTest.cs" company="Selenium Committers">
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

using NUnit.Framework;
using OpenQA.Selenium.BiDi.Modules.Network;
using System;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Storage;

class StorageTest : BiDiTestFixture
{
    [Test]
    public async Task CanGetCookieByName()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        var cookies = await bidi.Storage.GetCookiesAsync(new()
        {
            Filter = new()
            {
                Name = Guid.NewGuid().ToString(),
                Value = "set"
            }
        });

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies, Is.Empty);
    }

    [Test]
    public async Task CanGetCookieInDefaultUserContext()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        var userContexts = await bidi.Browser.GetUserContextsAsync();

        var cookies = await context.Storage.GetCookiesAsync(new()
        {
            Filter = new()
            {
                Name = Guid.NewGuid().ToString(),
                Value = "set"
            }
        });

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies, Is.Empty);
        Assert.That(cookies.PartitionKey.UserContext, Is.EqualTo(userContexts[0].UserContext));
    }

    [Test]
    public async Task CanAddCookie()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        var partitionKey = await context.Storage.SetCookieAsync(new("fish", "cod", UrlBuilder.HostName));

        Assert.That(partitionKey, Is.Not.Null);
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Edge, "GetCookiesAsync returns incorrect cookies: https://github.com/MicrosoftEdge/EdgeWebDriver/issues/194")]
    public async Task CanAddAndGetCookie()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        var expiry = DateTime.Now.AddDays(1);

        await context.Storage.SetCookieAsync(new("fish", "cod", UrlBuilder.HostName)
        {
            Path = "/common/animals",
            HttpOnly = true,
            Secure = false,
            SameSite = SameSite.Lax,
            Expiry = expiry
        });

        var cookies = await context.Storage.GetCookiesAsync();

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies, Has.Count.EqualTo(1));

        var cookie = cookies[0];

        Assert.That(cookie.Name, Is.EqualTo("fish"));
        Assert.That((cookie.Value as StringBytesValue).Value, Is.EqualTo("cod"));
        Assert.That(cookie.Path, Is.EqualTo("/common/animals"));
        Assert.That(cookie.HttpOnly, Is.True);
        Assert.That(cookie.Secure, Is.False);
        Assert.That(cookie.SameSite, Is.EqualTo(SameSite.Lax));
        Assert.That(cookie.Size, Is.EqualTo(7));
        // Assert.That(cookie.Expiry, Is.EqualTo(expiry)); // chrome issue
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Edge, "GetCookiesAsync returns incorrect cookies: https://github.com/MicrosoftEdge/EdgeWebDriver/issues/194")]
    public async Task CanGetAllCookies()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        driver.Manage().Cookies.AddCookie(new("key1", "value1"));
        driver.Manage().Cookies.AddCookie(new("key2", "value2"));

        var cookies = await bidi.Storage.GetCookiesAsync();

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies, Has.Count.EqualTo(2));
        Assert.That(cookies[0].Name, Is.EqualTo("key1"));
        Assert.That(cookies[1].Name, Is.EqualTo("key2"));
    }

    [Test]
    public async Task CanDeleteAllCookies()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        driver.Manage().Cookies.AddCookie(new("key1", "value1"));
        driver.Manage().Cookies.AddCookie(new("key2", "value2"));

        var result = await bidi.Storage.DeleteCookiesAsync();

        Assert.That(result, Is.Not.Null);

        var cookies = await bidi.Storage.GetCookiesAsync();

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies.Count, Is.EqualTo(0));
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Edge, "GetCookiesAsync returns incorrect cookies: https://github.com/MicrosoftEdge/EdgeWebDriver/issues/194")]
    public async Task CanDeleteCookieWithName()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        driver.Manage().Cookies.AddCookie(new("key1", "value1"));
        driver.Manage().Cookies.AddCookie(new("key2", "value2"));

        var result = await bidi.Storage.DeleteCookiesAsync(new() { Filter = new() { Name = "key1" } });

        Assert.That(result, Is.Not.Null);

        var cookies = await bidi.Storage.GetCookiesAsync();

        Assert.That(cookies, Is.Not.Null);
        Assert.That(cookies, Has.Count.EqualTo(1));
        Assert.That(cookies[0].Name, Is.EqualTo("key2"));
    }

    [Test]
    public async Task AddCookiesWithDifferentPathsThatAreRelatedToOurs()
    {
        driver.Url = UrlBuilder.WhereIs("animals");

        await context.Storage.SetCookieAsync(new("fish", "cod", UrlBuilder.HostName)
        {
            Path = "/common/animals"
        });

        driver.Url = UrlBuilder.WhereIs("simpleTest");

        var result = driver.Manage().Cookies.AllCookies;

        Assert.That(result, Is.Empty);
    }
}
