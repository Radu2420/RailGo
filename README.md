# RailGo - Sistem informatic pentru rezervarea biletelor feroviare

RailGo este o aplicație web realizată în cadrul lucrării de licență, având ca scop rezervarea, emiterea și administrarea biletelor feroviare pentru o companie privată de transport.

Aplicația permite utilizatorului să caute trenuri disponibile, să aleagă ruta, data călătoriei, clasa, vagonul și locul în tren. După completarea datelor pasagerului și confirmarea unei plăți simulate, sistemul generează un bilet digital cu cod QR și trimite automat biletul pe email.

## Funcționalități principale

- căutarea trenurilor după stația de plecare, destinație și dată;
- filtrarea destinațiilor în funcție de stația de plecare;
- rezervare bilet dus și dus-întors;
- alegerea clasei de călătorie;
- alegerea vagonului și a locului în tren;
- afișarea locurilor libere, ocupate și selectate;
- aplicarea reducerii de 50% pentru studenți;
- plată simulată;
- generarea biletului digital;
- generarea codului QR pentru validare;
- trimiterea automată a biletului pe email;
- istoric bilete pe baza adresei de email;
- dashboard administrativ;
- calcul grad de ocupare tren;
- estimare profitabilitate rută.

## Tehnologii utilizate

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- HTML
- CSS
- JavaScript
- QRCoder
- SMTP pentru trimiterea emailurilor

## Cerințe pentru rulare

Pentru rularea aplicației sunt necesare:

- .NET SDK instalat;
- SQL Server / SQL Server Express;
- Visual Studio Code sau Visual Studio;
- un browser web modern;
- configurarea conexiunii la baza de date în `appsettings.json`.

## Configurarea aplicației

Înainte de rulare, se creează fișierul `appsettings.json` pe baza fișierului `appsettings.example.json`.

Exemplu de configurație:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=RailGoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "RailGo",
    "SenderEmail": "email@example.com",
    "Username": "email@example.com",
    "Password": "parola_aplicatie"
  }
}