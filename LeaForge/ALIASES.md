# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- Page values use one or two letters: `U`, `T`, `H`, `Q`, `D`, `N`, `Bs`, and `Ls`.
- Page commands use one or two letters: `t`, `a`, `bg`, `fg`, and `fs`.
- New public aliases should be no more than three letters.
- Longer names may remain inside the engine when they make the implementation safer to maintain.

## Tiny example

```csharp
var e = LeaForge.L.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
```

The aliases are the small public language of Lea. The readable internal classes
are the implementation underneath it.

`GoA` and `RlA` are the asynchronous forms used by browser windows. `St` cancels
an active network request. The shorter synchronous forms remain useful for tests.

`P.D` is the document root. Each node uses `K` (kind), `X` (text), `A`
(attributes), `C` (children), and `Av(name)` for a short attribute lookup.
`P.Q` preserves the URL query, including on local `file:` pages where older
.NET Framework URI parsing does not expose it through `Uri.Query`.

Computed style uses `S`: `F` is foreground color, `Bg` is background, `Fs` is
font size, `B` is bold, and `D` is display behavior.

Box spacing stays compact: `Mt/Mr/Mb/Ml` are margins and `Pt/Pr/Pb/Pl` are
padding. `N.Up` links a document node to its parent for descendant selectors.

The WinForms view exposes `Dh` (document height), `Sy` (scroll position), and
`Bc` (cached layout-box count) for compact host diagnostics. `Sc(delta)` moves
the bounded scroll position.

Forms use `Fi`: `K` is field kind, `N` is name, `V` is the editable value, `P`
is placeholder text, `C` is checked, and `Ds`/`Ro` are disabled/read-only.
`F`, `U`, and `M` identify the owning form, action URL, and method. `D` and `Dc`
hold reset defaults. Pages expose fields through `P.Fs`, while `B.I` links a
paint block to its field. The WinForms view raises `Ed` for edits and `Sb` for
form actions.

Hosts can call `Rs(field)` to reset a form and `Sm(field)` or `SmA(field)` to
submit a supported GET form synchronously or asynchronously.
