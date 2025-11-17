# Frontend Implementation Roadmap - Detailed Task Breakdown

**Created**: November 17, 2025  
**Status**: Active - Implementation Guide  
**Purpose**: Comprehensive, prioritized roadmap for building all UI components and pages

---

## Current Status: Foundation Complete ✅

**What We Have:**
- ✅ Design system (CSS variables, typography, spacing)
- ✅ All services (Auth, Receipt, Warranty, Profile, Chatbot, Sharing)
- ✅ HTTP infrastructure (interceptors, guards)
- ✅ TypeScript models for all entities
- ✅ Navbar component (responsive, with user menu)
- ✅ Project structure and build pipeline

**What We Need:**
- ❌ Reusable UI components (buttons, inputs, cards, modals)
- ❌ All pages and features
- ❌ Routing configuration
- ❌ Form components with validation
- ❌ Error handling and notifications

---

## Implementation Priority

### 🔥 **PRIORITY 1: Shared UI Components** (Build First)
These are reusable building blocks needed by all pages. Build these first to avoid duplication.

### 🔥 **PRIORITY 2: Authentication Flow** (Core User Journey)
Login, register, email confirmation - essential for users to access the app.

### 🔥 **PRIORITY 3: Receipt Management** (Main Feature)
The core functionality - uploading, viewing, and managing receipts.

### 🔥 **PRIORITY 4: Warranty Dashboard** (Value Proposition)
The reason users use this app - tracking warranty expirations.

### 🔥 **PRIORITY 5: Advanced Features** (Enhancement)
Profile, settings, sharing, chatbot - nice-to-haves that enhance the experience.

---

## PRIORITY 1: Shared UI Components Library 🎨

### 1.1 Button Component (`shared/components/button`)
**File**: `src/app/shared/components/button/button.component.ts`

**Variants Needed:**
- `primary` - Solid blue background (main actions)
- `secondary` - Outlined blue border (secondary actions)
- `ghost` - Transparent background (tertiary actions)
- `danger` - Red background (destructive actions)
- `success` - Green background (positive actions)

**Sizes:**
- `sm` - Small (32px height)
- `md` - Medium (40px height) - default
- `lg` - Large (48px height)

**Props:**
```typescript
@Input() variant: 'primary' | 'secondary' | 'ghost' | 'danger' | 'success' = 'primary';
@Input() size: 'sm' | 'md' | 'lg' = 'md';
@Input() disabled: boolean = false;
@Input() loading: boolean = false;
@Input() fullWidth: boolean = false;
@Input() type: 'button' | 'submit' | 'reset' = 'button';
@Output() onClick = new EventEmitter<MouseEvent>();
```

**Usage Example:**
```html
<app-button variant="primary" size="md" (onClick)="handleSubmit()">
  Submit
</app-button>
```

**Styling Notes:**
- Use design tokens from `styles.scss`
- Hover effect: lift 1px, increase shadow
- Active effect: scale 0.98
- Disabled: opacity 0.6, cursor not-allowed
- Loading: show spinner, disable interaction

---

### 1.2 Input Component (`shared/components/input`)
**File**: `src/app/shared/components/input/input.component.ts`

**Types:**
- `text` - Standard text input
- `email` - Email with validation
- `password` - Password with toggle visibility
- `number` - Numeric input
- `tel` - Phone number
- `url` - URL input
- `search` - Search with icon

**Props:**
```typescript
@Input() type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search' = 'text';
@Input() label?: string;
@Input() placeholder?: string;
@Input() value: string = '';
@Input() disabled: boolean = false;
@Input() required: boolean = false;
@Input() error?: string;
@Input() hint?: string;
@Input() icon?: string;
@Input() autocomplete?: string;
@Output() valueChange = new EventEmitter<string>();
@Output() onBlur = new EventEmitter<void>();
@Output() onFocus = new EventEmitter<void>();
```

**Features:**
- Label with optional asterisk for required fields
- Error state with red border and error message
- Hint text below input
- Password visibility toggle (eye icon)
- Floating label animation (optional)
- Icon support (prefix/suffix)
- Character counter for max length

**Usage Example:**
```html
<app-input
  type="email"
  label="Email Address"
  placeholder="you@example.com"
  [error]="emailError"
  [(value)]="email"
  required>
</app-input>
```

---

### 1.3 Textarea Component (`shared/components/textarea`)
**File**: `src/app/shared/components/textarea/textarea.component.ts`

**Props:**
```typescript
@Input() label?: string;
@Input() placeholder?: string;
@Input() value: string = '';
@Input() rows: number = 4;
@Input() maxLength?: number;
@Input() disabled: boolean = false;
@Input() required: boolean = false;
@Input() error?: string;
@Input() hint?: string;
@Output() valueChange = new EventEmitter<string>();
```

**Features:**
- Auto-resize option
- Character counter
- Same styling as input component

---

### 1.4 Checkbox Component (`shared/components/checkbox`)
**File**: `src/app/shared/components/checkbox/checkbox.component.ts`

**Props:**
```typescript
@Input() label: string = '';
@Input() checked: boolean = false;
@Input() disabled: boolean = false;
@Input() indeterminate: boolean = false;
@Output() checkedChange = new EventEmitter<boolean>();
```

