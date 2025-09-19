# Feature: Envoi de QR Codes par Email

## Description
Cette feature permet aux Administrators de l'application Kont d'envoyer des QR codes par email aux joueurs pour qu'ils puissent rejoindre un événement spécifique et un pool spécifique.

## Fonctionnalités

### Endpoint
- **POST** `/Events/{eventId}/send-qr-codes`
- **Autorisation**: Admin ou Manager uniquement
- **Body**: `List<string>` (liste d'emails)

### Request Body
```json
["email1@example.com", "email2@example.com", "email3@example.com"]
```

### URL générée
L'URL générée pour le QR code suit le format :
```
{ApplicationBaseUrl}/event/{event.EventLink}/{pool.Id}
```

## Services implémentés

### IQrCodeService
- Génère des QR codes en PNG (bytes ou base64)
- Utilise la bibliothèque QRCoder

### IEmailService  
- Envoie des emails HTML avec pièces jointes
- Utilise MailKit pour SMTP
- Support des templates HTML

### IEmailTemplateService
- Rend les templates Razor pour les emails
- Support de la compilation runtime des vues
- Gestion des modèles typés pour les templates

### IEventInvitationService
- Orchestre l'envoi des QR codes
- Utilise les templates Razor pour les emails
- Gère la validation des événements et pools
- Prend automatiquement le premier pool de l'événement

## Configuration

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

## Templates d'email
- **Template Razor** : `Views/EmailTemplates/EventInvitation.cshtml`
- Support français/anglais avec localisation dynamique
- Design responsive avec CSS inline
- QR code intégré en base64
- Lien de fallback cliquable
- Informations de l'événement (nom, site, date)
- Template maintenable et facilement modifiable

## Tests
- Tests unitaires pour le contrôleur
- Tests unitaires pour QrCodeService
- Validation des cas d'erreur (événement non trouvé, pool non trouvé, etc.)

## Packages ajoutés
- `QRCoder` (1.6.0) - Génération de QR codes
- `MailKit` (4.13.0) - Envoi d'emails SMTP

## Utilisation
1. L'administrator sélectionne un événement et un pool
2. Il saisit la liste des emails des joueurs
3. Il peut choisir la langue (fr/en) et s'il veut recevoir une copie
4. Le système génère un QR code unique pour l'URL de l'événement/pool
5. Un email HTML est envoyé à chaque destinataire avec le QR code intégré
6. Les joueurs peuvent scanner le QR code ou cliquer sur le lien pour rejoindre l'événement
