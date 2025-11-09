# FantaScommesse - PWA Pronostici Serie A

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

Applicazione web progressiva (PWA) per gestire un gioco di pronostici tra amici sulla Serie A. Sistema completo con gestione utenti, pronostici, calcolo punteggi con bonus/penalità, classifiche e pagamenti.

## 🎯 Caratteristiche Principali

- **PWA completa**: Installabile, funziona offline, notifiche push
- **Sistema pronostici 4-3-3**: 4 doppie, 3 fisse, 3 speciali (GG/NG/OVER/UNDER)
- **Calcolo automatico punteggi**: Con bonus complessi (10/10 solo/condiviso, top score, risultati unici, fisse uniche)
- **Penalità**: Ritardi e errori di compilazione
- **Classifiche**: Giornaliere, generali, andata/ritorno
- **Gestione pagamenti**: Piani rateali, sconti referral
- **API REST complete**: Autenticazione JWT, endpoint per tutte le operazioni
- **Database SQL Server**: Schema completo con audit trail

## 🏗️ Architettura

### Stack Tecnologico

- **Backend**: ASP.NET Core 8.0, C#
- **Data Access**: ADO.NET (puro, no ORM)
- **Database**: SQL Server 2019+
- **Frontend**: Razor Views + JavaScript vanilla
- **PWA**: Service Worker, Web App Manifest
- **Autenticazione**: JWT Bearer + BCrypt
- **Pattern**: Repository Pattern, Dependency Injection, MVC

### Struttura Progetto

```
FantaScommesse/
├── API/                    # API Controllers (REST endpoints)
│   ├── AuthController.cs
│   ├── PredictionsController.cs
│   ├── RoundsController.cs
│   └── ScoreboardsController.cs
├── Controllers/            # MVC Controllers (Views)
│   └── HomeController.cs
├── Models/
│   ├── Entities/          # Domain entities
│   │   ├── BaseAuditEntity.cs
│   │   ├── User.cs
│   │   ├── Season.cs
│   │   ├── Prediction.cs
│   │   └── ...
│   └── DTOs/              # Data Transfer Objects
│       ├── LoginRequestDto.cs
│       ├── PredictionSubmitDto.cs
│       └── ...
├── Repositories/          # Data access (ADO.NET)
│   ├── IRepository.cs
│   ├── BaseRepository.cs
│   ├── UserRepository.cs
│   ├── PredictionRepository.cs
│   └── ...
├── Services/              # Business logic
│   ├── AuthService.cs
│   └── ScoringService.cs (calcolo punteggi)
├── Validators/            # Validation logic
│   └── PredictionValidator.cs
├── Views/                 # Razor views
│   ├── Shared/
│   │   └── _Layout.cshtml
│   └── Home/
│       └── Index.cshtml
├── wwwroot/               # Static files
│   ├── manifest.webmanifest
│   ├── sw.js             # Service Worker
│   ├── css/
│   ├── js/
│   └── images/
├── db/                    # Database scripts
│   ├── migrations/
│   │   ├── 001_create_schema.sql
│   │   ├── 002_create_indexes.sql
│   │   └── 003_create_views.sql
│   └── seed/
│       └── 001_seed_data.sql
├── Program.cs
├── appsettings.json
└── FantaScommesse.csproj
```

## 📋 Requisiti di Sistema

### Prerequisiti

- .NET 8.0 SDK o superiore
- SQL Server 2019+ (o SQL Server Express)
- Browser moderno con supporto PWA (Chrome, Edge, Firefox, Safari)

### Sviluppo

```bash
# Verifica versione .NET
dotnet --version  # Deve essere >= 8.0

# SQL Server
# Assicurati che SQL Server sia in esecuzione e accessibile
```

## 🚀 Setup e Installazione

### 1. Clone del Repository

```bash
git clone https://github.com/yourusername/FantaScommesse.git
cd FantaScommesse
```

### 2. Configurazione Database

#### a) Crea il database SQL Server

```sql
CREATE DATABASE FantaScommesse;
GO
```

#### b) Esegui gli script di migrazione

```bash
# Esegui gli script nell'ordine:
sqlcmd -S localhost -d FantaScommesse -i db/migrations/001_create_schema.sql
sqlcmd -S localhost -d FantaScommesse -i db/migrations/002_create_indexes.sql
sqlcmd -S localhost -d FantaScommesse -i db/migrations/003_create_views.sql

# Opzionale: carica dati di esempio
sqlcmd -S localhost -d FantaScommesse -i db/seed/001_seed_data.sql
```

### 3. Configurazione Applicazione

