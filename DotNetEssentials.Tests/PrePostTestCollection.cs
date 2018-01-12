using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotNetEssentials.Tests
{
	[CollectionDefinition("PrePostTestCollection")]
	public class PrePostTestCollection : ICollectionFixture<PrePostTestFixture>
	{
		// This class has no code, and is never created. Its purpose is simply
		// to be the place to apply [CollectionDefinition] and all the
		// ICollectionFixture<> interfaces.
	}
}
