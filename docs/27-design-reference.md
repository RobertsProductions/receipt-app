# Design Reference Document - Warranty App

**Created**: November 16, 2025  
**Status**: Active  
**Version**: 1.0

## Overview

This document defines the visual design language, UI/UX principles, and style guidelines for the Warranty App frontend. The design philosophy emphasizes **minimalism**, **calm aesthetics**, **snappy performance**, and **responsive design** that works seamlessly across desktop, tablet, and mobile devices.

## Design Philosophy

### Core Principles

1. **Minimalism First**: Clean, uncluttered interfaces with plenty of white space
2. **Calm & Focused**: Soothing color palette that reduces visual stress
3. **Snappy & Responsive**: Fast, fluid interactions with immediate feedback
4. **Mobile-First**: Designed for touch and small screens, scales up beautifully
5. **Accessible**: WCAG 2.1 AA compliant with high contrast ratios

## Color Palette

### Primary Colors

```css
--primary-50:  #E3F2FD;   /* Lightest blue - backgrounds */
--primary-100: #BBDEFB;   /* Light blue - hover states */
--primary-200: #90CAF9;   /* Medium blue - borders */
--primary-300: #64B5F6;   /* Blue - secondary actions */
--primary-400: #42A5F5;   /* Blue - primary actions */
--primary-500: #2196F3;   /* Main brand blue */
--primary-600: #1E88E5;   /* Dark blue - active states */
--primary-700: #1976D2;   /* Darker blue - emphasis */
--primary-800: #1565C0;   /* Very dark blue */
--primary-900: #0D47A1;   /* Darkest blue */
```

**Usage**: Primary actions, links, active states, branding

### Neutral Colors

```css
--gray-50:  #FAFAFA;   /* Background - lightest */
--gray-100: #F5F5F5;   /* Background - light */
--gray-200: #EEEEEE;   /* Borders, dividers */
--gray-300: #E0E0E0;   /* Borders - darker */
--gray-400: #BDBDBD;   /* Disabled text */
--gray-500: #9E9E9E;   /* Placeholder text */
--gray-600: #757575;   /* Secondary text */
--gray-700: #616161;   /* Primary text - light */
--gray-800: #424242;   /* Primary text */
--gray-900: #212121;   /* Headings, emphasis */
```

**Usage**: Text, backgrounds, borders, shadows

### Accent Colors

```css
--success-light: #C8E6C9;   /* Light green */
--success:       #4CAF50;   /* Green - success states */
--success-dark:  #388E3C;   /* Dark green */

--warning-light: #FFF9C4;   /* Light yellow */
--warning:       #FFC107;   /* Amber - warnings */
--warning-dark:  #FFA000;   /* Dark amber */

--error-light:   #FFCDD2;   /* Light red */
--error:         #F44336;   /* Red - errors */
--error-dark:    #D32F2F;   /* Dark red */

--info-light:    #B3E5FC;   /* Light cyan */
--info:          #00BCD4;   /* Cyan - info messages */
--info-dark:     #0097A7;   /* Dark cyan */
```

**Usage**: Status indicators, alerts, notifications, validation feedback

### Semantic Colors

```css
--background:       #FFFFFF;   /* Main background */
--surface:          #F8F9FA;   /* Card/panel background */
--surface-elevated: #FFFFFF;   /* Elevated cards (with shadow) */
--text-primary:     #212121;   /* Main text color */
--text-secondary:   #616161;   /* Secondary text */
--text-disabled:    #BDBDBD;   /* Disabled text */
--border:           #E0E0E0;   /* Default border color */
--divider:          #EEEEEE;   /* Subtle dividers */
```

## Typography

### Font Families

**Primary Font (UI)**: 
```css
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
             'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
             sans-serif;
```

**Monospace Font (Code)**: 
```css
font-family: 'SF Mono', 'Monaco', 'Inconsolata', 'Fira Code', 'Droid Sans Mono',
             'Source Code Pro', monospace;
```

### Font Scale

```css
--text-xs:   0.75rem;  /* 12px - captions, labels */
--text-sm:   0.875rem; /* 14px - small text */
--text-base: 1rem;     /* 16px - body text */
--text-lg:   1.125rem; /* 18px - large text */
--text-xl:   1.25rem;  /* 20px - headings */
--text-2xl:  1.5rem;   /* 24px - large headings */
--text-3xl:  1.875rem; /* 30px - page titles */
--text-4xl:  2.25rem;  /* 36px - hero text */
```

