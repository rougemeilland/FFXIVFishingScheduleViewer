# À propos du format du mémo de poisson

---

[[日本語で表示する](AboutFishMemo.md)] [[Display in English](AboutFishMemo_en.md)] [Affichage en français]  
[[Revenir à README](README_fr.md)]

---



## 1.À propos de la force du remorqueur

«!» (Point d'exclamation) indique la force du remorqueur. Cela a la même signification que ce qui apparaît au-dessus de la tête du personnage du joueur lors de la capture d'un poisson.
  -  «!» signifie remorqueur léger.
  -  «!!» signifie remorqueur moyen.
  -  «!!!» signifie remorqueur louds.

## 2. Comment attraper un poisson

Comment pêcher est écrit dans le mémo comme suit.

```
<nom de l'appât de pêche>
    ⇒ (<points d'exclamation>)<nom du poisson>
```

Par exemple, s'il est écrit «Krill polaire ⇒ (!!) Homard des rochers», remplacez-le comme suit.

1. Utilisez l'action «Pêche» après avoir sélectionné «Krill polaire» comme appât de pêche.
2. Si vous utilisez l'action «Ferrage» lorsque la force du remorqueur est moyenne, vous pourrez peut-être attraper un «Homard des rochers».

De même, dans le cas de la Pêche au vif, il s'écrit comme suit.

```
<nom de l'appât de pêche>
    ⇒ (<points d'exclamation>)<nom du poisson>
    ⇒ (<points d'exclamation>)<nom du poisson>
```

Par exemple, si `Pill bug ⇒ (!!) Hareng portuaire HQ ⇒ (!!!) Mégalopoulpe)` est écrit, remplacez-le comme suit.

1. Utilisez l'action «Pêche» après avoir sélectionné «Cloportee comme appât de pêche.
2. Utilisez l'action «Ferrage» lorsque la force du remorqueur est moyenne. Ensuite, vous pourrez peut-être attraper «Hareng portuaire HQ». Et si vous pouvez l'attraper, utilisez l'action «Pêche au vif».
3. Utilisez l'action «Ferrage» lorsque la force du remorqueur est élevée. Ensuite, vous pourrez peut-être attraper «Mégalopoulpe».

## 3. Dois-je utiliser «Ferrage puissant» ou «Ferrage précis» ?

Lorsque vous utilisez l'action «Patience», utilisez les actions «Ferrage» suivantes par défaut.

- Utilisez l'action «Ferrage précis» si la force du remorqueur est «!» (léger remorqueur)
- Utilisez l'action «Ferrage puissant» si la force du remorqueur est «!!» (remorqueur moyen)
- Utilisez l'action «Ferrage puissant» si la force du remorqueur est «!!!» (remorqueur lourd)

Si le poisson mentionné ci-dessus ne convient pas à l'hameçon, la note indiquera l'hameçon approprié pour ce poisson.

Par exemple, si la note dit `Cloporte ⇒ (!!! Ferrage précis) Papillon d'or`, il convient d'utiliser l'action «Ferrage précis» au lieu de «Ferrage puissant».
