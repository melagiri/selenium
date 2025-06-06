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

'use strict';

const fs = require('fs'),
    path = require('path');

const COPYRIGHT_TEXT = [
  '// Licensed to the Software Freedom Conservancy (SFC) under one',
  '// or more contributor license agreements.  See the NOTICE file',
  '// distributed with this work for additional information',
  '// regarding copyright ownership.  The SFC licenses this file',
  '// to you under the Apache License, Version 2.0 (the',
  '// "License"); you may not use this file except in compliance',
  '// with the License.  You may obtain a copy of the License at',
  '//',
  '//   http://www.apache.org/licenses/LICENSE-2.0',
  '//',
  '// Unless required by applicable law or agreed to in writing,',
  '// software distributed under the License is distributed on an',
  '// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY',
  '// KIND, either express or implied.  See the License for the',
  '// specific language governing permissions and limitations',
  '// under the License.',
  ''
].join('\n');


let BASE_PATH = path.normalize(path.join(__dirname, '../javascript'));

// Ignore files whose copyright is not SFC or need manual inspection.
let IGNORE_PATHS = [
  path.join(BASE_PATH, 'atoms/test/jquery.min.js'),
];

updateDir(BASE_PATH);

function updateDir(dirname) {
  dirname = path.normalize(dirname);
  console.log('Scanning directory %s', dirname);
  fs.readdirSync(dirname).forEach(function(filePath) {
    if (filePath === 'node_modules') return;

    filePath = path.normalize(path.join(dirname, filePath));
    if (IGNORE_PATHS.indexOf(filePath) !== -1) {
      return;
    }

    if (fs.statSync(filePath).isDirectory()) {
      updateDir(filePath);
    } else if (/.*\.js$/.test(filePath)) {
      let index = -1;
      let lines = fs.readFileSync(filePath, 'utf8').split(/\n/);
      lines.some(function(line) {
        if (line.slice(0, 2) === '//') {
          index += 1;
          return false;
        }
        return true;
      });

      let content = COPYRIGHT_TEXT;
      if (index === -1) {
        console.log('...file is missing copyright header: %s', filePath);
        content += '\n' + lines.join('\n');
      } else {
        let current = lines.slice(0, index + 1).join('\n') + '\n';
        if (current === content) {
          // console.log('...header is up-to-date: %s', filePath);
          return;
        } else {
          console.log('...replacing copyright header: %s', filePath);
          content += lines.slice(index + 1).join('\n');
        }
      }
      fs.writeFileSync(filePath, content, 'utf8');
    }
  })
}
