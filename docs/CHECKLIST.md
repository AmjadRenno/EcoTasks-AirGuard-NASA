# ✅ Pre-Submission Checklist

## 🔒 Security (Critical)
- [x] `.dockerignore` fixed – Dockerfiles stay in the build context
- [x] API keys removed from public docs
- [x] Security warnings documented for contributors
- [x] Security headers implemented (`X-Content-Type-Options`, `X-Frame-Options`)
- [x] HSTS guarded behind production-only switch
- [x] HTTPS redirection configured for production

## 🐳 Docker Configuration
- [x] CORS origins corrected (client only)
- [x] Health checks wired into `/health`
- [x] `outbox` volume configured for reports
- [x] Multi-stage Dockerfiles optimized

## 📖 Documentation
- [x] API key placeholders used everywhere
- [x] Security practices summarized in `docs/SECURITY.md`
- [x] Future improvements captured for judges
- [x] Docker Compose profile example noted
- [x] Instructions to request API keys included

## 🧪 Testing Recommendations

### Before demo
```powershell
cd docker
docker compose down -v
docker compose up --build
```

Verify:
- Frontend: http://localhost:5188
- API: http://localhost:5100
- Health: http://localhost:5100/health
- Swagger: http://localhost:5100/swagger

Functional walk-through:
- Dashboard loads and city selector responds
- Charts render data
- Reports generate and download
- Alerts surface scenarios

### Troubleshooting
- Ports conflict: `netstat -ano | findstr :5100` (or `:5188`) then `taskkill /PID <id> /F`
- No data: confirm API keys in `src/AirGuard.Server/appsettings.json`
- Docker build failures: re-check `.dockerignore`

## 🎯 Judge Presentation Points

Technical highlights:
1. Clean architecture separation (client, API, shared DTOs)
2. Hardened security headers + CORS controls
3. One-command Docker deployment with health checks
4. Interactive Swagger for rapid testing
5. Air quality forecasts powered by AirNow
6. Data visualizations with charting components
7. Clear production roadmap (scaling, auth, logging)

Suggested demo flow:
1. Launch with `docker compose up`
2. Tour dashboard and switch cities
3. Show real-time AQI + forecasts
4. Generate a PDF report
5. Trigger the "Send to Authority" demo action
6. Highlight Swagger endpoints
7. Recap security upgrades

## 🚀 Final Checks Before Submission

- [ ] Test full Docker deployment end-to-end
- [ ] Click through every link in `README.md`
- [ ] Capture and attach fresh screenshots
- [ ] Store API keys locally (never commit)
- [ ] Smoke-test on a clean environment if time allows
- [ ] Rehearse the 2-minute pitch
- [ ] Re-read `docs/SECURITY.md`

## 📊 What changed (quick summary)

Critical fixes:
1. `.dockerignore` – removed Dockerfile exclusion
2. API docs – swapped real keys for placeholders

Security enhancements:
3. Added security headers (`nosniff`, `SAMEORIGIN`)
4. HTTPS/HSTS gated for production
5. Health checks standardized

Polish:
6. CORS now client-only
7. Future improvements documented
8. Security playbook published

---

**Status:** ✅ Ready for submission  
**Last review:** October 5, 2025
