# About format of fish memo

---

[[日本語で表示する](AboutFishMemo.md#%E9%AD%9A%E3%81%AE%E3%83%A1%E3%83%A2%E3%81%AE%E5%BD%A2%E5%BC%8F%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6)][Display in English][[Afficher en français](AboutFishMemo_fr.md#%C3%A0-propos-du-format-du-m%C3%A9mo-de-poisson)][[Anzeige auf Deutsch](AboutFishMemo_de.md#format-der-fischnotiz)]  
[[Return to README](README_en.md#ffxiv-fishing-schedule-viewer)]

---

## 1. About the strength of the tug

The "!" (exclamation point) represents the strength of the tug of the fish.
It has the same meaning as the effect that appears above the player character's head when the fish bites.

  -  The "!" means light tug.
  -  The "!!" means medium tug.
  -  The "!!!" means heavy tug.

For details, please refer to the item of "Adjustments have been made to the animation used to indicate a bite when fishing." in [Patch 5.2 Notes](https://na.finalfantasyxiv.com/lodestone/topics/detail/b0151eaed1faecb46061b947cf9c08bed75d230d).

## 2. How to catch the fish

How to fish is written in the memo as follows.

```
<name of fishing tackle>
    ⇒ (<exclamation points>)<fish name>
```

For example, if the memo says "Fistful of northern krill ⇒ (!!) Rock Lobster", please replace it as follows.

1. Use the "Cast" action after selecting "Fistful of northern krill" as the fishing tackle.
2. If you use the "Hook" action when the strength of the tug is medium, you may be able to catch a Rock Lobster.

Similarly, in the case of mooching, it is written as follows.

```
<name of fishing tackle>
    ⇒ (<exclamation points>)<fish name>
    ⇒ (<exclamation points>)<fish name>
```

For example, if it is written as "Pill bug⇒(!!) Harbor Herring HQ⇒(!!!) Octmammoth)", replace it as follows.

1. Use the "Cast" action after selecting "Pill bug" as the fishing tackle.
2. Use the "Hook" action when the strength of the tug is medium. Then you may be able to catch "Harbor Herring HQ". And if you can catch it, use the "Mooch" action.
3. Use the "Hook" action when the strength of the tug is heavy. Then you may be able to catch "Octomammoth".

## 3. Which action should you use, “Powerful Hookset” or “Precision Hookset”?

When using the "Patience" action to catch fish, the fish notes assume that the following actions are used by default.

- Use the action "Precision Hookset" if the strength of the tug is light ("!")
- Use the action "Powerful Hookset" if the strength of the tug is mediumt ("!!")
- Use the action "Powerful Hookset" if the strength of the tug is heavy ("!!!")


If the action above is inappropriate for a fish, the note will indicate the appropriate action for that fish.

For example, if the note says `Pill bug ⇒ (!!! Precision Hoookset) Goldenfin`,
it is appropriate to use Precision Hookset instead of Powerful Hookset.
