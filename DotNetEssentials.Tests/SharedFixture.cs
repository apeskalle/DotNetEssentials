using DotNetEssentials.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetEssentials.Tests
{
	public class SharedFixture : IDisposable
	{
		public SharedFixture()
		{
			// Initialize tests...

			Logger.SetMinimumLevel(LogLevel.Debug);
			Logger.SetTypes(LogMode.Debug);
		}

		public void Dispose()
		{
			// Cleanup tests...
		}
	}
}
