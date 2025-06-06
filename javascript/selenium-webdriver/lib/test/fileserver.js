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

'use strict'

const path = require('node:path')
const url = require('node:url')

const express = require('express')
const multer = require('multer')
const serveIndex = require('serve-index')

const { isDevMode } = require('./build')
const resources = require('./resources')
const { Server } = require('./httpserver')

const WEB_ROOT = '/common'
const DATA_ROOT = '/data'
const JS_ROOT = '/javascript'

const baseDirectory = resources.locate('common/src/web')
const dataDirectory = path.join(__dirname, 'data')
const jsDirectory = resources.locate('javascript')

const Pages = (function () {
  let pages = {}

  function addPage(page, path) {
    pages.__defineGetter__(page, function () {
      return exports.whereIs(path)
    })
  }

  addPage('ajaxyPage', 'ajaxy_page.html')
  addPage('alertsPage', 'alerts.html')
  addPage('basicAuth', 'basicAuth')
  addPage('blankPage', 'blank.html')
  addPage('bodyTypingPage', 'bodyTypingTest.html')
  addPage('booleanAttributes', 'booleanAttributes.html')
  addPage('childPage', 'child/childPage.html')
  addPage('chinesePage', 'cn-test.html')
  addPage('clickJacker', 'click_jacker.html')
  addPage('clickEventPage', 'clickEventPage.html')
  addPage('clicksPage', 'clicks.html')
  addPage('colorPage', 'colorPage.html')
  addPage('deletingFrame', 'deletingFrame.htm')
  addPage('draggableLists', 'draggableLists.html')
  addPage('dragAndDropPage', 'dragAndDropTest.html')
  addPage('droppableItems', 'droppableItems.html')
  addPage('documentWrite', 'document_write_in_onload.html')
  addPage('dynamicallyModifiedPage', 'dynamicallyModifiedPage.html')
  addPage('dynamicPage', 'dynamic.html')
  addPage('echoPage', 'echo')
  addPage('errorsPage', 'errors.html')
  addPage('xhtmlFormPage', 'xhtmlFormPage.xhtml')
  addPage('formPage', 'formPage.html')
  addPage('formSelectionPage', 'formSelectionPage.html')
  addPage('framesetPage', 'frameset.html')
  addPage('grandchildPage', 'child/grandchild/grandchildPage.html')
  addPage('html5Page', 'html5Page.html')
  addPage('html5OfflinePage', 'html5/offline.html')
  addPage('iframePage', 'iframes.html')
  addPage('javascriptEnhancedForm', 'javascriptEnhancedForm.html')
  addPage('javascriptPage', 'javascriptPage.html')
  addPage('linkedImage', 'linked_image.html')
  addPage('longContentPage', 'longContentPage.html')
  addPage('macbethPage', 'macbeth.html')
  addPage('mapVisibilityPage', 'map_visibility.html')
  addPage('metaRedirectPage', 'meta-redirect.html')
  addPage('missedJsReferencePage', 'missedJsReference.html')
  addPage('mouseTrackerPage', 'mousePositionTracker.html')
  addPage('nestedPage', 'nestedElements.html')
  addPage('readOnlyPage', 'readOnlyPage.html')
  addPage('rectanglesPage', 'rectangles.html')
  addPage('relativeLocators', 'relative_locators.html')
  addPage('redirectPage', 'redirect')
  addPage('resultPage', 'resultPage.html')
  addPage('richTextPage', 'rich_text.html')
  addPage('printPage', 'printPage.html')
  addPage('scrollingPage', 'scrollingPage.html')
  addPage('selectableItemsPage', 'selectableItems.html')
  addPage('selectPage', 'selectPage.html')
  addPage('selectSpacePage', 'select_space.html')
  addPage('simpleTestPage', 'simpleTest.html')
  addPage('simpleXmlDocument', 'simple.xml')
  addPage('sleepingPage', 'sleep')
  addPage('slowIframes', 'slow_loading_iframes.html')
  addPage('slowLoadingAlertPage', 'slowLoadingAlert.html')
  addPage('svgPage', 'svgPiechart.xhtml')
  addPage('tables', 'tables.html')
  addPage('underscorePage', 'underscore.html')
  addPage('unicodeLtrPage', 'utf8/unicode_ltr.html')
  addPage('uploadPage', 'upload.html')
  addPage('veryLargeCanvas', 'veryLargeCanvas.html')
  addPage('webComponents', 'webComponents.html')
  addPage('xhtmlTestPage', 'xhtmlTest.html')
  addPage('uploadInvisibleTestPage', 'upload_invisible.html')
  addPage('userpromptPage', 'userprompt.html')
  addPage('virtualAuthenticator', 'virtual-authenticator.html')
  addPage('logEntryAdded', 'bidi/logEntryAdded.html')
  addPage('scriptTestAccessProperty', 'bidi/scriptTestAccessProperty.html')
  addPage('scriptTestRemoveProperty', 'bidi/scriptTestRemoveProperty.html')
  addPage('emptyPage', 'bidi/emptyPage.html')
  addPage('emptyText', 'bidi/emptyText.txt')
  addPage('redirectedHttpEquiv', 'bidi/redirected_http_equiv.html')
  addPage('releaseAction', 'bidi/release_action.html')
  addPage('fedcm', 'fedcm/fedcm_async_js.html')
  return pages
})()

