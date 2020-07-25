# Format der Fischnotiz

---

[[日本語で表示する](AboutFishMemo.md#%E9%AD%9A%E3%81%AE%E3%83%A1%E3%83%A2%E3%81%AE%E5%BD%A2%E5%BC%8F%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6)][[Display in English](AboutFishMemo_en.md#about-format-of-fish-memo)][[Afficher en français](AboutFishMemo_fr.md#%C3%A0-propos-du-format-du-m%C3%A9mo-de-poisson)][Anzeige auf Deutsch]  
[[Kehren Sie zu README zurück](README_de.md#ffxiv-fishing-schedule-viewer)]

---

## 1. Über die Stärke der Zugkraft

"!" (Ausrufezeichen) indicates the strength of the tug. It has the same meaning as what appears above the player character's head when catching a fish.

- "!" bedeutet schwacher Zug.
- "!!" bedeutet Zug mittlerer Stärke.
- "!!!" bedeutet starker Zug.

Einzelheiten finden Sie unter "Beim Angeln wurde ein neuer Effekt hinzugefügt, der beim Anbeißen eines Fisches abgespielt wird." in [Veröffentlichung der Details zu Patch 5.2!](https://de.finalfantasyxiv.com/lodestone/topics/detail/cc103274e6edc3c440533480dc9be0608f939607).


## 2. Wie man fischt

Wie man fischt, steht im Memo wie folgt.

```
<Angelköder Name>
    ⇒ (<Ausrufezeichen>)<Fischname>
```

Wenn auf dem Memo beispielsweise "Nordkrill ⇒ (!!) Steinhummer" steht, ersetzen Sie es bitte wie folgt.

1. Verwenden Sie die Aktion "Angeln", nachdem Sie "Nordkrill" als Angelköder ausgewählt haben.
2. Wenn Sie die Aktion "Anschlag" verwenden, wenn die Zugkraft mittel ist, können Sie möglicherweise "Steinhummer" fangen.

In ähnlicher Weise heißt es in der Anmerkung zum Angeln, wo NatürlKöder verwendet werden sollten:

```
<Angelköder Name>
    ⇒ (<Ausrufezeichen>)<Fischname>
    ⇒ (<Ausrufezeichen>)<Fischname>
```

Wenn auf dem Memo beispielsweise "Pillendreher⇒(!!) Hafenhering HQ⇒(!!!) Oktoboss" steht, ersetzen Sie es bitte wie folgt.

1. Verwenden Sie die Aktion "Angeln", nachdem Sie "Pillendreher" als Angelköder ausgewählt haben.
2. Verwenden Sie die Aktion "Anschlag", wenn die Zugkraft mittel ist. Dann können Sie möglicherweise "Hafenhering HQ" fangen. Und wenn Sie es fangen können, verwenden Sie die Aktion "NatürKöder".
3. Verwenden Sie die Aktion "Anschlag", wenn die Zugkraft stark ist. Dann können Sie möglicherweise "Octoboss" fangen.

## 3. Welche Aktion sollten Sie verwenden, "Fester Anschlag" oder "Präziser Anschlag" ?

Wenn Sie die Aktion "Geduld" zum Fangen von Fischen verwenden, wird in den Fischnotizen davon ausgegangen, dass die folgenden Aktionen standardmäßig verwendet werden.

- Verwenden Sie die Aktion "Präziser Anschlag", wenn die Zugkraft schwach ist ("!").
- Verwenden Sie die Aktion "Fester Anschlag", wenn die Zugkraft mittel ist ("!!").
- Verwenden Sie die Aktion "Fester Anschlag", wenn die Zugkraft stark ist ("!!!").

Wenn die obige Aktion für einen Fisch ungeeignet ist, gibt der Hinweis die geeignete Aktion für diesen Fisch an.


Wenn in der Notiz beispielsweise `Pillendreher ⇒ (!!! Präziser Anschlag) Goldflosse` steht,
ist es angebracht, die Aktion "Präziser Anschlag" anstelle der Aktion "Fester Anschlag" zu verwenden.
