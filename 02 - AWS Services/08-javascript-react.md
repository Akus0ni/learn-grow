# JavaScript & React — Interview Prep

> Bridged to your Vue.js/Angular experience. Enterprise-depth coverage.

---

## 1. Modern JavaScript (ES6+) — The Foundation

### var vs let vs const

```javascript
// var — function-scoped, hoisted, avoid in modern code
var x = 1;
if (true) { var x = 2; }  // same variable! x = 2

// let — block-scoped, not hoisted
let y = 1;
if (true) { let y = 2; }  // different variable, outer y = 1

// const — block-scoped, reference immutable (object contents CAN change)
const config = { timeout: 5000 };
config.timeout = 3000;  // OK — mutating object
config = {};            // ERROR — reassigning const
```

**Rule:** Default to `const`. Use `let` when you need to reassign. Never use `var`.

---

### Arrow Functions

```javascript
// Regular function — 'this' is dynamic
function greet() { console.log(this.name); }

// Arrow function — 'this' is lexically bound (inherited from enclosing scope)
const greet = () => console.log(this.name);

// Implicit return (single expression)
const double = n => n * 2;
const add = (a, b) => a + b;

// Multi-line
const fetchUser = async (id) => {
    const response = await fetch(`/api/users/${id}`);
    return response.json();
};

// In React: use arrows for event handlers to preserve 'this'
<button onClick={() => handleClick(item.id)}>Click</button>
```

---

### Destructuring

```javascript
// Object destructuring
const { id, name, email } = user;
const { address: { city } } = user;          // nested
const { name = 'Anonymous', role = 'user' } = user;  // defaults

// Array destructuring
const [first, second, ...rest] = items;
const [, second] = items;  // skip first

// In React function params
const UserCard = ({ name, email, role = 'user' }) => (
    <div>{name} - {email} ({role})</div>
);

// Function params
function processReport({ id, title, status = 'draft' }) {
    console.log(id, title, status);
}
```

---

### Spread & Rest

```javascript
// Spread — expand iterable
const merged = { ...defaults, ...overrides };          // object merge
const combined = [...existingItems, newItem];          // array merge
const copy = { ...report, status: 'active' };         // clone + override

// Rest — collect remaining
const { id, ...rest } = report;    // rest has everything except id
function log(level, ...messages) { // rest params
    messages.forEach(msg => console.log(`[${level}]`, msg));
}

// In React state updates
setReport(prev => ({ ...prev, status: 'active' }));   // immutable update
setItems(prev => [...prev, newItem]);
```

---

### Promises & async/await

```javascript
// Promise chain (older style)
fetch('/api/reports')
    .then(res => res.json())
    .then(data => console.log(data))
    .catch(err => console.error(err))
    .finally(() => setLoading(false));

// async/await (preferred — reads like sync code)
async function fetchReports() {
    try {
        const response = await fetch('/api/reports');
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return await response.json();
    } catch (error) {
        console.error('Failed to fetch reports:', error);
        throw error;  // re-throw so caller can handle
    }
}

// Parallel async operations
async function fetchDashboardData(userId) {
    const [user, reports, notifications] = await Promise.all([
        fetchUser(userId),
        fetchReports(userId),
        fetchNotifications(userId)
    ]);
    return { user, reports, notifications };
}

// Promise.allSettled — continue even if some fail
const results = await Promise.allSettled([task1(), task2(), task3()]);
results.forEach(result => {
    if (result.status === 'fulfilled') console.log(result.value);
    else console.error(result.reason);
});
```

---

### Optional Chaining & Nullish Coalescing

```javascript
// Optional chaining (?.) — stops if null/undefined
const city = user?.address?.city;
const firstTag = report?.tags?.[0];
const result = api?.getData?.();

// Nullish coalescing (??) — fallback only for null/undefined (not 0, '')
const name = user?.name ?? 'Anonymous';
const timeout = config?.timeout ?? 5000;

// vs OR (||) — falls back on ANY falsy (including 0, '', false)
const count = data.count || 10;   // 0 would incorrectly use 10
const count = data.count ?? 10;   // 0 correctly stays 0

// Nullish assignment (??=)
user.settings ??= {};  // only assign if null/undefined
```

---

### Modules (ES Modules)

```javascript
// Named exports
export const API_BASE = '/api';
export function formatDate(date) { ... }
export class ReportService { ... }

// Default export
export default function App() { ... }

// Imports
import App from './App';                          // default
import { formatDate, API_BASE } from './utils';  // named
import { formatDate as fd } from './utils';      // aliased
import * as Utils from './utils';                // namespace

// Dynamic import (lazy loading)
const { Chart } = await import('./Chart');
```