**Styling:**
- Custom checkbox design (not native)
- Blue checkmark on checked
- Smooth animation
- Larger touch target (44x44px minimum)

---

### 1.5 Radio Button Component (`shared/components/radio`)
Similar to checkbox but for single selection.

---

### 1.6 Select/Dropdown Component (`shared/components/select`)
**File**: `src/app/shared/components/select/select.component.ts`

**Props:**
```typescript
@Input() label?: string;
@Input() options: Array<{value: any, label: string}> = [];
@Input() value: any;
@Input() placeholder?: string;
@Input() disabled: boolean = false;
@Input() required: boolean = false;
@Input() error?: string;
@Input() searchable: boolean = false;
@Output() valueChange = new EventEmitter<any>();
```

**Features:**
- Custom styled dropdown (not native select)
- Search/filter options (if searchable)
- Keyboard navigation
- Multi-select variant

---

### 1.7 Card Component (`shared/components/card`)
**File**: `src/app/shared/components/card/card.component.ts`

**Props:**
```typescript
@Input() elevated: boolean = true; // Show shadow
@Input() hoverable: boolean = false; // Lift on hover
@Input() padding: 'sm' | 'md' | 'lg' = 'md';
@Input() clickable: boolean = false;
@Output() onClick = new EventEmitter<void>();
```

**Slots:**
- `header` - Card header section
- `body` - Main content (default)
- `footer` - Card footer section

**Usage Example:**
```html
<app-card [hoverable]="true" [clickable]="true" (onClick)="viewDetails()">
  <div header>
    <h3>Card Title</h3>
  </div>
  <div body>
    <p>Card content goes here</p>
  </div>
  <div footer>
    <button>Action</button>
  </div>
</app-card>
```

---

### 1.8 Modal/Dialog Component (`shared/components/modal`)
**File**: `src/app/shared/components/modal/modal.component.ts`

**Props:**
```typescript
@Input() isOpen: boolean = false;
@Input() title?: string;
@Input() size: 'sm' | 'md' | 'lg' | 'xl' | 'full' = 'md';
@Input() closeOnBackdrop: boolean = true;
@Input() closeOnEscape: boolean = true;
@Input() showCloseButton: boolean = true;
@Output() onClose = new EventEmitter<void>();
```

**Features:**
- Backdrop overlay with blur
- Focus trap (tab stays within modal)
- Escape key to close
- Scroll lock on body
- Smooth fade-in animation
- Mobile responsive (full screen on mobile)

**Slots:**
- `header` - Modal header
- `body` - Modal content
- `footer` - Modal actions (buttons)

---

### 1.9 Toast Notification Service + Component
**Service**: `src/app/shared/services/toast.service.ts`  
**Component**: `src/app/shared/components/toast/toast.component.ts`

**Service Methods:**
```typescript
success(message: string, duration?: number): void
error(message: string, duration?: number): void
warning(message: string, duration?: number): void
info(message: string, duration?: number): void
```

**Features:**
- Toast container (top-right corner)
- Auto-dismiss after duration (default 5s)
- Multiple toasts stack vertically
- Slide-in animation
- Manual dismiss button
- Icon for each type (✓ success, ✗ error, ⚠ warning, ℹ info)

**Usage:**
```typescript
constructor(private toast: ToastService) {}

onSubmit() {
  this.authService.login(data).subscribe({
    next: () => this.toast.success('Login successful!'),
    error: () => this.toast.error('Login failed. Please try again.')
  });
}
```

---

### 1.10 Loading Spinner Component (`shared/components/spinner`)
**File**: `src/app/shared/components/spinner/spinner.component.ts`

**Props:**
```typescript
@Input() size: 'sm' | 'md' | 'lg' = 'md';
@Input() color: 'primary' | 'white' | 'gray' = 'primary';
@Input() text?: string;
```

**Variants:**
- Inline spinner (for buttons)
- Page loader (full screen overlay)
- Section loader (overlay on card/section)

---

### 1.11 Badge Component (`shared/components/badge`)
**File**: `src/app/shared/components/badge/badge.component.ts`

**Props:**
```typescript
@Input() variant: 'success' | 'warning' | 'error' | 'info' | 'neutral' = 'neutral';
@Input() size: 'sm' | 'md' = 'md';
@Input() rounded: boolean = true; // Pill shape
```

**Usage:**
```html
<app-badge variant="warning">Expires Soon</app-badge>
<app-badge variant="success">Verified</app-badge>
```

---

### 1.12 Avatar Component (`shared/components/avatar`)
**File**: `src/app/shared/components/avatar/avatar.component.ts`

**Props:**
```typescript
@Input() src?: string; // Image URL
@Input() alt?: string;
@Input() initials?: string; // Fallback if no image
@Input() size: 'xs' | 'sm' | 'md' | 'lg' | 'xl' = 'md';
@Input() status?: 'online' | 'offline' | 'away'; // Status indicator
```

---

### 1.13 Empty State Component (`shared/components/empty-state`)
**File**: `src/app/shared/components/empty-state/empty-state.component.ts`

**Props:**
```typescript
@Input() icon?: string;
@Input() title: string = '';
@Input() description?: string;
@Input() actionText?: string;
@Output() onAction = new EventEmitter<void>();
```