### Font Weights

```css
--font-light:   300;   /* Light text */
--font-normal:  400;   /* Body text */
--font-medium:  500;   /* Emphasized text */
--font-semibold: 600;  /* Headings */
--font-bold:    700;   /* Strong emphasis */
```

### Line Heights

```css
--leading-tight:  1.25;  /* Headings */
--leading-normal: 1.5;   /* Body text */
--leading-relaxed: 1.75; /* Comfortable reading */
```

## Spacing System

Based on 8px grid system for consistency:

```css
--space-0:  0;
--space-1:  0.25rem;  /* 4px */
--space-2:  0.5rem;   /* 8px */
--space-3:  0.75rem;  /* 12px */
--space-4:  1rem;     /* 16px */
--space-5:  1.25rem;  /* 20px */
--space-6:  1.5rem;   /* 24px */
--space-8:  2rem;     /* 32px */
--space-10: 2.5rem;   /* 40px */
--space-12: 3rem;     /* 48px */
--space-16: 4rem;     /* 64px */
--space-20: 5rem;     /* 80px */
--space-24: 6rem;     /* 96px */
```

## Border Radius

```css
--radius-none: 0;
--radius-sm:   0.125rem;  /* 2px - subtle rounding */
--radius-base: 0.25rem;   /* 4px - default */
--radius-md:   0.375rem;  /* 6px - cards */
--radius-lg:   0.5rem;    /* 8px - buttons */
--radius-xl:   0.75rem;   /* 12px - large cards */
--radius-2xl:  1rem;      /* 16px - modals */
--radius-full: 9999px;    /* Pills, avatars */
```

## Shadows

Subtle, layered shadows for depth:

```css
--shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
--shadow:    0 1px 3px 0 rgba(0, 0, 0, 0.1), 
             0 1px 2px 0 rgba(0, 0, 0, 0.06);
--shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 
             0 2px 4px -1px rgba(0, 0, 0, 0.06);
--shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 
             0 4px 6px -2px rgba(0, 0, 0, 0.05);
--shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 
             0 10px 10px -5px rgba(0, 0, 0, 0.04);
```

## Animation & Transitions

### Timing Functions

```css
--ease-in:     cubic-bezier(0.4, 0, 1, 1);
--ease-out:    cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
--ease-snappy: cubic-bezier(0.4, 0, 0.6, 1);  /* Custom snappy feel */
```

### Durations

```css
--duration-fast:   150ms;  /* Micro-interactions */
--duration-base:   200ms;  /* Default transitions */
--duration-medium: 300ms;  /* Smooth transitions */
--duration-slow:   500ms;  /* Emphasized transitions */
```

### Common Transitions

```css
/* Default transition for most elements */
transition: all 200ms cubic-bezier(0.4, 0, 0.2, 1);

/* Button hover */
transition: background-color 150ms cubic-bezier(0.4, 0, 0.6, 1),
            transform 150ms cubic-bezier(0.4, 0, 0.6, 1);

/* Modal entrance */
transition: opacity 300ms cubic-bezier(0, 0, 0.2, 1),
            transform 300ms cubic-bezier(0, 0, 0.2, 1);
```

## Component Specifications

### Buttons

**Primary Button**:
```css
background: var(--primary-500);
color: #FFFFFF;
padding: 0.75rem 1.5rem;
border-radius: var(--radius-lg);
font-weight: var(--font-medium);
box-shadow: var(--shadow-sm);
transition: all 150ms var(--ease-snappy);

/* Hover */
background: var(--primary-600);
transform: translateY(-1px);
box-shadow: var(--shadow-md);

/* Active */
background: var(--primary-700);
transform: translateY(0);
box-shadow: var(--shadow-sm);

/* Disabled */
background: var(--gray-300);
color: var(--gray-500);
cursor: not-allowed;
```

**Secondary Button**:
```css
background: transparent;
color: var(--primary-500);
border: 2px solid var(--primary-500);
padding: 0.625rem 1.375rem; /* Adjust for border */
border-radius: var(--radius-lg);
font-weight: var(--font-medium);
transition: all 150ms var(--ease-snappy);

/* Hover */
background: var(--primary-50);
border-color: var(--primary-600);
color: var(--primary-600);
```