---

### Closures & Higher-Order Functions

```javascript
// Closure — inner function retains access to outer scope
function createCounter(initial = 0) {
    let count = initial;
    return {
        increment: () => ++count,
        decrement: () => --count,
        value: () => count
    };
}
const counter = createCounter(10);
counter.increment(); // 11

// Higher-order functions — functions that take/return functions
const withLogging = (fn) => (...args) => {
    console.log('Calling with', args);
    const result = fn(...args);
    console.log('Result:', result);
    return result;
};

// Array HOFs (critical for React rendering)
const activeReports = reports.filter(r => r.status === 'active');
const titles = reports.map(r => r.title);
const total = reports.reduce((sum, r) => sum + r.count, 0);
const found = reports.find(r => r.id === targetId);
const hasActive = reports.some(r => r.status === 'active');
const allActive = reports.every(r => r.status === 'active');
```

---

## 2. TypeScript Essentials (Your Stack at Energy Exemplar)

```typescript
// Types & Interfaces
interface Report {
    id: number;
    title: string;
    status: 'draft' | 'active' | 'archived';  // union literal type
    owner?: User;                               // optional
    tags: string[];
    metadata: Record<string, unknown>;          // dynamic keys
}

// Generic types
interface ApiResponse<T> {
    data: T;
    error?: string;
    timestamp: string;
}

async function fetchReport(id: number): Promise<ApiResponse<Report>> {
    const res = await fetch(`/api/reports/${id}`);
    return res.json();
}

// Type narrowing
function processResult(result: Report | null) {
    if (result === null) return;
    // TypeScript knows result is Report here
    console.log(result.title);
}

// Utility types
type ReportSummary = Pick<Report, 'id' | 'title' | 'status'>;
type CreateReport = Omit<Report, 'id'>;
type PartialReport = Partial<Report>;
type ReadonlyReport = Readonly<Report>;
```

---

## 3. React — Your Vue.js Knowledge Mapped to React

### Vue → React Mental Model

| Concept | Vue.js | React |
|---|---|---|
| Component template | `<template>` | JSX in `return ()` |
| Reactive data | `data()` / `ref()` | `useState()` |
| Computed property | `computed` | `useMemo()` |
| Lifecycle (mount) | `onMounted` | `useEffect(fn, [])` |
| Lifecycle (update) | `watch` / `watchEffect` | `useEffect(fn, [deps])` |
| Props | `props: ['name']` | Function parameter `({ name })` |
| Emit event | `$emit('update', val)` | Callback prop `onUpdate(val)` |
| v-if | `v-if="show"` | `{show && <Component />}` |
| v-for | `v-for="item in items"` | `{items.map(item => ...)}` |
| v-model | `v-model="value"` | `value={val} onChange={setVal}` |
| Slot | `<slot />` | `{children}` |

---

### JSX Rules

```jsx
// JSX = JavaScript + XML-like syntax
// Rules:
// 1. Return single root element (or Fragment)
// 2. className not class, htmlFor not for
// 3. Expressions in {}
// 4. Self-close empty elements: <img />, <br />
// 5. camelCase event handlers: onClick, onChange, onSubmit

function ReportCard({ report, onDelete }) {
    return (
        <div className="card">
            <h2>{report.title}</h2>
            <span className={`badge badge-${report.status}`}>
                {report.status}
            </span>
            {report.tags.length > 0 && (
                <ul>
                    {report.tags.map(tag => (
                        <li key={tag}>{tag}</li>  {/* key is required for lists */}
                    ))}
                </ul>
            )}
            <button onClick={() => onDelete(report.id)}>Delete</button>
        </div>
    );
}
```

---

### Core Hooks

#### useState

```jsx
// Basic state
const [count, setCount] = useState(0);
const [report, setReport] = useState(null);
const [loading, setLoading] = useState(false);

// Functional update (safe when new state depends on old)
setCount(prev => prev + 1);

// Object state — always spread to preserve other fields
const [form, setForm] = useState({ name: '', email: '' });
setForm(prev => ({ ...prev, name: 'Akash' }));

// Array state
const [items, setItems] = useState([]);
setItems(prev => [...prev, newItem]);                  // add
setItems(prev => prev.filter(i => i.id !== targetId)); // remove
setItems(prev => prev.map(i => i.id === id ? {...i, ...changes} : i)); // update
```

#### useEffect

