# Utility Billing System (C# WPF, MS SQL LocalDB, MVVM be bibliotekų)

## Reikalavimai
- Visual Studio 2022
- .NET 8 SDK
- SQL Server LocalDB (paprastai įdiegtas su VS)

## Paleidimas
1. Atidaryk `UtilityBillingSystem.sln`
2. Startup Project: `UtilityBillingSystem.UI.Wpf`
3. Paleisk (F5)

Programa automatiškai:
- sukuria DB `UtilityBillingSystemDb` LocalDB serveryje,
- sukuria lenteles (FK, UNIQUE, CHECK),
- jei vartotojų nėra – sukuria admin:
  - username: `admin`
  - password: `admin`

## Prisijungimų generavimas (pagal užduotį)
Admin kuriant vartotoją:
- username = Vardas (jei užimtas → Vardas1, Vardas2, ...)
- password = Pavardė (saugomas kaip PBKDF2 hash)

## Rolės
- Administratorius: bendrijų/paslaugų/vartotojų CRUD + auto prisijungimai
- Vadybininkas: paslaugų priskyrimas + kainų nustatymas
- Gyventojas: savo bendrijos paslaugų ir kainų peržiūra + paieška

## SQL
- Lentelių kūrimas: `database/schema.sql`
- Pagrindinės SQL užklausos yra Repository klasėse (Infrastructure projekte)
