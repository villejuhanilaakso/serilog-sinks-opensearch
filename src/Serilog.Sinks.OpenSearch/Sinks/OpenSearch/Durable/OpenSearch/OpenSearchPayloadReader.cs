﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenSearch.Net;

namespace Serilog.Sinks.OpenSearch.Durable
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenSearchPayloadReader: APayloadReader<List<string>>
    {
        private readonly string _pipelineName;
        private readonly Func<object, string> _serialize;
        private readonly Func<string, DateTime,string> _getIndexForEvent;
        private readonly OpenSearchOpType _openSearchOpType;
        private readonly RollingInterval _rollingInterval;
        private List<string> _payload;
        private int _count;
        private DateTime _date;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipelineName"></param>
        /// <param name="serialize"></param>
        /// <param name="getIndexForEvent"></param>
        /// <param name="openSearchOpType"></param>
        /// <param name="rollingInterval"></param>
        public OpenSearchPayloadReader(string pipelineName, Func<object, string> serialize,
            Func<string, DateTime, string> getIndexForEvent, OpenSearchOpType openSearchOpType, RollingInterval rollingInterval)
        {
            if ((int)rollingInterval < (int)RollingInterval.Day)
            {
                throw new ArgumentException("Rolling intervals less frequent than RollingInterval.Day are not supported");
            }
            
            _pipelineName = pipelineName;
            _serialize = serialize;
            _getIndexForEvent = getIndexForEvent;
            _openSearchOpType = openSearchOpType;
            _rollingInterval = rollingInterval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> GetNoPayload()
        {
            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        protected override void InitPayLoad(string filename)
        {
            _payload = new List<string>();
            _count = 0;
            var lastToken = filename.Split('-').Last();

            // lastToken should be something like 20150218.json or 20150218_3.json now
            if (!lastToken.ToLowerInvariant().EndsWith(".json"))
            {
                throw new FormatException(string.Format("The file name '{0}' does not seem to follow the right file pattern - it must be named [whatever]-{{Date}}[_n].json", Path.GetFileName(filename)));
            }

            var dateFormat = _rollingInterval.GetFormat();
            var dateString = lastToken.Substring(0, dateFormat.Length);
            _date = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
        }
       /// <summary>
       /// 
       /// </summary>
       /// <returns></returns>
        protected override List<string> FinishPayLoad()
        {
            return _payload;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextLine"></param>
        protected override void AddToPayLoad(string nextLine)
        {
            var indexName = _getIndexForEvent(nextLine, _date);
            var action = OpenSearchSink.CreateOpenSearchAction(
                opType: _openSearchOpType, 
                indexName: indexName, pipelineName: _pipelineName,
                id: _count + "_" + Guid.NewGuid());
            var actionJson = LowLevelRequestResponseSerializer.Instance.SerializeToString(action);

            _payload.Add(actionJson);
            _payload.Add(nextLine);
            _count++;
        }
    }
}
