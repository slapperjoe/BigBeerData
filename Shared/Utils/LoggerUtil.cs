using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BigBeerData.Shared.Utils
{
	public static class LoggerUtil
	{
		public static void LogOutput(this StringBuilder resultString, ILogger log, string content)
		{
			resultString.Append(content);
			resultString.Append(Environment.NewLine);
			log.LogInformation("{content}", content);
		}

		public static void LogError(this StringBuilder resultString, ILogger log, Exception content)
		{
			resultString.Append(content.Message);
			resultString.Append(Environment.NewLine);
			log.LogError(content, "Error occured");
		}
	}
}