**Usage:**
```html
<app-empty-state
  icon="📋"
  title="No receipts yet"
  description="Upload your first receipt to get started"
  actionText="Upload Receipt"
  (onAction)="openUploadDialog()">
</app-empty-state>
```

---

### 1.14 Pagination Component (`shared/components/pagination`)
**File**: `src/app/shared/components/pagination/pagination.component.ts`

**Props:**
```typescript
@Input() currentPage: number = 1;
@Input() totalPages: number = 1;
@Input() totalItems: number = 0;
@Input() pageSize: number = 20;
@Output() pageChange = new EventEmitter<number>();
```

---

### 1.15 File Upload Component (`shared/components/file-upload`)
**File**: `src/app/shared/components/file-upload/file-upload.component.ts`

**Props:**
```typescript
@Input() accept: string = 'image/*,application/pdf';
@Input() multiple: boolean = false;
@Input() maxSize: number = 10 * 1024 * 1024; // 10MB
@Input() maxFiles?: number;
@Output() filesSelected = new EventEmitter<File[]>();
```

**Features:**
- Drag-and-drop zone
- Click to browse
- File preview (thumbnail for images)
- File size validation
- File type validation
- Progress bar during upload
- Remove file before upload
- Error messages for invalid files

---

### 1.16 Date Picker Component (`shared/components/date-picker`)
**Option A**: Use Angular Material Datepicker  
**Option B**: Use ngx-bootstrap Datepicker  
**Option C**: Build custom

**Recommended**: Use library to save time

---

### 1.17 Progress Bar Component (`shared/components/progress-bar`)
**File**: `src/app/shared/components/progress-bar/progress-bar.component.ts`

**Props:**
```typescript
@Input() value: number = 0; // 0-100
@Input() max: number = 100;
@Input() color: 'primary' | 'success' | 'warning' | 'error' = 'primary';
@Input() height: 'sm' | 'md' | 'lg' = 'md';
@Input() showLabel: boolean = false;
```

---

### 1.18 Skeleton Loader Component (`shared/components/skeleton`)
**File**: `src/app/shared/components/skeleton/skeleton.component.ts`

**Types:**
- Text line (various widths)
- Circle (for avatars)
- Rectangle (for images/cards)
- Custom shape

**Usage:**
```html
<!-- While loading receipts -->
<app-skeleton type="card" [count]="3"></app-skeleton>
```

---

### 1.19 Confirmation Dialog Component (`shared/components/confirm-dialog`)
**File**: `src/app/shared/components/confirm-dialog/confirm-dialog.component.ts`

**Props:**
```typescript
@Input() title: string = 'Confirm';
@Input() message: string = '';
@Input() confirmText: string = 'Confirm';
@Input() cancelText: string = 'Cancel';
@Input() confirmVariant: 'primary' | 'danger' = 'primary';
@Output() onConfirm = new EventEmitter<void>();
@Output() onCancel = new EventEmitter<void>();
```

**Usage:**
```typescript
// Via service
confirmDialog.show({
  title: 'Delete Receipt',
  message: 'Are you sure? This cannot be undone.',
  confirmText: 'Delete',
  confirmVariant: 'danger'
}).then(confirmed => {
  if (confirmed) this.deleteReceipt();
});
```

---

### 1.20 Breadcrumb Component (`shared/components/breadcrumb`)
**File**: `src/app/shared/components/breadcrumb/breadcrumb.component.ts`

For navigation context (e.g., Home > Receipts > Details)

---

## PRIORITY 2: Authentication Pages 🔐

### 2.1 Landing Page (`/`)
**File**: `src/app/features/auth/pages/landing/landing.component.ts`

**Sections:**
1. **Hero Section**
   - App name and tagline
   - Brief description
   - CTA buttons (Login, Sign Up)
   - Hero image or illustration

2. **Features Section**
   - Grid of 4-6 key features with icons
   - Receipt upload with OCR
   - Warranty tracking
   - Expiration notifications
   - AI chatbot
   - Receipt sharing

3. **How It Works Section**
   - 3-step process
   - Upload → Track → Get Notified

4. **Footer**
   - Copyright
   - Links (Privacy, Terms, Contact)

**Design Notes:**
- Clean, minimal design
- Lots of white space
- Blue accents (primary color)
- Mobile responsive

---

### 2.2 Login Page (`/login`)
**File**: `src/app/features/auth/pages/login/login.component.ts`

**Form Fields:**
- Email input (validated)
- Password input (with visibility toggle)
- Remember me checkbox
- Submit button (with loading state)

**Additional Elements:**
- Error message display (toast or inline)
- "Forgot password?" link
- "Don't have an account? Sign up" link
- Conditional 2FA code input (if user has 2FA enabled)

**Form Validation:**
- Email: required, email format
- Password: required, min 6 characters

**Flow:**
1. User enters credentials
2. Click submit → show loading spinner
3. If 2FA enabled → show 2FA code input
4. If success → redirect to `/receipts`
5. If error → show error message

