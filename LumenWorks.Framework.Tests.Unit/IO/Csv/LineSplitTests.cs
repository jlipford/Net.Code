using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using NUnit.Framework;

namespace LumenWorks.Framework.Tests.Unit.IO.Csv
{
	[TestFixture]
	public class LineSplitTests
	{

		[Test]
		public void SplitsSimpleDelimitedLine()
		{
			var splitLineParams = new CsvLayout('"', ';');
			var result = "1;2;3".Split(splitLineParams);
			CollectionAssert.AreEqual(new[]{"1", "2", "3"}, result);
		}

		[Test]
		public void TrimsTrailingWhitespaceOfUnquotedField()
		{
			var splitLineParams = new CsvLayout('"', ';');
			var result = "1;2;3 \t".Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result);
		}

		[Test]
		public void DoesNotTrimTrailingWhitespaceOfQuotedField()
		{
			var splitLineParams = new CsvLayout('"', ';');
			var result = "1;2;\"3 \t\"".Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "1", "2", "3 \t" }, result);
		}

		[Test]
		public void StripsQuotes()
		{
			const string line = @"""FieldContent""";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[]{"FieldContent"}, result);
		}

		[Test]
		public void TrimsLeadingWhitespaceFromUnquotedField()
		{
			const string line = @"x, y,z";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "x", "y", "z" }, result);
		}

		[Test]
		public void TrimsTrailingWhitespaceFromUnquotedField()
		{
			const string line = @"x,y   ,z";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "x", "y", "z" }, result);
		}

		[Test]
		public void SupportsFieldsWithEscapedQuotes()
		{
			const string line = "x \"y\",z";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "x \"y\"", "z" }, result);
		}

		[Test]
		public void EmptyFields()
		{
			const string line = @",x,,y";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "", "x", "", "y" }, result);
		}

		[Test]
		public void QuotedStringWithDelimiter()
		{
			const string line = "\"x \"\"y\"\", z\",u";
			var splitLineParams = new CsvLayout('"', ',');
			var result = line.Split(splitLineParams);
			CollectionAssert.AreEqual(new[] { "x \"y\", z", "u" }, result);

		}

		[Test]
		public void WhenValueTrimmingIsNone_LastFieldWithLeadingAndTrailingWhitespace_WhitespaceIsNotTrimmed()
		{
			const string data = "x,y, z ";
			var splitLineParams = new CsvLayout('"', ',', '"');
            var result = data.Split(splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
			CollectionAssert.AreEqual(new[]{@"x", "y", " z "}, result);

		}

		[Test]
		public void CarriageReturnCanBeUsedAsDelimiter()
		{
			const string data = "1\r2\n";
			var splitLineParams = new CsvLayout('"', '\r');
			var result = data.Split(splitLineParams);
			CollectionAssert.AreEqual(new[]{"1", "2"}, result);
		}

		[Test]
		public void EscapeCharacterInsideQuotedStringIsEscaped()
		{
			const string data = "\"\\\\\"";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = data.Split(splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
			Assert.AreEqual("\\", result.Single());
		}

		[Test]
		public void LineWithOnlySeparatorIsSplitIntoTwoEmptyStrings()
		{
			const string data = ",";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = data.Split(splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
			CollectionAssert.AreEqual(new[]{"",""}, result);
		}

		[Test]
		public void CanWorkWithMultilineField()
		{
			const string data = @"a,b,""line1
line2""";
			var splitLineParams = new CsvLayout('"', ',', '\\');
            var result = data.Split(splitLineParams, new CsvBehaviour(ValueTrimmingOptions.None));
			CollectionAssert.AreEqual(new[] { "a", "b", @"line1
line2" }, result);
		}

	}
}
