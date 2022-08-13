﻿using FluentAssertions;
using Xunit;

namespace Deeplex.Saverwalter.Model.Tests
{
    public class UmlageschluesselExtensionsTests
    {
        [Fact]
        public void ToDescriptionStringTest()
        {
            for (var i = 0; i < 5; ++i)
            {
                var stub = (Umlageschluessel)i;
                stub.ToDescriptionString().Should().NotBeNull();
            }
        }
    }
}