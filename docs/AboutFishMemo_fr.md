# À propos du format du mémo de poisson

---

[[日本語で表示する](AboutFishMemo.md#%E9%AD%9A%E3%81%AE%E3%83%A1%E3%83%A2%E3%81%AE%E5%BD%A2%E5%BC%8F%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6)][[Display in English](AboutFishMemo_en.md#about-format-of-fish-memo)][Afficher en français][[Anzeige auf Deutsch](AboutFishMemo_de.md#format-der-fischnotiz)]  
[[Revenir à README](README_fr.md#ffxiv-fishing-schedule-viewer)]

---



## 1.À propos de la force du remorqueur

«!» (Point d'exclamation) indique la force du remorqueur.
Il a la même signification que l'effet qui apparaît au-dessus de la tête du personnage du joueur lorsque le poisson mord.

- Le «!» signifie remorqueur léger.
- Le «!!» signifie remorqueur moyen.
- Le «!!!» signifie remorqueur lourd.

Pour plus de détails, veuillez vous référer à l'élément «Lors de la pêche, l'animation lorsqu'un poisson mord à l'hameçon a été modifiée.» dans [Notes de mise à jour 5.2](https://fr.finalfantasyxiv.com/lodestone/topics/detail/7a5be8934cc1cfb4a34ec7c7cbee5a03ee8d62c2).

## 2. Comment attraper un poisson

Comment pêcher est écrit dans le mémo comme suit.

```
<nom de l'appât de pêche>
    ⇒ (<points d'exclamation>)<nom du poisson>
```

Par exemple, si le mémo dit «Krill polaire ⇒ (!!) Homard des rochers», veuillez le remplacer comme suit.

1. Utilisez l'action «Pêche» après avoir sélectionné «Krill polaire» comme appât de pêche.
2. Si vous utilisez l'action «Ferrage» lorsque la force du remorqueur est moyenne, vous pourrez peut-être attraper un «Homard des rochers».

De même, dans le cas de la pêche au vif, il s'écrit comme suit.

```
<nom de l'appât de pêche>
    ⇒ (<points d'exclamation>)<nom du poisson>
    ⇒ (<points d'exclamation>)<nom du poisson>
```

Par exemple, si `Pill bug ⇒ (!!) Hareng portuaire HQ ⇒ (!!!) Mégalopoulpe)` est écrit, remplacez-le comme suit.

1. Utilisez l'action «Pêche» après avoir sélectionné «Cloportee comme appât de pêche.
2. Utilisez l'action «Ferrage» lorsque la force du remorqueur est moyenne. Ensuite, vous pourrez peut-être attraper «Hareng portuaire HQ». Et si vous pouvez l'attraper, utilisez l'action «Pêche au vif».
3. Utilisez l'action «Ferrage» lorsque la force du remorqueur est élevée. Ensuite, vous pourrez peut-être attraper «Mégalopoulpe».

## 3. Quelle action devez-vous utiliser, «Ferrage puissant» ou «Ferrage précis» ?

Lorsque vous utilisez l'action «Patience» pour attraper du poisson, les notes sur les poissons supposent que les actions suivantes sont utilisées par défaut.

- Utilisez l'action «Ferrage précis» si la force du remorqueur est légère («!»)
- Utilisez l'action «Ferrage puissant» si la force du remorqueur est moyenne («!!»)
- Utilisez l'action «Ferrage puissant» si la force du remorqueur est lourde («!!!»)

Si l'action ci-dessus est inappropriée pour un poisson, la note indiquera l'action appropriée pour ce poisson.

Par exemple, si la note dit `Cloporte ⇒ (!!! Ferrage précis) Papillon d'or`, il convient d'utiliser l'action «Ferrage précis» au lieu de «Ferrage puissant».
