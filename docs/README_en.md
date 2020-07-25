# FFXIV Fishing Schedule Viewer

---

[[日本語で表示する](README.md#ffxiv-fishing-schedule-viewer)] [Display in English] [[Afficher en français](README_fr.md#ffxiv-fishing-schedule-viewer)][[Anzeige auf Deutsch](README_de.md#ffxiv-fishing-schedule-viewer)]

---

This application predicts in-game weather changes in FINAL FANTASY XIV and gives you an easy-to-understand view of when you can catch fish depending on weather and time of day conditions.

## [Demonstration]

- You can display the weather forecast for each area on the "Weather Forecast" screen.
![forecastweather_en](https://user-images.githubusercontent.com/28302784/88461499-fb283800-cede-11ea-83b4-4e7691868867.png)

- On the "Fishing chance List" screen, you can predict when the specified time and weather conditions will be met for the specified fish.
![fishchancelist_en](https://user-images.githubusercontent.com/28302784/88461501-fcf1fb80-cede-11ea-899d-c8c731fe93b9.png)

- On the "Options" screen, you can specify the fish to be displayed on the "Fishing chance List" screen and other settings.
![option_en](https://user-images.githubusercontent.com/28302784/88461500-fc596500-cede-11ea-9f59-68eec7073088.png)

## [Features]

- This application can predict the weather of each area in the game for up to 7 days in Eorzea time, and also display the weather 8 hours before in each area.
- This application predicts when the conditions (time and weather) for catching fish will be met and displays them in a graph. The user can specify which fish to display in the graph.
- The fish that can be displayed with this application are as follows.
   - It is a fish already implemented in FINAL FANTASY XIV patch 5.2, and
   - "Big fish" or "Living legends" or a fish that cannot be caught depending on the weather or time of day.
- This application supports multiple languages, and the currently supported languages are Japanese/English/French/German.


## [FINAL FANTASY XIV version]

This application supports FINAL FANTASY XIV patch 5.2.

## [Requirement]

* Windwos (Operation has been confirmed on Windows 10 64bit version)
* .NET Framework 4.7.2


## [Installation]

### How to install

1. Follow the procedure below to install `.NET Framework 4.7.2`.
    1. Select `.NET Framework 4.7.2 Runtime` on the [Microsoft official website](https://dotnet.microsoft.com/download/dotnet-framework/net472) and download it.
    2. Execute the downloaded file.
2. Follow the steps below to install this application.
    1. Download the latest .zip file from [Published location of this application](https://github.com/rougemeilland/FFXIVFishingScheduleViewer/releases).
    2. Unzip the downloaded .zip file and copy it to an appropriate folder.

### How to uninstall

1. Delete the folder where this application is installed along with the files under it.

## [Usage]

### 1. How to start

Please start FFXIVFishingScheduleViewer.exe in the folder where this application is installed.

To start it, select "Run" from the Windows Start menu or open Explorer.

### 2. About "Fishing chance list" screen

- On this screen, the time when the conditions (time and weather) of the fish that have been checked on the "Options" screen are satisfied will be displayed in a graph.
The following information is also displayed for each fish item.
  - Time at which fish can be caught (both Eorzean time and local time)
  - A fishing spot where you can catch the fish
  - Required fishing tackles
  - Notes for fishing tips (user can freely edit in "Options" screen)
- By right-clicking on the fish item, you can edit the memo of the fish or hide the fish from the "Fishing chance list" screen.
- "Discovery difficulty" is a unique ranking by this application, and is a measure of "how difficult it is to meet the time zone conditions and weather conditions of the fish."
Please note that it does not always match the difficulty level of the fishing itself.

### 3. About format of fish memo
The memo initially set for each fish emphasizes the compactness of the displayed contents, so the notation is considerably omitted.
The reading of the part that seems to be particularly difficult to understand is explained in [another document](AboutFishMemo_en.md#about-format-of-fish-memo).

***The fish memo can be freely edited by the user, and it is not necessary to edit it according to these notations.***

## [Note]

- This application may be updated without notice.
- Please note that the developer is not responsible for any damage caused to the user by using this application, whether it is in-game or out-of-game.
- Although some of the fish that can be caught in FINAL FANTASY XIV are not strictly called "fish", they are referred to as "fish" for convenience in this document.
- On the "Fishing chance list" screen, if multiple fish are expected to appear during the same time period, the fish with the higher "discovery difficulty" will be displayed preferentially. If the fish you checked in the "Options" screen does not appear even though the weather and time of day conditions are met, another fish may have been prioritized. In that case, it's a good idea to uncheck some fish in the "Options" screen.

## [Author]

[Palmtree Software](https://github.com/rougemeilland)

## [License]

"FFXIV Fishing Schedule Viewer" is under [MIT license](https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

FINAL FANTASY XIV © 2010 - 2020 SQUARE ENIX CO., LTD. All Rights Reserved.