// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework.Interfaces;

namespace NUnit.Framework.Internal.Filters
{
    public class PartitionFilterTests : TestFilterTests
    {
        private PartitionFilter _filter;
        private ITest _testMatchingPartition;
        private ITest _testNotMatchingPartition;

        [SetUp]
        public void CreateFilter()
        {
            // Configure a new PartitionFilter with the provided partition count and number
            _filter = new PartitionFilter(7, 10);

            _testMatchingPartition = _fixtureWithMultipleTests.Tests[1];
            _testNotMatchingPartition = _fixtureWithMultipleTests.Tests[0];
        }

        [Test]
        public void IsNotEmpty()
        {
            Assert.That(_filter.IsEmpty, Is.False);
        }

        [Test]
        public void MatchTest()
        {
            // Validate
            Assert.That(_filter.ComputePartitionNumber(_testMatchingPartition), Is.EqualTo(7));
            Assert.That(_filter.ComputePartitionNumber(_testNotMatchingPartition), Is.EqualTo(8));

            // Assert
            Assert.That(_filter.Match(_testMatchingPartition), Is.True);
            Assert.That(_filter.Match(_testNotMatchingPartition), Is.False);
        }

        [Test]
        public void PassTest()
        {
            // This test fixture contains both one matching and one non-matching test
            // The fixture should therefore pass as True because one of the child tests are a match
            Assert.That(_filter.Pass(_fixtureWithMultipleTests), Is.True);

            // Validate that our matching and non-matching tests return the correct Pass result
            Assert.That(_filter.Pass(_testMatchingPartition), Is.True);
            Assert.That(_filter.Pass(_testNotMatchingPartition), Is.False);

            // This other test fixture has no matching tests for this partition number
            Assert.That(_filter.Pass(_specialFixture), Is.False);
        }

        [Test]
        public void ExplicitMatchTest()
        {
            // Top level TestFixture should always Pass
            Assert.That(_filter.IsExplicitMatch(_fixtureWithMultipleTests));

            // Assert
            Assert.That(_filter.IsExplicitMatch(_testMatchingPartition), Is.True);
            Assert.That(_filter.IsExplicitMatch(_testNotMatchingPartition), Is.False);
        }

        [Test]
        public void FromXml()
        {
            TestFilter filter = TestFilter.FromXml(@"<filter><partition>7/10</partition></filter>");

            Assert.That(filter, Is.TypeOf<PartitionFilter>());

            var partitionFilter = (PartitionFilter)filter;
            Assert.That(partitionFilter.PartitionNumber, Is.EqualTo(7));
            Assert.That(partitionFilter.PartitionCount, Is.EqualTo(10));
        }

        [Test]
        public void ToXml()
        {
            Assert.That(_filter.ToXml(false).OuterXml, Is.EqualTo(@"<partition>7/10</partition>"));
        }
    }
}