 HOW TO RUN:
1. Open RaceDayAPI.sln in Visual Studio
2. Press F5 to start
3. Swagger opens at: https://localhost:7224/swagger

 DATABASE:
- Uses SQL Server LocalDB
- Connection string in appsettings.json
- Run these in Package Manager Console if needed:
  Add-Migration InitialCreate
  Update-Database

ENDPOINTS:
- POST /api/Auth/Register — Create account
- POST /api/Auth/Login   — Get JWT token
- GET /api/Events        — View all events
- POST /api/Events       — Create event
- GET /api/Categories    — View categories
- GET /api/Enrollments   — View enrollments
- GET /api/Results       — View race results
 LOGIN DETAILS:
- Admin: admin@raceday.com / Admin123!
- User:  user@raceday.com  / User123!