#### a) Aggiorna `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FantaScommesse;User Id=SA;Password=YourPassword;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "CAMBIA_QUESTA_CHIAVE_CON_UNA_STRINGA_SICURA_DI_ALMENO_32_CARATTERI",
    "Issuer": "FantaScommesse",
    "Audience": "FantaScommesseUsers",
    "ExpiryMinutes": 1440
  }
}
```

**IMPORTANTE**:
- Cambia la password del database
- Genera una chiave JWT sicura (min 32 caratteri)
- Non committare mai `appsettings.json` con dati sensibili

#### b) Crea `appsettings.Development.json` (gitignored)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FantaScommesse;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### 4. Installa Dipendenze

```bash
dotnet restore
```

### 5. Build del Progetto

```bash
dotnet build
```

### 6. Esegui l'Applicazione

```bash
dotnet run
```

L'applicazione sarà disponibile su:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## 📱 PWA - Progressive Web App

### Installazione PWA

1. Apri l'app in un browser supportato (Chrome, Edge, Safari)
2. Cerca l'icona "Installa" nella barra degli indirizzi
3. Oppure usa il pulsante "Installa App" nell'interfaccia

### Funzionalità Offline

- **Service Worker**: Cache delle risorse statiche e API
- **Background Sync**: Sincronizzazione pronostici offline
- **Push Notifications**: Notifiche per deadline e risultati

### Configurazione Notifiche Push

Per abilitare le notifiche push, configura VAPID keys:

```bash
# Genera chiavi VAPID
npm install -g web-push
web-push generate-vapid-keys

# Aggiorna wwwroot/js/app.js con la chiave pubblica
```

## 🔑 API Endpoints

### Autenticazione

```
POST /api/v1/auth/register
POST /api/v1/auth/login
```

### Giornate e Partite

```
GET  /api/v1/rounds/current
GET  /api/v1/rounds/{roundId}
```

### Pronostici

```
POST /api/v1/predictions/{roundId}        # Crea/aggiorna bozza
POST /api/v1/predictions/{roundId}/submit # Invio definitivo
GET  /api/v1/predictions/{roundId}        # Dettagli pronostico
```

### Classifiche

```
GET  /api/v1/scoreboards/{roundId}        # Classifica giornaliera
GET  /api/v1/scoreboards/season/{seasonId} # Classifica stagionale
```

### Esempi Richieste

#### Login

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

#### Invia Pronostico

```bash
curl -X POST https://localhost:5001/api/v1/predictions/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "roundId": 1,
    "isDraft": false,
    "items": [
      {"matchId": 1, "selection": "1X"},
      {"matchId": 2, "selection": "X2"},
      {"matchId": 3, "selection": "12"},
      {"matchId": 4, "selection": "1X"},
      {"matchId": 5, "selection": "1"},
      {"matchId": 6, "selection": "X"},
      {"matchId": 7, "selection": "2"},
      {"matchId": 8, "selection": "GG"},
      {"matchId": 9, "selection": "NG"},
      {"matchId": 10, "selection": "OVER"}
    ]
  }'
