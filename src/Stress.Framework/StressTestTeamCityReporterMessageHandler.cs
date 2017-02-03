// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Reporters;

namespace Stress.Framework
{
    public class StressTestTeamCityReporterMessageHandler: TeamCityReporterMessageHandler
    {
        private Func<string, string> _escape;

        private Func<string, string> _toFlowId;

        private IRunnerLogger _logger;

        private string _statisticFolder;

        public StressTestTeamCityReporterMessageHandler(IRunnerLogger logger,
            Func<string, string> flowIdMapper = null,
            TeamCityDisplayNameFormatter displayNameFormatter = null) : base(logger, flowIdMapper, displayNameFormatter)
        {
            _logger = logger;

            // I know, I know, but I want them so bad and ITestProgressMessage and ITestStatisticsMessage won't
            // be written too often, so should not hurt repformance.

            var escapeMethodInfo = typeof(TeamCityReporterMessageHandler).GetMethod("Escape", BindingFlags.Static | BindingFlags.NonPublic);
            _escape = (s) => (string) escapeMethodInfo.Invoke(this, new[] {s});

            var toFlowIdMethodInfo = typeof(TeamCityReporterMessageHandler).GetMethod("ToFlowId", BindingFlags.Instance | BindingFlags.NonPublic);
            _toFlowId = (s) => (string)toFlowIdMethodInfo.Invoke(this, new[] { s });

            _statisticFolder = StressConfig.Instance.StatisticOutputFolder;
        }

        public override bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var more = true;
            if (message is ITestProgressMessage progressMessage)
            {
                more = Visit(progressMessage);
            }

            if (more && message is ITestSummaryStatisticsMessage summaryMessage)
            {
                more = Visit(summaryMessage);
            }

            if (more && message is ITestStatisticsMessage statisticsMessage)
            {
                more = Visit(statisticsMessage);
            }

            return more && base.OnMessageWithTypes(message, messageTypes);
        }

        protected virtual bool Visit(ITestProgressMessage message)
        {
            _logger.LogImportantMessage($"##teamcity[progressMessage '{_escape(message.Message)}' ]");
            return true;
        }

        protected virtual bool Visit(ITestStatisticsMessage message)
        {
            if (!string.IsNullOrEmpty(_statisticFolder))
            {
                var fullName = Path.Combine(_statisticFolder, GetKey(message));
                File.AppendAllText(fullName, Convert.ToString(message.Value, CultureInfo.InvariantCulture) + Environment.NewLine);
            }
            return true;
        }

        protected virtual bool Visit(ITestSummaryStatisticsMessage message)
        {
            _logger.LogImportantMessage($"##teamcity[buildStatisticValue key='{_escape(GetKey(message))}' value='{_escape(Convert.ToString(message.Value))}' flowId='{_toFlowId(message.TestCollection.DisplayName)}' ]");
            return true;
        }

        private static string GetKey(ITestStatisticsMessage message)
        {
            var key = message.TestCase.DisplayName + " " + message.Key;
            return Regex.Replace(key, "[^a-zA-Z0-9]+", "_");
        }
    }
}