# Frontend Implementation Workflows

**Created**: November 16, 2025  
**Status**: Planning  
**Purpose**: Break down frontend development into actionable workflows and tasks

## Overview

This document defines the key user workflows for the Warranty App frontend and breaks them down into implementable tasks. Each workflow represents a complete user journey through the application, from authentication to core features.

## Workflow Categories

1. **Authentication & Onboarding** - User registration, login, verification
2. **Receipt Management** - Upload, view, organize receipts
3. **Warranty Tracking** - Monitor and manage warranty expirations
4. **User Profile & Settings** - Manage account and preferences
5. **AI Interactions** - OCR processing and chatbot queries
6. **Sharing & Collaboration** - Share receipts with others

---

## Workflow 1: Authentication & Onboarding

### User Journey
1. User visits the app for the first time
2. User registers a new account
3. User confirms email address
4. User logs in
5. User sets up 2FA (optional)

### Pages Required
- **Landing Page** (`/`)
- **Login Page** (`/login`)
- **Registration Page** (`/register`)
- **Email Confirmation Page** (`/confirm-email`)
- **2FA Setup Page** (`/2fa/setup`)
- **2FA Verification Page** (`/2fa/verify`)

### Components Required

#### 1. Landing Page Components
- [ ] **Hero Section**: App introduction and CTA
- [ ] **Features Overview**: Key features showcase
- [ ] **Navigation Bar**: Logo, Login, Register buttons
- [ ] **Footer**: Links and copyright

#### 2. Login Form Component
- [ ] **Email Input**: Validated email field
- [ ] **Password Input**: Password field with visibility toggle
- [ ] **Remember Me**: Checkbox for persistent login
- [ ] **Submit Button**: Primary action button
- [ ] **Error Display**: Inline error messages
- [ ] **Links**: "Forgot Password?", "Create Account"
- [ ] **2FA Code Input**: Conditional field when 2FA enabled
- [ ] **Loading State**: Spinner/skeleton while authenticating

#### 3. Registration Form Component
- [ ] **Username Input**: Validated username field
- [ ] **Email Input**: Validated email field
- [ ] **Password Input**: Password with strength indicator
- [ ] **Confirm Password**: Password confirmation field
- [ ] **First Name**: Optional text field
- [ ] **Last Name**: Optional text field
- [ ] **Terms Checkbox**: Accept terms and conditions
- [ ] **Submit Button**: Create account action
- [ ] **Error Display**: Inline validation errors
- [ ] **Link**: "Already have account? Login"

#### 4. Email Confirmation Component
- [ ] **Success Message**: Email sent confirmation
- [ ] **Resend Button**: Resend confirmation email
- [ ] **Check Status**: Auto-check confirmation status
- [ ] **Countdown Timer**: Resend cooldown (60 seconds)
- [ ] **Error Handling**: Display confirmation errors

#### 5. 2FA Setup Component
- [ ] **QR Code Display**: Show TOTP QR code
- [ ] **Manual Key Display**: Show shared key for manual entry
- [ ] **Verification Input**: 6-digit code input
- [ ] **Recovery Codes Display**: Show 10 recovery codes
- [ ] **Download/Copy Codes**: Save recovery codes
- [ ] **Confirm Button**: Verify and enable 2FA
- [ ] **Skip Option**: Optional 2FA setup

### Services/State Management
- [ ] **Auth Service**: Login, register, logout, token management
- [ ] **Auth Guard**: Route protection for authenticated users
- [ ] **Token Interceptor**: Add JWT to API requests
- [ ] **Auth State**: Store user authentication state
- [ ] **Form Validators**: Email, password, username validation

### API Integration
- [ ] `POST /api/Auth/register` - Create new account
- [ ] `POST /api/Auth/login` - Authenticate user
- [ ] `POST /api/Auth/login/2fa` - Login with 2FA code
- [ ] `POST /api/Auth/logout` - End session
- [ ] `POST /api/Auth/refresh` - Refresh access token
- [ ] `GET /api/Auth/confirm-email` - Confirm email token
- [ ] `POST /api/Auth/resend-confirmation-email` - Resend confirmation
- [ ] `POST /api/Auth/2fa/enable` - Start 2FA setup
- [ ] `POST /api/Auth/2fa/verify` - Verify and enable 2FA
- [ ] `GET /api/Auth/2fa/status` - Check 2FA status