**Ghost Button**:
```css
background: transparent;
color: var(--gray-700);
padding: 0.75rem 1.5rem;
border-radius: var(--radius-lg);
font-weight: var(--font-medium);
transition: all 150ms var(--ease-snappy);

/* Hover */
background: var(--gray-100);
color: var(--gray-900);
```

**Button Sizes**:
- Small: `padding: 0.5rem 1rem; font-size: 0.875rem;`
- Medium: `padding: 0.75rem 1.5rem; font-size: 1rem;` (default)
- Large: `padding: 1rem 2rem; font-size: 1.125rem;`

### Input Fields

```css
background: #FFFFFF;
border: 2px solid var(--gray-300);
border-radius: var(--radius-md);
padding: 0.75rem 1rem;
font-size: var(--text-base);
color: var(--text-primary);
transition: all 200ms var(--ease-in-out);

/* Focus */
border-color: var(--primary-500);
outline: none;
box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.1);

/* Error */
border-color: var(--error);
box-shadow: 0 0 0 3px rgba(244, 67, 54, 0.1);

/* Disabled */
background: var(--gray-100);
color: var(--gray-500);
cursor: not-allowed;
```

**Labels**:
```css
font-size: var(--text-sm);
font-weight: var(--font-medium);
color: var(--gray-700);
margin-bottom: var(--space-2);
```

### Cards

```css
background: var(--surface-elevated);
border-radius: var(--radius-xl);
padding: var(--space-6);
box-shadow: var(--shadow);
transition: all 200ms var(--ease-in-out);

/* Hover (interactive cards) */
box-shadow: var(--shadow-md);
transform: translateY(-2px);
```

### Modals

```css
/* Backdrop */
background: rgba(0, 0, 0, 0.5);
backdrop-filter: blur(4px);

/* Modal */
background: #FFFFFF;
border-radius: var(--radius-2xl);
padding: var(--space-8);
box-shadow: var(--shadow-xl);
max-width: 90vw;
max-height: 90vh;
overflow-y: auto;
```

### Navigation Bar

```css
background: #FFFFFF;
border-bottom: 1px solid var(--divider);
padding: var(--space-4) var(--space-6);
box-shadow: var(--shadow-sm);
height: 64px;

/* Mobile */
@media (max-width: 768px) {
  padding: var(--space-3) var(--space-4);
  height: 56px;
}
```

### Status Badges

```css
/* Success */
background: var(--success-light);
color: var(--success-dark);
padding: 0.25rem 0.75rem;
border-radius: var(--radius-full);
font-size: var(--text-xs);
font-weight: var(--font-medium);

/* Warning */
background: var(--warning-light);
color: var(--warning-dark);

/* Error */
background: var(--error-light);
color: var(--error-dark);

/* Info */
background: var(--info-light);
color: var(--info-dark);
```

## Responsive Design

### Breakpoints

```css
--breakpoint-sm: 640px;   /* Mobile landscape */
--breakpoint-md: 768px;   /* Tablet portrait */
--breakpoint-lg: 1024px;  /* Tablet landscape / small desktop */
--breakpoint-xl: 1280px;  /* Desktop */
--breakpoint-2xl: 1536px; /* Large desktop */
```

### Mobile-First Approach

Design and develop for mobile first, then enhance for larger screens:

```css
/* Mobile (default) */
.container {
  padding: 1rem;
  font-size: 1rem;
}

/* Tablet and up */
@media (min-width: 768px) {
  .container {
    padding: 2rem;
  }
}

/* Desktop and up */
@media (min-width: 1024px) {
  .container {
    padding: 3rem;
    font-size: 1.125rem;
  }
}
```

### Touch Targets

All interactive elements must be at least **44x44px** for touch accessibility:

```css
/* Minimum touch target */
min-height: 44px;
min-width: 44px;

/* Preferred touch target */
min-height: 48px;
min-width: 48px;
```

### Container Widths

```css
/* Mobile */
width: 100%;
padding: 0 1rem;

/* Tablet */
@media (min-width: 768px) {
  max-width: 720px;
  margin: 0 auto;
}

/* Desktop */
@media (min-width: 1024px) {
  max-width: 960px;
}

/* Large Desktop */
@media (min-width: 1280px) {
  max-width: 1200px;
}
```

