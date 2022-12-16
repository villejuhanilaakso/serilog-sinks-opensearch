# Serilog.Sinks.OpenSearch

This repository contains two nuget packages: `Serilog.Sinks.OpenSearch` and `Serilog.Formatting.OpenSearch`.

## Table of contents

* [What is this sink](#what-is-this-sink)
* [Features](#features)
* [Quick start](#quick-start)
  * [OpenSearch sinks](#opensearch-sinks)
  * [OpenSearch formatters](#opensearch-formatters)
* [More information](#more-information)
  * [A note about fields inside OpenSearch](#a-note-about-fields-inside-opensearch)
  * [A note about Kibana](#a-note-about-opensearch-dashboard)
  * [JSON `appsettings.json` configuration](#json-appsettingsjson-configuration)
  * [Handling errors](#handling-errors)
  * [Breaking changes](#breaking-changes)

## What is this sink

The Serilog OpenSearch sink project is a sink (basically a writer) for the Serilog logging framework. Structured log events are written to sinks and each sink is responsible for writing it to its own backend, database, store etc. This sink delivers the data to OpenSearch, a NoSQL search engine originally forked from Elastic Search. It does this in a similar structure as Logstash and makes it easy to use OpenSearch Dashboard for visualizing your logs. This sink is a fork of the [Serilog.Sinks.Elasticsearch](https://github.com/serilog-contrib/serilog-sinks-elasticsearch) project.

## Features

* Simple configuration to get log events published to OpenSearch. Only server address is needed.
* All properties are stored inside fields in OpenSearch. This allows you to query on all the relevant data but also run analytics over this data.
* Be able to customize the store; specify the index name being used, the serializer or the connections to the server (load balanced).
* Durable mode; store the logevents first on disk before delivering them to OpenSearch making sure you never miss events if you have trouble connecting to your OpenSearch cluster.
* Automatically create the right mappings for the best usage of the log events in OpenSearch or automatically upload your own custom mapping.
* Compatible with OpenSearch starting from version 2.0.


## Quick start

### OpenSearch sinks

To use this package currently, you will need to create a nuget package and install it locally.

Register the sink in code or using the appSettings reader (from v2.0.42+) as shown below. Make sure to specify the version of ES you are targeting. Be aware that the AutoRegisterTemplate option will not overwrite an existing template.

```csharp
var loggerConfig = new LoggerConfiguration()
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200") ){
             AutoRegisterTemplate = true
     });
```

This example shows the options that are currently available when using the appSettings reader.

```xml
  <appSettings>
    <add key="serilog:using" value="Serilog.Sinks.Elasticsearch"/>
    <add key="serilog:write-to:Elasticsearch.nodeUris" value="http://localhost:9200;http://remotehost:9200"/>
    <add key="serilog:write-to:Elasticsearch.indexFormat" value="custom-index-{0:yyyy.MM}"/>
    <add key="serilog:write-to:Elasticsearch.templateName" value="myCustomTemplate"/>    
    <add key="serilog:write-to:Elasticsearch.pipelineName" value="myCustomPipelineName"/>
    <add key="serilog:write-to:Elasticsearch.batchPostingLimit" value="50"/>
    <add key="serilog:write-to:Elasticsearch.batchAction" value="Create"/><!-- "Index" is default -->
    <add key="serilog:write-to:Elasticsearch.period" value="2"/>
    <add key="serilog:write-to:Elasticsearch.inlineFields" value="true"/>
    <add key="serilog:write-to:Elasticsearch.restrictedToMinimumLevel" value="Warning"/>
    <add key="serilog:write-to:Elasticsearch.bufferBaseFilename" value="C:\Temp\SerilogElasticBuffer"/>
    <add key="serilog:write-to:Elasticsearch.bufferFileSizeLimitBytes" value="5242880"/>
    <add key="serilog:write-to:Elasticsearch.bufferLogShippingInterval" value="5000"/>
    <add key="serilog:write-to:Elasticsearch.bufferRetainedInvalidPayloadsLimitBytes" value="5000"/>
    <add key="serilog:write-to:Elasticsearch.bufferFileCountLimit " value="31"/>
    <add key="serilog:write-to:Elasticsearch.connectionGlobalHeaders" value="Authorization=Bearer SOME-TOKEN;OtherHeader=OTHER-HEADER-VALUE" />
    <add key="serilog:write-to:Elasticsearch.connectionTimeout" value="5" />
    <add key="serilog:write-to:Elasticsearch.emitEventFailure" value="WriteToSelfLog" />
    <add key="serilog:write-to:Elasticsearch.queueSizeLimit" value="100000" />
    <add key="serilog:write-to:Elasticsearch.autoRegisterTemplate" value="true" />    
    <add key="serilog:write-to:Elasticsearch.overwriteTemplate" value="false" />
    <add key="serilog:write-to:Elasticsearch.registerTemplateFailure" value="IndexAnyway" />
    <add key="serilog:write-to:Elasticsearch.deadLetterIndexName" value="deadletter-{0:yyyy.MM}" />
    <add key="serilog:write-to:Elasticsearch.numberOfShards" value="20" />
    <add key="serilog:write-to:Elasticsearch.numberOfReplicas" value="10" />
    <add key="serilog:write-to:Elasticsearch.formatProvider" value="My.Namespace.MyFormatProvider, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.connection" value="My.Namespace.MyConnection, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.serializer" value="My.Namespace.MySerializer, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.connectionPool" value="My.Namespace.MyConnectionPool, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.customFormatter" value="My.Namespace.MyCustomFormatter, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.customDurableFormatter" value="My.Namespace.MyCustomDurableFormatter, My.Assembly.Name" />
    <add key="serilog:write-to:Elasticsearch.failureSink" value="My.Namespace.MyFailureSink, My.Assembly.Name" />
  </appSettings>
```

With the appSettings configuration the `nodeUris` property is required. Multiple nodes can be specified using `,` or `;` to separate them. All other properties are optional. Also required is the `<add key="serilog:using" value="Serilog.Sinks.OpenSearch"/>` setting to include this sink. All other properties are optional. If you do not explicitly specify an indexFormat-setting, a generic index such as 'logstash-[current_date]' will be used automatically.

And start writing your events using Serilog.

### OpenSearch formatters

To use this package currently, you will need to create a nuget package and install it locally.

The `Serilog.Formatting.OpenSearch` nuget package consists of a several formatters:

* `OpenSearchJsonFormatter` - custom json formatter that respects the configured property name handling and forces `Timestamp` to @timestamp.
* `ExceptionAsObjectJsonFormatter` - a json formatter which serializes any exception into an exception object.

Override default formatter if it's possible with selected sink

```csharp
var loggerConfig = new LoggerConfiguration()
  .WriteTo.Console(new OpenSearchJsonFormatter());
```

## More information

* Report issues to the [issue tracker](https://github.com/villejuhanilaakso/serilog-sinks-opensearch/issues). PR welcome, but please do this against the dev branch.
* For an overview of recent changes, have a look at the [change log](https://github.com/villejuhanilaakso/serilog-sinks-opensearch/blob/master/CHANGES.md).

### A note about fields inside OpenSearch

Be aware that there is an explicit and implicit mapping of types inside an OpenSearch index. A value called `X` as a string will be indexed as being a string. Sending the same `X` as an integer in a next log message will not work. OpenSearch will raise a mapping exception, however it is not that evident that your log item was not stored due to the bulk actions performed.

So be careful about defining and using your fields (and type of fields). It is easy to miss that you first send a {User} as a simple username (string) and next as a User object. The first mapping dynamically created in the index wins. See also issue [#184](https://github.com/serilog/serilog-sinks-elasticsearch/issues/184) for details and a possible solution. There are also limits in OpenSearch on the number of dynamic fields you can actually throw inside an index.

### A note about OpenSearch Dashboard

In order to avoid a potentially deeply nested JSON structure for exceptions with inner exceptions,
by default the logged exception and it's inner exception is logged as an array of exceptions in the field `exceptions`. Use the 'Depth' field to traverse the inner exceptions flow.

However, not all features in OpenSearch work just as well with JSON arrays - for instance, including
exception fields on dashboards and visualizations. Therefore, we provide an alternative formatter,  `ExceptionAsObjectJsonFormatter`, which will serialize the exception into the `exception` field as an object with nested `InnerException` properties. This was also the default behavior of the sink before version 2.

To use it, simply specify it as the `CustomFormatter` when creating the sink:

```csharp
    new OpenSearchSink(new OpenSearchSinkOptions(url)
    {
      CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage:true)
    });
```

### JSON `appsettings.json` configuration

To use the OpenSearch sink with _Microsoft.Extensions.Configuration_, for example with ASP.NET Core or .NET Core, use the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package. First install that package if you have not already done so:

```powershell
Install-Package Serilog.Settings.Configuration
```

Instead of configuring the sink directly in code, call `ReadFrom.Configuration()`:

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

In your `appsettings.json` file, under the `Serilog` node, :

```json
{
  "Serilog": {
    "WriteTo": [{
        "Name": "OpenSearch",
        "Args": {
          "nodeUris": "http://localhost:9200;http://remotehost:9200/",
          "indexFormat": "custom-index-{0:yyyy.MM}",
          "templateName": "myCustomTemplate",          
          "pipelineName": "myCustomPipelineName",
          "batchPostingLimit": 50,
          "batchAction": "Create",
          "period": 2,
          "inlineFields": true,
          "restrictedToMinimumLevel": "Warning",
          "bufferBaseFilename":  "C:/Temp/docker-elk-serilog-web-buffer",
          "bufferFileSizeLimitBytes": 5242880,
          "bufferLogShippingInterval": 5000,
          "bufferRetainedInvalidPayloadsLimitBytes": 5000,
          "bufferFileCountLimit": 31,
          "connectionGlobalHeaders" :"Authorization=Bearer SOME-TOKEN;OtherHeader=OTHER-HEADER-VALUE",
          "connectionTimeout": 5,
          "emitEventFailure": "WriteToSelfLog",
          "queueSizeLimit": "100000",
          "autoRegisterTemplate": true,
          "overwriteTemplate": false,
          "registerTemplateFailure": "IndexAnyway",
          "deadLetterIndexName": "deadletter-{0:yyyy.MM}",
          "numberOfShards": 20,
          "numberOfReplicas": 10,
          "templateCustomSettings": [{ "index.mapping.total_fields.limit": "10000000" } ],
          "formatProvider": "My.Namespace.MyFormatProvider, My.Assembly.Name",
          "connection": "My.Namespace.MyConnection, My.Assembly.Name",
          "serializer": "My.Namespace.MySerializer, My.Assembly.Name",
          "connectionPool": "My.Namespace.MyConnectionPool, My.Assembly.Name",
          "customFormatter": "My.Namespace.MyCustomFormatter, My.Assembly.Name",
          "customDurableFormatter": "My.Namespace.MyCustomDurableFormatter, My.Assembly.Name",
          "failureSink": "My.Namespace.MyFailureSink, My.Assembly.Name"
        }
    }]
  }
}
```

See the XML `<appSettings>` example above for a discussion of available `Args` options.

### Handling errors

You have the option to specify how to handle issues with OpenSearch. Since the sink delivers in a batch, it might be possible that one or more events could actually not be stored in the OpenSearch store.
Can be a mapping issue for example. It is hard to find out what happened here. There is a new option called *EmitEventFailure* which is an enum (flagged) with the following options:

* WriteToSelfLog, the default option in which the errors are written to the SelfLog.
* WriteToFailureSink, the failed events are send to another sink. Make sure to configure this one by setting the FailureSink option.
* ThrowException, in which an exception is raised.
* RaiseCallback, the failure callback function will be called when the event cannot be submitted to OpenSearch. Make sure to set the FailureCallback option to handle the event.

An example:

```csharp
.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)
                })
```

With the AutoRegisterTemplate option the sink will write a default template to OpenSearch. When this template is not there, you might not want to index as it can influence the data quality.
You can use the RegisterTemplateFailure option. Set it to one of the following options:

* IndexAnyway; the default option, the events will be send to the server
* IndexToDeadletterIndex; using the deadletterindex format, it will write the events to the deadletter queue. When you fix your template mapping, you can copy your data into the right index.
* FailSink; this will simply fail the sink by raising an exception.

You can also specify an action to do when log row was denied by the OpenSearch because of the data (payload) if durable file is specied.
i.e.

```csharp
BufferCleanPayload = (failingEvent, statuscode, exception) =>
                    {
                        dynamic e = JObject.Parse(failingEvent);
                        return JsonConvert.SerializeObject(new Dictionary<string, object>()
                        {
                            { "@timestamp",e["@timestamp"]},
                            { "level","Error"},
                            { "message","Error: "+e.message},
                            { "messageTemplate",e.messageTemplate},
                            { "failingStatusCode", statuscode},
                            { "failingException", exception}
                        });
                    },
```

The IndexDecider didnt worked well when durable file was specified so an option to specify BufferIndexDecider is added.
Datatype of logEvent is string
i.e.

```csharp
 BufferIndexDecider = (logEvent, offset) => "log-serilog-" + (new Random().Next(0, 2)),
```

Option BufferFileCountLimit is added. The maximum number of log files that will be retained. including the current log file. For unlimited retention, pass null. The default is 31.
Option BufferFileSizeLimitBytes is added The maximum size, in bytes, to which the buffer log file for a specific date will be allowed to grow. By default `100L * 1024 * 1024` will be applied.