const Path = {
  BASIC_AUTH: WEB_ROOT + '/basicAuth',
  ECHO: WEB_ROOT + '/echo',
  GENERATED: WEB_ROOT + '/generated',
  MANIFEST: WEB_ROOT + '/manifest',
  REDIRECT: WEB_ROOT + '/redirect',
  PAGE: WEB_ROOT + '/page',
  SLEEP: WEB_ROOT + '/sleep',
  UPLOAD: WEB_ROOT + '/upload',
}

var app = express()

app
  .get('/', sendIndex)
  .get('/favicon.ico', function (_req, res) {
    res.writeHead(204)
    res.end()
  })
  .use(JS_ROOT, serveIndex(jsDirectory), express.static(jsDirectory))
  .post(Path.UPLOAD, handleUpload)
  .use(WEB_ROOT, serveIndex(baseDirectory), express.static(baseDirectory))
  .use(DATA_ROOT, serveIndex(dataDirectory), express.static(dataDirectory))
  .get(Path.ECHO, sendEcho)
  .get(Path.PAGE, sendInifinitePage)
  .get(Path.PAGE + '/*', sendInifinitePage)
  .get(Path.REDIRECT, redirectToResultPage)
  .get(Path.SLEEP, sendDelayedResponse)
  .get(Path.BASIC_AUTH, sendBasicAuth)

if (isDevMode()) {
  var closureDir = resources.locate('third_party/closure/goog')
  app.use('/third_party/closure/goog', serveIndex(closureDir), express.static(closureDir))
}
var server = new Server(app)

function redirectToResultPage(_, response) {
  response.writeHead(303, {
    Location: Pages.resultPage,
  })
  return response.end()
}

function sendInifinitePage(request, response) {
  // eslint-disable-next-line n/no-deprecated-api
  var pathname = url.parse(request.url).pathname
  var lastIndex = pathname.lastIndexOf('/')
  var pageNumber = lastIndex == -1 ? 'Unknown' : pathname.substring(lastIndex + 1)
  var body = [
    '<!DOCTYPE html>',
    '<title>Page',
    pageNumber,
    '</title>',
    'Page number <span id="pageNumber">',
    pageNumber,
    '</span>',
    '<p><a href="../xhtmlTest.html" target="_top">top</a>',
  ].join('')
  response.writeHead(200, {
    'Content-Length': Buffer.byteLength(body, 'utf8'),
    'Content-Type': 'text/html; charset=utf-8',
  })
  response.end(body)
}

function sendBasicAuth(request, response) {
  const denyAccess = function () {
    response.writeHead(401, { 'WWW-Authenticate': 'Basic realm="test"' })
    response.end('Access denied')
  }

  const basicAuthRegExp = /^\s*basic\s+([a-z0-9\-._~+/]+)=*\s*$/i
  const auth = request.headers.authorization
  const match = basicAuthRegExp.exec(auth || '')
  if (!match) {
    denyAccess()
    return
  }

  const userNameAndPass = Buffer.from(match[1], 'base64').toString()
  const parts = userNameAndPass.split(':', 2)
  if (parts[0] !== 'genie' || parts[1] !== 'bottle') {
    denyAccess()
    return
  }

  response.writeHead(200, { 'content-type': 'text/plain' })
  response.end('Access granted!')
}

function sendDelayedResponse(request, response) {
  var duration = 0
  // eslint-disable-next-line n/no-deprecated-api
  var query = url.parse(request.url).search.substr(1) || ''
  var match = query.match(/\btime=(\d+)/)
  if (match) {
    duration = parseInt(match[1], 10)
  }

  setTimeout(function () {
    var body = ['<!DOCTYPE html>', '<title>Done</title>', '<body>Slept for ', duration, 's</body>'].join('')
    response.writeHead(200, {
      'Content-Length': Buffer.byteLength(body, 'utf8'),
      'Content-Type': 'text/html; charset=utf-8',
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache',
      Expires: 0,
    })
    response.end(body)
  }, duration * 1000)
}

