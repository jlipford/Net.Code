using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using NUnit.Framework;

namespace LumenWorks.Framework.Tests.Unit.IO.Csv
{
	[TestFixture]
	public class BufferSplitTests
	{
        private static IEnumerable<string> Split(string line, CsvLayout splitLineParams)
        {
            return Split(line, splitLineParams, new CsvBehaviour());
        }

        private static IEnumerable<string> Split(string line, CsvLayout splitLineParams, CsvBehaviour behaviour)
        {
            var splitter = new BufferSplit(new StringReader(line), splitLineParams, behaviour);
            var result = splitter.Split();
            return result.First();
        }
        
		[Test]
		public void SplitsSimpleDelimitedLine()
		{
            var splitLineParams = new CsvLayout('"', ';');
			var result = Split("1;2;3", splitLineParams);
			CollectionAssert.AreEqual(new[]{"1", "2", "3"}, result);
		}

		[Test]
		public void TrimsTrailingWhitespaceOfUnquotedField()
		{
			var splitLineParams = new CsvLayout('"', ';');
			var result = Split("1;2;3 \t", splitLineParams);
			CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result);
		}

		[Test]
		public void DoesNotTrimTrailingWhitespaceOfQuotedField()
		{
			var splitLineParams = new CsvLayout('"', ';');
            var result = Split("1;2;\"3 \t\"", splitLineParams);
			CollectionAssert.AreEqual(new[] { "1", "2", "3 \t" }, result);
		}

		[Test]
		public void StripsQuotes()
		{
			const string line = @"""FieldContent""";
			var splitLineParams = new CsvLayout('"', ',');
			var result = Split(line, splitLineParams);
			CollectionAssert.AreEqual(new[]{"FieldContent"}, result);
		}

	    [Test]
		public void TrimsLeadingWhitespaceFromUnquotedField()
		{
			const string line = @"x, y,z";
			var splitLineParams = new CsvLayout('"', ',');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "x", "y", "z" }, result);
		}

		[Test]
		public void TrimsTrailingWhitespaceFromUnquotedField()
		{
			const string line = @"x,y   ,z";
			var splitLineParams = new CsvLayout('"', ',');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "x", "y", "z" }, result);
		}

		[Test]
		public void SupportsFieldsWithEscapedQuotes()
		{
			const string line = "x \"y\",z";
			var splitLineParams = new CsvLayout('"', ',');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "x \"y\"", "z" }, result);
		}

		[Test]
		public void EmptyFields()
		{
			const string line = @",x,,y";
			var splitLineParams = new CsvLayout('"', ',');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "", "x", "", "y" }, result);
		}

		[Test]
		public void QuotedStringWithDelimiter()
		{
            // "x ""y"", z"
			const string line = "\"x \"y\" z, u\",v";
			var splitLineParams = new CsvLayout('"', ',');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "x \"y\" z, u", "v" }, result);

		}

		[Test]
		public void WhenValueTrimmingIsNone_LastFieldWithLeadingAndTrailingWhitespace_WhitespaceIsNotTrimmed()
		{
			const string line = "x,y, z ";
			var splitLineParams = new CsvLayout('"', ',', '"');
            var result = Split(line, splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
            CollectionAssert.AreEqual(new[] { @"x", "y", " z " }, result);

		}

		[Test]
		public void CarriageReturnCanBeUsedAsDelimiter()
		{
			const string line = "1\r2\n";
			var splitLineParams = new CsvLayout('"', '\r');
            var result = Split(line, splitLineParams);
            CollectionAssert.AreEqual(new[] { "1", "2" }, result);
		}

		[Test]
		public void EscapeCharacterInsideQuotedStringIsEscaped()
		{
			const string line = "\"\\\\\"";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = Split(line, splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
            Assert.AreEqual("\\", result.Single());
		}

		[Test]
		public void LineWithOnlySeparatorIsSplitIntoTwoEmptyStrings()
		{
			const string line = ",";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = Split(line, splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
            CollectionAssert.AreEqual(new[] { "", "" }, result);
		}

		[Test]
		public void CanWorkWithMultilineField()
		{
			const string data = @"a,b,""line1
line2""";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = Split(data, splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
            CollectionAssert.AreEqual(new[] { "a", "b", @"line1
line2" }, result);
		}

        [Test]
        public void MultipleLinesAreSplitCorrectly()
        {
            var data1 = @"1;2;3
4;5;6";

            var csvLayout = new CsvLayout('\"', ';');

            var splitter = new BufferSplit(new StringReader(data1), csvLayout, new CsvBehaviour());

            var result = splitter.Split().ToArray();

            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result[0]);
            CollectionAssert.AreEqual(new[] { "4", "5", "6" }, result[1]);

        }

        [Test]
        public void WorksWithQuotedStringInsideQuotedFieldButOnlyWhitespaceAfterSecondQuote()
        {
            var data1 = @"""1"";"" 2  ""inside""   "";3";

            var csvLayout = new CsvLayout('\"', ';');

            var splitter = new BufferSplit(new StringReader(data1), csvLayout, new CsvBehaviour());

            var result = splitter.Split().ToArray();

            CollectionAssert.AreEqual(new[] { "1", @" 2  ""inside""   ", "3" }, result[0]);

        }

        [Test]
        public void WorksWithQuotedStringInsideQuotedField()
        {
            var data1 = @"""1"";"" 2  ""inside""  x "";3";

            var csvLayout = new CsvLayout('\"', ';');

            var splitter = new BufferSplit(new StringReader(data1), csvLayout, new CsvBehaviour());

            var result = splitter.Split().ToArray();

            CollectionAssert.AreEqual(new[] { "1", @" 2  ""inside""  x ", "3" }, result[0]);

        }

        [Test]
        public void WorksWithQuotedMultilineString()
        {
            var data1 = @"""1"";"" 2  ""in
side""  x "";3";

            var csvLayout = new CsvLayout('\"', ';');

            var splitter = new BufferSplit(new StringReader(data1), csvLayout, new CsvBehaviour());

            var result = splitter.Split().ToArray();

            CollectionAssert.AreEqual(new[] { "1", @" 2  ""in
side""  x ", "3" }, result[0]);

        }

        [Test]
        public void SampleDataSplitTest()
        {
            var data = CsvReaderSampleData.SampleData1;

            var splitter = new BufferSplit(new StringReader(data), new CsvLayout(), new CsvBehaviour());

            var result = splitter.Split().ToArray();

            CsvReaderSampleData.CheckSampleData1(false, 0, result[0]);

        }



	}
}
