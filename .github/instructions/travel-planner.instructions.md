---
applyTo: "**/Pages/TravelPlanner/**"
description: "Instructions metier pour YouSummarize et TravelPlanner"
---

# TravelPlanner

- Voyages : TravelDto ; resumes : YoutubeResumeDto ; categories : CategoryDto.
- Statuts : ToReview, Reviewed, Selected, Rejected.
- TravelSidebar emet des EventCallback et ne porte aucun etat de filtrage.
- Filtres multi-selection : HashSet<T> ; favoris : bool.
- Les filtres et les compteurs sont calcules par la page parente.
- Filtres visuels : MudPaper Outlined et PillClass(bool isActive), avec les
  classes rounded-xl cursor-pointer et mud-border-primary si actif.
- Icones Filled pour actif, Outlined pour inactif ; garder la meme icone.
- Compteurs : Typo.body2, Color.Secondary, a droite du label.
- Regrouper les voyages archives dans une entree Archives si IsArchived existe.