```jsx
// Run once on mount (empty deps array)
useEffect(() => {
    fetchReports();
}, []);

// Run when deps change (like Vue watch)
useEffect(() => {
    if (userId) fetchUserData(userId);
}, [userId]);

// Cleanup (like Vue onUnmounted)
useEffect(() => {
    const subscription = eventBus.subscribe(handler);
    return () => subscription.unsubscribe();  // cleanup
}, []);

// Async in useEffect
useEffect(() => {
    let cancelled = false;

    async function load() {
        setLoading(true);
        try {
            const data = await fetchReports();
            if (!cancelled) setReports(data);
        } catch (err) {
            if (!cancelled) setError(err.message);
        } finally {
            if (!cancelled) setLoading(false);
        }
    }

    load();
    return () => { cancelled = true; };  // prevent state update if unmounted
}, []);
```

#### useCallback & useMemo

```jsx
// useMemo — memoize expensive computed value (like Vue computed)
const filteredReports = useMemo(
    () => reports.filter(r => r.status === selectedStatus),
    [reports, selectedStatus]  // recompute only when these change
);

const stats = useMemo(() => ({
    total: reports.length,
    active: reports.filter(r => r.status === 'active').length,
}), [reports]);

// useCallback — memoize function (stable reference for child props)
const handleDelete = useCallback(async (id) => {
    await deleteReport(id);
    setReports(prev => prev.filter(r => r.id !== id));
}, []);  // empty = stable forever (function doesn't use changing values)

// With dependency
const handleFilter = useCallback((status) => {
    setSelectedStatus(status);
    onFilterChange?.(status);   // optional chaining on prop
}, [onFilterChange]);
```

#### useRef

```jsx
// Mutable ref — doesn't trigger re-render
const inputRef = useRef(null);
const timerRef = useRef(null);
const prevCountRef = useRef(count);

// DOM access
<input ref={inputRef} />
inputRef.current.focus();

// Store mutable values without re-render
useEffect(() => {
    timerRef.current = setTimeout(() => setExpired(true), 5000);
    return () => clearTimeout(timerRef.current);
}, []);
```

#### useContext

```jsx
// Context — avoid prop drilling (like Vue provide/inject)
const AuthContext = createContext(null);

// Provider (at top level)
function App() {
    const [user, setUser] = useState(null);
    return (
        <AuthContext.Provider value={{ user, setUser }}>
            <Router />
        </AuthContext.Provider>
    );
}

// Consumer (anywhere in tree)
function Header() {
    const { user, setUser } = useContext(AuthContext);
    return <div>Hello, {user?.name}</div>;
}
```

#### useReducer (for complex state)

```jsx
// Like a mini Redux — action → reducer → new state
const initialState = { reports: [], loading: false, error: null };

function reportReducer(state, action) {
    switch (action.type) {
        case 'FETCH_START':
            return { ...state, loading: true, error: null };
        case 'FETCH_SUCCESS':
            return { ...state, loading: false, reports: action.payload };
        case 'FETCH_ERROR':
            return { ...state, loading: false, error: action.payload };
        case 'DELETE':
            return { ...state, reports: state.reports.filter(r => r.id !== action.id) };
        default:
            return state;
    }
}

function ReportList() {
    const [state, dispatch] = useReducer(reportReducer, initialState);

    useEffect(() => {
        dispatch({ type: 'FETCH_START' });
        fetchReports()
            .then(data => dispatch({ type: 'FETCH_SUCCESS', payload: data }))
            .catch(err => dispatch({ type: 'FETCH_ERROR', payload: err.message }));
    }, []);
}
```

---

### Custom Hooks (Encapsulate Reusable Logic)

```jsx
// useFetch — data fetching hook (your eGain API work mapped to React)
function useFetch(url) {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        let cancelled = false;
        setLoading(true);

        fetch(url)
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(json => { if (!cancelled) setData(json); })
            .catch(err => { if (!cancelled) setError(err.message); })
            .finally(() => { if (!cancelled) setLoading(false); });

        return () => { cancelled = true; };
    }, [url]);

    return { data, loading, error };
}

// Usage
function ReportPage({ id }) {
    const { data: report, loading, error } = useFetch(`/api/reports/${id}`);
    if (loading) return <Spinner />;
    if (error) return <ErrorMessage message={error} />;
    return <ReportDetail report={report} />;
}

// useLocalStorage
function useLocalStorage(key, initialValue) {
    const [value, setValue] = useState(() => {
        try {
            return JSON.parse(localStorage.getItem(key)) ?? initialValue;
        } catch {
            return initialValue;
        }
    });

    const setStoredValue = useCallback((newValue) => {
        setValue(newValue);
        localStorage.setItem(key, JSON.stringify(newValue));
    }, [key]);

    return [value, setStoredValue];
}
```

---

## 4. Component Patterns

