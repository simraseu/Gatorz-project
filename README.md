--Systembeskrivelse--
Dette projekt er et rejsebooking system som er er blevet lavet i ASP.NET Core Blazor. Den tillader brugere at søge efter rejser og booke 
dem hvor både hotel og fly bliver sammenlagt og vist som et samlet rejsetilbud. Systemet bruger ekstern API-integration og der bruges
Amadeus for developers API for at hente både flytilbud og hoteltilbud. Systemet inkluderer også rollebaseret funktionalitet hvor der findes en
Admin, SalesAgent og Customer, som har hver især deres egne funktioner som de kan tilgå og benytte sig af.

* Framework: ASP.NET Core 8.0
* Projekttype: Blazor Server Web App
* Database: Entity Framework og SQL Server Management Studio
* Authentication: ASP.NET Core Identity
* Hosting: Azure

--Opsætningsvejledning--

      
*NuGet: Microsoft.AspNetCore.Components.WebAssembly.Server
        Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
        Microsoft.AspNetCore.Identity.EntityFrameworkCore 
        Microsoft.EntityFrameworkCore
        Microsoft.EntityFrameworkCore.Design
        Microsoft.EntityFrameworkCore.SqlServer
        Microsoft.EntityFrameworkCore.Tools
        Newtonsoft.Json

*Opsætning af database: 1. Naviger til appsettings.json under serversiden af solution explorer og udskift servernavnet med dit eget lokale sql servvernavn i ConnectionString > Husk "Build Solution"
                        2. Naviger til Tools > NuGet Package Manager > Package Manager Console
                        3. Skriv Update-Database og "enter"
                        4. Databasen bør nu være oprettet i SQL Server Management Studio i din egen lokale database server med alle tabeller.
                        
--Når man kører appen og har browseren åbent--
Man kan registrere sig som bruger(customer),
eller man kan indtaste følgende for at logge ind som Admin.

Email:    admin@Gotorz.com
Password: Admin123!