### Routing
```typescript
const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-email', component: EmailConfirmationComponent },
  { path: '2fa/setup', component: TwoFactorSetupComponent, canActivate: [AuthGuard] },
  { path: '2fa/verify', component: TwoFactorVerifyComponent },
];
```

### Acceptance Criteria
- [ ] User can register with email and password
- [ ] User receives confirmation email
- [ ] User can confirm email address
- [ ] User can login with credentials
- [ ] User can login with 2FA if enabled
- [ ] User stays logged in with refresh tokens
- [ ] User can logout and clear session
- [ ] Form validation shows helpful error messages
- [ ] Loading states display during API calls
- [ ] Errors display user-friendly messages

---

## Workflow 2: Receipt Management

### User Journey
1. User navigates to receipts page
2. User views list of receipts
3. User uploads a new receipt (image or PDF)
4. User optionally runs OCR on receipt
5. User views receipt details
6. User edits receipt information
7. User downloads original receipt file
8. User deletes receipt

### Pages Required
- **Receipts List Page** (`/receipts`)
- **Upload Receipt Page** (`/receipts/upload`)
- **Receipt Detail Page** (`/receipts/:id`)
- **Receipt Edit Page** (`/receipts/:id/edit`)

### Components Required

#### 1. Receipts List Component
- [ ] **Search Bar**: Search by merchant, product, amount
- [ ] **Filter Dropdown**: Filter by date, warranty status
- [ ] **Sort Dropdown**: Sort by date, amount, merchant
- [ ] **Receipt Cards Grid**: Responsive card layout
- [ ] **Empty State**: Message when no receipts
- [ ] **Pagination**: Navigate through pages
- [ ] **Loading Skeleton**: Show while loading
- [ ] **Floating Action Button**: Quick upload button

#### 2. Receipt Card Component
- [ ] **Thumbnail**: Receipt image preview
- [ ] **Merchant Name**: Display merchant
- [ ] **Purchase Date**: Formatted date
- [ ] **Amount**: Currency formatted
- [ ] **Product Name**: Product description
- [ ] **Warranty Badge**: Expiration status indicator
- [ ] **Actions Menu**: Edit, Delete, Share, Download
- [ ] **Hover Effects**: Lift on hover
- [ ] **Click Handler**: Navigate to detail page

#### 3. Upload Receipt Component
- [ ] **File Input**: Hidden input with custom UI
- [ ] **Drop Zone**: Drag and drop area
- [ ] **File Preview**: Show selected file
- [ ] **File Validation**: Size and type checks
- [ ] **OCR Checkbox**: Option to run OCR
- [ ] **Manual Fields**: Merchant, amount, date, product
- [ ] **Progress Bar**: Upload progress indicator
- [ ] **Submit Button**: Upload receipt
- [ ] **Cancel Button**: Discard upload
- [ ] **Success Message**: Upload confirmation

#### 4. Receipt Detail Component
- [ ] **Image Viewer**: Full-size receipt image with zoom
- [ ] **Metadata Display**: All receipt information
- [ ] **Warranty Countdown**: Days until expiration
- [ ] **Edit Button**: Navigate to edit page
- [ ] **Delete Button**: Delete with confirmation
- [ ] **Download Button**: Download original file
- [ ] **Share Button**: Open share modal
- [ ] **OCR Button**: Run OCR if not processed
- [ ] **Back Button**: Return to list
- [ ] **Loading State**: Skeleton while fetching

#### 5. Receipt Edit Form Component
- [ ] **Merchant Input**: Text field
- [ ] **Amount Input**: Currency input
- [ ] **Purchase Date**: Date picker
- [ ] **Product Name**: Text field
- [ ] **Warranty Months**: Number input
- [ ] **Notes**: Textarea
- [ ] **Save Button**: Update receipt
- [ ] **Cancel Button**: Discard changes
- [ ] **Validation**: Field validation
- [ ] **Auto-calculate**: Warranty expiration date