### Controlled vs Uncontrolled Components

```jsx
// Controlled — React owns the state (preferred)
function ControlledForm() {
    const [email, setEmail] = useState('');

    return (
        <input
            value={email}                      // value from state
            onChange={e => setEmail(e.target.value)}  // update state on change
        />
    );
}

// Uncontrolled — DOM owns the state (use for file inputs, integrations)
function UncontrolledForm() {
    const inputRef = useRef();
    const handleSubmit = () => console.log(inputRef.current.value);
    return <input ref={inputRef} />;
}
```

### Lifting State Up

```jsx
// Move state to the nearest common ancestor when siblings need to share it
function ReportManager() {
    const [selectedId, setSelectedId] = useState(null);  // lifted state

    return (
        <div>
            <ReportList onSelect={setSelectedId} />
            <ReportDetail reportId={selectedId} />
        </div>
    );
}
```

### Compound Components (Enterprise Pattern)

```jsx
// Components that work together through context
const TableContext = createContext();

function DataTable({ children, data }) {
    return (
        <TableContext.Provider value={{ data }}>
            <table>{children}</table>
        </TableContext.Provider>
    );
}
DataTable.Header = function Header() { ... };
DataTable.Body = function Body() { ... };
DataTable.Pagination = function Pagination() { ... };

// Usage — clean API
<DataTable data={reports}>
    <DataTable.Header />
    <DataTable.Body />
    <DataTable.Pagination />
</DataTable>
```

---

## 5. State Management Beyond useState

### Context + useReducer (No Library Needed for Medium Apps)

```jsx
// Full state management without Redux
const StoreContext = createContext();

function StoreProvider({ children }) {
    const [state, dispatch] = useReducer(rootReducer, initialState);
    return (
        <StoreContext.Provider value={{ state, dispatch }}>
            {children}
        </StoreContext.Provider>
    );
}

export const useStore = () => useContext(StoreContext);
```

### Redux Toolkit (Enterprise Standard)

```javascript
// Modern Redux — much less boilerplate than classic Redux
import { createSlice, createAsyncThunk, configureStore } from '@reduxjs/toolkit';

const fetchReports = createAsyncThunk('reports/fetchAll', async () => {
    const response = await fetch('/api/reports');
    return response.json();
});

const reportsSlice = createSlice({
    name: 'reports',
    initialState: { items: [], loading: false, error: null },
    reducers: {
        deleteReport: (state, action) => {
            state.items = state.items.filter(r => r.id !== action.payload);
        },
    },
    extraReducers: (builder) => {
        builder
            .addCase(fetchReports.pending, (state) => { state.loading = true; })
            .addCase(fetchReports.fulfilled, (state, action) => {
                state.loading = false;
                state.items = action.payload;
            })
            .addCase(fetchReports.rejected, (state, action) => {
                state.loading = false;
                state.error = action.error.message;
            });
    },
});

// In component
const { items, loading } = useSelector(state => state.reports);
const dispatch = useDispatch();
dispatch(fetchReports());
dispatch(deleteReport(id));
```

---

## 6. React Router (Navigation)

```jsx
import { BrowserRouter, Routes, Route, useNavigate, useParams, Link } from 'react-router-dom';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Dashboard />} />
                <Route path="/reports" element={<ReportList />} />
                <Route path="/reports/:id" element={<ReportDetail />} />
                <Route path="/reports/new" element={<CreateReport />} />
                <Route path="*" element={<NotFound />} />
            </Routes>
        </BrowserRouter>
    );
}

// Route params
function ReportDetail() {
    const { id } = useParams();
    const navigate = useNavigate();

    const handleBack = () => navigate(-1);
    const handleEdit = () => navigate(`/reports/${id}/edit`);
    // ...
}

// Protected routes
function ProtectedRoute({ children }) {
    const { user } = useContext(AuthContext);
    return user ? children : <Navigate to="/login" replace />;
}
```

---

## 7. API Integration Patterns

### Axios with Interceptors (Enterprise Pattern)

```javascript
// api.js — centralized API client
import axios from 'axios';

const api = axios.create({
    baseURL: process.env.REACT_APP_API_URL,
    timeout: 10000,
});

// Request interceptor — attach token
api.interceptors.request.use(config => {
    const token = localStorage.getItem('token');
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

// Response interceptor — handle 401
api.interceptors.response.use(
    response => response.data,
    async error => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export const reportApi = {
    getAll: () => api.get('/reports'),
    getById: (id) => api.get(`/reports/${id}`),
    create: (data) => api.post('/reports', data),
    update: (id, data) => api.put(`/reports/${id}`, data),
    delete: (id) => api.delete(`/reports/${id}`),
};
```