```

## 🎮 Regole del Gioco

### Compilazione Colonna (10 partite)

- **4 doppie**: 1X, X2 o 12
- **3 fisse**: 1, X o 2
- **3 speciali**: GG, NG, OVER o UNDER

### Punteggi

#### Punti Base
- 1 punto per ogni pronostico corretto (max 10/10)

#### Bonus
- **10/10 solo**: +5 punti (se unico con 10/10)
- **10/10 condiviso**: +3 punti (se 10/10 con altri)
- **Top score solo**: +3 punti (punteggio più alto <10, da solo)
- **Top score condiviso**: +1 punto (punteggio più alto <10, con altri)
- **Risultato unico**: +5 punti (unico con una selezione corretta su un match)
- **Fissa unica**: +1 punto (unico con una fissa corretta)

#### Penalità
- **Ritardo**: -1 punto (invio dopo deadline)
- **Errori**: -1 punto + **esclusione dai bonus** (errori di compilazione)

### Premi

- **Settimanali**: In base al numero di partecipanti
- **Finali**: 1°, 2°, 3° posto (fasce per soglie partecipanti)
- **Mid-season**: Opzionali per andata/ritorno

## 🗃️ Database

### Schema Principale

- `fs_user`: Utenti (partecipanti, organizzatori, admin)
- `fs_season`: Stagioni
- `fs_team`: Squadre Serie A
- `fs_round`: Giornate
- `fs_match`: Partite
- `fs_participation`: Iscrizioni utente/stagione
- `fs_prediction`: Pronostici (colonne)
- `fs_prediction_item`: Singole selezioni
- `fs_score`: Punteggi calcolati
- `fs_bonus_event`: Eventi bonus
- `fs_penalty_event`: Eventi penalità
- `fs_payment`: Pagamenti
- `fs_referral`: Referral
- `fs_prize_rule`: Regole premi

### Viste Utili

- `vw_round_scoreboard`: Classifica giornaliera
- `vw_season_scoreboard`: Classifica stagionale
- `vw_user_payment_status`: Stato pagamenti
- `vw_match_results`: Risultati partite con nomi squadre
- `vw_prediction_detail`: Dettaglio pronostici con correttezza

## 🧪 Testing

### Dati di Test

Dopo aver eseguito lo script `db/seed/001_seed_data.sql`, avrai:

- **Admin**: `admin@fantascommesse.it` / `Admin123!`
- **Organizer**: `organizer@fantascommesse.it` / `Organizer123!`
- **Users**: `user1@test.it` ... `user5@test.it` / `Test123!`
- **Squadre**: 20 squadre Serie A
- **Stagione**: Serie A 2025/26
- **Giornata 1**: 10 partite configurate

### Test Manuale

1. Accedi come utente test
2. Compila un pronostico per la giornata 1
3. Come admin/organizer, inserisci i risultati delle partite
4. Esegui il calcolo punteggi (implementare endpoint admin)
5. Verifica la classifica

## 🔒 Sicurezza

### Best Practices Implementate

- ✅ Password hashing con BCrypt
- ✅ JWT con scadenza configurabile
- ✅ Prepared statements (ADO.NET parametrizzato)
- ✅ HTTPS enforcement (production)
- ✅ CORS configurabile
- ✅ Audit trail completo (chi/quando crea/modifica)
- ✅ Validazione input lato server

### Raccomandazioni Produzione

- [ ] Abilita HTTPS con certificato valido
- [ ] Configura CORS per domini specifici
- [ ] Implementa rate limiting
- [ ] Abilita logging strutturato (Serilog)
- [ ] Configura backup automatici database
- [ ] Monitora performance e errori (Application Insights)
- [ ] Implementa health checks

## 🚢 Deployment

### Azure App Service (Raccomandato)

```bash
# Pubblica su Azure
dotnet publish -c Release -o ./publish

# Deploy tramite Azure CLI
az webapp deployment source config-zip \
  --resource-group FantaScommesse-RG \
  --name fantascommesse-app \
  --src publish.zip
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FantaScommesse.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FantaScommesse.dll"]
```

```bash
# Build
docker build -t fantascommesse:latest .

# Run
docker run -d -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Secret="..." \
  fantascommesse:latest
```

## 📊 Performance

### Ottimizzazioni Implementate

- Indici su tutte le foreign keys e colonne di ricerca
- Viste materializzate per classifiche
- Cache API con Service Worker
- Lazy loading componenti frontend
- Connection pooling ADO.NET
- Transazioni per operazioni atomiche

### Benchmark Attesi

- Calcolo giornata (1000 utenti): < 5 secondi
- Query classifica: < 100ms
- Login/Register: < 200ms

## 🤝 Contribuire

1. Fork del progetto
2. Crea un branch (`git checkout -b feature/AmazingFeature`)
3. Commit dei cambiamenti (`git commit -m 'Add AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Apri una Pull Request

## 📝 Roadmap

### MVP (Completato)
- [x] Autenticazione JWT
- [x] Gestione pronostici con validazione 4-3-3
- [x] Calcolo automatico punteggi e bonus
- [x] Classifiche giornaliere e stagionali
- [x] PWA con offline support
- [x] API REST complete

### Fase 2 (Prossimi sviluppi)
- [ ] Integrazione API esterne risultati (TheSportsDB/Sportmonks)
- [ ] Pagamenti online (Stripe/PayPal)
- [ ] Login social (Google/Facebook)
- [ ] Multi-leghe e competizioni personalizzate
- [ ] Statistiche avanzate e grafici
- [ ] Chat/commenti tra partecipanti
- [ ] Mobile app nativa (React Native/Flutter)

## 📄 Licenza

Questo progetto è distribuito sotto licenza MIT. Vedi il file `LICENSE` per maggiori dettagli.

## 👥 Autori

- **Il tuo nome** - *Initial work* - [YourGitHub](https://github.com/yourusername)

## 🙏 Riconoscimenti

- Ispirato alle classiche schedine del Totocalcio
- Built with ❤️ per gli amanti del calcio e dei pronostici
- Documentazione basata sulle specifiche funzionali fornite

## 📧 Supporto

Per domande o supporto:
- 📧 Email: support@fantascommesse.it
- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/FantaScommesse/issues)
- 📚 Docs: [Wiki](https://github.com/yourusername/FantaScommesse/wiki)

---

**Buon divertimento con FantaScommesse! ⚽🏆**
