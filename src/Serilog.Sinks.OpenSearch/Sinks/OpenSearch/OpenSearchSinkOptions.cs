// Copyright 2014 Serilog Contributors
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
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.OpenSearch
{
    /// <summary>
    /// Provides OpenSearchSink with configurable options
    /// </summary>
    public class OpenSearchSinkOptions
    {
        private int _queueSizeLimit;

        /// <summary>
        /// When set to true the sink will register an index template for the logs in OpenSearch.
        /// This template is optimized to deal with serilog events
        /// </summary>
        public bool AutoRegisterTemplate { get; set; }

        /// <summary>
        /// Specifies the option on how to handle failures when writing the template to OpenSearch. This is only applicable when using the AutoRegisterTemplate option.
        /// </summary>
        public RegisterTemplateRecovery RegisterTemplateFailure { get; set; }

        ///<summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature this allows you to override the default template name.
        /// Defaults to: serilog-events-template
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to override the default template content.
        /// If not provided, a default template that is optimized to deal with Serilog events is used.
        /// </summary>
        public Func<object> GetTemplateContent { get; set; }

        /// <summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to override the default template content.
        /// If not provided, a default template that is optimized to deal with Serilog events is used.
        /// </summary>
        public Dictionary<string,string> TemplateCustomSettings { get; set; }

        /// <summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to overwrite the template in OpenSearch if it already exists.
        /// Defaults to: false
        /// </summary>
        public bool OverwriteTemplate { get; set; }

        /// <summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to override the default number of shards.
        /// If not provided, this will default to the default number_of_shards configured in OpenSearch.
        /// </summary>
        public int? NumberOfShards { get; set; }

        /// <summary>
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to override the default number of replicas.
        /// If not provided, this will default to the default number_of_replicas configured in OpenSearch.
        /// </summary>
        public int? NumberOfReplicas { get; set; }

        /// <summary>
        /// Index aliases. Sets alias/aliases to an index in OpenSearch.
        /// TODO: To be tested with OpenSearch
        /// When using the <see cref="AutoRegisterTemplate"/> feature, this allows you to set index aliases.
        /// If not provided, index aliases will be blank.
        /// </summary>
        public string[] IndexAliases { get; set; }

        ///<summary>
        /// Connection configuration to use for connecting to the cluster.
        /// </summary>
        public Func<ConnectionConfiguration, ConnectionConfiguration> ModifyConnectionSettings { get; set; }

        ///<summary>
        /// The index name formatter. A string.Format using the DateTimeOffset of the event is run over this string.
        /// defaults to "logstash-{0:yyyy.MM.dd}"
        /// Needs to be lowercased.
        /// </summary>
        public string IndexFormat { get; set; }

        /// <summary>
        /// Optionally set this value to the name of the index that should be used when the template cannot be written to OpenSearch.
        /// defaults to "deadletter-{0:yyyy.MM.dd}"
        /// </summary>
        public string DeadLetterIndexName { get; set; }

        /// <summary>
        /// Configures the <see cref="OpType"/> being used when bulk indexing documents.
        /// In order to use data streams, this needs to be set to OpType.Create. 
        /// </summary>
        public OpenSearchOpType BatchAction { get; set; } = OpenSearchOpType.Index;

        /// <summary>
        /// Function to decide which Pipeline to use for the LogEvent
        /// </summary>
        public Func<LogEvent, string> PipelineNameDecider { get; set; }

        /// <summary>
        /// Name the Pipeline where log events are sent to sink. Please note that the Pipeline should be existing before the usage starts.
        /// </summary>
        public string PipelineName { get; set; }

        ///<summary>
        /// The maximum number of events to post in a single batch. Defaults to: 50.
        /// </summary>
        public int BatchPostingLimit { get; set; }

        ///<summary>
        /// The maximum length of a an event record to be sent. Defaults to: null (No Limit) only used in file buffer mode
        /// </summary>
        public long? SingleEventSizePostingLimit { get; set; }

        ///<summary>
        /// The time to wait between checking for event batches. Defaults to 2 seconds.
        /// </summary>
        public TimeSpan Period { get; set; }

        ///<summary>
        /// Supplies culture-specific formatting information, or null.
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        ///<summary>
        /// Allows you to override the connection used to communicate with OpenSearch.
        /// </summary>
        public IConnection Connection { get; set; }

        /// <summary>
        /// The connection timeout (in milliseconds) when sending bulk operations to OpenSearch (defaults to 5000).
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// When true fields will be written at the root of the json document.
        /// </summary>
        public bool InlineFields { get; set; }

        /// <summary>
        /// The minimum log event level required in order to write an event to the sink. Ignored when LoggingLevelSwitch is specified.
        /// </summary>
        public LogEventLevel? MinimumLogEventLevel { get; set; }

        /// <summary>
        /// A switch allowing the pass-through minimum level to be changed at runtime.
        /// </summary>
        public LoggingLevelSwitch LevelSwitch { get; set; }

        ///<summary>
        /// When passing a serializer unknown object will be serialized to object instead of relying on their ToString representation
        /// </summary>
        public IOpenSearchSerializer Serializer { get; set; }

        /// <summary>
        /// The connection pool describing the cluster to write event to
        /// </summary>
        public IConnectionPool ConnectionPool { get; private set; }

        /// <summary>
        /// Function to decide which index to write the LogEvent to, when using file see: BufferIndexDecider
        /// </summary>
        public Func<LogEvent, DateTimeOffset, string> IndexDecider { get; set; }

        /// <summary>
        /// Function to decide which index to write the LogEvent to when using file buffer
        /// Arguments is: logRow, DateTime of logfile
        /// </summary>
        public Func<string, DateTime, string> BufferIndexDecider { get; set; }
        /// <summary>
        /// Optional path to directory that can be used as a log shipping buffer for increasing the reliability of the log forwarding.
        /// </summary>
        public string BufferBaseFilename { get; set; }

        /// <summary>
        /// The maximum size, in bytes, to which the buffer log file for a specific date will be allowed to grow. By default 100L * 1024 * 1024 will be applied.
        /// </summary>
        public long? BufferFileSizeLimitBytes { get; set; }

        /// <summary>
        /// The interval between checking the buffer files.
        /// </summary>
        public TimeSpan? BufferLogShippingInterval { get; set; }

        /// <summary>
        /// An action to do when log row was denied by the OpenSearch because of the data (payload).
        /// The arguments is: The log row, status code from server, error message
        /// </summary>
        public Func<string, long?, string,string>  BufferCleanPayload { get; set; }

        /// <summary>
        /// A soft limit for the number of bytes to use for storing failed requests.  
        /// The limit is soft in that it can be exceeded by any single error payload, but in that case only that single error
        /// payload will be retained.
        /// </summary>
        public long? BufferRetainedInvalidPayloadsLimitBytes { get; set; }
        /// <summary>
        /// Customizes the formatter used when converting log events into OpenSearch documents. Please note that the formatter output must be valid JSON :)
        /// </summary>
        public ITextFormatter CustomFormatter { get; set; }

        /// <summary>
        /// Customizes the formatter used when converting log events into the durable sink. Please note that the formatter output must be valid JSON :)
        /// </summary>
        public ITextFormatter CustomDurableFormatter { get; set; }

        /// <summary>
        /// Specifies how failing emits should be handled.
        /// </summary>
        public EmitEventFailureHandling EmitEventFailure { get; set; }

        /// <summary>
        /// Sink to use when OpenSearch is unable to accept the events. This is optional and depends on the EmitEventFailure setting.
        /// </summary>
        public ILogEventSink FailureSink { get; set; }

        /// <summary>
        /// A callback which can be used to handle logevents which are not submitted to OpenSearch
        /// like when it is unable to accept the events. This is optional and depends on the EmitEventFailure setting.
        /// </summary>
        public Action<LogEvent> FailureCallback { get; set; }

        /// <summary>
        /// The maximum number of events that will be held in-memory while waiting to ship them to
        /// OpenSearch. Beyond this limit, events will be dropped. The default is 100,000. Has no effect on
        /// durable log shipping.
        /// </summary>
        public int QueueSizeLimit
        {
            get { return _queueSizeLimit; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(QueueSizeLimit), "Queue size limit must be non-zero.");
                _queueSizeLimit = value;
            }
        }
        /// <summary>
        /// The maximum number of log files that will be retained,
        /// including the current log file. For unlimited retention, pass null. The default is 31.
        /// </summary>
        public int? BufferFileCountLimit { get; set; }

        /// <summary>
        /// When set to true splits the StackTrace by new line and writes it as a an array of strings.
        /// </summary>
        public bool FormatStackTraceAsArray { get; set; }
        
        /// <summary>
        /// The interval at which buffer log files will roll over to a new file. The default is <see cref="RollingInterval.Day"/>.
        /// Less frequent intervals like <see cref="RollingInterval.Infinite"/>, <see cref="RollingInterval.Year"/>,
        /// <see cref="RollingInterval.Month"/> are not supported.
        /// </summary>
        public RollingInterval BufferFileRollingInterval { get; set; }

        /// <summary>
        /// Configures the OpenSearch sink defaults
        /// </summary>
        private OpenSearchSinkOptions()
        {
            IndexFormat = "logstash-{0:yyyy.MM.dd}";
            DeadLetterIndexName = "deadletter-{0:yyyy.MM.dd}";
            Period = TimeSpan.FromSeconds(2);
            BatchPostingLimit = 50;
            SingleEventSizePostingLimit = null;
            TemplateName = "serilog-events-template";
            ConnectionTimeout = TimeSpan.FromSeconds(5);
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog;
            RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway;
            QueueSizeLimit = 100000;
            BufferFileCountLimit = 31;
            BufferFileSizeLimitBytes = 100L * 1024 * 1024;
            FormatStackTraceAsArray = false;
            ConnectionPool = new SingleNodeConnectionPool(_defaultNode);
            BufferFileRollingInterval = RollingInterval.Day;
        }

        /// <summary>
        /// Configures the OpenSearch sink
        /// </summary>
        /// <param name="connectionPool">The connectionpool to use to write events to</param>
        public OpenSearchSinkOptions(IConnectionPool connectionPool)
            : this()
        {
            ConnectionPool = connectionPool;
        }

        /// <summary>
        /// Configures the OpenSearch sink
        /// </summary>
        /// <param name="nodes">The nodes to write to</param>
        public OpenSearchSinkOptions(IEnumerable<Uri> nodes)
            : this()
        {
            var materialized = nodes?.Where(n => n != null).ToArray();
            if (materialized == null || materialized.Length == 0)
                materialized = new[] { _defaultNode };
            if (materialized.Length == 1)
                ConnectionPool = new SingleNodeConnectionPool(materialized.First());
            else
                ConnectionPool = new StaticConnectionPool(materialized);
        }

        /// <summary>
        /// Configures the OpenSearch sink
        /// </summary>
        /// <param name="node">The node to write to</param>
        public OpenSearchSinkOptions(Uri node) : this(new[] { node }) { }

        private readonly Uri _defaultNode = new Uri("http://localhost:9200");
    }

    /// <summary>
    /// Sepecifies options for handling failures when emitting the events to OpenSearch. Can be a combination of options.
    /// </summary>
    [Flags]
    public enum EmitEventFailureHandling
    {
        /// <summary>
        /// Send the error to the SelfLog
        /// </summary>
        WriteToSelfLog = 1,

        /// <summary>
        /// Write the events to another sink. Make sure to configure this one.
        /// </summary>
        WriteToFailureSink = 2,

        /// <summary>
        /// Throw the exception to the caller.
        /// </summary>
        ThrowException = 4,

        /// <summary>
        /// The failure callback function will be called when the event cannot be submitted to OpenSearch.
        /// </summary>
        RaiseCallback = 8
    }

    /// <summary>
    /// Specifies what to do when the template could not be created. This can mean that your data is not correctly indexed, so you might want to handle this failure.
    /// </summary>
    public enum RegisterTemplateRecovery
    {
        /// <summary>
        /// Ignore the issue and keep indexing. This is the default option.
        /// </summary>
        IndexAnyway = 1,

        ///// <summary>
        ///// Keep buffering the data until it is written. be aware you might hit a limit here.                  
        ///// </summary>
        //BufferUntilSuccess = 2,

        /// <summary>
        /// When the template cannot be registered, move the events to the deadletter index instead.
        /// </summary>
        IndexToDeadletterIndex = 4,

        /// <summary>
        /// When the template cannot be registered, throw an exception and fail the sink.
        /// </summary>
        FailSink = 8
    }

    /// <summary>
    /// Collection of op_type:s that can be used when indexing documents
    /// https://opensearch.org/docs/1.0/opensearch/rest-api/document-apis/index-document/
    ///
    /// This is the same as the internal <seealso cref="OpType"/> but we don't want to
    /// expose it in the API.
    /// </summary>
    public enum OpenSearchOpType
    {
        /// <summary>
        /// Default option if a document ID is specified, creates or updates a document.
        /// </summary>
        [EnumMember(Value = "index")] Index,
        /// <summary>
        /// Default option if no document ID is specified. Creates a new index.
        /// - If a document with the specified _id already exists, the indexing operation will fail.
        /// - If the request targets a data stream, an op_type of create is required.
        /// </summary>
        [EnumMember(Value = "create")] Create
    }
}