**Service Integration:**
```typescript
onSubmit() {
  this.loading = true;
  this.authService.login(this.form.value).subscribe({
    next: (response) => {
      if (response.requiresTwoFactor) {
        this.show2FAInput = true;
      } else {
        this.router.navigate(['/receipts']);
        this.toast.success('Welcome back!');
      }
    },
    error: (err) => {
      this.error = err.error.message || 'Login failed';
      this.toast.error(this.error);
    },
    complete: () => this.loading = false
  });
}
```

---

### 2.3 Register Page (`/register`)
**File**: `src/app/features/auth/pages/register/register.component.ts`

**Form Fields:**
- Username input
- Email input
- Password input (with strength indicator)
- Confirm password input
- First name (optional)
- Last name (optional)
- Terms & conditions checkbox
- Submit button

**Password Strength Indicator:**
- Weak (red) - < 8 chars
- Medium (yellow) - 8+ chars, mixed case
- Strong (green) - 8+ chars, mixed case, numbers, symbols

**Form Validation:**
- Username: required, min 3 chars, alphanumeric
- Email: required, valid email
- Password: required, min 8 chars
- Confirm password: must match password
- Terms: must be checked

**Flow:**
1. User fills form
2. Real-time validation feedback
3. Submit → loading state
4. Success → redirect to email confirmation page
5. Error → show error message

---

### 2.4 Email Confirmation Page (`/confirm-email`)
**File**: `src/app/features/auth/pages/confirm-email/confirm-email.component.ts`

**Display:**
- Success message: "Check your email!"
- Icon (envelope or checkmark)
- Email address shown
- "Resend confirmation email" button (with cooldown)
- Countdown timer (60 seconds before can resend)
- "Back to login" link

**Features:**
- Auto-check if email is confirmed (poll every 5s)
- Resend button with loading state
- Cooldown prevents spam

---

### 2.5 Email Confirmation Handler (`/confirm-email?token=xxx`)
**Same component as 2.4, but handles token query param**

**Flow:**
1. User clicks link in email
2. Extract token from URL
3. Call API to confirm
4. Show success → redirect to login
5. Or show error → offer resend

---

### 2.6 2FA Setup Page (`/2fa/setup`)
**File**: `src/app/features/auth/pages/two-factor-setup/two-factor-setup.component.ts`

**Protected Route** - Requires authentication

**Sections:**
1. **QR Code Section**
   - Display QR code for authenticator app
   - Instructions to scan with app (Google Authenticator, Authy, etc.)

2. **Manual Key Section**
   - Show shared key (for manual entry)
   - Copy button

3. **Verification Section**
   - 6-digit code input
   - Verify button

4. **Recovery Codes Section** (after verification)
   - Display 10 recovery codes
   - Download button (as .txt file)
   - Copy all button
   - Warning: "Save these codes! You'll need them if you lose access to your authenticator app"

**Flow:**
1. Call API to enable 2FA → get QR code URL and shared key
2. Show QR code and manual key
3. User scans with app
4. User enters 6-digit code
5. Verify code → get recovery codes
6. User saves recovery codes
7. Complete → redirect to profile

---

### 2.7 2FA Verification Page (`/2fa/verify`)
**File**: `src/app/features/auth/pages/two-factor-verify/two-factor-verify.component.ts`

**Used during login if user has 2FA enabled**

**Display:**
- Icon (lock or shield)
- "Enter your 2FA code"
- 6-digit code input (auto-focus)
- Verify button
- "Use recovery code instead" link
- "Back to login" link

**Flow:**
1. User enters code from authenticator app
2. Submit → verify
3. Success → login complete → redirect
4. Error → show error, allow retry

---

## PRIORITY 3: Receipt Management Pages 📋

### 3.1 Receipts List Page (`/receipts`)
**File**: `src/app/features/receipts/pages/receipts-list/receipts-list.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌─────────────────────────────────────────────┐
│  Search bar                      [+ Upload] │
├─────────────────────────────────────────────┤
│  Filters: [All] [Expiring Soon] [Expired]  │
│  Sort: [Date ▼] [Amount] [Merchant]        │
├─────────────────────────────────────────────┤
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐   │
│  │Card 1│  │Card 2│  │Card 3│  │Card 4│   │
│  └──────┘  └──────┘  └──────┘  └──────┘   │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐   │
│  │Card 5│  │Card 6│  │Card 7│  │Card 8│   │
│  └──────┘  └──────┘  └──────┘  └──────┘   │
├─────────────────────────────────────────────┤
│         Pagination: « 1 2 3 ... 10 »        │
└─────────────────────────────────────────────┘
```

**Components Needed:**
- Search input (debounced)
- Filter chips/buttons
- Sort dropdown
- Receipt card (see 3.2)
- Pagination component
- Empty state (if no receipts)
- Floating action button (mobile - quick upload)

**Receipt Card** (see 3.2 for details)

**Features:**
- Real-time search (debounced 300ms)
- Filter by warranty status
- Sort by date, amount, merchant
- Pagination (20 items per page)
- Loading skeleton while fetching
- Empty state with CTA to upload first receipt

**Service Integration:**
```typescript
ngOnInit() {
  this.loadReceipts();
}

loadReceipts() {
  this.loading = true;
  this.receiptService.getReceipts(this.page, this.pageSize).subscribe({
    next: (data) => {
      this.receipts = data.receipts;
      this.totalCount = data.totalCount;
    },
    error: (err) => this.toast.error('Failed to load receipts'),
    complete: () => this.loading = false
  });
}

onSearch(query: string) {
  // Debounce and filter receipts
}

onUploadClick() {
  // Open upload modal
}
```