---

## 8. Performance Optimization

### React.memo

```jsx
// Prevents re-render if props haven't changed
const ReportCard = React.memo(function ReportCard({ report, onDelete }) {
    return (
        <div className="card">
            <h3>{report.title}</h3>
            <button onClick={() => onDelete(report.id)}>Delete</button>
        </div>
    );
});
// Note: onDelete must be wrapped in useCallback in parent, otherwise memo won't help
```

### Code Splitting & Lazy Loading

```jsx
import { lazy, Suspense } from 'react';

// Lazy load heavy components (charts, editors)
const AnalyticsDashboard = lazy(() => import('./AnalyticsDashboard'));
const ReportEditor = lazy(() => import('./ReportEditor'));

function App() {
    return (
        <Suspense fallback={<LoadingSpinner />}>
            <Routes>
                <Route path="/analytics" element={<AnalyticsDashboard />} />
                <Route path="/editor/:id" element={<ReportEditor />} />
            </Routes>
        </Suspense>
    );
}
```

### Virtualization (Large Lists)

```jsx
// For lists with 1000+ items — render only visible rows
import { FixedSizeList } from 'react-window';

function VirtualReportList({ reports }) {
    const Row = ({ index, style }) => (
        <div style={style}>
            <ReportCard report={reports[index]} />
        </div>
    );

    return (
        <FixedSizeList height={600} itemCount={reports.length} itemSize={80}>
            {Row}
        </FixedSizeList>
    );
}
```

---

## 9. Error Boundaries

```jsx
// Class component — only way to catch render errors
class ErrorBoundary extends React.Component {
    state = { hasError: false, error: null };

    static getDerivedStateFromError(error) {
        return { hasError: true, error };
    }

    componentDidCatch(error, info) {
        console.error('Boundary caught:', error, info.componentStack);
        // Send to error tracking (Sentry, etc.)
    }

    render() {
        if (this.state.hasError) {
            return <ErrorFallback error={this.state.error} />;
        }
        return this.props.children;
    }
}

// Usage — wrap sections that might fail
<ErrorBoundary>
    <AnalyticsDashboard />
</ErrorBoundary>
```

---

## 10. Common Interview Questions

**Q: What is the Virtual DOM and why does React use it?**
> The Virtual DOM is a lightweight in-memory representation of the real DOM. When state changes, React re-renders the virtual DOM, diffs it against the previous version, and applies only the minimal set of real DOM changes. This is faster than directly manipulating the DOM on every state change.

**Q: What are React hooks? Why were they introduced?**
> Hooks (useState, useEffect, etc.) let function components use state and lifecycle features previously only available in class components. Introduced in React 16.8 to eliminate class boilerplate, make logic reusable via custom hooks, and avoid confusing `this` binding issues.

**Q: What is the difference between useMemo and useCallback?**
> `useMemo` memoizes a computed value — reruns the function only when dependencies change. `useCallback` memoizes a function reference — returns the same function instance unless dependencies change. Use `useCallback` when passing callbacks to memoized children (React.memo) to prevent unnecessary re-renders.

**Q: When would you use useReducer over useState?**
> When state logic is complex, involves multiple sub-values that change together, or when next state depends on the previous in non-trivial ways. Also when you want to keep state transitions explicit and testable (reducer is a pure function). Similar reason you'd choose Vuex over local component state in Vue.

**Q: How do you prevent unnecessary re-renders?**
> React.memo for components, useMemo for expensive computations, useCallback for stable function references. Also: avoid creating objects/arrays in JSX (they create new references on every render), lift state only as high as needed, and use context selectively.

**Q: What is prop drilling and how do you solve it?**
> Passing props through multiple intermediate components that don't use them, just to reach a deeply nested component. Solutions: Context API (for global state), component composition (pass components as props), or state management libraries (Redux).

**Q: Explain the React component lifecycle in hooks.**
> Mount: `useEffect(() => { /* setup */ }, [])`. Update: `useEffect(() => { /* on change */ }, [deps])`. Unmount: `useEffect(() => { return () => { /* cleanup */ } }, [])`. State changes cause re-renders; React batches multiple setState calls since React 18.

**Q: How is React different from Vue.js?**
> React is a library (just UI/view layer) — you choose your own routing, state management, etc. Vue is a more opinionated progressive framework with official solutions. React uses JSX (JS-in-HTML); Vue uses templates (HTML-in-HTML). Both use virtual DOM and component model. React's composition (hooks, HOC) vs Vue's Options/Composition API. My Vue experience transfers directly — same mental model, different syntax.