## Layout Patterns

### Grid System

12-column grid with gap spacing:

```css
.grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-6);
}

/* Mobile: Single column */
.col {
  grid-column: span 12;
}

/* Tablet: Half-width */
@media (min-width: 768px) {
  .col-md-6 {
    grid-column: span 6;
  }
}

/* Desktop: Third-width */
@media (min-width: 1024px) {
  .col-lg-4 {
    grid-column: span 4;
  }
}
```

### Common Layouts

**Dashboard Layout**:
```
┌────────────────────────────────┐
│         Navigation Bar          │ 64px
├────────────────────────────────┤
│  ┌──────────┐  ┌─────────────┐ │
│  │          │  │             │ │
│  │ Sidebar  │  │   Content   │ │
│  │  240px   │  │    Fluid    │ │
│  │          │  │             │ │
│  └──────────┘  └─────────────┘ │
└────────────────────────────────┘

Mobile: Sidebar hidden, hamburger menu
```

**Card Grid Layout**:
```
┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐
│     │ │     │ │     │ │     │
│ Card│ │ Card│ │ Card│ │ Card│
│     │ │     │ │     │ │     │
└─────┘ └─────┘ └─────┘ └─────┘

Mobile: 1 column
Tablet: 2 columns
Desktop: 3-4 columns
```

## Performance Guidelines

### Snappy Interactions

1. **Instant Feedback**: Visual feedback within 100ms
2. **Optimistic Updates**: Show changes immediately, sync in background
3. **Skeleton Screens**: Show loading placeholders, not spinners
4. **Lazy Loading**: Load images and components on demand
5. **Debounce Search**: 300ms delay on search inputs

### Loading States

**Skeleton Loader**:
```css
.skeleton {
  background: linear-gradient(
    90deg,
    var(--gray-200) 25%,
    var(--gray-300) 50%,
    var(--gray-200) 75%
  );
  background-size: 200% 100%;
  animation: loading 1.5s ease-in-out infinite;
}

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

**Progress Indicators**:
```css
/* Linear progress bar */
height: 4px;
background: var(--gray-200);
border-radius: var(--radius-full);
overflow: hidden;

.progress-bar {
  background: var(--primary-500);
  height: 100%;
  transition: width 300ms var(--ease-out);
}
```

## Accessibility

### Color Contrast

Ensure WCAG 2.1 AA compliance:

- **Normal text**: 4.5:1 contrast ratio minimum
- **Large text (18px+)**: 3:1 contrast ratio minimum
- **UI components**: 3:1 contrast ratio minimum

### Focus States

All interactive elements must have visible focus indicators:

```css
:focus-visible {
  outline: 2px solid var(--primary-500);
  outline-offset: 2px;
}

/* Custom focus for specific components */
.button:focus-visible {
  box-shadow: 0 0 0 3px rgba(33, 150, 243, 0.3);
  outline: none;
}
```

### Keyboard Navigation

- Tab order follows visual hierarchy
- All interactive elements are keyboard accessible
- Escape key closes modals and dropdowns
- Arrow keys navigate lists and menus

### Screen Reader Support

- Use semantic HTML (`<nav>`, `<main>`, `<article>`, etc.)
- Provide `aria-label` for icon-only buttons
- Use `aria-live` regions for dynamic content
- Ensure form labels are associated with inputs

## Icons

### Icon Library

Use **Material Icons** or **Heroicons** for consistency:

**Size Scale**:
```css
--icon-xs:  16px;
--icon-sm:  20px;
--icon-base: 24px;
--icon-lg:  32px;
--icon-xl:  40px;
```

**Icon Colors**:
```css
/* Default */
color: var(--gray-600);

/* Primary */
color: var(--primary-500);

