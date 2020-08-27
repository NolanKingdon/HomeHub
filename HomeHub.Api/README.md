## Articles

- https://florimond.dev/blog/articles/2018/08/restful-api-design-13-best-practices-to-make-your-users-happy/


## Endpoints
/api/[controller]

### Database Related
- GET: Unsorted Spotify genres
  - /api/database/Spotify/genres
- GET: Authentication Token
  - /api/database/Spotify/authentication

### System Related
- POST: Add user
- GET: Quick Queried logs?
  - /api/system/logs/{Type}/{[starttime?]}
- GET: Temperatures
  - /api/system/logs/temperatures/
- POST: Start download
- GET: DownloadStatus
- POST: Activate VPN
- POST: Deactivate VPN
