using System.Text;

namespace Astra.Text.Tests;

public class ValueStringBuilderTestRemove
{
    [Test]
    public async Task RemovePart()
    {
        var       str = "The quick brown fox jumps over the lazy dog.";
        using var zsb = new ValueStringBuilder();
        zsb.Append(str);
        var bcl = new StringBuilder(str);

        // Remove "brown "
        zsb.Remove(10, 6);
        bcl.Remove(10, 6);

        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task RemoveAll()
    {
        var       str = "The quick brown fox jumps over the lazy dog.";
        using var zsb = new ValueStringBuilder();
        zsb.Append(str);
        var bcl = new StringBuilder(str);

        zsb.Remove(0, str.Length);
        bcl.Remove(0, str.Length);

        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task RemoveTail()
    {
        var       str = "foo,bar,baz";
        using var zsb = new ValueStringBuilder();
        zsb.Append(str);
        var bcl = new StringBuilder(str);

        zsb.Remove(7, 4);
        bcl.Remove(7, 4);

        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }
}