#### 6. Batch Upload Component
- [ ] **Multiple File Select**: Select multiple files
- [ ] **File List**: Show all selected files
- [ ] **Remove File**: Remove from batch
- [ ] **Batch OCR Option**: Run OCR on all
- [ ] **Upload All Button**: Process batch
- [ ] **Progress List**: Show per-file progress
- [ ] **Results Summary**: Success/failed counts

#### 7. Delete Confirmation Modal
- [ ] **Warning Message**: Confirm deletion
- [ ] **Receipt Preview**: Show what will be deleted
- [ ] **Confirm Button**: Delete action (danger)
- [ ] **Cancel Button**: Abort deletion
- [ ] **Loading State**: Processing deletion

### Services/State Management
- [ ] **Receipt Service**: CRUD operations for receipts
- [ ] **Upload Service**: File upload with progress
- [ ] **OCR Service**: Trigger and monitor OCR
- [ ] **Receipt State**: Store receipts list and current receipt
- [ ] **Filter State**: Store search, filter, sort preferences
- [ ] **Cache Service**: Utilize preloaded receipt cache

### API Integration
- [ ] `GET /api/receipts` - List user receipts (paginated)
- [ ] `GET /api/receipts/{id}` - Get receipt details
- [ ] `POST /api/receipts/upload` - Upload receipt with OCR option
- [ ] `PUT /api/receipts/{id}` - Update receipt
- [ ] `DELETE /api/receipts/{id}` - Delete receipt
- [ ] `GET /api/receipts/{id}/download` - Download original file
- [ ] `POST /api/receipts/{id}/ocr` - Process OCR on existing receipt
- [ ] `POST /api/receipts/batch-ocr` - Batch OCR processing

### Routing
```typescript
const routes: Routes = [
  { path: 'receipts', component: ReceiptsListComponent, canActivate: [AuthGuard] },
  { path: 'receipts/upload', component: UploadReceiptComponent, canActivate: [AuthGuard] },
  { path: 'receipts/:id', component: ReceiptDetailComponent, canActivate: [AuthGuard] },
  { path: 'receipts/:id/edit', component: ReceiptEditComponent, canActivate: [AuthGuard] },
];
```

### Acceptance Criteria
- [ ] User can view all their receipts in a grid
- [ ] User can search and filter receipts
- [ ] User can upload receipt with drag-and-drop
- [ ] User can upload multiple receipts at once
- [ ] User can run OCR automatically on upload
- [ ] User can view full receipt details
- [ ] User can edit receipt information
- [ ] User can download original receipt file
- [ ] User can delete receipt with confirmation
- [ ] Warranty status badges display correctly
- [ ] Loading and error states are handled gracefully
- [ ] Mobile view shows receipts in single column

---

## Workflow 3: Warranty Tracking & Notifications

### User Journey
1. User navigates to warranty dashboard
2. User views expiring warranties
3. User sees notification preferences
4. User configures notification settings
5. User views notification history

### Pages Required
- **Warranty Dashboard** (`/warranties`)
- **Notification Settings** (`/settings/notifications`)

### Components Required

#### 1. Warranty Dashboard Component
- [ ] **Summary Cards**: Total, expiring soon, expired counts
- [ ] **Expiring Soon List**: Receipts expiring within threshold
- [ ] **Timeline View**: Visual timeline of expirations
- [ ] **Calendar View**: Calendar with expiration dates
- [ ] **Filter by Days**: 7, 30, 60, 90 days
- [ ] **Notification Status**: Show if notifications enabled
- [ ] **Empty State**: No warranties message
- [ ] **Quick Actions**: View receipt, edit, extend warranty

#### 2. Warranty Card Component
- [ ] **Receipt Preview**: Thumbnail
- [ ] **Merchant & Product**: Display names
- [ ] **Expiration Date**: Formatted date
- [ ] **Days Remaining**: Countdown with urgency color
- [ ] **Status Badge**: Critical, warning, ok
- [ ] **Progress Bar**: Visual time remaining
- [ ] **Action Button**: View details

#### 3. Notification Settings Component
- [ ] **Toggle Switch**: Enable/disable notifications
- [ ] **Channel Selection**: Email, SMS, Both, None
- [ ] **Threshold Slider**: Days before expiration (1-90)
- [ ] **Phone Number Input**: For SMS notifications
- [ ] **Verify Phone Button**: SMS verification flow
- [ ] **Test Notification Button**: Send test notification
- [ ] **Save Button**: Update preferences
- [ ] **Status Messages**: Success/error feedback

