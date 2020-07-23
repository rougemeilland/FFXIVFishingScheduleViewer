# FFXIV Fishing Schedule Viewer

---

[[日本語で表示する](README.md)] [Displaying in English]

---

This application predicts changes in the in-game weather on FINAL FANTASY XIV, and displays the appearance times of fish depending on the weather and time zone conditions in an easy-to-understand manner.

In addition, there are many things that can be caught in FINAL FANTASY XIV that the name "fish" is not appropriate, not fish, but in this document, they are all called "fish" for convenience.

## [DEMO]

- You can display the weather forecast for each area on the "Weather Forecast" screen.
![weathermap](https://user-images.githubusercontent.com/28302784/88042870-98a20580-cb87-11ea-8924-fb918e9f0e82.png)

- On the "Fishing chance List" screen, you can predict when the specified time and weather conditions will be met for the specified fish.
![chance](https://user-images.githubusercontent.com/28302784/88042871-993a9c00-cb87-11ea-9d43-4a05ee2db797.png)

- On the "Options" screen, you can specify the fish to be displayed on the "Fishing chance List" screen and other settings.
![optionwindow](https://user-images.githubusercontent.com/28302784/88042868-9770d880-cb87-11ea-9e97-312750a1a8ec.png)

## [Features]

- The weather forecast for each region in the game is displayed. The displayed period is up to 7 days (Eorzea time) from now, and the weather 8 hours ago is also displayed.
- A graph is displayed showing when the conditions for time and weather conditions in which the user can fish the fish are specified.
- The fish supported by this application are as follows.
    1. It is a fish implemented up to "FINAL FANTASY XIV patch 5.2", and
    2. Big fish or living legends or all fish with conditions depending on the weather or time of day.
- Supports multiple languages. Currently supported languages are Japanese / English / French / German.


## [FINAL FANTASY XIV version]
This application is compatible with FINAL FANTASY XIV patch 5.2.


## [Requirement]

* Windows (Confirmed to work only on Windows 10 64bit version)
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

1. Delete all the files under the folder where this application is installed.

## [Usage]

### 1. How to start

Please start FFXIVFishingScheduleViewer.exe in the folder where this application is installed.

To start it, select "Run" from the Windows Start menu or open Explorer.

### 2. About "Fishing chance list" screen

- For the fish checked in the "Option" screen, a graph will be displayed showing when the time zone and weather conditions will be met.
The following information is also displayed for each fish.
  - Fishing spot to go
  - Required fishing baits
  - Time when the fish can be caught (both Eorzea time and Earth time)
  - Memo that is a hint for fishing (user can freely edit the memo on the "Options" screen)
- By right-clicking on the fish item, you can edit the memo of the fish or hide the fish from the "Fishing chance list" screen.
- "Discovery difficulty" is a unique ranking of this application and is a measure of "how difficult it is to meet the time zone conditions and weather conditions of the fish." Please note that it does not always match the difficulty level of the fishing itself.

### 3. About format of fish memo
The memo initially set for each fish emphasizes the compactness of the displayed contents, so the notation is considerably omitted.
A separate document explains how to read the part that seems to be particularly difficult to understand.

***The fish memo can be freely edited by the user, and it is not necessary to edit it according to these notations.***

## [Note]

- This application may be updated without notice.
- Please note that the developer is not responsible for any damage caused to the user by using this application, whether it is in-game or out-of-game.
- On the "Fishing chance list" screen, if multiple fish are duplicated at the same time, the fish with the higher "Discovery difficulty" will be displayed with priority.
If the fish you checked in the "Options" screen does not appear even though it meets the weather and time zone conditions,
Please suspect that other fish may have been given priority.
In such a case, it is recommended to try reducing the number of fish checked on the "Options" screen.

## [Author]

[Palmtree Software](https://github.com/rougemeilland)

## [License]

"FFXIV Fishing Schedule Viewer" is under [MIT license](https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

FINAL FANTASY XIV © 2010 - 2020 SQUARE ENIX CO., LTD. All Rights Reserved.