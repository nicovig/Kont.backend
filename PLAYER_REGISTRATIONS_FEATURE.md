# Feature - Gestion des PlayerRegistration

## Description
Cette feature permet de visualiser les joueurs inscrits à un événement via leur PlayerRegistration. Elle affiche la liste des joueurs qui se sont inscrits, leurs informations et l'heure de leur inscription/check-in.

## Architecture

### Backend

#### Endpoint
- **GET** `/Events/{eventId}/player-registrations`
- **Autorisation**: Admin ou Manager uniquement
- **Response**: `List<PlayerRegistrationResponse>`

#### DTO Response
```csharp
public class PlayerRegistrationResponse
{
    public Guid Id { get; set; }
    public string PlayerFirstname { get; set; }
    public string PlayerLastname { get; set; }
    public string PlayerEmail { get; set; }
    public string PlayerUsername { get; set; }
    public PlayerType PlayerType { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
}
```

#### Service
- `IEventsService.GetPlayerRegistrationsByEventAsync(Guid eventId)`
- Récupère le premier pool de l'événement
- Retourne les PlayerRegistration avec les informations du Player

### Frontend

#### Composant
- `PlayerRegistrationsComponent` - Affiche la liste des joueurs inscrits
- Intégré dans `EventDetailComponent`
- Table Material avec colonnes : Nom, Email, Username, Type, Inscription, Check-in

#### Service
- `AdminService.getPlayerRegistrations(eventId: string)`
- Appel direct à l'API (pas de NgRx pour l'instant)

## Fonctionnalités

### Affichage
- **Liste des joueurs** inscrits à l'événement
- **Informations complètes** : nom, email, username, type
- **Timestamps** : date d'inscription et de check-in
- **Statut visuel** : check-in effectué ou en attente
- **Types de joueurs** : Joueur normal ou Joueur clé

### Interface
- **Table responsive** avec Material Design
- **Loading state** pendant le chargement
- **Empty state** quand aucun joueur inscrit
- **Icons** pour chaque type d'information
- **Chips** pour les types de joueurs

## Structure de données

### PlayerRegistration (DB)
```csharp
public class PlayerRegistration
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public Pool Pool { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
}
```

### Player (DB)
```csharp
public class Player : User
{
    public string Username { get; set; }
    public PlayerType PlayerType { get; set; }
}

public class User
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
}
```

## Tests

### Backend
- `GetPlayerRegistrations_ReturnsOk_WhenValidEvent`
- `GetPlayerRegistrations_ReturnsUnauthorized_WhenNoUser`
- `GetPlayerRegistrations_ReturnsNotFound_WhenArgumentException`

### Scénarios testés
- Récupération réussie des inscriptions
- Gestion des erreurs (événement non trouvé, pas de pool)
- Autorisation requise

## Utilisation

1. **Administrator** ouvre la page de détail d'un événement
2. **Section "Joueurs inscrits"** s'affiche automatiquement
3. **Liste des joueurs** avec toutes leurs informations
4. **Refresh automatique** possible

## Notes techniques

- **Pool automatique** : utilise le premier pool de l'événement
- **Tri** : par date d'inscription (plus ancien en premier)
- **Performance** : requête optimisée avec Include
- **Responsive** : table adaptée aux petits écrans
