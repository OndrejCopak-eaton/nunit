// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework.Constraints;

namespace NUnit.Framework.Tests.Constraints
{
    [TestFixture]
    public class LessThanOrEqualConstraintTests : ComparisonConstraintTestBase
    {
        protected override Constraint TheConstraint { get; } = new LessThanOrEqualConstraint(5);

        [SetUp]
        public void SetUp()
        {
            ExpectedDescription = "less than or equal to 5";
            StringRepresentation = "<lessthanorequal 5>";
        }

#pragma warning disable IDE0052 // Remove unread private members
        private static readonly object[] SuccessData = new object[] { 4, 5 };
        private static readonly object[] FailureData = new object[] { new object[] { 6, "6" } };
#pragma warning restore IDE0052 // Remove unread private members

        [Test]
        public void CanCompareIComparables()
        {
            ClassWithIComparable expected = new ClassWithIComparable(42);
            ClassWithIComparable actual = new ClassWithIComparable(0);
            Assert.That(actual, Is.LessThanOrEqualTo(expected));
        }

        [Test]
        public void CanCompareIComparablesOfT()
        {
            ClassWithIComparableOfT expected = new ClassWithIComparableOfT(42);
            ClassWithIComparableOfT actual = new ClassWithIComparableOfT(0);
            Assert.That(actual, Is.LessThanOrEqualTo(expected));
        }

        [Test]
        public void CanCompareIComparablesOfInt()
        {
            int expected = 42;
            ClassWithIComparableOfT actual = new ClassWithIComparableOfT(0);
            Assert.That(actual, Is.LessThanOrEqualTo(expected));
        }

        [TestCase(4.0, 5.0, 0.05)]
        [TestCase(4.95, 5.0, 0.05)] // lower range bound
        [TestCase(4.9501, 5.0, 0.05)] // lower range bound + .01
        [TestCase(4.9999, 5.0, 0.05)]
        [TestCase(5.0001, 5.0, 0.05)]
        [TestCase(5.05, 5.0, 0.05)] // upper range bound
        [TestCase(190, 200, 5)]
        [TestCase(195, 200, 5)] // lower range bound
        [TestCase(196, 200, 5)] // lower range bound + 1
        [TestCase(198, 200, 5)]
        [TestCase(202, 200, 5)]
        [TestCase(205, 200, 5)] // upper range bound
        public void SimpleTolerance(object actual, object expected, object tolerance)
        {
#pragma warning disable NUnit2042 // Comparison constraint on object
            Assert.That(actual, Is.LessThanOrEqualTo(expected).Within(tolerance));
#pragma warning restore NUnit2042 // Comparison constraint on object
        }

        [TestCase(6.0, 5.0, 0.05)]
        [TestCase(210, 200, 5)]
        public void SimpleTolerance_Failure(object actual, object expected, object tolerance)
        {
#pragma warning disable NUnit2042 // Comparison constraint on object
            var ex = Assert.Throws<AssertionException>(
                () => Assert.That(actual, Is.LessThanOrEqualTo(expected).Within(tolerance)),
                "Assertion should have failed");
#pragma warning restore NUnit2042 // Comparison constraint on object

            Assert.That(ex?.Message, Contains.Substring("Expected: less than or equal to " + expected));
        }

        [TestCase(4.0, 5.0, 1)]
        [TestCase(4.95, 5.0, 1)] // lower range bound
        [TestCase(4.9501, 5.0, 1)] // lower range bound + .01
        [TestCase(4.9999, 5.0, 1)]
        [TestCase(5.0001, 5.0, 1)]
        [TestCase(5.05, 5.0, 1)] // upper range bound
        [TestCase(190, 200, 2.5)]
        [TestCase(195, 200, 2.5)] // lower range bound
        [TestCase(196, 200, 2.5)] // lower range bound + 1
        [TestCase(198, 200, 2.5)]
        [TestCase(202, 200, 2.5)]
        [TestCase(205, 200, 2.5)] // upper range bound
        public void PercentTolerance(object actual, object expected, object tolerance)
        {
#pragma warning disable NUnit2042 // Comparison constraint on object
            Assert.That(actual, Is.LessThanOrEqualTo(expected).Within(tolerance).Percent);
#pragma warning restore NUnit2042 // Comparison constraint on object
        }

        [TestCase(6.0, 5.0, 1)]
        [TestCase(210, 200, 2.5)]
        public void PercentTolerance_Failure(object actual, object expected, object tolerance)
        {
#pragma warning disable NUnit2042 // Comparison constraint on object
            var ex = Assert.Throws<AssertionException>(
                () => Assert.That(actual, Is.LessThanOrEqualTo(expected).Within(tolerance).Percent),
                "Assertion should have failed");
#pragma warning restore NUnit2042 // Comparison constraint on object

            Assert.That(ex?.Message, Contains.Substring("Expected: less than or equal to " + MsgUtils.FormatValue(expected) + " within " + MsgUtils.FormatValue(tolerance) + " percent"));
        }
    }
}
