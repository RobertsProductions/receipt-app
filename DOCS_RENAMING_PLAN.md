# Documentation Renaming Strategy

**Date**: November 17, 2025

## Current Issues with Naming

1. **Number-based naming** (00-39) - Not descriptive, requires memorization
2. **Missing numbers** (30-31, 33, 35-38, 40-41 now archived)
3. **Unclear purpose** from filename alone
4. **Difficult to sort** logically (current numerical order doesn't match logical order)

## Proposed Naming Convention

**Format**: `{category}-{feature-name}.md`

### Categories:
- `setup-` - Getting started and configuration
- `backend-` - Backend feature implementation
- `infra-` - Infrastructure, ops, deployment
- `frontend-` - Frontend design and implementation
- `guide-` - Complete guides and summaries

---

## Renaming Map

### Setup & Getting Started (5 docs)
| Current | New | Reason |
|---------|-----|--------|
| 00-quickstart.md | `setup-quickstart.md` | Clear entry point |
| 01-initial-setup.md | `setup-initial.md` | Initial configuration |
| 02-api-registration.md | `setup-aspire-registration.md` | Aspire-specific |
| 05-database-resources-aspire.md | `setup-database-aspire.md` | Database via Aspire |
| 06-docker-database-setup.md | `setup-database-docker.md` | Docker database |

### Backend Features (15 docs)
| Current | New | Reason |
|---------|-----|--------|
| 04-authentication-authorization.md | `backend-authentication.md` | Core auth feature |
| 08-receipt-upload-feature.md | `backend-receipts-upload.md` | Receipt uploads |
| 09-ocr-openai-integration.md | `backend-ocr-openai.md` | OCR integration |
| 10-warranty-expiration-notifications.md | `backend-warranty-notifications.md` | Warranty monitoring |
| 11-email-sms-notifications.md | `backend-notifications-email-sms.md` | Email/SMS setup |
| 12-user-profile-management.md | `backend-user-profile.md` | Profile management |
| 13-pdf-ocr-support.md | `backend-ocr-pdf.md` | PDF OCR support |
| 14-phone-verification.md | `backend-phone-verification.md` | Phone verification |
| 15-batch-ocr-processing.md | `backend-ocr-batch.md` | Batch OCR |
| 16-refresh-token-support.md | `backend-auth-refresh-tokens.md` | Refresh tokens |
| 17-two-factor-authentication.md | `backend-auth-2fa.md` | Two-factor auth |
| 18-email-confirmation.md | `backend-auth-email-confirmation.md` | Email confirmation |
| 23-receipt-sharing.md | `backend-receipts-sharing.md` | Receipt sharing |
| 24-ai-chatbot-receipt-queries.md | `backend-chatbot.md` | AI chatbot |
| 26-user-data-caching.md | `backend-caching-user-data.md` | User data caching |

### Infrastructure & Operations (5 docs)
| Current | New | Reason |
|---------|-----|--------|
| 03-cicd-setup.md | `infra-cicd-github-actions.md` | CI/CD pipeline |
| 07-connection-fixes.md | `infra-troubleshooting-database.md` | DB troubleshooting |
| 19-monitoring-and-alerting.md | `infra-monitoring-health-checks.md` | Health monitoring |
| 20-testing-strategy.md | `infra-testing-strategy.md` | Testing approach |
| 21-automated-deployment.md | `infra-deployment-azure.md` | Azure deployment |
| 22-code-quality-improvements.md | `infra-code-quality.md` | Code quality |
| 25-performance-optimization.md | `infra-performance.md` | Performance tuning |

### Frontend Documentation (5 docs)
| Current | New | Reason |
|---------|-----|--------|
| 27-design-reference.md | `frontend-design-system.md` | Design system |
| 28-frontend-workflows.md | `frontend-workflows.md` | User workflows |
| 29-angular-aspire-integration.md | `frontend-aspire-integration.md` | Aspire setup |
| 32-aspire-angular-proxy-fix.md | `frontend-aspire-proxy.md` | Proxy configuration |
| 34-frontend-implementation-roadmap.md | `frontend-roadmap.md` | Implementation roadmap |

### Complete Implementation Summary (1 doc)
| Current | New | Reason |
|---------|-----|--------|
| 39-complete-implementation-summary.md | `guide-complete-implementation.md` | Main guide |

### Archive - Session Logs
| Current | New | Reason |
|---------|-----|--------|
| 35-session-9-components-pages.md | `session-09-components-pages.md` | Session 9 log |
| 36-batch1-remaining-pages.md | `session-batch1-pages.md` | Batch 1 work |
| 36-session-nov17-foundational-components.md | `session-nov17-foundational.md` | Nov 17 session 1 |
| 37-batch3-test-attributes-plan.md | `session-batch3-testing.md` | Batch 3 plan |
| 37-session-nov17-priority2-components.md | `session-nov17-priority2.md` | Nov 17 session 2 |
| 38-login-error-interceptor-fix.md | `session-bugfix-login-interceptor.md` | Bug fix log |

### Archive - Progress Reports
| Current | New | Reason |
|---------|-----|--------|
| 30-frontend-setup-complete.md | `progress-frontend-setup-complete.md` | Frontend setup done |
| 31-aspire-integration-complete.md | `progress-aspire-integration-complete.md` | Aspire integration done |
| 33-frontend-progress.md | `progress-frontend-sessions.md` | Session-by-session log |
| 38-batch3-progress.md | `progress-batch3.md` | Batch 3 status |
| 40-git-commit-summary.md | `progress-git-commits-session9.md` | Session 9 commits |
| 41-readme-condensation-summary.md | `progress-readme-condensation.md` | README update log |

### Archive - Reference
| Current | New | Reason |
|---------|-----|--------|
| 35-frontend-roadmap-summary.md | `reference-frontend-roadmap-summary.md` | Quick reference |

---

## Benefits of New Naming

### ✅ **Discoverability**
- **Before**: `04-authentication-authorization.md` - What's in this?
- **After**: `backend-authentication.md` - Clear purpose

### ✅ **Logical Grouping**
Files sort alphabetically by category:
```
backend-auth-2fa.md
backend-auth-email-confirmation.md
backend-auth-refresh-tokens.md
backend-authentication.md
```

### ✅ **No Number Gaps**
- No confusion about missing numbers
- Easy to add new docs without renumbering

### ✅ **Self-Documenting**
- Filename alone tells you:
  - Category (setup/backend/infra/frontend)
  - Feature (auth/receipts/OCR/etc)
  - Purpose (clear and concise)

### ✅ **Easier Maintenance**
- Add new docs without renumbering sequence
- Category changes don't break numbering
- Logical relationships clear from names

---

## Implementation Order

1. ✅ Rename active docs (main docs/ folder)
2. ✅ Rename archived docs (docs/archive/)
3. ✅ Update README.md links
4. ✅ Update MyApi/README.md links
5. ✅ Update WarrantyApp.Web/README.md links
6. ✅ Update docs/archive/README.md
7. ✅ Update any internal cross-references
8. ✅ Commit with detailed message

---

## Notes

- **Preserve git history**: Use `git mv` for all renames
- **Test all links**: Verify no broken references
- **Update documentation**: Archive README needs updating
- **Consistent format**: All lowercase, hyphens for spaces
- **Clear categories**: Prefix makes organization obvious