#### 4. Notification History Component
- [ ] **Notification List**: Past notifications sent
- [ ] **Notification Item**: Date, receipt, channel
- [ ] **Filter by Date**: Date range picker
- [ ] **Filter by Type**: Email/SMS filter
- [ ] **Pagination**: Navigate history
- [ ] **Empty State**: No notifications message

### Services/State Management
- [ ] **Warranty Service**: Fetch expiring warranties
- [ ] **Notification Service**: Manage notification settings
- [ ] **User Preference Service**: Save user preferences
- [ ] **Warranty State**: Store warranty data
- [ ] **Notification State**: Store settings and history

### API Integration
- [ ] `GET /api/warranty-notifications/expiring` - Get expiring warranties
- [ ] `GET /api/user-profile` - Get user notification preferences
- [ ] `PUT /api/user-profile` - Update notification preferences
- [ ] `POST /api/user-profile/verify-phone` - Send verification code
- [ ] `POST /api/user-profile/confirm-phone` - Verify phone code

### Routing
```typescript
const routes: Routes = [
  { path: 'warranties', component: WarrantyDashboardComponent, canActivate: [AuthGuard] },
  { path: 'settings/notifications', component: NotificationSettingsComponent, canActivate: [AuthGuard] },
];
```

### Acceptance Criteria
- [ ] User can view all expiring warranties
- [ ] Warranties display with urgency indicators
- [ ] User can filter by expiration timeframe
- [ ] User can enable/disable notifications
- [ ] User can choose notification channel (Email/SMS)
- [ ] User can set notification threshold (days)
- [ ] User can verify phone number for SMS
- [ ] Dashboard shows summary statistics
- [ ] Calendar view shows expiration dates
- [ ] Mobile view is fully functional

---

## Workflow 4: User Profile & Settings

### User Journey
1. User navigates to profile page
2. User views current profile information
3. User edits profile details
4. User changes password
5. User manages 2FA settings
6. User configures app preferences

### Pages Required
- **Profile Page** (`/profile`)
- **Edit Profile Page** (`/profile/edit`)
- **Change Password Page** (`/profile/password`)
- **2FA Management Page** (`/profile/2fa`)
- **Settings Page** (`/settings`)

### Components Required

#### 1. Profile View Component
- [ ] **Avatar Display**: User avatar or initials
- [ ] **Name Display**: Full name
- [ ] **Email Display**: Email with verified badge
- [ ] **Phone Display**: Phone with verified badge
- [ ] **Member Since**: Registration date
- [ ] **Last Login**: Last login timestamp
- [ ] **Edit Button**: Navigate to edit page
- [ ] **Stats Display**: Receipts count, warranties tracked

#### 2. Profile Edit Form Component
- [ ] **Avatar Upload**: Change profile picture
- [ ] **First Name Input**: Text field
- [ ] **Last Name Input**: Text field
- [ ] **Email Display**: Non-editable (verified)
- [ ] **Phone Input**: Phone number field
- [ ] **Verify Phone Button**: Trigger verification
- [ ] **Save Button**: Update profile
- [ ] **Cancel Button**: Discard changes
- [ ] **Success Message**: Profile updated confirmation

#### 3. Change Password Form Component
- [ ] **Current Password Input**: Verification field
- [ ] **New Password Input**: Password with strength meter
- [ ] **Confirm Password Input**: Confirmation field
- [ ] **Password Requirements**: Display rules
- [ ] **Submit Button**: Change password
- [ ] **Success Message**: Password changed confirmation
- [ ] **Validation**: Real-time validation

#### 4. 2FA Management Component
- [ ] **Status Display**: 2FA enabled/disabled
- [ ] **Enable 2FA Button**: Start setup flow
- [ ] **Disable 2FA Button**: Disable with verification
- [ ] **Recovery Codes**: View/regenerate codes
- [ ] **Trusted Devices**: List trusted devices
- [ ] **Verification Input**: Code input for changes

