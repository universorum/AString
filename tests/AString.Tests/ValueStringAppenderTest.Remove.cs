using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestRemove
{
    [Test]
    public async Task Remove()
    {
        var       str = "foo,bar,baz";
        using var zsb = new ValueStringAppender();
        zsb.Append(str);
        var bcl = new StringBuilder(str);

        zsb.Remove(4);
        bcl.Remove(7, 4);

        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }
}