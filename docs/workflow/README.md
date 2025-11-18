# Development Workflow Guide

**Last Updated**: November 18, 2025  
**Purpose**: Guide for working effectively on the Warranty Management System project

This guide provides best practices for coding sessions, documentation maintenance, context management, and efficient development workflows. Use this to start any coding or debugging session.

## 🚀 Quick Start

**Before doing ANYTHING, check if Aspire is running, then start it if needed:**

```powershell
# Check if Aspire dashboard port is in use (17263 = HTTPS dashboard)
netstat -ano | findstr "17263"

# If nothing returned, Aspire is NOT running. Start it:
cd AppHost
dotnet run
```

**On first run, Aspire will prompt you for the OpenAI API key.**  
This key is stored securely in user secrets and reused for all future sessions.

**When Aspire starts successfully, you'll see:**
```
info: Aspire.Hosting.DistributedApplication[0]
      Now listening on: https://localhost:17263
info: Aspire.Hosting.DistributedApplication[0]
      Login to the dashboard at https://localhost:17263/login?t=...
info: Aspire.Hosting.DistributedApplication[0]
      Distributed application started. Press Ctrl+C to shut down.
```

This single command:
- ✅ Prompts for OpenAI API key (first run only)
- ✅ Starts backend API with OpenAI key configured
- ✅ Starts frontend dev server
- ✅ Starts PostgreSQL database
- ✅ Configures all service dependencies
- ✅ Opens monitoring dashboard at **https://localhost:17263** (hardcoded port)

**Why Aspire is mandatory:**
1. **OpenAI Integration**: Manages API key via user secrets (prompts on first run)
2. **Zero Configuration**: No manual setup of connection strings or ports
3. **Service Orchestration**: Ensures proper startup order and dependencies
4. **Monitoring**: Real-time logs and health checks in dashboard

**⚠️ DO NOT run services standalone** unless you have a specific reason and know what you're doing.  
**⚠️ DO NOT start Aspire twice** - check if port 17263 is in use first to avoid conflicts.

## 📋 Table of Contents