#### 5. Settings Component
- [ ] **Theme Toggle**: Light/Dark mode (future)
- [ ] **Language Selector**: Language preference
- [ ] **Data Export**: Export all user data
- [ ] **Delete Account**: Account deletion with confirmation
- [ ] **Privacy Settings**: Data sharing preferences
- [ ] **Email Preferences**: Marketing emails toggle

#### 6. Phone Verification Modal
- [ ] **Phone Display**: Show phone number
- [ ] **Code Input**: 6-digit verification code
- [ ] **Resend Button**: Resend SMS code
- [ ] **Verify Button**: Confirm verification
- [ ] **Timer**: Cooldown countdown
- [ ] **Error Display**: Validation errors

### Services/State Management
- [ ] **Profile Service**: Manage user profile data
- [ ] **Password Service**: Handle password changes
- [ ] **2FA Service**: Manage 2FA settings
- [ ] **Settings Service**: App preferences
- [ ] **User State**: Store current user data

### API Integration
- [ ] `GET /api/Auth/me` - Get current user
- [ ] `PUT /api/user-profile` - Update profile
- [ ] `POST /api/user-profile/change-password` - Change password
- [ ] `POST /api/user-profile/verify-phone` - Send verification SMS
- [ ] `POST /api/user-profile/confirm-phone` - Verify phone code
- [ ] `GET /api/Auth/2fa/status` - Check 2FA status
- [ ] `POST /api/Auth/2fa/enable` - Enable 2FA
- [ ] `POST /api/Auth/2fa/disable` - Disable 2FA
- [ ] `POST /api/Auth/2fa/recovery-codes/regenerate` - Regenerate codes

### Routing
```typescript
const routes: Routes = [
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'profile/edit', component: EditProfileComponent, canActivate: [AuthGuard] },
  { path: 'profile/password', component: ChangePasswordComponent, canActivate: [AuthGuard] },
  { path: 'profile/2fa', component: TwoFactorManagementComponent, canActivate: [AuthGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [AuthGuard] },
];
```

### Acceptance Criteria
- [ ] User can view profile information
- [ ] User can edit name and phone number
- [ ] User can change password securely
- [ ] User can enable/disable 2FA
- [ ] User can verify phone number
- [ ] User can view and regenerate recovery codes
- [ ] User can export their data
- [ ] User can delete account with confirmation
- [ ] All changes show success/error feedback
- [ ] Form validation works correctly

---

## Workflow 5: AI Interactions (OCR & Chatbot)

### User Journey - OCR
1. User uploads receipt
2. User selects "Process with OCR"
3. System extracts data automatically
4. User reviews extracted data
5. User corrects any errors
6. User saves receipt

### User Journey - Chatbot
1. User opens chatbot interface
2. User sees suggested questions
3. User types or selects a question
4. Chatbot responds with relevant receipts/data
5. User can follow up with more questions
6. User can clear conversation history

### Pages Required
- **OCR Processing Modal** (modal overlay)
- **Chatbot Page** (`/chatbot`)

### Components Required

#### 1. OCR Processing Modal
- [ ] **Loading State**: Processing animation
- [ ] **Extracted Data Display**: Show detected fields
- [ ] **Confidence Indicators**: Show OCR confidence
- [ ] **Edit Fields**: Allow corrections
- [ ] **Accept Button**: Save extracted data
- [ ] **Manual Entry Button**: Fill manually instead
- [ ] **Retry Button**: Run OCR again
- [ ] **Error Display**: OCR failure message

#### 2. Batch OCR Component
- [ ] **File Selection**: Multiple file picker
- [ ] **Processing Queue**: Show processing status
- [ ] **Progress Indicator**: Overall progress
- [ ] **Results List**: Success/failure per file
- [ ] **Review Button**: Review extracted data
- [ ] **Retry Failed**: Reprocess failed items
- [ ] **Cancel Button**: Stop processing

#### 3. Chatbot Interface Component
- [ ] **Message List**: Conversation history
- [ ] **Message Bubble**: User/assistant messages
- [ ] **Input Field**: Message input with send button
- [ ] **Suggested Questions**: Quick action chips
- [ ] **Receipt Cards**: Inline receipt previews
- [ ] **Typing Indicator**: Assistant is typing...
- [ ] **Clear History Button**: Reset conversation
- [ ] **Scroll to Bottom**: Auto-scroll to new messages
- [ ] **Loading State**: Fetching history

