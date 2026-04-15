# React Basics — Interview Q&A

> You know Vue.js and Angular well. This file maps those concepts to React so you can answer confidently.
> Key message: "I've built enterprise UIs with Vue.js and Angular. React uses similar component-based
> concepts and I've been actively picking it up."

---

## Core Concepts

**Q: What is React and what problem does it solve?**

A: React is a JavaScript library (not a full framework) for building component-based UIs. It solves the problem of efficiently updating the DOM when application state changes, using a **Virtual DOM** to batch and minimize real DOM operations.

---

**Q: What is the Virtual DOM? How does React use it?**

A: The Virtual DOM is an in-memory representation of the real DOM. When state changes:
1. React creates a new Virtual DOM tree.
2. **Diffing algorithm** compares new vs. previous Virtual DOM.
3. Only the **changed nodes** are updated in the real DOM (**reconciliation**).

> **Analogy from Vue:** Vue 3 uses a similar virtual DOM diffing strategy.

---

**Q: What is JSX?**

A: JSX is a syntax extension that lets you write HTML-like code inside JavaScript. Babel compiles it to `React.createElement()` calls.

```jsx
// JSX
const element = <h1 className="title">Hello, {name}</h1>;

// Compiled to
const element = React.createElement('h1', { className: 'title' }, `Hello, ${name}`);
```

---

**Q: What is the difference between a functional component and a class component?**

```jsx
// Class Component (older, verbose)
class Welcome extends React.Component {
    render() {
        return <h1>Hello, {this.props.name}</h1>;
    }
}

// Functional Component (modern, preferred)
function Welcome({ name }) {
    return <h1>Hello, {name}</h1>;
}
```

Modern React uses **functional components + hooks** exclusively. Class components are legacy.

> **Analogy:** Like Vue 2 Options API (class-like) vs Vue 3 Composition API (functional).

---

**Q: What are React Hooks? Name the most important ones.**

A: Hooks let functional components use state and lifecycle features.

```jsx
import { useState, useEffect, useCallback, useMemo, useRef, useContext } from 'react';
```

| Hook | Purpose | Vue equivalent |
|---|---|---|
| `useState` | Local component state | `ref()` / `reactive()` |
| `useEffect` | Side effects, lifecycle | `onMounted`, `watch` |
| `useContext` | Access React context | `inject()` |
| `useRef` | Mutable ref, DOM access | `ref()` for DOM |
| `useMemo` | Memoize computed value | `computed()` |
| `useCallback` | Memoize function | N/A (manual) |
| `useReducer` | Complex state logic | Pinia store |

---

**Q: Explain `useState`.**

```jsx
import { useState } from 'react';

function Counter() {
    const [count, setCount] = useState(0);  // [value, setter]

    return (
        <div>
            <p>Count: {count}</p>
            <button onClick={() => setCount(count + 1)}>Increment</button>
            <button onClick={() => setCount(prev => prev - 1)}>Decrement</button>
        </div>
    );
}
```

- `useState(initialValue)` returns `[currentValue, setter]`
- Calling the setter triggers a re-render
- Use the **functional update form** (`prev =>`) when new state depends on old state

---

**Q: Explain `useEffect`.**

```jsx
useEffect(() => {
    // Runs after every render (no dependency array)
});

useEffect(() => {
    // Runs only on mount (empty array)
    fetchOrders();
}, []);

useEffect(() => {
    // Runs when clientId changes
    fetchOrdersForClient(clientId);
}, [clientId]);

useEffect(() => {
    const subscription = subscribe();
    return () => subscription.unsubscribe(); // cleanup on unmount
}, []);
```

> **Analogy:** Like Vue's `watch` + `onMounted` combined.

---

**Q: What are props in React?**

A: Props are read-only inputs passed from parent to child components.

