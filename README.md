# RTFM: Real-Time Freakout Monitor

So yes, this is a performance dashboard or performance.
And yes, it will snitch on your machine/server by email when usage goes nuclear.
This is a test project to learn Dotnet with Realtime SignalR from Microslop

## Main idea

- backend watches system metrics
- frontend shows pretty live numbers
- alerts fire when thresholds are crossed
- your inbox gets panic mail (respecting cooldown)

## Current Limitation:

- Incorrect GPU usage because idk Dotnet dont really provide api for this especially if you are using multiple GPU like me then yeah, the number maybe incorrect for windows user (Linux of course will be better lol)

## Plan:

- More components ? (Like multiple CPU cores, threads for server)
- More functionality

## Tech Stack

- ASP.NET Core (.NET 10)
- SignalR (live updates, no page refresh nonsense)
- Next.js + React + TypeScript
- MailKit (for Email alerts) and using Gmail SMTP, you can change to different SMTP if you like
- xUnit (because we are at least a little responsible)
- Jest (for testing)
- Hardware.Info (for getting system info)

## Project Structure

- `backdoor/` -> API, SignalR hub, background worker, email alert logic
- `frontdoor/` -> dashboard UI
- `backdoor.Tests/` -> unit tests
- `backdoor.slnx` -> solution file

## What It Does

- Streams CPU, memory, GPU, disk, and OS info in real time
- Lets you set alert thresholds for CPU/memory/GPU/disk
- Lets you set alert cooldown (minutes)
- Lets you set recipient email
- Sends SignalR alert events + email when thresholds are hit

## How It Works (quick and painless)

1. `PulseWorker` runs every second.
2. It grabs metrics from `ISysMonitor`.
3. It sends live data to clients via SignalR event `ReceiveData`.
4. If usage >= threshold, backend sends `SendAlert` and tries email.
5. Cooldown stops your inbox from becoming a crime scene.

## Requirements

- .NET SDK 10 preview (project targets `net10.0`)
- Node.js 20+
- npm
- Gmail + App Password (for SMTP)

## Config

Update `backdoor/appsettings.json` (or better: user-secrets/env vars):

```json
{
  "Email": {
    "UserEmail": "your_email@gmail.com",
    "AppPassword": "your_gmail_app_password"
  },
  "Alert": {
    "CpuThresholdPercent": 90,
    "MemoryThresholdPercent": 90,
    "GpuThresholdPercent": 95,
    "DiskThresholdPercent": 95,
    "CooldownMinutes": 10
  },
  "Gmail": {
    "AlertTo": "ops@example.com"
  }
}
```

Notes:
- `Email:*` is used for SMTP auth.
- `Gmail:AlertTo` is optional default recipient.
- If no alert recipient is set, it falls back to sender email.

## Endpoints

- SignalR hub: `http://localhost:5276/hubs/monitor`
- GET alert settings: `http://localhost:5276/api/alerts/settings`
- POST alert settings: `http://localhost:5276/api/alerts/settings`

Example POST body:

```json
{
  "cpuThresholdPercent": 85,
  "memoryThresholdPercent": 88,
  "gpuThresholdPercent": 90,
  "diskThresholdPercent": 92,
  "cooldownMinutes": 15,
  "alertToEmail": "ops@example.com"
}
```

## Tests

```powershell
dotnet test backdoor.Tests/backdoor.Tests.csproj
```

## Heads Up

- Frontend currently points to fixed backend URL `http://localhost:5276`.
- Backend CORS currently allows `http://localhost:3000`.

## Security (serious for 2 seconds)

- Do not commit real email credentials.
- Use `dotnet user-secrets` or environment variables.
- If credentials were committed, rotate app passwords immediately.
