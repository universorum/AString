AString
===

**AString** is a high-performance string builder library for modern .NET.

* `ValueStringBuilder` struct — full-featured builder backed by a pooled `ArrayPool` buffer
* `ValueStringAppender` struct — lightweight append-only variant for large text
* `AString` static class — convenient `Concat`, `Format`, and `Join` helpers

Thanks to `ZString` for the inspiration; part of the code also comes from `ZString`.

## Getting Started

Install from NuGet:

```
dotnet add package Astra.AString
```

```csharp
using Astra.Text;

// Static helpers
string s1 = AString.Concat("Hello, ", name, "!");
string s2 = AString.Format("x:{0}, y:{1}", x, y);
string s3 = AString.Join(", ", items);

// Builder
using var sb = AString.CreateBuilder(); // or new ValueStringBuilder();
sb.Append("Hello");
sb.AppendLine(42);
sb.AppendFormat("{0:F2}", 3.14);
string result = sb.ToString();

// Append is suggest for very large strings (32k+ chars)
using var sa = new ValueStringAppender();
sa.Append("Hello");
sa.AppendLine(42);
sa.AppendFormat("{0:F2}",
string largeText = sb.ToString();

// Interpolated strings (no boxing, no intermediate string)
sb.Append($"x={x}, y={y}");

// IBufferWriter<char>
using var writer = AString.CreateBufferWriter();

// IBufferWriter<byte> (UTF-8)
using var utf8 = AString.CreateUtf8BufferWriter();
```

## Key Types

### `ValueStringBuilder`

| Member                                                   | Description                                             |
|----------------------------------------------------------|---------------------------------------------------------|
| `SetDefaultSize()`                                       | Adjust initial buffer size                              |
| `GuestStringLength { get; set; }`                        | Adjuest tempory buffer size for cahce formatted string  |
| `RegisterTryFormat<T>(ValueFormatter<T>)`                | Register custom formatter for `ValueStringBuilder` type |
| `Length`                                                 | Current character count                                 |
| `AsSpan()` / `AsMemory()`                                | Access the written buffer without allocating            |
| `Append(value)`                                          | Append any value; generics avoid boxing                 |
| `AppendLine(value)`                                      | Append value + line terminator                          |
| `AppendFormat(format, ...)`                              | Append the value following the format                   |
| `AppendFormat<T>(format, T, Action<ref sender, int ,T>)` | Append a string with the format and giving parameter    |
| `AppendJoin(separator, values)`                          | Join a sequence with a separator                        |
| `Insert(index, value)`                                   | Insert at a position                                    |
| `Remove(start, length)`                                  | Remove a range                                          |
| `Replace(old, new)`                                      | Replace occurrences                                     |
| `CopyTo(...)`                                            | Copy to a `char[]` or `Span<char>`                      |
| `TryCopyTo(Span<char>, out int)`                         | Try-copy to a destination span                          |
| `ToString()`                                             | Build the final string                                  |
| `EnumerateTextElement()`                                 | Enumerate all text elements                             |
| `GetRuneAt(int)`                                         | Get Unicode code point                                  |
| `TryGetRuneAt(int, out Rune)`                            | Try to get Unicode code point                           |
| `EnumerateRunes()`                                       | Enumerate all Unicode code point                        |
| `Dispose()`                                              | Return the inner buffer to the pool                     |

`ValueStringBuilder` is a **mutable struct** — pass by `ref` to avoid accidental copies.

### `ValueStringAppender`

Like `ValueStringBuilder`, but optimized for very large strings.

It use many arrays to combine the string to avoid copying between ararays
so it doesn't support some operate like insert/replace and limited support remove.

### `AStringCompositeFormat`

A pre-parsed composite format string, analogous to `CompositeFormat`. Parse once, format many times to avoid
repeated template parsing.

| Member                                          | Description                                    |
|-------------------------------------------------|------------------------------------------------|
| `Format`                                        | Orginal string that build this objet           |
| `MinimumArgumentCount`                          | How many aurgments parsed                      |
| `CreateString<T>(T, Action<Sender, int, T>)`    | Create a string with giving parameter          |
| `CreateUtf8Array<T>(T, Action<Sender, int, T>)` | Create a UFT8 byte array with giving parameter |

```csharp
var fmt = new AStringCompositeFormat("x:{0}, y:{1}");

using var sb = AString.CreateBuilder();
sb.AppendFormat(fmt, x, y);

// or
var s = fmt.CreateString(dict, static (ref sender, i, dict) => sender.Send(dict[i]));
```

### `AStringWriter`

A `TextWriter` backed by `ValueStringBuilder`. Useful for APIs that accept a `TextWriter`.

```csharp
using var writer = new AStringWriter();
someApi.WriteTo(writer);
string result = writer.ToString();
```

### Buffer writers

| Type               | Interface             | Use                            |
|--------------------|-----------------------|--------------------------------|
| `CharBufferWriter` | `IBufferWriter<char>` | Pass to APIs that write chars  |
| `Utf8BufferWriter` | `IBufferWriter<byte>` | Pass to UTF-8 serializers etc. |

```csharp
using var utf8Writer = AString.CreateUtf8BufferWriter();
JsonSerializer.Serialize(utf8Writer, obj);
```