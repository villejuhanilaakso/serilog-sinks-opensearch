﻿// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Serilog.Core;
using Serilog.Events;


namespace Serilog.Sinks.OpenSearch.Durable
{
    class DurableOpenSearchSink : ILogEventSink, IDisposable
    {
        // we rely on the date in the filename later!
        const string FileNameSuffix = "-.json";

        readonly Logger _sink;
        readonly LogShipper<List<string>> _shipper;
        readonly OpenSearchSinkState _state;

        public DurableOpenSearchSink(OpenSearchSinkOptions options)
        {
            _state = OpenSearchSinkState.Create(options);

            if (string.IsNullOrWhiteSpace(options.BufferBaseFilename))
            {
                throw new ArgumentException("Cannot create the durable OpenSearch sink without a buffer base file name!");
            }

            _sink = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(_state.DurableFormatter,
                    options.BufferBaseFilename + FileNameSuffix,
                    rollingInterval: options.BufferFileRollingInterval,
                    fileSizeLimitBytes: options.BufferFileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: options.BufferFileCountLimit,
                    levelSwitch: _state.Options.LevelSwitch,
                    encoding: Encoding.UTF8)
                .CreateLogger();
            
            var openSearchLogClient = new OpenSearchLogClient(
                openSearchLowLevelClient: _state.Client, 
                cleanPayload: _state.Options.BufferCleanPayload,
                openSearchOpType: _state.Options.BatchAction);

            var payloadReader = new OpenSearchPayloadReader(
                 pipelineName: _state.Options.PipelineName,  
                 serialize:_state.Serialize,  
                 getIndexForEvent: _state.GetBufferedIndexForEvent,
                 openSearchOpType: _state.Options.BatchAction,
                 rollingInterval: options.BufferFileRollingInterval);

            _shipper = new OpenSearchLogShipper(
                bufferBaseFilename: _state.Options.BufferBaseFilename,
                batchPostingLimit: _state.Options.BatchPostingLimit,
                period: _state.Options.BufferLogShippingInterval ?? TimeSpan.FromSeconds(5),
                eventBodyLimitBytes: _state.Options.SingleEventSizePostingLimit,
                levelControlSwitch: _state.Options.LevelSwitch,
                logClient: openSearchLogClient,
                payloadReader: payloadReader,
                retainedInvalidPayloadsLimitBytes: _state.Options.BufferRetainedInvalidPayloadsLimitBytes,
                bufferSizeLimitBytes: _state.Options.BufferFileSizeLimitBytes,
                registerTemplateIfNeeded: _state.RegisterTemplateIfNeeded,
                rollingInterval: options.BufferFileRollingInterval);
                
        }

        public void Emit(LogEvent logEvent)
        {
            // This is a lagging indicator, but the network bandwidth usage benefits
            // are worth the ambiguity.
            if (_shipper.IsIncluded(logEvent))
            {
                _sink.Write(logEvent);
            }
        }

        public void Dispose()
        {
            _sink.Dispose();
            _shipper.Dispose();
        }
    }
}