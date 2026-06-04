# RailGo - Sistem informatic pentru rezervarea biletelor feroviare

RailGo este o aplicație web realizată pentru lucrarea de licență, având ca scop digitalizarea procesului de rezervare și emitere a biletelor feroviare. Aplicația permite căutarea trenurilor disponibile, selectarea rutei, alegerea locului în tren, completarea datelor pasagerului, simularea plății și generarea unui bilet digital cu cod QR.

Proiectul este realizat pentru o companie privată fictivă de transport feroviar și urmărește să ilustreze modul în care un proces clasic de vânzare a biletelor poate fi transformat într-un flux digital, mai rapid și mai ușor de administrat.

## Funcționalități principale

Aplicația include următoarele funcționalități:

* căutarea trenurilor după stația de plecare, destinație și dată;
* filtrarea automată a destinațiilor în funcție de stația de plecare;
* rezervarea biletelor de tip dus și dus-întors;
* alegerea clasei de călătorie;
* alegerea vagonului și a locului în tren;
* afișarea locurilor libere, ocupate și selectate;
* aplicarea reducerii de 50% pentru pasagerii de tip student;
* completarea datelor pasagerului;
* simularea plății online;
* generarea biletului digital;
* generarea codului QR pentru bilet;
* trimiterea automată a biletului pe email;
* consultarea istoricului de bilete pe baza adresei de email;
* modul de administrare cu indicatori despre rezervări, venituri, grad de ocupare și profitabilitate.

## Tehnologii utilizate

Proiectul a fost dezvoltat folosind următoarele tehnologii:

* C#;
* ASP.NET Core MVC;
* Entity Framework Core;
* SQL Server;
* HTML;
* CSS;
* JavaScript;
* QRCoder;
* SMTP pentru trimiterea emailurilor.

## Structura aplicației

Aplicația este organizată pe arhitectura MVC:

* `Controllers` - gestionează logica aplicației și acțiunile utilizatorului;
* `Models` - definesc entitățile principale ale aplicației;
* `ViewModels` - conțin datele necesare pentru afișarea paginilor;
* `Views` - conțin paginile aplicației;
* `Data` - conține contextul bazei de date și datele demonstrative;
* `Services` - conține logica pentru trimiterea emailurilor;
* `Database` - conține scriptul SQL pentru baza de date;
* `wwwroot` - conține fișierele CSS, JavaScript și resursele statice.

## Cerințe pentru rulare

Pentru rularea aplicației sunt necesare:

* .NET SDK instalat;
* SQL Server sau SQL Server Express;
* Visual Studio Code sau Visual Studio;
* un browser web modern;
* configurarea conexiunii la baza de date;
* configurarea unui cont SMTP pentru trimiterea emailurilor.

## Configurarea aplicației

Înainte de rulare, se creează fișierul `appsettings.json` pe baza fișierului `appsettings.example.json`.

Exemplu de configurare:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=RailGoDb;Trusted_Connection=True;TrustServerCertificate=True;"
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
```

Fișierul `appsettings.json` nu este inclus în repository, deoarece poate conține date sensibile. Pentru rulare locală, acesta trebuie creat manual.

## Baza de date

Baza de date conține informații despre stații, rute, trenuri, călătorii, rezervări și bilete.

Pentru configurarea bazei de date se poate utiliza scriptul SQL din folderul:

```text
Database/RailGo_Database.sql
```

De asemenea, proiectul conține migrații Entity Framework Core, care pot fi folosite pentru generarea structurii bazei de date.

## Rularea aplicației

După configurarea bazei de date și a fișierului `appsettings.json`, aplicația poate fi rulată local cu următoarele comenzi:

```bash
dotnet restore
dotnet build
dotnet run
```

După pornire, aplicația poate fi accesată în browser la adresa afișată în terminal.

## Fluxul principal al aplicației

Utilizatorul începe prin completarea formularului de căutare a trenurilor. După alegerea rutei și a datei, aplicația afișează cursele disponibile. Utilizatorul alege trenul, selectează vagonul și locul, completează datele pasagerului și ajunge la pagina de plată simulată.

După confirmarea plății simulate, sistemul generează rezervarea și biletul digital. Biletul conține informații despre pasager, rută, tren, vagon, loc, clasă, preț și cod QR. La final, biletul este trimis automat pe emailul introdus de pasager.

## Modulul de administrare

Aplicația include și o zonă de administrare, unde pot fi urmărite informații despre activitatea sistemului. Dashboard-ul afișează date despre rezervări, bilete emise, venituri, grad de ocupare și profitabilitate estimată.

Această parte a aplicației are rolul de a demonstra utilitatea datelor colectate în procesul de rezervare pentru analiza activității unei companii de transport feroviar.

## Observații

Aplicația are scop demonstrativ și a fost realizată în cadrul lucrării de licență. Plata este simulată și nu se realizează tranzacții financiare reale. Datele cardului nu sunt salvate în baza de date.

Proiectul poate fi extins prin integrarea unui procesator real de plăți, introducerea conturilor de utilizator, validarea reală a codului QR și dezvoltarea unui modul administrativ complet pentru gestionarea rutelor, trenurilor și călătoriilor.