/* Success/Error/Warning */
color: var(--success); /* or --error, --warning */
```

## Animation Principles

### Micro-interactions

1. **Button Press**: Scale down slightly (0.98) on click
2. **Hover States**: Lift element 1-2px with shadow
3. **Slide-in**: Enter from right/left with opacity fade
4. **Fade-in**: Opacity 0 to 1 with slight upward movement
5. **Pulse**: Subtle scale animation for notifications

### Page Transitions

```css
/* Fade transition */
.fade-enter-active, .fade-leave-active {
  transition: opacity 200ms var(--ease-in-out);
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* Slide transition */
.slide-enter-active, .slide-leave-active {
  transition: transform 300ms var(--ease-in-out),
              opacity 300ms var(--ease-in-out);
}
.slide-enter-from {
  transform: translateX(100%);
  opacity: 0;
}
.slide-leave-to {
  transform: translateX(-100%);
  opacity: 0;
}
```

## Dark Mode (Future Enhancement)

Planned support for dark mode with minimal changes:

```css
@media (prefers-color-scheme: dark) {
  --background: #121212;
  --surface: #1E1E1E;
  --text-primary: #E0E0E0;
  --text-secondary: #B0B0B0;
  --border: #2C2C2C;
  /* ... other dark mode colors */
}
```

## Implementation Notes

### CSS Architecture

1. Use **CSS Custom Properties** for all design tokens
2. Follow **BEM naming convention** for classes
3. Use **CSS Modules** or **styled-components** for Angular
4. Keep specificity low, avoid deep nesting
5. Use **mobile-first media queries**

### Component Library

Consider using **Angular Material** as a base with heavy customization:
- Provides accessible components out of the box
- Can be themed with custom color palette
- Includes responsive utilities
- Well-documented and maintained

### Testing

- Test on real devices (iOS, Android)
- Verify touch targets are adequate
- Test with screen readers
- Validate color contrast ratios
- Test animations on low-end devices

## Examples

### Receipt Card Component

```typescript
// receipt-card.component.ts
<div class="receipt-card">
  <div class="receipt-header">
    <h3 class="receipt-merchant">{{ receipt.merchant }}</h3>
    <span class="receipt-date">{{ receipt.date | date:'short' }}</span>
  </div>
  <div class="receipt-body">
    <p class="receipt-amount">{{ receipt.amount | currency }}</p>
    <p class="receipt-product">{{ receipt.productName }}</p>
  </div>
  <div class="receipt-footer">
    <span class="badge badge-warning" *ngIf="isExpiringSoon">
      Expires in {{ daysUntilExpiration }} days
    </span>
  </div>
</div>
```

```scss
// receipt-card.component.scss
.receipt-card {
  background: var(--surface-elevated);
  border-radius: var(--radius-xl);
  padding: var(--space-6);
  box-shadow: var(--shadow);
  transition: all 200ms var(--ease-in-out);
  cursor: pointer;

  &:hover {
    box-shadow: var(--shadow-md);
    transform: translateY(-2px);
  }

  .receipt-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-4);
  }

  .receipt-merchant {
    font-size: var(--text-lg);
    font-weight: var(--font-semibold);
    color: var(--text-primary);
    margin: 0;
  }

  .receipt-date {
    font-size: var(--text-sm);
    color: var(--text-secondary);
  }

  .receipt-amount {
    font-size: var(--text-2xl);
    font-weight: var(--font-bold);
    color: var(--primary-600);
    margin: var(--space-2) 0;
  }

  .receipt-product {
    font-size: var(--text-base);
    color: var(--text-secondary);
    margin: 0;
  }

  .receipt-footer {
    margin-top: var(--space-4);
    padding-top: var(--space-4);
    border-top: 1px solid var(--divider);
  }
}

// Mobile adjustments
@media (max-width: 768px) {
  .receipt-card {
    padding: var(--space-4);
  }

  .receipt-merchant {
    font-size: var(--text-base);
  }

  .receipt-amount {
    font-size: var(--text-xl);
  }
}
```

## Resources

### Tools

- **Color Contrast Checker**: https://webaim.org/resources/contrastchecker/
- **Responsive Design Tester**: https://responsivedesignchecker.com/
- **Animation Easings**: https://easings.net/
- **Accessibility Checker**: https://wave.webaim.org/

### Design Inspiration

- **Material Design**: https://material.io/design
- **Apple Human Interface Guidelines**: https://developer.apple.com/design/
- **Stripe Dashboard**: Clean, minimalist design
- **Linear**: Snappy, responsive interactions

### Typography

- **Modular Scale Calculator**: https://www.modularscale.com/
- **Type Scale**: https://typescale.com/

---

**Last Updated**: November 16, 2025  
**Maintained By**: Development Team  
**Review Schedule**: Quarterly
