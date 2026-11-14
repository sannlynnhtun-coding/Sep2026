Database access defaults to Windows integrated authentication against local SQL Server. Override with the `ConnectionStrings__DefaultConnection` environment variable; keep credentials outside source control. Rotate any previously exposed database credentials.

Optional seed import: place `ZodiacAndHoroscopeData.json` in `DataFiles` under this project before starting. Without the file, the API starts without importing data; use its CRUD endpoints to populate records.
