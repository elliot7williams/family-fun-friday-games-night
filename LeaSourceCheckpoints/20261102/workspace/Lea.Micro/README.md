# Lea.Micro

Lea.Micro is the compressed experiment for Lea.

It keeps the same basic idea as the clearer engine, but squeezes it into one short C# file:

- fetch page
- read title
- strip tags into text
- collect links
- back, forward, reload
- tiny commands

## Run

```powershell
dotnet run --project Lea.Micro
```

## Commands

```text
u site   open URL
g n      go to link number
b        back
f        forward
r        reload
l        links
q        quit
```

## Why This Exists

`Lea.Core` is the readable engine.

`Lea.Micro` is the compression test: short names, one file, fewer moving parts.

That lets us compare two styles:

- readable and expandable
- tiny and fast to load
