# Exemple d'utilisation de l'API QR Code Email

## Endpoint
```
POST /Events/{eventId}/send-qr-codes
```

## Headers requis
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

## Exemple de requête

### 1. Envoyer des QR codes
```bash
curl -X POST "https://localhost:5000/Events/123e4567-e89b-12d3-a456-426614174000/send-qr-codes" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '[
    "joueur1@example.com",
    "joueur2@example.com",
    "joueur3@example.com"
  ]'
```

## Réponses

### Succès (200)
```json
{
  "message": "QR codes sent successfully"
}
```

### Erreur - Événement non trouvé (400)
```json
{
  "message": "Event not found"
}
```

### Erreur - Pool non trouvé (400)
```json
{
  "message": "No pool found in this event"
}
```

### Erreur - Non autorisé (401)
```json
{
  "message": "User not authenticated"
}
```

## Email généré

L'email envoyé contient :
- **Sujet** : "Rejoindre [Nom Événement] - Groupe [Nom Pool]"
- **Template HTML** : Design professionnel avec logo Kont
- **QR Code** : Image PNG intégrée en base64
- **Lien de fallback** : URL cliquable pour rejoindre l'événement
- **Informations** : Nom de l'événement, site, date, groupe

## URL générée

L'URL du QR code suit le format :
```
{ApplicationBaseUrl}/event/{event.EventLink}/{pool.Id}
```

Exemple : `https://kont.com/event/tournoi-2024/456e7890-e89b-12d3-a456-426614174001`

## Configuration requise

### appsettings.json
```json
{
  "ApplicationBaseUrl": "https://your-frontend-url.com",
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "noreply@kont.com"
  }
}
```

## Notes importantes

1. **Autorisation** : Seuls les utilisateurs avec le rôle Admin ou Manager peuvent utiliser cet endpoint
2. **Validation** : L'événement doit exister et avoir au moins un pool
3. **Pool** : Le premier pool de l'événement est utilisé automatiquement
4. **Performance** : L'envoi se fait en parallèle pour tous les emails
5. **Template** : Le template email est généré dynamiquement avec Razor
6. **Localisation** : Français par défaut
