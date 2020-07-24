# FFXIV Fishing Schedule Viewer

---

[[日本語で表示する](README.md)] [[Display in English](README_en.md)] [Affichage en français]

---

Cette application prédit les changements de temps dans le jeu sur FINAL FANTASY XIV et affiche les heures d'apparition des poissons en fonction des conditions météorologiques et du fuseau horaire d'une manière facile à comprendre.

## [Manifestation]

- Vous pouvez afficher les prévisions météorologiques pour chaque zone sur l'écran «Prévisions météorologiques».
![weathermap](https://user-images.githubusercontent.com/28302784/88042870-98a20580-cb87-11ea-8924-fb918e9f0e82.png)

- Sur l'écran "Liste des possibilités de pêche", vous pouvez prédire quand l'heure et les conditions météorologiques spécifiées seront réunies pour le poisson spécifié.
![chance](https://user-images.githubusercontent.com/28302784/88042871-993a9c00-cb87-11ea-9d43-4a05ee2db797.png)

- Sur l'écran «Options», vous pouvez spécifier le poisson à afficher sur l'écran «Liste des possibilités de pêche» et d'autres paramètres.
![optionwindow](https://user-images.githubusercontent.com/28302784/88042868-9770d880-cb87-11ea-9e97-312750a1a8ec.png)

## [Caractéristiques]

- Les prévisions météorologiques pour chaque région du jeu sont affichées. La période affichée est jusqu'à 7 jours (heure d'Éorzéa) à partir de maintenant, et la météo d'il y a 8 heures est également affichée.
- Un graphique est affiché indiquant quand les conditions de temps et les conditions météorologiques dans lesquelles l'utilisateur peut pêcher le poisson sont spécifiées.
- Les poissons pris en charge par cette application sont les suivants.
  - Il s'agit d'un poisson implémenté jusqu'à "FINAL FANTASY XIV patch 5.2", et
  - «Poissons légendaires» ou «poissons mythiques» ou tous poissons avec des conditions en fonction de la météo ou de l'heure de la journée.
- Prend en charge plusieurs langues. Les langues actuellement prises en charge sont le japonais / anglais / français / allemand.


## [Version de Final Fantasy XIV]
Cette application est compatible avec FINAL FANTASY XIV patch 5.2.

## [Exigence]

* Windows (confirmé pour fonctionner uniquement sur la version 64 bits de Windows 10)
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

1. Supprimez tous les fichiers du dossier dans lequel cette application est installée.

## [Usage]

### 1. Comment commencer

Veuillez démarrer FFXIVFishingScheduleViewer.exe dans le dossier où cette application est installée.

Pour le démarrer, sélectionnez "Exécuter" dans le menu Démarrer de Windows ou ouvrez l'Explorateur.

### 2. A propos de l'écran « Liste des possibilités de pêche »

- Pour les poissons cochés dans l'écran "Option", un graphique sera affiché indiquant quand le fuseau horaire et les conditions météorologiques seront réunis.
Les informations suivantes sont également affichées pour chaque poisson.
  - Heure à laquelle le poisson peut être capturé (à la fois l'heure d'Éorzéa et l'heure de la Terre)
  - Un lieu de pêche où vous pouvez attraper le poisson
  - Appât de pêche requis
  - Une note qui donne des conseils sur la façon d'attraper le poisson (la note peut être librement éditée par l'utilisateur dans l'écran "Options")
- En cliquant avec le bouton droit de la souris sur l'élément poisson, vous pouvez éditer le mémo du poisson ou masquer le poisson de l'écran "Liste des possibilités de pêche".
- "Difficulté de découverte" est un classement unique par cette application, et est une mesure de "combien il est difficile de répondre aux conditions de temps et de temps du poisson". Veuillez noter que cela ne correspond pas toujours à la difficulté de la pêche elle-même.

### 3. À propos du format du mémo de poisson
Le contenu initial de chaque note de poisson met l'accent sur la brièveté et est abrégé.
La lecture de la partie qui semble particulièrement difficile à comprendre est expliquée dans [un autre document](AboutFishMemo_fr.md).

***Le mémo de poisson peut être librement édité par l'utilisateur, et il n'est pas nécessaire de l'éditer selon ces notations.***

## [Remarque]

- Cette application peut être mise à jour sans préavis.
- Veuillez noter que le développeur n'est pas responsable des dommages causés à l'utilisateur en utilisant cette application, que ce soit dans le jeu ou hors du jeu.
- Bien que certains des poissons que vous pouvez pêcher dans FINAL FANTASY XIV ne soient pas des noms propres pour les poissons, ils sont tous appelés «poissons» par commodité dans ce document.
- Sur l'écran "Liste des possibilités de pêche", si plusieurs poissons sont censés apparaître en même temps, le poisson avec la "difficulté de découverte" la plus élevée sera affiché de préférence. Si le poisson que vous avez coché dans l'écran "Options" n'apparaît pas même si les conditions météorologiques et d'heure sont réunies, il se peut qu'un autre poisson ait été priorisé. Dans ce cas, il est recommandé de vérifier le nombre de poissons confirmé dans l'écran «Option».

## [Auteur]

[Palmtree Software](https://github.com/rougemeilland)

## [License]

"FFXIV Fishing Schedule Viewer" est sous [licence MIT] (https://raw.githubusercontent.com/rougemeilland/FFXIVFishingScheduleViewer/master/LICENSE).  
© 2020 Palmtree Software.  

FINAL FANTASY XIV (C) 2010 - 2020 SQUARE ENIX CO., Ltd. FINAL FANTASY est une marque déposée de Square Enix Holdings Co., Ltd. Tous les matériels sont utilisés sous licence.