#### 4. Chatbot Message Component
- [ ] **User Message**: Right-aligned bubble
- [ ] **Assistant Message**: Left-aligned bubble
- [ ] **Timestamp**: Message timestamp
- [ ] **Receipt Preview**: Clickable receipt card
- [ ] **Statistics Display**: Charts/graphs inline
- [ ] **Copy Button**: Copy message text
- [ ] **Action Buttons**: Quick actions from response

#### 5. Suggested Questions Component
- [ ] **Question Chips**: Clickable suggestions
- [ ] **Categories**: Grouped by topic
- [ ] **Dynamic Suggestions**: Based on user data
- [ ] **Recent Queries**: Show recent searches

### Services/State Management
- [ ] **OCR Service**: Process receipts with AI
- [ ] **Chatbot Service**: Send messages, get responses
- [ ] **Conversation State**: Store chat history
- [ ] **OCR State**: Store processing status

### API Integration
- [ ] `POST /api/receipts/upload?UseOcr=true` - Upload with OCR
- [ ] `POST /api/receipts/{id}/ocr` - Process existing receipt
- [ ] `POST /api/receipts/batch-ocr` - Batch OCR
- [ ] `POST /api/chatbot/message` - Send message to chatbot
- [ ] `GET /api/chatbot/history` - Get conversation history
- [ ] `DELETE /api/chatbot/history` - Clear conversation
- [ ] `GET /api/chatbot/suggested-questions` - Get suggestions

### Routing
```typescript
const routes: Routes = [
  { path: 'chatbot', component: ChatbotComponent, canActivate: [AuthGuard] },
];
```

### Acceptance Criteria
- [ ] User can upload receipt with OCR enabled
- [ ] OCR automatically extracts merchant, amount, date, product
- [ ] User can review and edit extracted data
- [ ] User can retry OCR if results are poor
- [ ] User can process multiple receipts with batch OCR
- [ ] User can ask chatbot questions in natural language
- [ ] Chatbot provides relevant receipt information
- [ ] Chatbot shows statistics and insights
- [ ] Conversation history persists across sessions
- [ ] User can clear conversation history
- [ ] Suggested questions provide quick access
- [ ] Receipt previews are clickable and navigate to detail

---

## Workflow 6: Sharing & Collaboration

### User Journey
1. User navigates to receipt detail page
2. User clicks "Share" button
3. User enters recipient email/username
4. User sends share invitation
5. Recipient receives notification
6. Recipient views shared receipt (read-only)
7. User can view who has access
8. User can revoke access

### Pages Required
- **Shared Receipts Page** (`/receipts/shared`)
- **My Shares Page** (`/receipts/my-shares`)

### Components Required

#### 1. Share Receipt Modal
- [ ] **Recipient Input**: Email or username field
- [ ] **Recipient Search**: Autocomplete suggestions
- [ ] **Share Button**: Send share invitation
- [ ] **Cancel Button**: Close modal
- [ ] **Success Message**: Share sent confirmation
- [ ] **Error Display**: Invalid recipient error

#### 2. Shared With List Component
- [ ] **User Avatar**: Recipient avatar
- [ ] **User Name**: Recipient name/email
- [ ] **Shared Date**: When shared
- [ ] **Revoke Button**: Remove access
- [ ] **Empty State**: No one has access
- [ ] **Confirmation Modal**: Confirm revoke

#### 3. Shared Receipts List Component
- [ ] **Receipt Cards**: Shared receipt cards
- [ ] **Owner Badge**: Show original owner
- [ ] **Shared Date**: When received
- [ ] **Read-only Badge**: Indicate no edit access
- [ ] **Filter**: By owner
- [ ] **Empty State**: No shared receipts

#### 4. My Shares List Component
- [ ] **Receipt Cards**: Receipts I've shared
- [ ] **Shared With**: List of recipients
- [ ] **Manage Button**: View/revoke shares
- [ ] **Unshare All Button**: Revoke all access
- [ ] **Empty State**: Nothing shared yet

### Services/State Management
- [ ] **Sharing Service**: Share and manage access
- [ ] **Shared Receipts State**: Store shared receipts
- [ ] **Share Notifications**: Handle share notifications