function handleUpload(request, response) {
  let upload = multer({ storage: multer.memoryStorage() }).any()
  upload(request, response, function (err) {
    if (err) {
      response.writeHead(500)
      response.end(err + '')
    } else {
      if (!request.files) {
        return response.status(400).send('No files were uploaded')
      }

      let files = []
      let keys = Object.keys(request.files)

      keys.forEach((file) => {
        files.push(request.files[file].originalname)
      })

      response
        .status(200)
        .contentType('html')
        .send(files.join('\n') + '\n<script>window.top.window.onUploadDone();</script>')
    }
  })
}

function sendEcho(request, response) {
  if (request.query['html']) {
    const html = request.query['html']
    if (html) {
      response.writeHead(200, {
        'Content-Length': Buffer.byteLength(html, 'utf8'),
        'Content-Type': 'text/html; charset=utf-8',
      })
      response.end(html)
      return
    }
  }

  var body = [
    '<!DOCTYPE html>',
    '<title>Echo</title>',
    '<div class="request">',
    request.method,
    ' ',
    request.url,
    ' ',
    'HTTP/',
    request.httpVersion,
    '</div>',
  ]
  for (var name in request.headers) {
    body.push('<div class="header ', name, '">', name, ': ', request.headers[name], '</div>')
  }
  body = body.join('')
  response.writeHead(200, {
    'Content-Length': Buffer.byteLength(body, 'utf8'),
    'Content-Type': 'text/html; charset=utf-8',
  })
  response.end(body)
}

/**
 * Responds to a request for the file server's main index.
 * @param {!http.ServerRequest} request The request object.
 * @param {!http.ServerResponse} response The response object.
 */
function sendIndex(request, response) {
  // eslint-disable-next-line n/no-deprecated-api
  var pathname = url.parse(request.url).pathname

  var host = request.headers.host
  if (!host) {
    host = server.host()
  }

  var requestUrl = ['http://' + host + pathname].join('')

  function createListEntry(path) {
    var url = requestUrl + path
    return ['<li><a href="', url, '">', path, '</a>'].join('')
  }

  var data = ['<!DOCTYPE html><h1>/</h1><hr/><ul>', createListEntry('common'), createListEntry('data')]
  if (isDevMode()) {
    data.push(createListEntry('javascript'))
  }
  data.push('</ul>')
  data = data.join('')

  response.writeHead(200, {
    'Content-Type': 'text/html; charset=UTF-8',
    'Content-Length': Buffer.byteLength(data, 'utf8'),
  })
  response.end(data)
}

/**
 * Detects the hostname.
 * @return {string} The detected hostname or 'localhost' if not found.
 */
function getHostName() {
  const hostnameFromEnv = process.env.HOSTNAME
  return hostnameFromEnv ? hostnameFromEnv : 'localhost'
}

// PUBLIC application

/**
 * Starts the server on the specified port.
 * @param {number=} opt_port The port to use, or 0 for any free port.
 * @return {!Promise<Host>} A promise that will resolve
 *     with the server host when it has fully started.
 */
exports.start = server.start.bind(server)

/**
 * Stops the server.
 * @return {!Promise} A promise that will resolve when the
 *     server has closed all connections.
 */
exports.stop = server.stop.bind(server)

/**
 * Formats a URL for this server.
 * @param {string=} opt_pathname The desired pathname on the server.
 * @return {string} The formatted URL.
 * @throws {Error} If the server is not running.
 */
exports.url = server.url.bind(server)

/**
 * Builds the URL for a file in the //common/src/web directory of the
 * Selenium client.
 * @param {string} filePath A path relative to //common/src/web to compute a
 *     URL for.
 * @return {string} The formatted URL.
 * @throws {Error} If the server is not running.
 */
exports.whereIs = function (filePath) {
  filePath = filePath.replace(/\\/g, '/')
  if (!filePath.startsWith('/')) {
    filePath = `${WEB_ROOT}/${filePath}`
  }
  return server.url(filePath)
}

exports.getHostName = getHostName

exports.Pages = Pages

if (require.main === module) {
  server.start(2310).then(function () {
    console.log('Server running at ' + server.url())
  })
}
