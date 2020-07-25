# FFXIV Fishing Schedule Viewer

---

[日本語で表示する] [[Display in English](README_en.md#ffxiv-fishing-schedule-viewer)] [[Afficher en français](README_fr.md#ffxiv-fishing-schedule-viewer)][[Anzeige auf Deutsch](README_de.md#ffxiv-fishing-schedule-viewer)]

---

本アプリケーションは、FINAL FANTASY XIV でのゲーム内の天候の変化を予測し、天候や時間帯の条件に依存する魚をいつ釣ることができるかをわかりやすく表示します。

## 【使用例】

- 「天気予報」画面で、各地域の天気予報を表示できます。
![forecastweather_ja](https://user-images.githubusercontent.com/28302784/88461433-70dfd400-cede-11ea-90f0-47ba6a5c6151.png)

- 「釣りチャンス一覧」画面で、指定した魚の時刻や天候の条件がいつ成立するかを予測できます。
![fishchancelist_ja](https://user-images.githubusercontent.com/28302784/88461431-6faea700-cede-11ea-8a0e-68d3ec1a3bd3.png)

- 「オプション」画面で、「釣りチャンス一覧」画面に表示する魚の指定やその他の設定ができます。
![option_ja](https://user-images.githubusercontent.com/28302784/88461434-71786a80-cede-11ea-964b-16af51cae3cb.png)

## 【主な機能】

- 本アプリケーションは、ゲーム内の各エリアの天候をエオルゼア時間で最大7日間分予測して表示でき、更に、各エリアの8時間前の天候も表示します。
- 本アプリケーションは、魚が釣れる条件(時間と天候)が満たされる時期を予測し、グラフに表示します。 ユーザーは、どの魚をグラフに表示するかを指定できます。
- 本アプリケーションで表示可能な魚は以下の通りです。
  -  FINAL FANTASY XIV patch 5.2 で実装済みの魚であり、かつ、
  - 「ヌシ」または「オオヌシ」、または天候または時刻によっては釣ることができない魚。
- 本アプリケーションは複数の言語に対応しています。現在対応済みの言語は、日本語/英語/フランス語/ドイツ語です。

## 【FINAL FANTASY XIV のバージョンについて】

本アプリケーションは FINAL FANTASY XIV patch 5.2 をサポートしています。

## 【必要なもの】

* Windows (Windows 10 64bit版で動作を確認しています)
* .NET Framework 4.7.2


## 【インストール/アンインストールについて】

### 1. インストールの方法

1. 以下の手順で`.NET Framework 4.7.2`をインストールしてください。
    1. [Microsoftのサイト](https://dotnet.microsoft.com/download/dotnet-framework/net472)で`.NET Framework 4.7.2 Runtime`を選択してダウンロードをしてください。
    2. ダウンロードしたファイルを実行してください。
2. 以下の手順で`FFXIV Fishing Schedule Viewer`をインストールしてください。
    1. [本アプリケーションの公開場所](https://github.com/rougemeilland/FFXIVFishingScheduleViewer/releases)から最新版の.zipファイルをダウンロードしてください。
    2. ダウンロードした.zipファイルを解凍して、適当なフォルダにコピーしてください。

### 2. アンインストールの方法

1. 本アプリケーションをインストールしたフォルダを配下のファイルごと削除してください。

## 【使い方】

### 1. 起動方法

本アプリケーションをインストールしたフォルダの FFXIVFishingScheduleViewer.exe を起動してください。
起動は、Windowsのスタートメニューから「ファイル名を指定して実行」を選択するか、あるいはエクスプローラを開いて行ってください。

### 2. 「釣りチャンス一覧」画面について

- この画面では、「オプション」画面でチェックを入れた魚について、その魚の条件(時刻と天候)が満たされる時刻がグラフで表示されます。
それぞれの魚の項目には、以下の情報も一緒に表示されます。
  - その魚を釣ることができる時刻(エオルゼア時間とローカル時間の両方)
  - どこの釣り場に行けばいいか
  - どんな釣り餌が必要か
  - 釣り方のヒントとなるメモ (「オプション」画面でユーザが自由に編集可能です)
- 魚の項目を右クリックすると、その魚のメモを編集したり、「釣りチャンス一覧」画面からその魚を表示しないようにしたりすることができます。
- 「発見の難易度」は、本アプリケーションによる独自のランク分けであり、「その魚の時間帯の条件と天候の条件をどれだけ満たしにくいか」の目安です。釣り自体の難易度とは必ずしも一致していませんので注意してください。

### 3. 魚のメモの形式について

それぞれの魚のメモの初期内容は簡潔さが重視されており、かなり省略された表記になっています。
その中で特にわかりにくいと思われる部分の読み方が[別のドキュメント](AboutFishMemo.md#%E9%AD%9A%E3%81%AE%E3%83%A1%E3%83%A2%E3%81%AE%E5%BD%A2%E5%BC%8F%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6)で説明されています。

なお、***魚のメモはユーザが自由に編集可能であり、編集する場合は必ずしもこれらの表記方法に従って編集する必要はありません。***

## 【注意事項】

- 本アプリケーションは予告なくアップデートされることがあります。
- 本アプリケーションを使用することによって利用者が損害を受けた場合、それがゲーム内かゲーム外であるかにかかわらず、開発者は責任を負いかねますので、ご了承ください。
- FINAL FANTASY XIV において釣ることのできるものの中には魚類ではなく「魚」という呼称が適当ではないものもありますが、本ドキュメントでは便宜上それらをすべて「魚」と呼称しています。
- 「釣りチャンス一覧」画面で、同じ時間帯に複数の魚が出現することが予測される場合は「発見の難易度」が高い魚が優先的に表示されます。 「オプション」画面でチェックした魚が、天候や時間帯の条件を満たしていても表示されない場合は、別の魚が優先されている可能性があります。その場合は、「オプション」画面でいくつかの魚のチェックを外してみることをお勧めします。


## 【開発者について】

[Palmtree Software](https://github.com/rougemeilland)

## 【ライセンス】

"FFXIV Fishing Schedule Viewer" is under [MIT license](https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

Copyright (C) SQUARE ENIX CO., LTD. All Rights Reserved.