### API Integration
- [ ] `POST /api/receipt-sharing/share` - Share receipt
- [ ] `GET /api/receipt-sharing/shared-with-me` - List shared receipts
- [ ] `GET /api/receipt-sharing/my-shares` - List my shares
- [ ] `DELETE /api/receipt-sharing/{shareId}` - Revoke access
- [ ] `GET /api/receipt-sharing/{receiptId}/shares` - List receipt shares

### Routing
```typescript
const routes: Routes = [
  { path: 'receipts/shared', component: SharedReceiptsComponent, canActivate: [AuthGuard] },
  { path: 'receipts/my-shares', component: MySharesComponent, canActivate: [AuthGuard] },
];
```

### Acceptance Criteria
- [ ] User can share receipt by email/username
- [ ] Recipient receives notification
- [ ] Recipient can view shared receipt (read-only)
- [ ] User can view list of shared receipts
- [ ] User can view who has access to each receipt
- [ ] User can revoke share access
- [ ] Shared receipts display owner information
- [ ] Shared receipts appear in warranty dashboard
- [ ] User cannot edit shared receipts
- [ ] Share notifications are sent via email

---

## Cross-Cutting Concerns

### Global Components

#### Navigation Components
- [ ] **Top Navigation Bar**: Logo, menu, user menu
- [ ] **Mobile Navigation**: Hamburger menu, drawer
- [ ] **User Menu Dropdown**: Profile, settings, logout
- [ ] **Breadcrumbs**: Navigation path indicator
- [ ] **Back Button**: Browser-like back navigation

#### Layout Components
- [ ] **App Shell**: Main layout wrapper
- [ ] **Sidebar**: Navigation menu (desktop)
- [ ] **Content Container**: Page content wrapper
- [ ] **Footer**: App footer with links

#### Utility Components
- [ ] **Loading Spinner**: Global loading indicator
- [ ] **Toast Notifications**: Success/error toasts
- [ ] **Confirmation Dialog**: Generic confirmation modal
- [ ] **Error Boundary**: Catch and display errors
- [ ] **Offline Indicator**: Show when offline
- [ ] **Update Notification**: PWA update available

#### Form Components
- [ ] **Text Input**: Reusable text input
- [ ] **Email Input**: Email with validation
- [ ] **Password Input**: Password with visibility toggle
- [ ] **Number Input**: Number with validation
- [ ] **Date Picker**: Calendar date selector
- [ ] **File Input**: File upload with preview
- [ ] **Checkbox**: Styled checkbox
- [ ] **Radio Button**: Styled radio button
- [ ] **Toggle Switch**: On/off switch
- [ ] **Dropdown Select**: Dropdown menu
- [ ] **Textarea**: Multi-line text input
- [ ] **Search Input**: Search with icon
- [ ] **Currency Input**: Amount with currency symbol

### Global Services
- [ ] **HTTP Interceptor**: Add auth headers, handle errors
- [ ] **Error Handler**: Global error handling
- [ ] **Toast Service**: Show notifications
- [ ] **Storage Service**: LocalStorage wrapper
- [ ] **Theme Service**: Manage themes (future)
- [ ] **Analytics Service**: Track user events
- [ ] **PWA Service**: Service worker management

### State Management
- [ ] **Auth State**: User authentication
- [ ] **User State**: Current user data
- [ ] **Receipt State**: Receipts data
- [ ] **Notification State**: App notifications
- [ ] **UI State**: Loading, errors, modals

### Responsive Design
- [ ] **Mobile Navigation**: Hamburger menu
- [ ] **Responsive Grid**: Adjust columns by screen size
- [ ] **Touch Gestures**: Swipe, pinch, etc.
- [ ] **Mobile-Optimized Forms**: Larger inputs
- [ ] **Bottom Navigation** (mobile): Tab bar navigation
- [ ] **Floating Action Button** (mobile): Quick actions

### Performance Optimization
- [ ] **Lazy Loading**: Load routes on demand
- [ ] **Image Optimization**: Lazy load images
- [ ] **Virtual Scrolling**: For long lists
- [ ] **Debounce Inputs**: Search, filter inputs
- [ ] **Memoization**: Cache computed values
- [ ] **Code Splitting**: Bundle optimization

