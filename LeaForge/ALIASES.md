# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- Page values use one or two letters: `U`, `T`, `H`, `N`, and `Ls`.
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