---

### 3.2 Receipt Card Component (`features/receipts/components/receipt-card`)
**File**: `src/app/features/receipts/components/receipt-card/receipt-card.component.ts`

**Layout:**
```
┌─────────────────────────────────┐
│ [Thumbnail]  Walmart            │
│              $49.99             │
│              Nintendo Switch    │
│              May 15, 2024       │
│ ──────────────────────────────  │
│ [⚠ Expires in 30 days]          │
│              [⋮ Menu]           │
└─────────────────────────────────┘
```

**Data Displayed:**
- Receipt thumbnail (image preview)
- Merchant name
- Amount (large, bold)
- Product name
- Purchase date
- Warranty expiration badge (if applicable)
- Actions menu (3 dots)

**Actions Menu:**
- View Details
- Edit
- Download
- Share
- Delete

**Styling:**
- Hover effect: lift and show shadow
- Click: navigate to detail page
- Badge colors:
  - Green: warranty valid
  - Yellow: expiring soon (< 30 days)
  - Red: expired

**Props:**
```typescript
@Input() receipt!: Receipt;
@Output() onView = new EventEmitter<string>();
@Output() onEdit = new EventEmitter<string>();
@Output() onDelete = new EventEmitter<string>();
@Output() onShare = new EventEmitter<string>();
```

---

### 3.3 Receipt Upload Modal
**File**: `src/app/features/receipts/components/upload-receipt-modal/upload-receipt-modal.component.ts`

**Trigger:** Click "+ Upload" button on receipts list

**Layout:**
```
┌────────────────────────────────────┐
│  Upload Receipt              [×]   │
├────────────────────────────────────┤
│                                    │
│  ╔════════════════════════════╗    │
│  ║  Drag & drop file here     ║    │
│  ║  or click to browse        ║    │
│  ║  📁                         ║    │
│  ╚════════════════════════════╝    │
│                                    │
│  [File preview with remove button] │
│                                    │
│  ☐ Process with OCR                │
│                                    │
│  Or enter details manually:        │
│  Merchant: [_____________]         │
│  Amount:   [_____________]         │
│  Date:     [_____________]         │
│  Product:  [_____________]         │
│  Warranty: [_____________] months  │
│                                    │
│  [Cancel]          [Upload]        │
└────────────────────────────────────┘
```

**Features:**
- File upload component (drag-and-drop)
- File preview (thumbnail)
- OCR checkbox
- Manual fields (optional - can fill before upload)
- Upload progress bar
- Success message
- Error handling

**Flow:**
1. User drops/selects file
2. Validate file (type, size)
3. Show preview
4. User optionally enters metadata
5. User clicks Upload
6. Show progress bar
7. Success → close modal, refresh list, show toast
8. Error → show error message, allow retry

---

### 3.4 Receipt Detail Page (`/receipts/:id`)
**File**: `src/app/features/receipts/pages/receipt-detail/receipt-detail.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌────────────────────────────────────────────┐
│  ← Back to Receipts                        │
├────────────────────────────────────────────┤
│  ┌──────────────────┐  ┌─────────────────┐│
│  │                  │  │ Merchant: Walmart││
│  │   Receipt Image  │  │ Amount: $49.99  ││
│  │   (with zoom)    │  │ Date: May 15    ││
│  │                  │  │ Product: Switch ││
│  │                  │  │ Warranty: 12 mo ││
│  │                  │  │ Expires: May 15 ││
│  │                  │  │                 ││
│  │                  │  │ Notes:          ││
│  │                  │  │ [__________]    ││
│  │                  │  │                 ││
│  │                  │  │ [Edit] [Delete] ││
│  │                  │  │ [Share]         ││
│  └──────────────────┘  │ [Download]      ││
│                        │ [Run OCR]       ││
│                        └─────────────────┘│
└────────────────────────────────────────────┘
```

**Sections:**
1. **Image Viewer** (left side)
   - Large receipt image
   - Zoom controls (+/-)
   - Pan/scroll if zoomed
   - Full-screen option

2. **Details Panel** (right side)
   - All receipt metadata
   - Edit button → opens edit modal
   - Delete button → confirmation dialog
   - Share button → opens share modal
   - Download button → downloads original file
   - Run OCR button (if not already processed)

**Service Integration:**
```typescript
ngOnInit() {
  this.route.params.subscribe(params => {
    this.loadReceipt(params['id']);
  });
}

loadReceipt(id: string) {
  this.receiptService.getReceipt(id).subscribe({
    next: (receipt) => this.receipt = receipt,
    error: (err) => {
      this.toast.error('Receipt not found');
      this.router.navigate(['/receipts']);
    }
  });
}

onDelete() {
  this.confirmDialog.show({
    title: 'Delete Receipt',
    message: 'Are you sure? This cannot be undone.',
    confirmText: 'Delete',
    confirmVariant: 'danger'
  }).then(confirmed => {
    if (confirmed) this.deleteReceipt();
  });
}
```

---