1. [Starting a Development Session](#starting-a-development-session)
2. [Documentation Strategy](#documentation-strategy)
3. [Context Management](#context-management)
4. [Work Session Best Practices](#work-session-best-practices)
5. [Batch Processing Guidelines](#batch-processing-guidelines)
6. [Documentation Maintenance](#documentation-maintenance)
7. [Code Review Checklist](#code-review-checklist)
8. [Debugging Sessions](#debugging-sessions)
9. [Git Workflow](#git-workflow)
10. [Quick Reference](#quick-reference)

---

## Starting a Development Session

### Pre-Session Checklist

Before writing any code, complete these steps:

#### 1. **Read Relevant Documentation**

```bash
# Essential reading based on task type:

# For Backend Work:
- docs/backend/[feature-name].md
- MyApi/README.md
- MyApi.Tests/README.md

# For Frontend Work:
- docs/frontend/frontend-workflows.md
- docs/frontend/frontend-design-system.md
- WarrantyApp.Web/README.md

# For Testing:
- docs/testing/README.md
- MyApi.Tests/README.md (backend tests)
- WarrantyApp.Web/e2e/README.md (E2E tests)

# For Infrastructure:
- docs/infra/[topic].md
- docs/setup/setup-quickstart.md
```

**Pro Tip**: Use `grep` to find documentation quickly:
```bash
# Find docs about a specific feature
grep -r "warranty expiration" docs/
grep -r "OCR" docs/backend/
```

#### 2. **Understand Current State**

Review the main README to understand:
- Current project status (Backend: 100%, Frontend: 100%)
- Test status (146 backend passing, Auth E2E suite passing, other E2E in progress)
- Recent changes (check git log)

```bash
# Check project status
cat README.md | grep "Current Status" -A 20

# Review recent changes
git log --oneline -10
git log --graph --oneline -20

# Check for uncommitted changes
git status
```

#### 3. **Start Services & Verify Environment**

**IMPORTANT**: Always use Aspire to run services. It automatically handles:
- OpenAI API key management (prompts on first run, stores in user secrets)
- Database orchestration
- Service dependencies
- Configuration management

```bash
# 1. Check if Aspire is already running
netstat -ano | findstr "17263"

# 2. If not running, start All Services (via Aspire - REQUIRED)
cd AppHost
dotnet run
# First run: Will prompt for OpenAI API key
# Subsequent runs: Uses stored key from user secrets
# Opens Aspire Dashboard at https://localhost:17263
# ✅ Verify all services show "Running" status in dashboard
# ✅ Check API is accessible (port shown in Aspire dashboard)
# ✅ Check frontend is accessible (port shown in Aspire dashboard)

# 3. Verify Backend (if running standalone - NOT recommended)
cd MyApi
dotnet build
dotnet test

# 3. Verify Frontend
cd WarrantyApp.Web
npm install
npm run lint
```

**⚠️ Why Aspire is Default:**
- Manages OpenAI API key via user secrets (prompts once, stores securely)
- Handles database connection strings
- Orchestrates all services with proper dependencies
- Provides real-time monitoring dashboard
- Eliminates manual configuration for local development

#### 4. **Define Session Goal**

Write a clear, focused goal for the session:

**Good Examples:**
- ✅ "Add pagination to receipts list endpoint"
- ✅ "Fix E2E test failures in login.spec.ts"
- ✅ "Update warranty notification email template"
- ✅ "Add unit tests for ChatbotService"

**Bad Examples:**
- ❌ "Work on receipts feature" (too vague)
- ❌ "Fix everything that's broken" (unfocused)
- ❌ "Improve the app" (no clear deliverable)

**Template:**
```
Session Goal: [Specific, measurable task]
Estimated Time: [1-4 hours recommended]
Success Criteria: [How you'll know you're done]
Related Docs: [List 2-3 relevant docs]
```

---

## Documentation Strategy

### Core Principle: Documentation First

**Always consult documentation BEFORE coding**. This project has extensive documentation (35+ guides) covering all features, patterns, and practices.

### Documentation Hierarchy

**1. Quick Reference (Start Here)**
- **README.md** - Project overview, current status, quick links
- **docs/setup/setup-quickstart.md** - Getting started guide
- **docs/guide/guide-complete-implementation.md** - Implementation summary

**2. Feature Documentation**
- **docs/backend/** - All backend features (15 docs)
- **docs/frontend/** - Frontend implementation (5 docs)
- **docs/testing/** - Testing strategies (1 doc)
- **MyApi.Tests/README.md** - Backend test guide

**3. Infrastructure & Operations**
- **docs/infra/** - Deployment, CI/CD, troubleshooting (7 docs)
- **docs/setup/** - Setup and configuration (5 docs)

**4. Component-Level READMEs**
- **MyApi/README.md** - API documentation
- **WarrantyApp.Web/README.md** - Frontend documentation
- **MyApi.Tests/README.md** - Test suite documentation

### Finding the Right Documentation

**Use this decision tree:**

```
What are you working on?

├─ Backend Feature
│  ├─ Authentication? → docs/backend/backend-authentication.md
│  ├─ OCR/Receipts? → docs/backend/backend-ocr-openai.md
│  ├─ Notifications? → docs/backend/backend-warranty-notifications.md
│  └─ General API? → MyApi/README.md
│
├─ Frontend Feature
│  ├─ UI Components? → docs/frontend/frontend-design-system.md
│  ├─ User Flows? → docs/frontend/frontend-workflows.md
│  └─ General Frontend? → WarrantyApp.Web/README.md
│
├─ Testing
│  ├─ Backend Tests? → MyApi.Tests/README.md
│  ├─ E2E Tests? → docs/testing/README.md
│  └─ Strategy? → docs/infra/infra-testing-strategy.md
│
├─ Infrastructure
│  ├─ Deployment? → docs/infra/infra-deployment-azure.md
│  ├─ CI/CD? → docs/infra/infra-cicd-github-actions.md
│  └─ Troubleshooting? → docs/infra/infra-troubleshooting-*.md
│
└─ Getting Started? → docs/setup/setup-quickstart.md
```

**Search Documentation:**
```bash
# Find all docs mentioning a keyword
grep -r "keyword" docs/ --include="*.md"

# Find docs by feature
find docs/ -name "*authentication*"
find docs/ -name "*ocr*"

# List all available docs
ls -R docs/
```

---

## Context Management

### The Context Window Problem

When working with AI assistants or managing cognitive load, **context windows are limited**. This project addresses this with:

1. **Modular Documentation** - Each doc focuses on ONE feature/topic
2. **Component READMEs** - Self-contained documentation in each directory
3. **Clear Cross-References** - Docs link to related docs
4. **Summary Documents** - Quick overviews without deep details

### Managing Context Effectively

#### Strategy 1: Work in Layers

**Layer 1: Overview (5 min)**
- Read main README.md
- Understand project structure
- Identify your work area

**Layer 2: Feature Context (10 min)**
- Read specific feature documentation
- Review related code files
- Check test coverage

**Layer 3: Implementation (30-60 min)**
- Focus on single task
- Reference docs as needed
- Avoid scope creep

#### Strategy 2: Use Component READMEs

Each major component has its own README with full context:

```
MyApi/README.md
├─ API endpoints
├─ Configuration
├─ Running the API
└─ Troubleshooting

MyApi.Tests/README.md
├─ Test structure
├─ Running tests
├─ Patterns
└─ Adding tests

WarrantyApp.Web/README.md
├─ Components
├─ Running dev server
├─ E2E tests
└─ Build process
```

**Benefit**: You can load JUST what you need without overwhelming context.

#### Strategy 3: Session Notes

Keep lightweight session notes (don't save to repo):

```markdown
# Session: [Date] - [Task]

## Goal
[One-sentence goal]

## Docs Read
- [Doc 1]
- [Doc 2]

## Changes Made
- [File 1]: [What changed]
- [File 2]: [What changed]

## Next Steps
- [ ] Task 1
- [ ] Task 2

## Questions/Blockers
- [Any issues encountered]
```

**Where to keep notes:**
- Personal notes app (not in repo)
- Scratch paper
- IDE scratch file

**Don't create** session notes in docs/ unless it's a significant feature implementation that should be archived.

---

## Work Session Best Practices

### 1. Time-Box Your Sessions

**Recommended Durations:**
- **Small Task**: 30-60 minutes
- **Medium Task**: 1-2 hours
- **Large Task**: Break into multiple sessions

**Why?**
- Maintains focus
- Prevents burnout
- Forces clear deliverables
- Keeps context manageable

### 2. Single Responsibility Sessions

Each session should have ONE clear goal:

**✅ Good Session Goals:**
- "Add email validation to registration form"
- "Write unit tests for TokenService"
- "Fix Playwright test timeout in login.spec.ts"
- "Update README with new OCR feature"

**❌ Avoid Multi-Goal Sessions:**
- "Fix tests AND add new feature AND update docs"
- "Work on frontend and backend together"

### 3. Test-First Development

For new features:

```
1. Write tests first (TDD)
   └─ Backend: MyApi.Tests/
   └─ Frontend: e2e/

2. Implement feature
   └─ Make tests pass

3. Run full test suite
   └─ Ensure nothing broke

4. Update documentation
   └─ See Documentation Maintenance section
```

### 4. Regular Check-ins

Every 30 minutes:
- ✅ Am I still working on the session goal?
- ✅ Have I created scope creep?
- ✅ Should I commit what I have?
- ✅ Do I need to take a break?

### 5. Pre-Commit Checklist

Before committing ANY code:

```bash
# 1. Run tests
cd MyApi.Tests && dotnet test
cd WarrantyApp.Web && npm run lint

# 2. Check uncommitted changes
git status
git diff

# 3. Review your changes
git diff --staged

# 4. Ensure documentation is updated
# See "Documentation Maintenance" section
```

---

## Batch Processing Guidelines

### Why Batch Processing?

**Benefits:**
- Reduces context switching
- Maintains focus
- Lowers cognitive load
- Improves code quality
- Faster completion

### Batch Categories

#### Batch Type 1: Similar Tasks

Group similar work together:

**Example: UI Component Updates**
```
Batch: Update 3 form components
├─ Session 1: Login form (1 hour)
├─ Session 2: Register form (1 hour)
└─ Session 3: Profile form (1 hour)

Benefits:
- Consistent patterns
- Shared context
- Easier code review
```

**Example: Test Writing**
```
Batch: Add tests for notification services
├─ Session 1: EmailNotificationServiceTests
├─ Session 2: SmsNotificationServiceTests
└─ Session 3: CompositeNotificationServiceTests

Benefits:
- Same testing patterns
- Similar mocking setup
- Parallel structure
```

#### Batch Type 2: Feature Slicing

Break large features into vertical slices:

**Example: Receipt Sharing Feature**
```
Batch: Receipt Sharing (5 sessions)
├─ Session 1: Backend - Share model & API endpoint
├─ Session 2: Backend - Unit tests
├─ Session 3: Frontend - Share button & modal
├─ Session 4: Frontend - E2E tests
└─ Session 5: Documentation update

Benefits:
- Each session delivers value
- Can be tested independently
- Clear progress tracking
```

#### Batch Type 3: Documentation Updates

Update related docs in one session:

**Example: New Feature Documentation**
```
Batch: Document OCR batching feature
├─ Update: MyApi/README.md (API changes)
├─ Update: docs/backend/backend-ocr-openai.md (implementation)
├─ Update: README.md (feature list)
└─ Update: docs/guide/guide-complete-implementation.md (status)

Duration: 30-45 minutes
```

### Batch Processing Template

```markdown
# Batch: [Name]

## Goal
[Overall objective]

## Sessions
1. [ ] [Session 1 name] (Est: [time])
2. [ ] [Session 2 name] (Est: [time])
3. [ ] [Session 3 name] (Est: [time])

## Completion Criteria
- [ ] All code changes complete
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Changes committed

## Related Docs
- [Doc 1]
- [Doc 2]
```

---

## Documentation Maintenance

### When to Update Documentation

**ALWAYS update documentation when you:**
1. ✅ Add a new feature
2. ✅ Change existing behavior
3. ✅ Fix a significant bug
4. ✅ Add/modify API endpoints
5. ✅ Update configuration
6. ✅ Change test structure

**You MAY skip documentation for:**
- ❌ Internal refactoring (no behavior change)
- ❌ Code cleanup (no functional change)
- ❌ Minor formatting fixes

### What to Update

#### For New Backend Features

```
1. MyApi/README.md
   └─ Add to "Key Capabilities" or endpoint list

2. docs/backend/backend-[feature].md
   └─ Create new doc or update existing

3. README.md
   └─ Update "Key Features" if user-facing

4. MyApi.Tests/README.md
   └─ Add test coverage information (if tests added)

5. docs/testing/README.md
   └─ Update test counts and coverage
```

#### For New Frontend Features

```
1. WarrantyApp.Web/README.md
   └─ Add to "What Users Can Do"

2. docs/frontend/frontend-workflows.md
   └─ Document user flow

3. docs/frontend/frontend-design-system.md
   └─ Document UI components (if new)

4. README.md
   └─ Update "Key Features"

5. docs/testing/README.md
   └─ Update E2E test coverage
```

#### For Infrastructure Changes

```
1. docs/infra/[relevant-doc].md
   └─ Update specific infrastructure doc

2. docs/setup/setup-quickstart.md
   └─ Update if setup process changes

3. README.md
   └─ Update if affects getting started
```

### Documentation Update Checklist

After completing a feature:

```markdown
Documentation Update Checklist:
- [ ] Component README updated (MyApi/, WarrantyApp.Web/, MyApi.Tests/)
- [ ] Feature doc updated (docs/backend/, docs/frontend/, docs/infra/)
- [ ] Main README updated (if needed)
- [ ] Test documentation updated (docs/testing/, MyApi.Tests/)
- [ ] Cross-references added (link related docs)
- [ ] Examples included (code snippets, screenshots)
- [ ] Version/date updated in modified docs
```

### Documentation Style Guide

**Follow these conventions:**

1. **File Naming**
   - Use kebab-case: `backend-warranty-notifications.md`
   - Prefix with category: `backend-`, `frontend-`, `infra-`, `setup-`
   - Be descriptive: `infra-deployment-azure.md` not `deployment.md`

2. **Document Structure**
   ```markdown
   # Title
   
   **Status**: [Status]
   **Last Updated**: [Date]
   
   Brief description (1-2 sentences)
   
   ## Table of Contents
   
   ## Section 1
   
   ## Section 2
   
   ## Quick Reference (at end)
   ```

3. **Code Blocks**
   - Always specify language: ```bash, ```csharp, ```typescript
   - Include comments for clarity
   - Show expected output when relevant

4. **Cross-References**
   ```markdown
   See [Feature Name](../path/to/doc.md) for details.
   ```

5. **Status Indicators**
   - ✅ Complete
   - ⚠️ In Progress
   - ❌ Not Started
   - 🎯 Critical

### Where to Place Documentation

```
docs/
├── backend/          # Backend feature docs
│   └── backend-[feature-name].md
├── frontend/         # Frontend feature docs
│   └── frontend-[topic].md
├── infra/            # Infrastructure & operations
│   └── infra-[topic].md
├── setup/            # Setup & configuration
│   └── setup-[topic].md
├── testing/          # Testing strategies
│   └── README.md
├── workflow/         # Development workflows (this doc!)
│   └── README.md
├── guide/            # Comprehensive guides
│   └── guide-[topic].md
└── archive/          # Historical notes (rarely updated)
    ├── sessions/
    ├── progress/
    └── reference/

Component READMEs:
├── MyApi/README.md           # API documentation
├── MyApi.Tests/README.md     # Test documentation
└── WarrantyApp.Web/README.md # Frontend documentation
```

---

## Code Review Checklist

### Self-Review (Before Committing)

```markdown
Code Quality:
- [ ] Code follows project conventions
- [ ] No commented-out code
- [ ] No console.log or debug statements
- [ ] Error handling implemented
- [ ] Edge cases handled

Testing:
- [ ] Backend tests pass (dotnet test)
- [ ] Frontend linting passes (npm run lint)
- [ ] E2E tests pass (if applicable)
- [ ] New tests added for new features
- [ ] Test coverage maintained or improved

Documentation:
- [ ] Component README updated
- [ ] Feature docs updated
- [ ] Main README updated (if needed)
- [ ] Code comments added (where needed)
- [ ] API docs updated (Swagger)

Git:
- [ ] Commit message is descriptive
- [ ] Changes are focused (single purpose)
- [ ] No unrelated changes included
- [ ] Branch is up to date with main
```

### Peer Review (Pull Request)

```markdown
Functional:
- [ ] Feature works as intended
- [ ] No regressions introduced
- [ ] Edge cases handled
- [ ] Error messages are clear

Code Quality:
- [ ] Code is readable and maintainable
- [ ] Follows project patterns
- [ ] No code duplication
- [ ] Proper error handling

Testing:
- [ ] Tests are comprehensive
- [ ] Tests are clear and maintainable
- [ ] All tests pass
- [ ] Coverage is adequate

Documentation:
- [ ] All documentation is updated
- [ ] API changes documented
- [ ] Breaking changes highlighted
- [ ] Examples are clear
```

---

## Debugging Sessions

### Debugging Workflow

#### 1. Reproduce the Issue

```markdown
Bug Report Template:
- [ ] What is the expected behavior?
- [ ] What is the actual behavior?
- [ ] Steps to reproduce
- [ ] Environment (browser, OS, .NET version)
- [ ] Error messages/logs
- [ ] Screenshots (if applicable)
```

#### 2. Gather Context

```bash
# Check logs
# Backend: Aspire Dashboard → Logs
# Frontend: Browser DevTools → Console

# Check git history
git log --oneline --grep="[related keyword]"
git blame [file]

# Check related docs
grep -r "[error message]" docs/
```

#### 3. Isolate the Problem

**Backend:**
```bash
# Run specific test
dotnet test --filter "FullyQualifiedName~[TestName]"

# Check health endpoints
curl http://localhost:5000/health

# Review recent changes
git diff main -- MyApi/
```

**Frontend:**
```bash
# IMPORTANT: Ensure Aspire services are running first!
cd AppHost
dotnet run
# Wait for all services to show "Running" in dashboard

# Then run E2E tests
cd WarrantyApp.Web
npm run e2e:ui  # Interactive debugging
npm run e2e     # Headless mode

# Check specific component
# Frontend is auto-started by Aspire
# Navigate to http://localhost:4200
```

**⚠️ E2E Test Prerequisites:**
- ✅ Aspire services must be running (`cd AppHost && dotnet run`)
- ✅ Backend API must be accessible (http://localhost:5134)
- ✅ Database must be ready
- ❌ Do NOT run frontend standalone for E2E tests

#### 4. Fix and Verify

```bash
# 1. Make fix

# 2. Run tests (with Aspire running for E2E tests)
dotnet test        # Backend (from MyApi.Tests)
npm run e2e        # Frontend E2E (requires Aspire)

# 3. Manual verification via Aspire Dashboard
cd AppHost
dotnet run
# Check dashboard: http://localhost:15299
# Access app: http://localhost:4200
# Test the scenario manually

# 4. Check for regressions
# Run full test suite
```

#### 5. Document the Fix

```markdown
If the bug was non-obvious:
- [ ] Add test to prevent regression
- [ ] Update docs if behavior changed
- [ ] Add comment explaining the fix
- [ ] Consider adding to troubleshooting guide
```

### Common Debugging Resources

**Backend Issues:**
- `docs/infra/infra-troubleshooting-database.md` - Database issues
- `docs/infra/infra-auth-troubleshooting.md` - Auth issues  
- `MyApi/README.md` - API troubleshooting section

**Frontend Issues:**
- `docs/frontend/frontend-aspire-proxy.md` - Proxy issues
- `docs/testing/README.md` - E2E test issues
- `WarrantyApp.Web/README.md` - Build/run issues

**Testing Issues:**
- `docs/testing/README.md` - Comprehensive test debugging
- `MyApi.Tests/README.md` - Backend test debugging

---

## Git Workflow

### Branch Strategy

```
main (production-ready)
  └─ feature/[feature-name]  # New features
  └─ fix/[bug-description]   # Bug fixes
  └─ docs/[doc-update]       # Documentation only
  └─ test/[test-update]      # Test updates only
```

### Commit Message Format

```
[type]: [Short description]

[Optional longer description]

[Optional breaking changes]

Examples:
feat: Add pagination to receipts endpoint
fix: Resolve E2E test timeout in login flow
docs: Update OCR documentation with batch processing
test: Add unit tests for TokenService
refactor: Extract email template logic to service
```

### Commit Best Practices

**DO:**
- ✅ Commit frequently (every 30-60 minutes)
- ✅ Keep commits focused (single purpose)
- ✅ Write descriptive messages
- ✅ Include test changes with code changes
- ✅ Update docs in same commit as code

**DON'T:**
- ❌ Commit broken code
- ❌ Mix unrelated changes
- ❌ Use vague messages ("fix stuff")
- ❌ Commit commented-out code
- ❌ Forget to update documentation

### Pre-Push Checklist

```bash
# Before pushing to remote:

# 1. Run full test suite
cd MyApi.Tests && dotnet test
cd WarrantyApp.Web && npm run e2e

# 2. Lint code
cd WarrantyApp.Web && npm run lint

# 3. Review all commits
git log origin/main..HEAD

# 4. Check for sensitive data
git diff origin/main

# 5. Ensure docs are updated
# Refer to "Documentation Maintenance" section
```

---

## Quick Reference

### Common Commands

```bash
# === Project Navigation ===
cd MyApi                  # Backend API
cd MyApi.Tests            # Backend tests
cd WarrantyApp.Web        # Frontend
cd AppHost                # Aspire orchestrator (START HERE)
cd docs                   # Documentation

# === Building & Running (via Aspire - RECOMMENDED) ===
cd AppHost
dotnet run                # Starts ALL services with proper config
# First run: Prompts for OpenAI API key
# Subsequent runs: Uses stored key from user secrets
# Dashboard: http://localhost:15299
# API: http://localhost:5134
# Frontend: http://localhost:4200

# === Building & Running (Standalone - NOT recommended) ===
cd MyApi
dotnet build              # Build backend only
dotnet run                # Run backend only (requires manual OpenAI key setup)

cd WarrantyApp.Web
npm start                 # Run frontend dev server only
npm run build:prod        # Production build

# === Testing ===
dotnet test               # All backend tests (from MyApi.Tests)
npm run e2e               # All E2E tests (requires Aspire running)
npm run e2e:ui            # E2E interactive mode
npm run lint              # Frontend linting

# === Git ===
git status                # Check status
git diff                  # See changes
git log --oneline -10     # Recent commits
git branch                # List branches

# === Documentation Search ===
grep -r "keyword" docs/   # Search docs
find docs/ -name "*.md"   # List all docs
ls -R docs/               # Directory structure
```

### Documentation Quick Links

**Start Here:**
- `README.md` - Project overview
- `docs/setup/setup-quickstart.md` - Getting started
- `docs/workflow/README.md` - This guide!

**Feature Implementation:**
- `docs/backend/` - All backend features
- `docs/frontend/` - All frontend features
- `docs/testing/` - Testing strategies

**Component READMEs:**
- `MyApi/README.md` - API docs
- `MyApi.Tests/README.md` - Test docs
- `WarrantyApp.Web/README.md` - Frontend docs

**Infrastructure:**
- `docs/infra/` - Deployment, CI/CD, troubleshooting
- `docs/setup/` - Configuration guides

### Session Starter Template

```markdown
# Development Session: [Date]

## Goal
[One clear, focused goal]

## Docs to Review
- [ ] [Doc 1]
- [ ] [Doc 2]

## Environment Check
- [ ] Check if Aspire running: `netstat -ano | findstr "17263"`
- [ ] Start if needed: `cd AppHost && dotnet run`
- [ ] Aspire Dashboard accessible: https://localhost:17263
- [ ] All services show "Running" in dashboard
- [ ] Backend tests pass: `dotnet test` (from MyApi.Tests)
- [ ] Frontend lints: `cd WarrantyApp.Web && npm run lint`

## Tasks
- [ ] [Task 1]
- [ ] [Task 2]
- [ ] [Task 3]

## Documentation Updates Needed
- [ ] [Doc 1]
- [ ] [Doc 2]

## Commit Message
[type]: [description]

## Success Criteria
- [ ] Feature works
- [ ] Tests pass
- [ ] Docs updated
- [ ] Changes committed
```

---

## Best Practices Summary

### ✅ DO

1. **Read docs FIRST** before coding
2. **Use Aspire** for running services (manages OpenAI key via user secrets)
3. **Keep sessions focused** on one goal
4. **Batch similar tasks** together
5. **Update documentation** with code changes
6. **Test before committing** (with Aspire running for E2E)
7. **Write descriptive commit messages**
8. **Use component READMEs** for context
9. **Take breaks** every 60-90 minutes

### ❌ DON'T

1. **Don't skip documentation review**
2. **Don't run services standalone** (use Aspire for proper config)
3. **Don't run E2E tests** without Aspire services running
4. **Don't multi-task** across unrelated features
5. **Don't commit broken code**
6. **Don't forget to update docs**
7. **Don't create scope creep**
8. **Don't work without clear goal**
9. **Don't ignore failing tests**
10. **Don't create docs in repo for personal notes**

---

## Support & Resources

### When Stuck

1. **Check Documentation**
   - Search with `grep -r "keyword" docs/`
   - Review relevant feature doc

2. **Check Tests**
   - Look at test files for usage examples
   - `MyApi.Tests/` has great examples

3. **Check Git History**
   - `git log --oneline --grep="keyword"`
   - `git blame [file]` to see context

4. **Check Troubleshooting Guides**
   - `docs/infra/infra-troubleshooting-*.md`
   - Component README troubleshooting sections

### Contributing

See `README.md` for contribution guidelines.

### Questions?

- Review project documentation (35+ guides)
- Check component READMEs
- Review test files for examples
- Check git history for context

---

**Last Updated**: November 18, 2025  
**Maintained By**: Development Team  
**Next Review**: As needed

**Remember**: Documentation first, focused sessions, test everything, update docs! 🚀
