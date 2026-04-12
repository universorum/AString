using System.Text;

namespace Astra.Text.Tests;

public class ValueStringBuilderInsert
{
    [Test]
    public async Task InsertStringTest()
    {
        var initialValue = "--[]--";

        using var zsb = new ValueStringBuilder();
        var       xyz = "xyz";

        zsb.Append(initialValue);
        var bcl = new StringBuilder(initialValue);

        zsb.Insert(3, xyz, 2);
        bcl.Insert(3, xyz, 2);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.Insert(3, xyz);
        bcl.Insert(3, xyz);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.Insert(0, "<<");
        bcl.Insert(0, "<<");
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        var endIndex = zsb.Length - 1;
        zsb.Insert(endIndex, ">>");
        bcl.Insert(endIndex, ">>");
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task InsertLargeStringTest()
    {
        var initialValue = "--[]--";

        using var zsb  = new ValueStringBuilder();
        var       text = new string('X', 32768);

        zsb.Append(initialValue);
        var bcl = new StringBuilder(initialValue);

        zsb.Insert(3, text);
        bcl.Insert(3, text);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task NotInserted()
    {
        using var zsb  = new ValueStringBuilder();
        var       text = "The quick brown dog jumps over the lazy cat.";
        zsb.Append(text);
        zsb.Insert(10, "");
        await Assert.That(zsb.ToString()).IsEqualTo(text);
    }
}