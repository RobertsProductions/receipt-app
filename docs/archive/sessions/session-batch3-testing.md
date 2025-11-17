# Batch 3: Add data-testid Attributes - Implementation Plan

## Strategy
Add data-testid attributes to all interactive elements and key UI components for reliable E2E testing with Playwright.

## Naming Convention
- Use kebab-case
- Be descriptive but concise
- Format: {component}-{element}-{action/type}

## Examples:
- Buttons: login-button, save-receipt-button, delete-button
- Inputs: mail-input, password-input, merchant-input
- Links: egister-link, orgot-password-link
- Cards: eceipt-card-{id}, warranty-card-{id}
- Modals: share-modal, delete-modal
- Lists: eceipt-list, warranty-list
- Navigation: 
av-receipts, 
av-profile

## Components to Update (Priority Order):

### High Priority (Core Flows)
1. Shared Components (17 components)
   - Button ✅ (already has testId input)
   - Input ✅ (already has testId input)
   - Card
   - Modal
   - Checkbox
   - Toggle
   - Dropdown ✅ (already has testId input)
   - FileUpload
   - Pagination
   - Avatar
   - Badge
   - Spinner
   - Toast
   - EmptyState
   - Slider
   - Tooltip
   - ProgressBar ✅ (already has testId input)

2. Auth Pages (7 pages)
   - Landing ✅ (partially done)
   - Login ✅ (partially done)
   - Register ✅ (partially done)
   - Forgot Password ✅ (done)
   - Reset Password ✅ (done)
   - Confirm Email ✅ (done)
   - Verify Phone ✅ (done)
   - 2FA Setup ✅ (done)

3. Receipt Pages (2 pages)
   - Receipt List
   - Receipt Detail

4. Warranty Pages (1 page)
   - Warranty Dashboard

5. Profile & Settings (2 pages)
   - Profile
   - Notification Settings

6. Sharing (2 pages/components)
   - Shared Receipts ✅ (done)
   - Share Receipt Modal ✅ (done)

7. Chatbot (1 page)
   - Chatbot page

### Components Already with testId Support:
- ButtonComponent ✅
- InputComponent ✅
- DropdownComponent ✅
- ProgressBarComponent ✅
- VerifyPhoneComponent ✅
- ConfirmEmailComponent ✅
- ForgotPasswordComponent ✅
- ResetPasswordComponent ✅
- TwofaSetupComponent ✅
- SharedReceiptsComponent ✅
- ShareReceiptModalComponent ✅

## Implementation Approach:
1. Add @Input() testId?: string to components that don't have it
2. Add [attr.data-testid]="testId" to component templates
3. Add specific data-testid values in page templates
4. Focus on interactive elements: buttons, inputs, links, cards