```jsx
// Parent
<OrderCard orderId={123} status="Active" onClose={() => handleClose()} />

// Child
function OrderCard({ orderId, status, onClose }) {
    return (
        <div>
            <p>Order #{orderId} — {status}</p>
            <button onClick={onClose}>Close</button>
        </div>
    );
}
```

Props flow **down** (parent → child). Events flow **up** (child calls parent's callback).

> **Analogy:** Like Vue's `props` (down) and `$emit` (up).

---

**Q: What is state lifting?**

A: When two sibling components need to share state, you "lift" it to their nearest common parent.

```jsx
function Parent() {
    const [selected, setSelected] = useState(null);

    return (
        <>
            <OrderList onSelect={setSelected} />
            <OrderDetail order={selected} />
        </>
    );
}
```

---

**Q: What is Redux? When do you use it?**

A: Redux is a predictable state management library. It centralizes all application state in a single **store**.

**Core concepts:**
- **Store** — Single source of truth for app state
- **Action** — Plain object describing what happened `{ type: 'ADD_ORDER', payload: order }`
- **Reducer** — Pure function that takes `(state, action)` → new state
- **Dispatch** — Send an action to the store
- **Selector** — Read data from the store

```jsx
// Modern Redux Toolkit (preferred)
const ordersSlice = createSlice({
    name: 'orders',
    initialState: [],
    reducers: {
        addOrder: (state, action) => { state.push(action.payload); },
        removeOrder: (state, action) => state.filter(o => o.id !== action.payload),
    }
});

// In component
const orders = useSelector(state => state.orders);
const dispatch = useDispatch();
dispatch(addOrder(newOrder));
```

**When to use Redux:** Complex apps with shared state across many components. For simple apps, `useState` + Context API is sufficient.

> **Analogy:** Like Pinia/Vuex in Vue.

---

**Q: What is the Context API?**

A: React's built-in way to share state across components without prop drilling.

```jsx
const ThemeContext = createContext('light');

// Provider wraps the tree
<ThemeContext.Provider value="dark">
    <App />
</ThemeContext.Provider>

// Any child consumes it
function Button() {
    const theme = useContext(ThemeContext);
    return <button className={theme}>Click</button>;
}
```

---

**Q: What is Webpack?**

A: A module bundler for JavaScript applications. It:
- Bundles JS, CSS, images into optimized output files
- Handles code splitting (lazy loading)
- Applies loaders (Babel for JSX/ES6, CSS loaders)
- Applies plugins (minification, environment variables)

In modern Create React App / Vite projects, Webpack is pre-configured. Most devs interact with it indirectly.

---

**Q: What is the key prop in lists? Why is it important?**

```jsx
orders.map(order => (
    <OrderRow key={order.id} order={order} />  // ✅ stable unique key
))
```

React uses `key` to identify which items changed, were added, or removed. Without it (or with index as key), React may re-render incorrectly and cause bugs with stateful list items.

---

## Quick-Fire

| Concept | React | Vue 3 equivalent |
|---|---|---|
| Component definition | `function MyComp() {}` | `<script setup>` |
| State | `useState` | `ref()` / `reactive()` |
| Computed | `useMemo` | `computed()` |
| Lifecycle (mount) | `useEffect(() => {}, [])` | `onMounted()` |
| Props down | `<Child value={x} />` | `:value="x"` |
| Events up | `<Child onClick={fn} />` | `@click="fn"` |
| Conditional render | `{show && <X />}` | `v-if` |
| List render | `items.map(i => <X key={i.id} />)` | `v-for` |
| Global state | Redux / Context | Pinia |

---

## React vs Angular (Your Angular Experience)

- Angular is a **full framework** (routing, DI, forms, HTTP all built-in). React is a **library** (you pick the ecosystem).
- Angular uses **TypeScript by default**, two-way binding (`[(ngModel)]`). React uses one-way data flow.
- Angular has **decorators** (`@Component`, `@Injectable`). React uses hooks.
- Both use component-based architecture — your Angular/Vue experience transfers well.
