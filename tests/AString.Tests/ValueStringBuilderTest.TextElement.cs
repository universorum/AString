#if NET8_0_OR_GREATER
using System.Globalization;

namespace Astra.Text.Tests;

public class ValueStringBuilderTestTextElement
{
    [Test]
    public async Task Enumerator()
    {
        var testString =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022";

        using var a = new ValueStringBuilder(testString);

        var aEnumerator   = a.EnumerateTextElement();
        var bclEnumerator = StringInfo.GetTextElementEnumerator(testString);

        while (true)
        {
            var next = bclEnumerator.MoveNext();
            await Assert.That(aEnumerator.MoveNext()).IsEqualTo(next);
            if (!next) { break; }

            await Assert.That(string.Create(aEnumerator.Current.Span)).IsEqualTo(bclEnumerator.Current as string);
        }

        aEnumerator.Reset();
        bclEnumerator.Reset();

        while (true)
        {
            var next = bclEnumerator.MoveNext();
            await Assert.That(aEnumerator.MoveNext()).IsEqualTo(next);
            if (!next) { break; }

            await Assert.That(string.Create(aEnumerator.Current.Span)).IsEqualTo(bclEnumerator.Current as string);
        }
    }
}
#endif