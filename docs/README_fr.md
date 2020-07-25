# FFXIV Fishing Schedule Viewer

---

[[日本語で表示する](README.md#ffxiv-fishing-schedule-viewer)] [[Display in English](README_en.md#ffxiv-fishing-schedule-viewer)] [Afficher en français][[Anzeige auf Deutsch](README_de.md#ffxiv-fishing-schedule-viewer)]

---

Cette application prédit les changements météorologiques dans le jeu dans FINAL FANTASY XIV et vous donne une vue facile à comprendre du moment où vous pouvez attraper du poisson en fonction des conditions météorologiques et de l'heure de la journée.

## [Manifestation]

- Vous pouvez afficher les prévisions météorologiques pour chaque zone sur l'écran «Prévisions météorologiques».
![forecastweather_fr](https://user-images.githubusercontent.com/28302784/88461529-3c204c80-cedf-11ea-86c8-7f01023f34e5.png)

- Sur l'écran «Liste des possibilités de pêche», vous pouvez prédire quand l'heure et les conditions météorologiques spécifiées seront réunies pour le poisson spécifié.
![fishchancelist_fr](https://user-images.githubusercontent.com/28302784/88461532-3d517980-cedf-11ea-8b01-6aedc8a252e3.png)

- Sur l'écran «Options», vous pouvez spécifier le poisson à afficher sur l'écran «Liste des possibilités de pêche» et d'autres paramètres.
![option_fr](https://user-images.githubusercontent.com/28302784/88461531-3d517980-cedf-11ea-9b9a-10cf14808c0d.png)

## [Caractéristiques]

- Cette application peut prédire la météo de chaque zone du jeu jusqu'à 7 jours à l'heure d'Éorzéa, et également afficher la météo 8 heures avant dans chaque zone.
- Cette application prédit quand les conditions (heure et météo) pour la capture du poisson seront réunies et les affiche dans un graphique. L'utilisateur peut spécifier les poissons à afficher dans le graphique.
- Les poissons qui peuvent être affichés avec cette application sont les suivants.
   - C'est un poisson déjà implémenté par FINAL FANTASY XIV patch 5.2, et
   - «Poissons légendaires» ou «poissons mythiques» ou poisson qui ne peut pas être pêché selon la météo ou l'heure de la journée.
- Cette application prend en charge plusieurs langues et les langues actuellement prises en charge sont le japonais / anglais / français / allemand.

## [Version de Final Fantasy XIV]

Cette application prend en charge FINAL FANTASY XIV patch 5.2.

## [Exigences]

* Windwos (l'opération a été confirmée sur la version Windows 10 64 bits)
* .NET Framework 4.7.2

## [Installation]

### Comment installer

1. Suivez la procédure ci-dessous pour installer `.NET Framework 4.7.2`.
    1. Sélectionnez «.NET Framework 4.7.2 Runtime» sur le [site Web officiel de Microsoft] (https://dotnet.microsoft.com/download/dotnet-framework/net472) et téléchargez-le.
    2. Exécutez le fichier téléchargé.
2. Suivez les étapes ci-dessous pour installer cette application.
    1. Téléchargez le dernier fichier .zip à partir de [Emplacement de publication de cette application] (https://github.com/rougemeilland/FFXIVFishingScheduleViewer/releases).
    2. Décompressez le fichier .zip téléchargé et copiez-le dans un dossier approprié.

### Comment désinstaller

1. Supprimez le dossier dans lequel cette application est installée ainsi que les fichiers qu'il contient.

## [Usage]

### 1. Comment commencer

Veuillez démarrer FFXIVFishingScheduleViewer.exe dans le dossier où cette application est installée.

Pour le démarrer, sélectionnez «Exécuter» dans le menu Démarrer de Windows ou ouvrez l'Explorateur.

### 2. A propos de l'écran « Liste des possibilités de pêche »

- Sur cet écran, l'heure à laquelle les conditions (heure et météo) des poissons vérifiées sur l'écran «Options» sont satisfaites sera affichée dans un graphique.
Les informations suivantes sont également affichées pour chaque poisson.
  - Heure à laquelle le poisson peut être pêché (heure éorzéenne et heure locale)
  - Un lieu de pêche où vous pouvez attraper le poisson
  - Appât de pêche requis
  - Notes pour les conseils de pêche (l'utilisateur peut librement modifier dans l'écran «Options»)
- En cliquant avec le bouton droit de la souris sur l'élément poisson, vous pouvez éditer le mémo du poisson ou masquer le poisson de l'écran «Liste des possibilités de pêche».
- «Difficulté de découverte» est un classement unique par cette application, et est une mesure de 
«combien il est difficile de répondre aux conditions de temps et de temps du poisson».
Veuillez noter que cela ne correspond pas toujours à la difficulté de la pêche elle-même.

### 3. À propos du format du mémo de poisson
Le contenu initial de chaque note de poisson met l'accent sur la brièveté et est abrégé.
La lecture de la partie qui semble particulièrement difficile à comprendre est expliquée dans [un autre document](AboutFishMemo_fr.md#%C3%A0-propos-du-format-du-m%C3%A9mo-de-poisson).

***Le mémo de poisson peut être librement édité par l'utilisateur, et il n'est pas nécessaire de l'éditer selon ces notations.***

## [Remarque]

- Cette application peut être mise à jour sans préavis.
- Veuillez noter que le développeur n'est pas responsable des dommages causés à l'utilisateur en utilisant cette application, que ce soit dans le jeu ou hors du jeu.
- Bien que certains des poissons qui peuvent être capturés dans FINAL FANTASY XIV ne soient pas strictement appelés «poissons», ils sont appelés «poissons» par commodité dans ce document.
- Sur l'écran «Liste des possibilités de pêche», si plusieurs poissons sont censés apparaître pendant la même période, le poisson avec la «difficulté de découverte» la plus élevée sera affiché de préférence. Si le poisson que vous avez coché dans l'écran «Options» n'apparaît pas même si les conditions météorologiques et l'heure de la journée sont réunies, un autre poisson peut avoir été priorisé. Dans ce cas, c'est une bonne idée de décocher certains poissons dans l'écran «Options».

## [Auteur]

[Palmtree Software](https://github.com/rougemeilland)

## [License]

«FFXIV Fishing Schedule Viewer» est sous [licence MIT](https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

FINAL FANTASY XIV (C) 2010 - 2020 SQUARE ENIX CO., Ltd. FINAL FANTASY est une marque déposée de Square Enix Holdings Co., Ltd. Tous les matériels sont utilisés sous licence.