### Testing
- [ ] **Unit Tests**: Component testing
- [ ] **Integration Tests**: Service testing
- [ ] **E2E Tests**: User flow testing with Playwright
- [ ] **Accessibility Tests**: WCAG compliance
- [ ] **Visual Regression Tests**: Screenshot diffs

---

## Implementation Phases

### Phase 1: Foundation (Weeks 1-2)
**Goal**: Set up project structure and authentication

- [ ] Initialize Angular project
- [ ] Configure ESLint and Prettier
- [ ] Set up routing
- [ ] Implement design system (colors, typography, spacing)
- [ ] Create global components (navbar, footer, layout)
- [ ] Implement authentication (login, register, 2FA)
- [ ] Set up state management
- [ ] Configure API service and interceptors

### Phase 2: Core Features (Weeks 3-4)
**Goal**: Implement receipt management

- [ ] Build receipt list page with grid
- [ ] Implement receipt upload with drag-and-drop
- [ ] Create receipt detail page
- [ ] Build receipt edit form
- [ ] Implement file download
- [ ] Add delete functionality with confirmation
- [ ] Implement search and filters
- [ ] Add pagination

### Phase 3: Advanced Features (Weeks 5-6)
**Goal**: Add warranty tracking and OCR

- [ ] Build warranty dashboard
- [ ] Implement warranty cards with countdown
- [ ] Create notification settings page
- [ ] Integrate OCR processing
- [ ] Build batch OCR interface
- [ ] Add chatbot interface
- [ ] Implement suggested questions

### Phase 4: Collaboration (Week 7)
**Goal**: Add sharing features

- [ ] Build share receipt modal
- [ ] Create shared receipts page
- [ ] Implement my shares page
- [ ] Add share management
- [ ] Implement share notifications

### Phase 5: Polish & Testing (Week 8)
**Goal**: Refinement and quality assurance

- [ ] Implement loading states everywhere
- [ ] Add error handling and messages
- [ ] Optimize performance (lazy loading, caching)
- [ ] Write unit and E2E tests
- [ ] Accessibility audit and fixes
- [ ] Mobile responsiveness testing
- [ ] Browser compatibility testing
- [ ] Performance testing

### Phase 6: Deployment (Week 9)
**Goal**: Production deployment

- [ ] Configure production environment
- [ ] Set up CI/CD pipeline for frontend
- [ ] Deploy to hosting platform (Azure/Vercel/Netlify)
- [ ] Configure CDN
- [ ] Set up monitoring and analytics
- [ ] Final testing in production

---

## Success Metrics

### Performance
- [ ] First Contentful Paint < 1.5s
- [ ] Time to Interactive < 3s
- [ ] Lighthouse score > 90
- [ ] Core Web Vitals pass

### Quality
- [ ] Test coverage > 80%
- [ ] Zero accessibility violations
- [ ] No console errors
- [ ] Cross-browser compatibility

### User Experience
- [ ] Mobile-friendly (responsive design)
- [ ] Smooth animations (60fps)
- [ ] Intuitive navigation
- [ ] Clear error messages
- [ ] Fast perceived performance

---

## Technology Decisions

### Framework & Libraries
- **Framework**: Angular 18+
- **State Management**: NgRx or Angular Signals
- **HTTP Client**: Angular HttpClient
- **Forms**: Reactive Forms
- **Routing**: Angular Router
- **UI Library**: Angular Material (customized)
- **Icons**: Material Icons or Heroicons
- **Date Handling**: date-fns or Day.js
- **Charts**: Chart.js or ngx-charts
- **File Upload**: ngx-dropzone
- **Image Viewer**: ngx-image-zoom

### Development Tools
- **Linting**: ESLint with Angular rules
- **Formatting**: Prettier
- **Testing**: Jasmine + Karma + Playwright
- **Build**: Angular CLI
- **Package Manager**: npm or pnpm

### Deployment
- **Hosting**: Azure Static Web Apps / Vercel / Netlify
- **CDN**: Cloudflare or Azure CDN
- **CI/CD**: GitHub Actions

---

**Next Steps**: Begin Phase 1 implementation with project setup and authentication workflows.

**Review Date**: After Phase 1 completion  
**Document Owner**: Development Team
