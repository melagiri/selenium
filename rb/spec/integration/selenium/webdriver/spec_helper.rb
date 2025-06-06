# frozen_string_literal: true

# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

require 'rubygems'
require 'time'
require 'rspec'

require 'selenium-webdriver'
require_relative 'spec_support'
require_relative '../../../rspec_matchers'

include Selenium # rubocop:disable Style/MixinUsage

GlobalTestEnv = WebDriver::SpecSupport::TestEnvironment.new

class SeleniumTestListener
  def example_finished(notification)
    exception = notification.example.exception
    assertion_failed = exception &&
                       (exception.is_a?(RSpec::Expectations::ExpectationNotMetError) ||
                        exception.is_a?(RSpec::Core::Pending::PendingExampleFixedError))
    pending_exception = exception.nil? && notification.example.pending && !notification.example.skip

    GlobalTestEnv.reset_driver! if (exception && !assertion_failed) || pending_exception
  end
end

RSpec.configure do |c|
  c.define_derived_metadata do |meta|
    meta[:aggregate_failures] = true
  end

  c.reporter.register_listener(SeleniumTestListener.new, :example_finished)

  c.include(WebDriver::SpecSupport::Helpers)

  c.before(:suite) do
    GlobalTestEnv.remote_server.start if GlobalTestEnv.driver == :remote && ENV['WD_REMOTE_URL'].nil?
    GlobalTestEnv.print_env
  end

  c.after(:suite) do
    GlobalTestEnv.quit_driver
  end

  c.filter_run_when_matching :focus
  c.run_all_when_everything_filtered = true
  c.default_formatter = c.files_to_run.count > 1 ? 'progress' : 'doc'

  c.before do |example|
    guards = WebDriver::Support::Guards.new(example, bug_tracker: 'https://github.com/SeleniumHQ/selenium/issues')
    guards.add_condition(:driver, GlobalTestEnv.driver)
    guards.add_condition(:browser, GlobalTestEnv.browser)
    guards.add_condition(:ci, WebDriver::Platform.ci)
    guards.add_condition(:platform, WebDriver::Platform.os)
    guards.add_condition(:headless, !ENV['HEADLESS'].nil?)
    guards.add_condition(:bidi, !ENV['WEBDRIVER_BIDI'].nil?)
    guards.add_condition(:rbe, GlobalTestEnv.rbe?)

    results = guards.disposition
    send(*results) if results
  end
end

WebDriver::Platform.exit_hook { GlobalTestEnv.quit }

$stdout.sync = true
