# About format of fish memo

---

[[日本語で表示する](AboutFishMemo.md)] [Displaying in English] [[Afficher en français](AboutFishMemo_fr.md)]  
[[Return to README](README_en.md)]

---

## 1. About the strength of the tug

"!" (Exclamation mark) indicates the strength of the tug. It has the same meaning as what appears above the player character's head when catching a fish.
  -  "!" means light tug.
  -  "!!" means medium tug.
  -  "!!!" means heavy tug.

## 2. How to catch the fish

How to fish is written in the memo as follows.

```
<name of fishing tackle>
    ⇒ (<exclamation points>)<fish name>
```

For example, if it is written as "Fistful of northern krill ⇒ (!!) Rock Lobster", replace it as follows.

1. Use the "Cast" action after selecting "Fistful of northern krill" as the fishing tackle.
2. If you use the "Hook" action when the strength of the tug is medium, you may be able to catch a Rock Lobster.

Similarly, in the case of mooching, it is written as follows.

```
<name of fishing bait>
    ⇒ (<exclamation points>)<fish name>
    ⇒ (<exclamation points>)<fish name>
```

For example, if it is written as "Pill bug⇒(!!) Harbor Herring HQ⇒(!!!) Octmammoth)", replace it as follows.

1. Use the "Cast" action after selecting "Pill bug" as the fishing tackle.
2. Use the "Hook" action when the strength of the tug is medium. Then you may be able to catch "Harbor Herring HQ". And if you can catch it, use the "Mooch" action.
3. Use the "Hook" action when the strength of the tug is heavy. Then you may be able to catch "Octomammoth".

## 3. Hooksets to use

The following hooksets should be used by default when using the action "Patience".

  - Use the action "Precision Hookset" if the strength of the tug is "!" (light tug)
  - Use the action "Powerful Hookset" if the strength of the tug is "!!" (mediumt tug)
  - Use the action "Powerful Hookset" if the strength of the tug is "!!!" (heavy tug)

If the fish mentioned above is not suitable for hookset, the note will show the appropriate hookset for that fish.

For example, if the note says `Pill bug ⇒ (!!! Precision Hoookset) Goldenfin`,
it is appropriate to use Precision Hookset instead of Powerful Hookset.
