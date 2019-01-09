# WwfHelper

A program for cheating at Words With Friends.  Made as a random project to destroy my enemies (i.e., roommates) a long time ago.

## Usage

At the root, run with .NET Core 2.2 or greater.

```bash
dotnet run
```

Then, you will get a prompt.

```
Enter your letters:
```

Enter the letters that you have in your "hand", with `*` for blank tiles, in any order (e.g., `alfo*fh`).

Then, you will get another prompt.

```
Enter board letters:
```

Basically, using the all of the english letters, `.`, and `-`, type out the pattern that you want to complete.  For example, you might have a part of a board where there is a `b`, then a space, then an `l`, then an open board.  Use the `.` to signify a space whwre you want one of your letters to go, and use the `-` to signify that you want to use as many letters as possible in the remaining space (note: `-`s can on be used at the beginning or end).

In the example above, you would enter `b.l-`, yielding.

```
[007, 03] bal
[008, 04] bola
[009, 04] ball
[009, 04] boll
```

The results take the form of `[{POINTS}, {LENGTH}] {WORD}`.

You can then repeat this step multiple times to try out different parts of the board.