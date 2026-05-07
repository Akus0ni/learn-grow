# Angular 10+ — Quick Reference

## Core Architecture

```
AppModule
  ├── Components (UI blocks)
  ├── Services (business logic / data)
  ├── Modules (feature grouping)
  ├── Directives (DOM behavior)
  └── Pipes (data transformation)
```

---

## Components

```typescript
@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  private destroy$ = new Subject<void>();

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.productService.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.products = data);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Lifecycle Hooks (in order)
1. `ngOnChanges` — input changes
2. `ngOnInit` — component initialized ← use this for data loading
3. `ngDoCheck`
4. `ngAfterContentInit / Checked`
5. `ngAfterViewInit / Checked`
6. `ngOnDestroy` ← clean up subscriptions here

---

## Data Binding

```html
<!-- Interpolation -->
<p>{{ product.name }}</p>

<!-- Property binding -->
<img [src]="product.imageUrl">

<!-- Event binding -->
<button (click)="onDelete(product.id)">Delete</button>

<!-- Two-way binding (requires FormsModule) -->
<input [(ngModel)]="searchTerm">

<!-- Async pipe (auto-subscribes + unsubscribes) -->
<div *ngIf="products$ | async as products">
  <div *ngFor="let p of products">{{ p.name }}</div>
</div>
```

---

## Services & Dependency Injection

```typescript
@Injectable({ providedIn: 'root' })  // singleton across app
export class ProductService {
  private apiUrl = 'https://api.example.com/products';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  create(product: CreateProductDto): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }
}
```

---

## RxJS Operators (most common in interviews)

```typescript
import { map, filter, switchMap, catchError, takeUntil, debounceTime } from 'rxjs/operators';

// map — transform each value
products$.pipe(map(p => p.name))

// filter — skip values
products$.pipe(filter(p => p.price > 10))

// switchMap — cancel previous, switch to new (search, route changes)
searchTerm$.pipe(
  debounceTime(300),
  switchMap(term => this.productService.search(term))
)

// catchError — handle errors without killing stream
this.http.get('/api/data').pipe(
  catchError(err => of([]))  // return empty array on error
)

// forkJoin — wait for all to complete (like Promise.all)
forkJoin([this.getUsers(), this.getOrders()])
  .subscribe(([users, orders]) => { ... });
```

### Hot vs Cold Observables
- **Cold**: start emitting on subscribe (HTTP calls, timers) — each subscriber gets its own execution
- **Hot**: emit regardless of subscribers (mouse events, Subject) — subscribers share the same stream

---

## Routing

```typescript
const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'products/:id', component: ProductDetailComponent },
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule),
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '' }
];
```

```typescript
// Route guard
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  canActivate(): boolean {
    return this.authService.isLoggedIn();
  }
}

// Navigate programmatically
this.router.navigate(['/products', productId]);

// Read route params
this.route.params.subscribe(p => this.id = p['id']);
```

---

## HTTP Interceptors

```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next.handle(authReq).pipe(
      catchError(err => {
        if (err.status === 401) this.router.navigate(['/login']);
        return throwError(() => err);
      })
    );
  }
}
```

---

## Forms

### Reactive Forms (preferred for complex forms)
```typescript
this.form = this.fb.group({
  name: ['', [Validators.required, Validators.minLength(3)]],
  email: ['', [Validators.required, Validators.email]],
  price: [0, [Validators.required, Validators.min(0)]]
});

// Access
this.form.get('name')?.errors
this.form.value  // { name: '', email: '', price: 0 }
this.form.valid  // boolean
```

---

## Angular 10+ Key Features
- **Ivy renderer** (default from v9) — faster builds, smaller bundles, better debugging
- **Strict mode** — stricter TypeScript checking, catches bugs early
- **Lazy loading** — `loadChildren` loads feature modules only when needed (faster initial load)
- **OnPush change detection** — component only re-renders when @Input reference changes
- **Standalone components** (v14+) — no need for NgModule declarations

---

## Performance Tips
- Use `OnPush` change detection for heavy components
- Use `trackBy` with `*ngFor` to avoid full DOM re-render
- Lazy load feature modules
- Use `async` pipe instead of manual subscribe/unsubscribe
- `takeUntil` pattern to avoid memory leaks from subscriptions

```html
<div *ngFor="let p of products; trackBy: trackByProductId">
```
```typescript
trackByProductId(index: number, product: Product): number {
  return product.id;
}
```
