using System.Text;

namespace Astra.Text.Tests;

public class ValueStringBuilderTestReplace
{
    [Test]
    public async Task ReplaceCharTest()
    {
        var s = new string(' ', 10);
        using (var zsb = new ValueStringBuilder())
        {
            zsb.Append(s);
            zsb.Replace(' ', '-', 2, 5);
            await Assert.That(zsb.ToString()).IsEqualTo(new StringBuilder(s).Replace(' ', '-', 2, 5).ToString());
        }

        s = "0";
        using (var zsb = new ValueStringBuilder())
        {
            zsb.Append(s);
            zsb.Replace('0', '1');
            await Assert.That(zsb.ToString()).IsEqualTo(new StringBuilder(s).Replace('0', '1').ToString());
        }
    }

    [Test]
    public async Task ReplaceStringTest()
    {
        using (var zsb = new ValueStringBuilder())
        {
            var text = "bra bra BRA bra bra";
            zsb.Append(text);
            var bcl = new StringBuilder(text);

            zsb.Replace("bra", null, 1, text.Length - 2);
            bcl.Replace("bra", null, 1, text.Length - 2);

            //  "bra BRA bra"
            await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
        }

        using (var zsb = new ValueStringBuilder())
        {
            var text = "num num num";
            zsb.Append(text);
            var bcl = new StringBuilder(text);

            // over DefaultBufferSize
            zsb.Replace("num", new string('1', 32768), 1, text.Length - 2);
            bcl.Replace("num", new string('1', 32768), 1, text.Length - 2);

            await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
        }

        using (var zsb = new ValueStringBuilder())
        {
            var text = "The quick brown dog jumps over the lazy cat.";
            zsb.Append(text);
            var bcl = new StringBuilder(text);

            // All "cat" -> "dog"
            zsb.Replace("cat", "dog");
            bcl.Replace("cat", "dog");
            await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

            // Some "dog" -> "fox"
            zsb.Replace("dog", "fox", 15, 20);
            bcl.Replace("dog", "fox", 15, 20);
            await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
        }
    }

    [Test]
    public async Task NotMatchTest()
    {
        using var zsb  = new ValueStringBuilder();
        var       text = "The quick brown dog jumps over the lazy cat.";
        zsb.Append(text);
        zsb.Replace("pig", "dog");
        await Assert.That(zsb.ToString()).IsEqualTo(text);
    }
}