### 3.5 Receipt Edit Modal
**File**: `src/app/features/receipts/components/edit-receipt-modal/edit-receipt-modal.component.ts`

**Form Fields:**
- Merchant (text)
- Amount (number)
- Purchase Date (date picker)
- Product Name (text)
- Warranty Months (number)
- Notes (textarea)

**Features:**
- Pre-fill with existing data
- Real-time validation
- Save button
- Cancel button
- Auto-calculate warranty expiration date

**Flow:**
1. Load existing receipt data
2. User edits fields
3. Validate on blur
4. Submit → API call
5. Success → close modal, refresh detail view, show toast
6. Error → show error message

---

### 3.6 Share Receipt Modal
**File**: `src/app/features/receipts/components/share-receipt-modal/share-receipt-modal.component.ts`

**Layout:**
```
┌────────────────────────────────────┐
│  Share Receipt              [×]    │
├────────────────────────────────────┤
│  Share with (email or username):   │
│  [_________________________]       │
│                        [Share]     │
├────────────────────────────────────┤
│  Shared with:                      │
│  ┌──────────────────────────────┐ │
│  │ [👤] john@example.com        │ │
│  │      Shared on Jan 1    [×]  │ │
│  └──────────────────────────────┘ │
│  ┌──────────────────────────────┐ │
│  │ [👤] jane@example.com        │ │
│  │      Shared on Jan 2    [×]  │ │
│  └──────────────────────────────┘ │
├────────────────────────────────────┤
│  [Close]                           │
└────────────────────────────────────┘
```

**Features:**
- Input for recipient email/username
- Share button
- List of current shares
- Revoke button for each share
- Confirmation before revoke

---

### 3.7 Batch OCR Modal
**File**: `src/app/features/receipts/components/batch-ocr-modal/batch-ocr-modal.component.ts`

**For processing multiple receipts at once**

**Layout:**
```
┌────────────────────────────────────┐
│  Batch OCR Processing        [×]   │
├────────────────────────────────────┤
│  Select receipts to process:       │
│  ┌──────────────────────────────┐ │
│  │ ☑ Receipt 1 - Walmart        │ │
│  │ ☑ Receipt 2 - Target         │ │
│  │ ☐ Receipt 3 - Best Buy       │ │
│  │ ☑ Receipt 4 - Amazon         │ │
│  └──────────────────────────────┘ │
│                                    │
│  [Select All] [Deselect All]      │
│                                    │
│  Processing: ████████░░░░ 67%      │
│  3 of 4 completed                  │
│                                    │
│  Results:                          │
│  ✓ Receipt 1 - Success             │
│  ✓ Receipt 2 - Success             │
│  ✗ Receipt 4 - Failed              │
│                                    │
│  [Cancel]              [Start]     │
└────────────────────────────────────┘
```

---

## PRIORITY 4: Warranty Dashboard 🔔

### 4.1 Warranty Dashboard Page (`/warranties`)
**File**: `src/app/features/warranties/pages/warranty-dashboard/warranty-dashboard.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌────────────────────────────────────────────┐
│  Warranty Dashboard                        │
├────────────────────────────────────────────┤
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │
│  │ 15   │  │  5   │  │  8   │  │  2   │  │
│  │Total │  │Soon  │  │Valid │  │Exp'd │  │
│  └──────┘  └──────┘  └──────┘  └──────┘  │
├────────────────────────────────────────────┤
│  Filter: [7 days] [30 days] [60 days] [All]│
├────────────────────────────────────────────┤
│  Expiring Soon:                            │
│  ┌────────────────────────────────────┐   │
│  │ [!] Nintendo Switch - Walmart      │   │
│  │     Expires in 5 days              │   │
│  │     $49.99 | May 15, 2024    [→]  │   │
│  └────────────────────────────────────┘   │
│  ┌────────────────────────────────────┐   │
│  │ [!] iPhone 13 - Apple Store        │   │
│  │     Expires in 12 days             │   │
│  │     $799.00 | Jan 3, 2024    [→]  │   │
│  └────────────────────────────────────┘   │
│                                            │
│  [View All Receipts]                       │
└────────────────────────────────────────────┘
```

**Components:**
- Summary cards (total, expiring soon, valid, expired)
- Filter buttons (7/30/60/all days)
- Warranty card list (sorted by urgency)
- Empty state (if no warranties)

**Warranty Card:**
- Urgency icon (⚠ or 🔔)
- Product and merchant
- Days until expiration (bold, colored)
- Amount and purchase date
- Click → navigate to receipt detail

**Color Coding:**
- Red: < 7 days until expiration
- Yellow: 7-30 days
- Green: > 30 days

---

### 4.2 Notification Settings (`/settings/notifications`)
**File**: `src/app/features/settings/pages/notification-settings/notification-settings.component.ts`

See section 5.2 below

---

## PRIORITY 5: Profile & Settings Pages ⚙️

