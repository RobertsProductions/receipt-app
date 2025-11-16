# WarrantyApp.Web - Angular Frontend

This is the frontend application for the Warranty Management System, built with Angular 18 and TypeScript.

## Technology Stack

- **Angular**: 18.2.0
- **TypeScript**: 5.5.2
- **RxJS**: 7.8.0
- **SCSS**: For styling
- **ESLint**: Code quality and linting (angular-eslint)
- **Karma + Jasmine**: Unit testing

## Prerequisites

- Node.js 18+ and npm 10+
- Angular CLI (installed locally in project)

## Getting Started

### Install Dependencies

```bash
npm install
```

### Development Server

Run the development server:

```bash
npm start
```

**When run standalone**: Navigate to `http://localhost:4200/`. The app will automatically reload when you make changes.

**When run via Aspire**: The PORT environment variable will be set by Aspire, and the app will start on the assigned port (visible in Aspire dashboard).

**Note**: The `start` script uses `start-server.js` which:
- Reads the PORT environment variable (set by Aspire) or defaults to 4200
- Starts Angular dev server with dynamic proxy configuration
- Proxy forwards `/api` requests to the backend API (dynamic URL from Aspire or localhost:5000)

To run without the proxy:
```bash
npm run start:no-proxy
```

### Build

Build the project for development:
```bash
npm run build
```

Build for production:
```bash
npm run build:prod
```

The build artifacts will be stored in the `dist/` directory.

### Running Unit Tests

Execute unit tests via Karma:
```bash
npm test
```

### Linting

Run ESLint to check code quality:
```bash
npm run lint
```

## Project Structure

```
src/
├── app/                      # Application components and modules
│   ├── app.component.ts      # Root component
│   ├── app.config.ts         # Application configuration
│   └── app.routes.ts         # Routing configuration
├── environments/             # Environment configurations
│   ├── environment.ts        # Default environment
│   ├── environment.development.ts  # Development environment
│   └── environment.prod.ts   # Production environment
├── index.html               # Main HTML file
├── main.ts                  # Application entry point
└── styles.scss              # Global styles
```

## Environment Configuration

The application uses environment files to manage configuration:

- **Development** (`environment.development.ts`): Used during `ng serve`
  - API URL: `http://localhost:5000/api`
  
- **Production** (`environment.prod.ts`): Used during production builds
  - API URL: `/api` (relative path)

## API Proxy Configuration

The `proxy.conf.mjs` file dynamically configures the dev server to proxy API requests to the backend:

```javascript
export default function() {
  // Aspire injects service URLs in the format: services__<servicename>__http__0
  const apiUrl = process.env.API_URL || 
                 process.env.services__myapi__http__0 || 
                 'http://localhost:5000';
  
  return {
    '/api': {
      target: apiUrl,
      secure: false,
      changeOrigin: true,
      logLevel: 'debug'
    }
  };
}
```

This allows the frontend to make requests to `/api/*` which are automatically forwarded to the backend API.

**How it works:**
1. When run via Aspire: Reads `API_URL` or `services__myapi__http__0` from environment
2. When run standalone: Falls back to `http://localhost:5000`
3. Logs the proxy target for debugging

See `../docs/32-aspire-angular-proxy-fix.md` for details on the dynamic proxy implementation.

## Code Style and Linting

The project uses ESLint with Angular-specific rules. Configuration is in `eslint.config.js`.

Key rules:
- Component selectors must use `app` prefix and kebab-case
- Directive selectors must use `app` prefix and camelCase
- Template accessibility rules enabled
- TypeScript recommended rules enforced

## Design System

See `../docs/27-design-reference.md` for the complete design system including:
- Color palette
- Typography scale
- Spacing system
- Component styles
- Animation guidelines

## Workflows and Components

See `../docs/28-frontend-workflows.md` for:
- User journey workflows
- Component breakdown
- Implementation phases
- API integration details

## Integration with .NET Aspire

The Angular app can be orchestrated with the .NET Aspire AppHost for unified development.

See `../docs/29-angular-aspire-integration.md` for:
- Aspire integration instructions
- AppHost configuration
- Service discovery setup
- CORS configuration

## NPM Scripts Reference

| Script | Description |
|--------|-------------|
| `npm start` | Start dev server with dynamic API proxy (uses PORT from Aspire or defaults to 4200) |
| `npm run start:no-proxy` | Start dev server without proxy |
| `npm run build` | Build for development |
| `npm run build:prod` | Build for production with optimizations |
| `npm run watch` | Build and watch for changes |
| `npm test` | Run unit tests |
| `npm run lint` | Run ESLint |

## Code Scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

1. Follow the Angular Style Guide
2. Run `npm run lint` before committing
3. Ensure all tests pass with `npm test`
4. Update documentation for new features

## Next Steps

- [ ] Implement authentication UI (login, register, 2FA)
- [ ] Create receipt management components
- [ ] Build warranty dashboard
- [ ] Implement user profile management
- [ ] Add receipt sharing features
- [ ] Integrate AI chatbot interface

## Further Help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

---

**Version**: 0.0.0  
**Last Updated**: November 16, 2025

