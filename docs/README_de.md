# FFXIV Fishing Schedule Viewer<a name="top_of_content"></a>

---

[[日本語で表示する](README.md#top_of_content)] [[Display in English](README_en.md#top_of_content)] [[Afficher en français](README_fr.md#top_of_content)][Anzeige auf Deutsch]

---

Diese Anwendung sagt Wetteränderungen im Spiel in FINAL FANTASY XIV voraus und gibt Ihnen einen leicht verständlichen Überblick darüber, wann Sie je nach Wetter und Tageszeit Fische fangen können.

## [Demonstration]

- Sie können die Wettervorhersage für jedes Gebiet auf dem Bildschirm "Wettervorhersage" anzeigen.
![forecastweather_de](https://user-images.githubusercontent.com/28302784/88461551-725dcc00-cedf-11ea-9991-61dcce1209d3.png)

- Auf dem Bildschirm "Angelchancenliste" können Sie vorhersagen, wann die angegebenen Zeit- und Wetterbedingungen für den angegebenen Fisch erfüllt werden.
![fishchancelist_de](https://user-images.githubusercontent.com/28302784/88461549-712c9f00-cedf-11ea-82a8-543ba4a3a5d2.png)

- Auf dem Bildschirm "Optionen" können Sie den Fisch angeben, der auf dem Bildschirm "Liste der Fangchancen" und anderen Einstellungen angezeigt werden soll.
![option_de](https://user-images.githubusercontent.com/28302784/88461552-72f66280-cedf-11ea-996f-a3add1bac30d.png)

## [Feature]

- Diese Anwendung kann das Wetter in jedem Bereich des Spiels für bis zu 7 Tage in Eorzea-Zeit vorhersagen und das Wetter 8 Stunden zuvor in jedem Bereich anzeigen.
- Diese Anwendung sagt voraus, wann die Bedingungen (Zeit und Wetter) für den Fischfang erfüllt sind, und zeigt sie in einer Grafik an. Der Benutzer kann angeben, welche Fische in der Grafik angezeigt werden sollen.

- Die Fische, die mit dieser Anwendung angezeigt werden können, sind wie folgt.
  - Es ist ein Fisch, der bereits von FINAL FANTASY XIV Patch 5.2 implementiert wurde, und
  - "Kapitalen Fische" oder "Legendären Kapitalfische" oder ein Fisch, der je nach Wetter oder Tageszeit nicht gefangen werden kann.
- Diese Anwendung unterstützt mehrere Sprachen und die derzeit unterstützten Sprachen sind Japanisch / Englisch / Französisch / Deutsch.


## [FINAL FANTASY XIV Version]

Diese Anwendung unterstützt FINAL FANTASY XIV Patch 5.2.

## [Bedarf]

* Windwos (Der Betrieb wurde unter Windows 10 64-Bit-Version bestätigt.)
* .NET Framework 4.7.2

## [Installation]

### Wie installiert man

1. Gehen Sie wie wie man kämpftfolgt vor, um `.NET Framework 4.7.2` zu installieren.
  1. Wählen Sie auf der [offiziellen Microsoft-Website] (https://dotnet.microsoft.com/download/dotnet-framework/net472) ".NET Framework 4.7.2 Runtime" aus und laden Sie es herunter.
  2. Führen Sie die heruntergeladene Datei aus.
2. Führen Sie die folgenden Schritte aus, um diese Anwendung zu installieren.
  1. Laden Sie die neueste ZIP-Datei von [Veröffentlichter Speicherort dieser Anwendung] herunter (https://github.com/rougemeilland/FFXIVFishingScheduleViewer/releases/latest).
  2. Entpacken Sie die heruntergeladene ZIP-Datei und kopieren Sie sie in einen geeigneten Ordner.

### So deinstallieren Sie

1. Löschen Sie den Ordner, in dem diese Anwendung installiert ist, zusammen mit den Dateien darunter.

## [Anleitung]

### 1. Wie man anfängt

Bitte starten Sie FFXIVFishingScheduleViewer.exe in dem Ordner, in dem diese Anwendung installiert ist.

Um es zu starten, wählen Sie "Ausführen" aus dem Windows-Startmenü oder öffnen Sie den Explorer.

### 2. Über den Bildschirm "Angelchancenliste"

- Auf diesem Bildschirm wird die Zeit, zu der die Bedingungen (Zeit und Wetter) der Fische, die auf dem Bildschirm "Optionen" überprüft wurden, erfüllt sind, in einer Grafik angezeigt.
Die folgenden Informationen werden auch für jeden Fischgegenstand angezeigt.
  - Zeitpunkt, zu dem der Fisch gefangen werden kann (Eorzea-Zeit und Ortszeit)
  - Ein Angelplatz, an dem Sie den Fisch fangen können
  - Erforderlicher Angelköder
  - Hinweise für Angeltipps (Benutzer kann im Bildschirm "Optionen" frei bearbeiten)
- Durch Klicken mit der rechten Maustaste auf das Fischelement können Sie das Memo des Fisches bearbeiten oder den Fisch im Bildschirm "Angelchancenliste” ausblenden.
- "Entdeckungsschwierigkeit" ist eine einzigartige Rangfolge dieser Anwendung und ein Maß dafür, "wie schwierig es ist, die Zeitzonen- und Wetterbedingungen der Fische zu erfüllen".
Bitte beachten Sie, dass es nicht immer dem Schwierigkeitsgrad des Fischfangs selbst entspricht.

### 3. Format der Fischnotiz

Der anfängliche Inhalt jeder Fischnote wird stark abgekürzt, wobei der Schwerpunkt auf der Einfachheit liegt.
Das Lesen des Teils, der besonders schwer zu verstehen scheint, wird in [einem anderen Dokument](AboutFishMemo_de.md#top_of_content) erläutert.

***Das Fischmemo kann vom Benutzer frei bearbeitet werden, und es ist nicht erforderlich, es gemäß diesen Notationen zu bearbeiten.***

## [Hinweis]

- Diese Anwendung kann ohne vorherige Ankündigung aktualisiert werden.
- Bitte beachten Sie, dass der Entwickler nicht für Schäden verantwortlich ist, die dem Benutzer durch die Verwendung dieser Anwendung im Spiel oder außerhalb des Spiels entstehen.
- Obwohl einige der Fische, die in FINAL FANTASY XIV gefangen werden können, nicht streng als "Fisch" bezeichnet werden, werden sie in diesem Dokument der Einfachheit halber als "Fisch" bezeichnet.
- Wenn auf dem Bildschirm "Angelchancenliste" erwartet wird, dass mehrere Fische im selben Zeitraum erscheinen, werden die Fische mit der höheren "Entdeckungsschwierigkeiten" bevorzugt angezeigt. Wenn der Fisch, den Sie im Bildschirm "Optionen" überprüft haben, nicht angezeigt wird, obwohl das Wetter und die Tageszeit erfüllt sind, wurde möglicherweise ein anderer Fisch priorisiert. In diesem Fall ist es eine gute Idee, einige Fische im Bildschirm "Optionen" zu deaktivieren.

## [Autor]

[Palmtree Software](https://github.com/rougemeilland)

## [Lizenz]

"FFXIV Fishing Schedule Viewer" steht unter [MIT-Lizenz](https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

FINAL FANTASY XIV (C) 2010 - 2020 SQUARE ENIX CO. LTD. FINAL FANTASY ist ein eingetragenes Markenzeichen von Square Enix Holdings Co., Ltd. Alle Materialien werden unter Lizenz genutzt.
