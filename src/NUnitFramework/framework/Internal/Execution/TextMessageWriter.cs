// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using System;
using System.Collections;
using NUnit.Framework.Constraints;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// TextMessageWriter writes constraint descriptions and messages
    /// in displayable form as a text stream. It tailors the display
    /// of individual message components to form the standard message
    /// format of NUnit assertion failure messages.
    /// </summary>
    public class TextMessageWriter : MessageWriter
    {
        #region Message Formats and Constants
        private static readonly int DEFAULT_LINE_LENGTH = 78;

        // Prefixes used in all failure messages. All must be the same
        // length, which is held in the PrefixLength field. Should not
        // contain any tabs or newline characters.
        /// <summary>
        /// Prefix used for the expected value line of a message
        /// </summary>
        public static readonly string Pfx_Expected = "  Expected: ";
        /// <summary>
        /// Prefix used for the actual value line of a message
        /// </summary>
        public static readonly string Pfx_Actual = "  But was:  ";
        /// <summary>
        /// Prefix used for the actual difference between actual and expected values if compared with a tolerance
        /// </summary>
        public static readonly string Pfx_Difference = "  Off by:   ";
        /// <summary>
        /// Length of a message prefix
        /// </summary>
        public static readonly int PrefixLength = Pfx_Expected.Length;

        #endregion

        #region Instance Fields
        private int _maxLineLength = DEFAULT_LINE_LENGTH;
        private bool _sameValDiffTypes = false;
        private string? _expectedType, _actualType;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct a TextMessageWriter
        /// </summary>
        public TextMessageWriter() { }

        /// <summary>
        /// Construct a TextMessageWriter, specifying a user message
        /// and optional formatting arguments.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <param name="args"></param>
        public TextMessageWriter(string? userMessage, params object?[]? args)
        {
            if (!string.IsNullOrEmpty(userMessage))
                WriteMessageLine(userMessage, args);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the maximum line length for this writer
        /// </summary>
        public override int MaxLineLength
        {
            get => _maxLineLength;
            set => _maxLineLength = value;
        }
        #endregion

        #region Public Methods - High Level
        /// <summary>
        /// Method to write single line message with optional args, usually
        /// written to precede the general failure message, at a given
        /// indentation level.
        /// </summary>
        /// <param name="level">The indentation level of the message</param>
        /// <param name="message">The message to be written</param>
        /// <param name="args">Any arguments used in formatting the message</param>
        public override void WriteMessageLine(int level, string message, params object?[]? args)
        {
            if (message is not null)
            {
                while (level-- >= 0) Write("  ");

                if (args is not null && args.Length > 0)
                    message = string.Format(message, args);

                WriteLine(MsgUtils.EscapeNullCharacters(message));
            }
        }

        /// <summary>
        /// Display Expected and Actual lines for a constraint. This
        /// is called by MessageWriter's default implementation of
        /// WriteMessageTo and provides the generic two-line display.
        /// </summary>
        /// <param name="result">The result of the constraint that failed</param>
        public override void DisplayDifferences(ConstraintResult result)
        {
            WriteExpectedLine(result);
            WriteActualLine(result);
            WriteAdditionalLine(result);
        }

        /// <summary>
        /// Gets the unique type name between expected and actual.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value causing the failure</param>
        /// <param name="expectedType">Output of the unique type name for expected</param>
        /// <param name="actualType">Output of the unique type name for actual</param>
        private void ResolveTypeNameDifference(object expected, object actual, out string expectedType, out string actualType)
        {
            TypeNameDifferenceResolver resolver = new TypeNameDifferenceResolver();
            resolver.ResolveTypeNameDifference(expected, actual, out expectedType, out actualType);

            expectedType = $" ({expectedType})";
            actualType = $" ({actualType})";
        }

        /// <summary>
        /// Display Expected and Actual lines for given values. This
        /// method may be called by constraints that need more control over
        /// the display of actual and expected values than is provided
        /// by the default implementation.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value causing the failure</param>
        public override void DisplayDifferences(object? expected, object? actual)
        {
            DisplayDifferences(expected, actual, null);
        }

        /// <summary>
        /// Display Expected and Actual lines for given values, including
        /// a tolerance value on the expected line.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value causing the failure</param>
        /// <param name="tolerance">The tolerance within which the test was made</param>
        public override void DisplayDifferences(object? expected, object? actual, Tolerance? tolerance)
        {
            if (expected is not null && actual is not null && expected.GetType() != actual.GetType() && MsgUtils.FormatValue(expected) == MsgUtils.FormatValue(actual))
            {
                _sameValDiffTypes = true;
                ResolveTypeNameDifference(expected, actual, out _expectedType, out _actualType);
            }
            WriteExpectedLine(expected, tolerance);
            WriteActualLine(actual);
            if (tolerance is not null)
            {
                WriteDifferenceLine(expected, actual, tolerance);
            }
        }

        /// <summary>
        /// Display the expected and actual string values on separate lines.
        /// If the mismatch parameter is >=0, an additional line is displayed
        /// line containing a caret that points to the mismatch point.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual string value</param>
        /// <param name="mismatch">The point at which the strings don't match or -1</param>
        /// <param name="ignoreCase">If true, case is ignored in string comparisons</param>
        /// <param name="clipping">If true, clip the strings to fit the max line length</param>
        public override void DisplayStringDifferences(string expected, string actual, int mismatch, bool ignoreCase, bool clipping)
        {
            // Maximum string we can display without truncating
            int maxDisplayLength = MaxLineLength
                - PrefixLength   // Allow for prefix
                - 2;             // 2 quotation marks

            if (clipping)
                MsgUtils.ClipExpectedAndActual(ref expected, ref actual, maxDisplayLength, mismatch);

            expected = MsgUtils.EscapeControlChars(expected);
            actual = MsgUtils.EscapeControlChars(actual);

            // The mismatch position may have changed due to clipping or white space conversion
            mismatch = MsgUtils.FindMismatchPosition(expected, actual, 0, ignoreCase);

            Write(Pfx_Expected);
            Write(MsgUtils.FormatValue(expected));
            if (ignoreCase)
                Write(", ignoring case");
            WriteLine();
            WriteActualLine(actual);
            //DisplayDifferences(expected, actual);
            if (mismatch >= 0)
                WriteCaretLine(mismatch);
        }
        #endregion

        #region Public Methods - Low Level

        /// <summary>
        /// Writes the text for an actual value.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        public override void WriteActualValue(object? actual)
        {
            WriteValue(actual);
        }

        /// <summary>
        /// Writes the text for a generalized value.
        /// </summary>
        /// <param name="val">The value.</param>
        public override void WriteValue(object? val)
        {
            Write(MsgUtils.FormatValue(val));
        }

        /// <summary>
        /// Writes the text for a collection value,
        /// starting at a particular point, to a max length
        /// </summary>
        /// <param name="collection">The collection containing elements to write.</param>
        /// <param name="start">The starting point of the elements to write</param>
        /// <param name="max">The maximum number of elements to write</param>
        public override void WriteCollectionElements(IEnumerable collection, long start, int max)
        {
            Write(MsgUtils.FormatCollection(collection, start, max));
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Write the generic 'Expected' line for a constraint
        /// </summary>
        /// <param name="result">The constraint that failed</param>
        private void WriteExpectedLine(ConstraintResult result)
        {
            Write(Pfx_Expected);
            WriteLine(result.Description);
        }

        /// <summary>
        /// Write the generic 'Expected' line for a given value
        /// and tolerance.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The tolerance within which the test was made</param>
        private void WriteExpectedLine(object? expected, Tolerance? tolerance)
        {
            Write(Pfx_Expected);
            Write(MsgUtils.FormatValue(expected));
            if (_sameValDiffTypes)
            {
                Write(_expectedType);
            }
            if (tolerance is not null && tolerance.HasVariance)
            {
                Write(" +/- ");
                Write(MsgUtils.FormatValue(tolerance.Amount));
                if (tolerance.Mode != ToleranceMode.Linear)
                    Write(" {0}", tolerance.Mode);
            }

            WriteLine();
        }

        /// <summary>
        /// Write the generic 'Actual' line for a constraint
        /// </summary>
        /// <param name="result">The ConstraintResult for which the actual value is to be written</param>
        private void WriteActualLine(ConstraintResult result)
        {
            Write(Pfx_Actual);
            result.WriteActualValueTo(this);
            WriteLine();
            //WriteLine(MsgUtils.FormatValue(result.ActualValue));
        }

        private void WriteAdditionalLine(ConstraintResult result)
        {
            result.WriteAdditionalLinesTo(this);
        }

        /// <summary>
        /// Write the generic 'Actual' line for a given value
        /// </summary>
        /// <param name="actual">The actual value causing a failure</param>
        private void WriteActualLine(object? actual)
        {
            Write(Pfx_Actual);
            WriteActualValue(actual);
            if (_sameValDiffTypes)
            {
                Write(_actualType);
            }
            WriteLine();
        }

        private void WriteDifferenceLine(object? expected, object? actual, Tolerance tolerance)
        {
            // It only makes sense to display absolute/percent difference
            if (tolerance.Mode != ToleranceMode.Linear && tolerance.Mode != ToleranceMode.Percent)
                return;

            string differenceString = tolerance.Amount is TimeSpan
                ? MsgUtils.FormatValue(DateTimes.Difference(expected, actual)) // TimeSpan tolerance applied in linear mode only
                : MsgUtils.FormatValue(Numerics.Difference(expected, actual, tolerance.Mode));

            if (differenceString != double.NaN.ToString())
            {
                Write(Pfx_Difference);
                Write(differenceString);
                if (tolerance.Mode != ToleranceMode.Linear)
                    Write(" {0}", tolerance.Mode);
                WriteLine();
            }
        }

        private void WriteCaretLine(int mismatch)
        {
            // We subtract 2 for the initial 2 blanks and add back 1 for the initial quote
            WriteLine("  {0}^", new string('-', PrefixLength + mismatch - 2 + 1));
        }
        #endregion
    }
}