### 5.1 Profile Page (`/profile`)
**File**: `src/app/features/profile/pages/profile/profile.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌────────────────────────────────────────────┐
│  Profile                                   │
├────────────────────────────────────────────┤
│  ┌─────────────┐                           │
│  │   Avatar    │  John Doe                 │
│  │     JD      │  john@example.com ✓       │
│  └─────────────┘  Member since Jan 2024    │
│                                            │
│  ────────────────────────────────          │
│                                            │
│  Personal Information                      │
│  First Name:  [John      ]   [Edit]       │
│  Last Name:   [Doe       ]                │
│  Email:       [john@ex...] ✓ Verified     │
│  Phone:       [+1-555-...] ✓ Verified     │
│                                            │
│  ────────────────────────────────          │
│                                            │
│  Security                                  │
│  Password:    ••••••••         [Change]    │
│  2FA:         Enabled ✓        [Manage]    │
│                                            │
│  ────────────────────────────────          │
│                                            │
│  Statistics                                │
│  Total Receipts:     15                    │
│  Warranties Tracked: 12                    │
│                                            │
│  [Edit Profile]  [View Settings]           │
└────────────────────────────────────────────┘
```

**Sections:**
1. Profile header (avatar, name, email)
2. Personal information (editable)
3. Security settings
4. Statistics
5. Action buttons

---

### 5.2 Notification Settings Page (`/settings/notifications`)
**File**: `src/app/features/settings/pages/notification-settings/notification-settings.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌────────────────────────────────────────────┐
│  Notification Settings                     │
├────────────────────────────────────────────┤
│  Enable Notifications:        [Toggle ON]  │
│                                            │
│  Notification Channels:                    │
│  ☑ Email                                   │
│  ☑ SMS (requires verified phone)          │
│                                            │
│  Notify me when warranty expires in:       │
│  [────○────────────────────] 30 days       │
│  (7-90 days)                               │
│                                            │
│  Phone Number (for SMS):                   │
│  [+1-555-1234____]  ✓ Verified             │
│                     [Verify]               │
│                                            │
│  ────────────────────────────────          │
│                                            │
│  Test Notification:                        │
│  Send a test notification to verify        │
│  your settings are working correctly.      │
│  [Send Test]                               │
│                                            │
│  [Save Changes]                            │
└────────────────────────────────────────────┘
```

**Features:**
- Enable/disable toggle
- Channel checkboxes
- Threshold slider (7-90 days)
- Phone verification
- Test notification button
- Save button

---

### 5.3 Phone Verification Modal
**File**: `src/app/features/profile/components/phone-verification-modal/phone-verification-modal.component.ts`

**Layout:**
```
┌────────────────────────────────────┐
│  Verify Phone Number         [×]   │
├────────────────────────────────────┤
│  We sent a 6-digit code to:        │
│  +1-555-1234                       │
│                                    │
│  Enter code:                       │
│  [_] [_] [_] [_] [_] [_]          │
│                                    │
│  Didn't receive code?              │
│  [Resend Code] (available in 45s)  │
│                                    │
│  [Cancel]              [Verify]    │
└────────────────────────────────────┘
```

**Features:**
- 6-digit code input (auto-tab to next input)
- Resend button with countdown
- Auto-verify when all 6 digits entered

---

### 5.4 Change Password Modal
**File**: `src/app/features/profile/components/change-password-modal/change-password-modal.component.ts`

**Form Fields:**
- Current password
- New password (with strength indicator)
- Confirm new password

---

### 5.5 2FA Management Page (`/profile/2fa`)
**File**: `src/app/features/profile/pages/two-factor-management/two-factor-management.component.ts`

**If 2FA Enabled:**
- Status: Enabled ✓
- Disable button
- View/regenerate recovery codes
- List trusted devices (future enhancement)

**If 2FA Disabled:**
- Status: Disabled
- Enable button → redirects to 2FA setup

---

## PRIORITY 6: Advanced Features 🚀

### 6.1 Shared Receipts Page (`/receipts/shared`)
**File**: `src/app/features/sharing/pages/shared-receipts/shared-receipts.component.ts`

**Shows receipts shared WITH me by others**

**Layout:** Similar to receipts list, but:
- Each card shows owner name/email
- Badge: "Shared by John Doe"
- Read-only (no edit/delete)
- Can view details

---

### 6.2 My Shares Page (`/receipts/my-shares`)
**File**: `src/app/features/sharing/pages/my-shares/my-shares.component.ts`

**Shows receipts I've shared with others**

**Layout:**
- List of my receipts that are shared
- For each receipt, show list of people it's shared with
- Revoke button for each share

---

### 6.3 Chatbot Page (`/chatbot`)
**File**: `src/app/features/chatbot/pages/chatbot/chatbot.component.ts`

**Protected Route** - Requires authentication

**Layout:**
```
┌────────────────────────────────────────────┐
│  AI Receipt Assistant            [Clear]   │
├────────────────────────────────────────────┤
│  Suggested Questions:                      │
│  [Show receipts from last month]           │
│  [What did I spend at Walmart?]            │
│  [Which warranties expire soon?]           │
├────────────────────────────────────────────┤
│  ┌────────────────────────────────────┐   │
│  │ You: Show me receipts from Walmart │   │
│  └────────────────────────────────────┘   │
│  ┌────────────────────────────────────┐   │
│  │ AI: I found 3 receipts from        │   │
│  │     Walmart...                     │   │
│  │     [Receipt Card 1]               │   │
│  │     [Receipt Card 2]               │   │
│  │     [Receipt Card 3]               │   │
│  └────────────────────────────────────┘   │
│  ┌────────────────────────────────────┐   │
│  │ You: What's my total spending?     │   │
│  └────────────────────────────────────┘   │
│  ┌────────────────────────────────────┐   │
│  │ AI: Your total spending is $2,453  │   │
│  │     [Chart showing breakdown]      │   │
│  └────────────────────────────────────┘   │
│                                            │
│  ▼ ▼ ▼ (auto-scroll to bottom)            │
├────────────────────────────────────────────┤
│  [Type a message...____________] [Send]    │
└────────────────────────────────────────────┘
```

