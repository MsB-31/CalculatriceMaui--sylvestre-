# Calculatrice Mobile – .NET MAUI

Application de calculatrice mobile développée avec **.NET MAUI** dans le cadre de l'Activité n°4 de l'Atelier de développement Mobile (ENSPD, Génie Logiciel).

L'interface s'adapte aux différentes tailles d'écran et aux changements d'orientation (portrait / paysage), sans déformation ni débordement.

## Aperçu

| Portrait | Paysage |
|---|---|
| ![Portrait](docs/portrait.png) | ![Paysage](docs/paysage.png) |

> Démonstration : voir la vidéo MP4 déposée dans l'espace de cours.

##  Fonctionnalités

**Opérations de base**
- Addition, soustraction, multiplication, division
- Saisie de nombres décimaux
- Remise à zéro totale (`C`) et effacement du dernier caractère (`⌫`)
- Changement de signe (`±`) et pourcentage (`%`)
- Gestion explicite de la division par zéro (message d'erreur, aucun plantage)
- Affichage de l'opération en cours au-dessus du résultat

**Gestion des cas limites**
- Un seul séparateur décimal par nombre
- Enchaînement d'opérateurs sans erreur
- Résultats longs défilables au lieu de déborder

## Layouts utilisés

Plusieurs types de layouts sont combinés au sein d'une même page (`MainPage.xaml`) :

| Layout | Rôle dans l'interface | Justification |
|---|---|---|
| `Grid` | Clavier (chiffres et opérateurs) | Lignes et colonnes en `*` : les boutons se répartissent l'espace de façon uniforme sur tout écran |
| `VerticalStackLayout` | Bloc d'affichage (opération + résultat) | Empile naturellement l'opération en cours au-dessus du résultat |
| `Border` | Cadre de l'écran d'affichage | Coins arrondis et contour, sans ajouter de logique de disposition |
| `ScrollView` | Zone du résultat | Permet de faire défiler un long nombre horizontalement au lieu de déborder |

> Adapte ce tableau aux layouts réellement présents dans ton code.

##  Structure du projet

```
CalculatriceMaui/
├── App.xaml / App.xaml.cs        # Point d'entrée de l'application
├── AppShell.xaml / .cs           # Navigation
├── MainPage.xaml                 # Interface (layouts)
├── MainPage.xaml.cs              # Logique et gestionnaires d'événements
├── MauiProgram.cs                # Configuration de l'application
├── Platforms/                    # Code spécifique par plateforme
├── Resources/                    # Styles, polices, images, icônes
└── CalculatriceMaui.csproj
```

## Installation et exécution

**Prérequis**
- [SDK .NET](https://dotnet.microsoft.com/download) (8.0 ou supérieur)
- Workload MAUI : `dotnet workload install maui`
- Un émulateur Android ou un appareil connecté

**Lancement**

```bash
git clone https://github.com/<ton-compte>/<ton-depot>.git
cd <ton-depot>
dotnet build -f net8.0-android
dotnet build -t:Run -f net8.0-android
```

Ou ouvre la solution dans Visual Studio / VS Code (extension .NET MAUI) et lance l'émulateur de ton choix.

## Technologies

- .NET MAUI (Single Project)
- XAML et C#
- Git / GitHub

##  Auteur

**Mbaïammadji Sylvestre Banyo (MsB)**
Étudiant en Génie Logiciel, ENSPD – Douala, Cameroun

##  Ressources

- [Documentation .NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [Documentation des layouts](https://learn.microsoft.com/dotnet/maui/user-interface/layouts/)