**Features:**
- Suggested questions (chips/buttons)
- Chat history (scrollable)
- Message bubbles (user vs AI)
- Receipt cards embedded in chat
- Charts/statistics inline
- Clear conversation button
- Input with send button
- Auto-scroll to latest message
- Typing indicator when AI is responding

---

### 6.4 Settings Page (`/settings`)
**File**: `src/app/features/settings/pages/settings/settings.component.ts`

**General settings:**
- Theme (future: light/dark mode)
- Language (future: i18n)
- Data export (download all data as JSON)
- Delete account (with confirmation)

---

## Routing Configuration

**File**: `src/app/app.routes.ts`

```typescript
import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  // Public routes
  { path: '', loadComponent: () => import('./features/auth/pages/landing/landing.component').then(m => m.LandingComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'confirm-email', loadComponent: () => import('./features/auth/pages/confirm-email/confirm-email.component').then(m => m.ConfirmEmailComponent) },
  { path: '2fa/verify', loadComponent: () => import('./features/auth/pages/two-factor-verify/two-factor-verify.component').then(m => m.TwoFactorVerifyComponent) },

  // Protected routes
  { 
    path: 'receipts', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipts-list/receipts-list.component').then(m => m.ReceiptsListComponent) 
  },
  { 
    path: 'receipts/:id', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipt-detail/receipt-detail.component').then(m => m.ReceiptDetailComponent) 
  },
  { 
    path: 'receipts/shared', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/sharing/pages/shared-receipts/shared-receipts.component').then(m => m.SharedReceiptsComponent) 
  },
  { 
    path: 'receipts/my-shares', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/sharing/pages/my-shares/my-shares.component').then(m => m.MySharesComponent) 
  },
  { 
    path: 'warranties', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/warranties/pages/warranty-dashboard/warranty-dashboard.component').then(m => m.WarrantyDashboardComponent) 
  },
  { 
    path: 'chatbot', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/chatbot/pages/chatbot/chatbot.component').then(m => m.ChatbotComponent) 
  },
  { 
    path: 'profile', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/pages/profile/profile.component').then(m => m.ProfileComponent) 
  },
  { 
    path: 'profile/2fa', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/pages/two-factor-management/two-factor-management.component').then(m => m.TwoFactorManagementComponent) 
  },
  { 
    path: '2fa/setup', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/auth/pages/two-factor-setup/two-factor-setup.component').then(m => m.TwoFactorSetupComponent) 
  },
  { 
    path: 'settings', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/pages/settings/settings.component').then(m => m.SettingsComponent) 
  },
  { 
    path: 'settings/notifications', 
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/pages/notification-settings/notification-settings.component').then(m => m.NotificationSettingsComponent) 
  },

  // Redirect and 404
  { path: '', redirectTo: '/', pathMatch: 'full' },
  { path: '**', redirectTo: '/' }
];
```

---

## Implementation Order Summary

### Week 1: Shared Components + Auth
1. Build all 20 shared UI components (buttons, inputs, cards, modals, etc.)
2. Create landing page
3. Create login page
4. Create register page
5. Create email confirmation page
6. Configure routing
7. Test authentication flow end-to-end

### Week 2: Receipt Management Core
8. Create receipts list page
9. Create receipt card component
10. Create upload modal
11. Create receipt detail page
12. Create edit modal
13. Test receipt CRUD operations

### Week 3: Receipt Management Advanced + Warranties
14. Create share modal
15. Create batch OCR modal
16. Create warranty dashboard
17. Test warranty tracking

### Week 4: Profile, Settings, Advanced Features
18. Create profile page
19. Create notification settings page
20. Create 2FA setup/management pages
21. Create phone verification
22. Create chatbot page
23. Create shared receipts pages

### Week 5: Polish & Testing
24. Loading states everywhere
25. Error handling
26. Responsive design testing
27. Accessibility audit
28. Performance optimization
29. E2E tests with Playwright
30. Final bug fixes

---

## Component Count Summary

**Shared Components**: 20  
**Feature Pages**: 15  
**Feature Components**: 10  
**Total**: ~45 components

---

## Next Immediate Steps

**START HERE:**

1. **Create Button Component** (`ng generate component shared/components/button`)
2. **Create Input Component** (`ng generate component shared/components/input`)
3. **Create Card Component** (`ng generate component shared/components/card`)
4. **Create Modal Component** (`ng generate component shared/components/modal`)
5. **Create Toast Service + Component** (`ng generate service shared/services/toast` + component)

Once these 5 foundational components are built, we can rapidly build all the pages.

---

**This roadmap provides complete specifications for every component and page needed to build the full application UI.**
