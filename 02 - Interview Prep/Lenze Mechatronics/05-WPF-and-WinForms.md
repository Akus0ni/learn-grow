# WPF and WinForms Desktop Development — Interview Q&A

> Targeted at Senior Software Engineer roles (6+ years, C#/.NET)

---

## WPF

### Q1: Explain the WPF architecture and how its rendering pipeline differs from WinForms.

**Answer:** WPF (Windows Presentation Foundation) uses a layered architecture built on top of DirectX rather than GDI/GDI+. The key layers from bottom to top are:

1. **milcore (Media Integration Layer):** An unmanaged component that interfaces directly with DirectX. It handles the composition and rendering of the visual tree. Because it is unmanaged, it can efficiently talk to the GPU.
2. **PresentationCore:** Managed wrapper around milcore. Exposes the `Visual` base class, hit testing, and the low-level rendering API.
3. **PresentationFramework:** The highest layer containing controls, layout panels, data binding, styles, templates, and the XAML engine.

The rendering pipeline works as follows:

- WPF maintains a **retained-mode graphics** system. You describe what you want (a visual tree), and WPF figures out how and when to render it.
- The visual tree is serialized into render instructions and passed to the **composition thread** (milcore), which is separate from the UI thread.
- milcore translates these into DirectX draw calls, enabling hardware-accelerated rendering, resolution independence, and vector-based graphics.

This is fundamentally different from WinForms, which uses GDI+ (immediate-mode, CPU-bound rendering) and wraps Win32 HWND controls. WPF controls are "lookless" — their visuals are defined entirely by templates, not by OS-level window handles.

---

### Q2: What is XAML and what are markup extensions? Give examples of commonly used markup extensions.

**Answer:** XAML (eXtensible Application Markup Language) is a declarative XML-based language used to define WPF UI trees. It maps directly to CLR object instantiation — every XAML element corresponds to a `new` call, and every attribute corresponds to a property set.

**Markup extensions** are special XAML constructs enclosed in curly braces `{}` that provide values that go beyond simple literal strings. They are evaluated at parse time or at runtime.

Common markup extensions:

| Extension | Purpose |
|---|---|
| `{Binding}` | Data binding to a source property |
| `{StaticResource}` | Looks up a resource at load time (one-time) |
| `{DynamicResource}` | Looks up a resource at runtime (tracks changes) |
| `{x:Static}` | References a static field or property |
| `{x:Type}` | Equivalent of `typeof()` |
| `{x:Null}` | Represents a null value |
| `{RelativeSource}` | Binding relative to the target element |
| `{TemplateBinding}` | Binding inside a ControlTemplate to a templated parent's property |

```xml
<!-- Binding with converter -->
<TextBlock Text="{Binding UserName, Mode=OneWay,
                  Converter={StaticResource UpperCaseConverter}}" />

<!-- Static resource for a style -->
<Button Style="{StaticResource PrimaryButtonStyle}" />

<!-- x:Static to reference a CLR constant -->
<TextBlock Text="{x:Static sys:Environment.MachineName}" />

<!-- RelativeSource to walk up the visual tree -->
<TextBlock Text="{Binding DataContext.Title,
                  RelativeSource={RelativeSource AncestorType=Window}}" />
```

You can also create **custom markup extensions** by deriving from `MarkupExtension` and overriding `ProvideValue`.

---

### Q3: Describe the four data binding modes in WPF and when you would use each.

**Answer:** WPF supports four binding modes set via the `Mode` property of a `Binding`:

| Mode | Direction | Use Case |
|---|---|---|
| **OneWay** | Source -> Target | Display-only UI (labels, read-only text). Target updates when source changes. |
| **TwoWay** | Source <-> Target | Editable fields (TextBox, CheckBox). Changes in either direction propagate. |
| **OneTime** | Source -> Target (once) | Static data that never changes after initial load. Avoids overhead of change tracking. |
| **OneWayToSource** | Target -> Source | Rare. The UI element pushes values back to the source, but does not read from it. Useful for write-only scenarios. |

```xml
<!-- OneWay: label displays but user cannot edit -->
<TextBlock Text="{Binding FullName, Mode=OneWay}" />

<!-- TwoWay: TextBox edits flow back to the ViewModel -->
<TextBox Text="{Binding Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- OneTime: loaded once, no further tracking -->
<TextBlock Text="{Binding AppVersion, Mode=OneTime}" />

<!-- OneWayToSource: pushes UI value back without reading -->
<PasswordBox local:PasswordHelper.BoundPassword="{Binding Password, Mode=OneWayToSource}" />
```

**Default modes** differ per control: `TextBox.Text` defaults to `TwoWay`, while `TextBlock.Text` defaults to `OneWay`. This is determined by the `FrameworkPropertyMetadataOptions.BindsTwoWayByDefault` flag on the dependency property.

The `UpdateSourceTrigger` property controls *when* the source is updated in TwoWay bindings: `PropertyChanged` (immediate), `LostFocus` (default for TextBox), or `Explicit` (manual call to `UpdateSource()`).

---

### Q4: How do INotifyPropertyChanged and ObservableCollection work, and why are they essential for WPF data binding?

**Answer:** WPF's binding engine relies on change notification interfaces to keep the UI in sync with the data.

**INotifyPropertyChanged (INPC):** Notifies the binding engine when a scalar property value changes. Without it, the UI will display the initial value but never update.

```csharp
public class CustomerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
```

**ObservableCollection<T>:** Implements `INotifyCollectionChanged`, which fires when items are added, removed, or the entire list is refreshed. An `ItemsControl` (ListBox, DataGrid, etc.) bound to a regular `List<T>` will not reflect additions or removals — you need `ObservableCollection<T>`.

```csharp
public ObservableCollection<Order> Orders { get; } = new();

// UI automatically reflects these changes:
Orders.Add(new Order { Id = 1, Total = 99.99m });
Orders.RemoveAt(0);
```

Key nuances for a senior-level answer:

- **ObservableCollection does not notify when a property of an item changes** — each item must implement INPC independently.
- Raising `PropertyChanged` with `string.Empty` or `null` signals that all properties changed.
- In .NET Community Toolkit, `[ObservableProperty]` source generators eliminate the boilerplate entirely.
- For bulk updates to ObservableCollection, consider subclassing it to support `AddRange` with a single `Reset` notification to avoid per-item UI refresh overhead.

---

### Q5: Explain the MVVM pattern in WPF. Why is it the preferred architectural pattern, and how do the pieces fit together?

**Answer:** MVVM (Model-View-ViewModel) separates concerns into three layers:

| Layer | Responsibility | WPF Construct |
|---|---|---|
| **Model** | Business logic, domain entities, data access | Plain C# classes, services |
| **View** | Visual layout and user interaction | XAML files, code-behind (minimal) |
| **ViewModel** | Presentation logic, state, commands | Classes implementing INPC, exposing ICommand |

The **ViewModel** never references the **View**. Communication happens entirely through data binding, commands, and sometimes messaging/eventing.

```csharp
// ViewModel
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IOrderService _orderService;

    public ObservableCollection<OrderDto> Orders { get; } = new();

    private OrderDto? _selectedOrder;
    public OrderDto? SelectedOrder
    {
        get => _selectedOrder;
        set { _selectedOrder = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }

    public MainViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        RefreshCommand = new RelayCommand(async () => await LoadOrdersAsync());
    }

    private async Task LoadOrdersAsync()
    {
        Orders.Clear();
        var orders = await _orderService.GetAllAsync();
        foreach (var o in orders) Orders.Add(o);
    }

    // INPC implementation omitted for brevity
}
```

```xml
<!-- View (XAML) -->
<Window x:Class="App.MainWindow"
        xmlns:vm="clr-namespace:App.ViewModels">
    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>

    <DockPanel>
        <Button DockPanel.Dock="Top" Content="Refresh"
                Command="{Binding RefreshCommand}" />
        <ListBox ItemsSource="{Binding Orders}"
                 SelectedItem="{Binding SelectedOrder}" />
    </DockPanel>
</Window>
```

**Why MVVM is preferred in WPF:**

1. **Testability:** ViewModels are plain classes — unit test them without spinning up a UI.
2. **Designer-developer workflow:** Designers can modify XAML independently.
3. **WPF was designed for it:** The binding engine, commands, DataTemplates, and dependency properties all naturally support this pattern.
4. **Separation of concerns:** No business logic in code-behind, no UI references in logic.

Popular MVVM frameworks: CommunityToolkit.Mvvm (Microsoft's recommended lightweight toolkit), Prism, Caliburn.Micro.

---

### Q6: What are dependency properties and how do they differ from standard CLR properties?

**Answer:** Dependency properties are a WPF-specific property system built on top of `DependencyObject`. They enable features that CLR properties alone cannot support: data binding, animation, styling, templating, default values with inheritance, and change notification — all without storing a backing field per instance.

| Feature | CLR Property | Dependency Property |
|---|---|---|
| Storage | Instance field | WPF property system (sparse storage — only stores non-default values) |
| Data Binding | No | Yes |
| Animation | No | Yes |
| Style/Template setters | No | Yes |
| Value inheritance | No | Yes (e.g., `FontSize` flows down the tree) |
| Change callbacks | Manual | Built-in via `PropertyChangedCallback` |

```csharp
public class LabeledInput : Control
{
    // 1. Register the dependency property
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(
            nameof(LabelText),                          // property name
            typeof(string),                             // property type
            typeof(LabeledInput),                       // owner type
            new FrameworkPropertyMetadata(
                string.Empty,                           // default value
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnLabelTextChanged));                   // change callback

    // 2. CLR wrapper (must only call GetValue/SetValue — no extra logic)
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    // 3. Optional change callback
    private static void OnLabelTextChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var control = (LabeledInput)d;
        // React to change
    }
}
```

**Attached properties** are a special form of dependency property that can be set on any `DependencyObject`, not just the owner. Classic examples: `Grid.Row`, `Canvas.Left`, `DockPanel.Dock`.

```csharp
public static readonly DependencyProperty IsHighlightedProperty =
    DependencyProperty.RegisterAttached(
        "IsHighlighted", typeof(bool), typeof(HighlightBehavior),
        new PropertyMetadata(false));

public static void SetIsHighlighted(UIElement el, bool value)
    => el.SetValue(IsHighlightedProperty, value);

public static bool GetIsHighlighted(UIElement el)
    => (bool)el.GetValue(IsHighlightedProperty);
```

The WPF property system resolves the effective value of a dependency property through a **value precedence** chain: Animation > Local value > Style triggers > Template triggers > Style setters > Inheritance > Default.

---

### Q7: Explain WPF routed events — bubbling, tunneling, and direct. Why does WPF use this system?

**Answer:** WPF uses a **routed event** system where events travel through the element tree rather than being handled only by the element that raised them. There are three routing strategies:

| Strategy | Direction | Naming Convention | Example |
|---|---|---|---|
| **Bubbling** | Source -> up to root | `MouseDown`, `Click` | Most common. Event fires on source, then each ancestor. |
| **Tunneling** | Root -> down to source | `PreviewMouseDown`, `PreviewKeyDown` | "Preview" prefix. Fires before the bubbling counterpart. |
| **Direct** | Only on source | `MouseEnter`, `Loaded` | Behaves like a normal .NET event. |

Tunneling and bubbling events come in pairs. The tunneling (Preview) event fires first; then the bubbling event fires. Setting `e.Handled = true` at any point stops further propagation.

```xml
<StackPanel PreviewMouseDown="StackPanel_PreviewMouseDown"
            MouseDown="StackPanel_MouseDown">
    <Button Content="Click Me"
            PreviewMouseDown="Button_PreviewMouseDown"
            MouseDown="Button_MouseDown" />
</StackPanel>
```

Execution order when the Button is clicked:
1. `StackPanel_PreviewMouseDown` (tunnel down)
2. `Button_PreviewMouseDown` (tunnel arrives at source)
3. `Button_MouseDown` (bubble starts at source)
4. `StackPanel_MouseDown` (bubble up)

```csharp
private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    // Intercept and suppress the event before it reaches the Button
    if (ShouldBlock)
        e.Handled = true;
}
```

**Why routed events matter:**

- **Centralized handling:** A parent can handle events from any descendant without wiring each child individually — essential for ItemsControl scenarios with dynamic children.
- **Preview events for interception:** Tunneling lets a parent suppress or modify behavior before the target element sees it (e.g., input validation).
- **Composition-friendly:** WPF controls are composed of many visual elements; routed events ensure clicks on inner elements still fire on the logical control.

Custom routed events are registered similarly to dependency properties:

```csharp
public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
    "Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyControl));

public event RoutedEventHandler Tap
{
    add => AddHandler(TapEvent, value);
    remove => RemoveHandler(TapEvent, value);
}

protected void RaiseTap() => RaiseEvent(new RoutedEventArgs(TapEvent));
```

---

### Q8: Describe Styles, ControlTemplate, DataTemplate, and Resources in WPF.

**Answer:**

**Resources:** A dictionary-based mechanism to store reusable objects (brushes, styles, templates, converters) at any level of the element tree. Resources defined higher up are available to all descendants.

```xml
<Window.Resources>
    <SolidColorBrush x:Key="AccentBrush" Color="#0078D7" />
    <local:BoolToVisibilityConverter x:Key="BoolToVis" />
</Window.Resources>
```

**Styles:** Apply a set of property values to elements, like CSS for WPF. They can include setters, triggers, and can inherit via `BasedOn`.

```xml
<Style x:Key="BaseButton" TargetType="Button">
    <Setter Property="FontSize" Value="14" />
    <Setter Property="Padding" Value="12,6" />
</Style>

<Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
    <Setter Property="Background" Value="{StaticResource AccentBrush}" />
    <Setter Property="Foreground" Value="White" />
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Opacity" Value="0.85" />
        </Trigger>
    </Style.Triggers>
</Style>
```

**ControlTemplate:** Replaces the entire visual tree of a control. This is what makes WPF controls "lookless" — you can completely redefine how a Button, TextBox, etc. looks while retaining its behavior.

```xml
<ControlTemplate x:Key="RoundButton" TargetType="Button">
    <Grid>
        <Ellipse Fill="{TemplateBinding Background}" />
        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" />
    </Grid>
    <ControlTemplate.Triggers>
        <Trigger Property="IsPressed" Value="True">
            <Setter TargetName="..." Property="Opacity" Value="0.7" />
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

**DataTemplate:** Defines how a data object (non-visual) should be rendered. Used by ItemsControl, ContentControl, etc. to visualize bound data.

```xml
<DataTemplate DataType="{x:Type models:Customer}">
    <StackPanel Orientation="Horizontal">
        <Ellipse Width="32" Height="32" Fill="Gray" />
        <StackPanel Margin="8,0">
            <TextBlock Text="{Binding FullName}" FontWeight="Bold" />
            <TextBlock Text="{Binding Email}" Foreground="Gray" />
        </StackPanel>
    </StackPanel>
</DataTemplate>
```

When a `DataTemplate` has a `DataType` but no `x:Key`, it becomes an **implicit template** — WPF automatically applies it whenever it encounters that type.

**Summary of differences:**

| Concept | Controls | Purpose |
|---|---|---|
| Style | Property values + triggers | *How properties are configured* |
| ControlTemplate | Visual structure of a control | *How a control looks* |
| DataTemplate | Visual structure for data objects | *How data is displayed* |

---

### Q9: How do commands work in WPF? Explain ICommand and the RelayCommand pattern.

**Answer:** Commands decouple the "what to do" from the "who triggers it." In MVVM, the ViewModel exposes `ICommand` properties that the View binds to — no event handlers in code-behind.

**ICommand interface:**

```csharp
public interface ICommand
{
    bool CanExecute(object? parameter);   // Determines if the command can run
    void Execute(object? parameter);       // Runs the command
    event EventHandler? CanExecuteChanged; // Notifies UI to re-evaluate CanExecute
}
```

WPF automatically disables a Button (or MenuItem) when `CanExecute` returns `false`.

**RelayCommand implementation** (simplified):

```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

Hooking into `CommandManager.RequerySuggested` means WPF automatically re-evaluates `CanExecute` whenever it detects input changes (focus, key press, etc.).

**Usage in ViewModel:**

```csharp
public ICommand DeleteCommand { get; }

public MainViewModel()
{
    DeleteCommand = new RelayCommand(
        execute: _ => DeleteSelectedItem(),
        canExecute: _ => SelectedItem != null
    );
}
```

```xml
<Button Content="Delete" Command="{Binding DeleteCommand}" />
```

**AsyncRelayCommand:** For async operations, CommunityToolkit.Mvvm provides `AsyncRelayCommand` that wraps `Func<Task>`, handles reentrancy, and exposes `IsRunning` for busy indicators.

```csharp
public IAsyncRelayCommand SaveCommand { get; }

public MyViewModel()
{
    SaveCommand = new AsyncRelayCommand(SaveAsync, () => IsDirty);
}

private async Task SaveAsync()
{
    await _repository.SaveChangesAsync();
    IsDirty = false;
}
```

**Built-in commands:** WPF also provides `ApplicationCommands` (Copy, Paste, Save), `NavigationCommands`, etc., which can be bound via `CommandBindings` on any `UIElement`. These are useful but less common in MVVM apps.

---

### Q10: Explain the WPF threading model and the role of the Dispatcher.

**Answer:** WPF enforces a **single-threaded apartment (STA)** model. All UI elements are owned by the UI thread and can only be accessed from that thread. Attempting to access a UI element from a background thread throws an `InvalidOperationException`.

The **Dispatcher** is the WPF message pump — it maintains a prioritized queue of work items and executes them on the UI thread. Every `DispatcherObject` (which includes all WPF controls) is tied to a Dispatcher.

**Invoking work on the UI thread from a background thread:**

```csharp
// Synchronous — blocks until the UI thread processes it
Application.Current.Dispatcher.Invoke(() =>
{
    StatusLabel.Content = "Done";
});

// Asynchronous — queues the work and returns immediately
await Application.Current.Dispatcher.InvokeAsync(() =>
{
    ProgressBar.Value = 75;
}, DispatcherPriority.Background);
```

**DispatcherPriority** controls execution order (from highest to lowest): Send > Normal > DataBind > Render > Background > ApplicationIdle > SystemIdle > Inactive.

**Best practices for a senior engineer:**

1. **Use `async`/`await` properly** — When you `await` a `Task` on the UI thread, the continuation automatically resumes on the UI thread (because of `SynchronizationContext`), so explicit Dispatcher calls are often unnecessary.

```csharp
// No Dispatcher needed — await resumes on UI thread
private async void OnLoadClick(object sender, RoutedEventArgs e)
{
    LoadingSpinner.Visibility = Visibility.Visible;
    var data = await Task.Run(() => _service.LoadHeavyData());
    DataGrid.ItemsSource = data;  // Back on UI thread
    LoadingSpinner.Visibility = Visibility.Collapsed;
}
```

2. **`Dispatcher.CheckAccess()`** allows you to check if you are already on the UI thread:

```csharp
public void UpdateStatus(string message)
{
    if (Application.Current.Dispatcher.CheckAccess())
        StatusLabel.Content = message;
    else
        Application.Current.Dispatcher.Invoke(() => StatusLabel.Content = message);
}
```

3. **Avoid long-running work on the UI thread.** Move computation to `Task.Run()` and marshal only the final result back.

4. **The composition thread** is a separate thread managed by milcore for rendering. It is not the UI thread, but you never interact with it directly.

---

## WinForms

### Q11: Describe the WinForms architecture and the Windows message loop.

**Answer:** WinForms is a managed wrapper around the Win32 API. Each WinForms `Control` wraps a native HWND (window handle), and user interaction is driven by the **Windows message loop**.

**Architecture layers:**

1. **Win32/User32:** OS-level window management, message dispatching.
2. **System.Windows.Forms:** Managed wrappers (`Form`, `Button`, `TextBox`, etc.) that internally call Win32 APIs via P/Invoke.
3. **GDI+/System.Drawing:** Rendering engine for painting controls and custom graphics.

**The message loop:**

When `Application.Run(new MainForm())` is called, WinForms enters a message loop that:

1. Retrieves messages from the OS message queue (`GetMessage`).
2. Translates and dispatches them (`TranslateMessage`, `DispatchMessage`).
3. Routes them to the appropriate control's `WndProc` method.
4. `WndProc` translates Win32 messages (e.g., `WM_CLICK`, `WM_PAINT`) into .NET events (e.g., `Click`, `Paint`).

```csharp
// Simplified conceptual loop
while (GetMessage(out msg, IntPtr.Zero, 0, 0))
{
    TranslateMessage(ref msg);
    DispatchMessage(ref msg);   // -> Control.WndProc()
}
```

You can override `WndProc` to intercept raw Win32 messages:

```csharp
protected override void WndProc(ref Message m)
{
    const int WM_NCHITTEST = 0x0084;
    if (m.Msg == WM_NCHITTEST)
    {
        // Custom hit-test logic for borderless window dragging
        base.WndProc(ref m);
        if (m.Result == (IntPtr)1) // HTCLIENT
            m.Result = (IntPtr)2;  // HTCAPTION — allows dragging
        return;
    }
    base.WndProc(ref m);
}
```

Key difference from WPF: WinForms controls are "heavy" — each one allocates a native window handle. WPF controls are lightweight visual objects rendered on a single HWND per window.

---

### Q12: How does event-driven programming work in WinForms? Explain the control lifecycle.

**Answer:** WinForms follows a classic event-driven programming model. You place controls on a form using the designer (or code), then subscribe to their events.

**Event wiring:**

```csharp
public partial class OrderForm : Form
{
    public OrderForm()
    {
        InitializeComponent();

        // Wire events
        btnSubmit.Click += BtnSubmit_Click;
        txtQuantity.TextChanged += TxtQuantity_TextChanged;
        this.FormClosing += OrderForm_FormClosing;
    }

    private void BtnSubmit_Click(object? sender, EventArgs e)
    {
        if (ValidateOrder())
            SubmitOrder();
    }

    private void TxtQuantity_TextChanged(object? sender, EventArgs e)
    {
        if (int.TryParse(txtQuantity.Text, out int qty))
            lblTotal.Text = $"Total: {qty * _unitPrice:C}";
    }

    private void OrderForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("Unsaved changes. Close?", "Confirm",
                MessageBoxButtons.YesNo);
            e.Cancel = result == DialogResult.No;
        }
    }
}
```

**Form/Control lifecycle events (in order):**

1. **Constructor** → `InitializeComponent()` called
2. **HandleCreated** → Native HWND created
3. **Load** → Form is about to be shown for the first time (ideal for initialization)
4. **Shown** → Form is visible on screen
5. **Activated** → Form gets focus
6. **Deactivate** → Form loses focus
7. **FormClosing** → Can cancel close
8. **FormClosed** → Close confirmed
9. **HandleDestroyed** → HWND released
10. **Dispose** → Managed/unmanaged cleanup

For controls: `Paint`, `Resize`, `Enter`/`Leave` (focus), `Validating`/`Validated` (validation chain) are particularly important.

---

### Q13: How does data binding work in WinForms compared to WPF?

**Answer:** WinForms data binding exists but is far less powerful than WPF's. It supports simple binding for individual properties and complex binding for lists.

**Simple binding** — binds a control property to a data source property:

```csharp
// Bind TextBox.Text to Customer.Name
txtName.DataBindings.Add("Text", customer, "Name",
    formattingEnabled: true,
    updateMode: DataSourceUpdateMode.OnPropertyChanged);
```

**Complex binding** — binds list controls to collections:

```csharp
var customers = new BindingList<Customer>(GetCustomers());

dgvCustomers.DataSource = customers;

// ComboBox binding
cboCustomer.DataSource = customers;
cboCustomer.DisplayMember = "FullName";
cboCustomer.ValueMember = "Id";
```

**BindingSource** is the central component for WinForms data binding — it acts as a proxy between data and controls, providing currency management (current item tracking), filtering, and sorting:

```csharp
var bindingSource = new BindingSource();
bindingSource.DataSource = typeof(Customer); // or an actual list

txtName.DataBindings.Add("Text", bindingSource, "Name");
txtEmail.DataBindings.Add("Text", bindingSource, "Email");
dgvCustomers.DataSource = bindingSource;

// Navigate
bindingSource.MoveNext();
bindingSource.MovePrevious();

// Filter
bindingSource.Filter = "City = 'Berlin'";
```

**Key differences from WPF binding:**

| Aspect | WinForms | WPF |
|---|---|---|
| Binding definition | Code-only (`DataBindings.Add`) | XAML or code (`{Binding}`) |
| Binding modes | Limited (OnPropertyChanged, OnValidation, Never) | OneWay, TwoWay, OneTime, OneWayToSource |
| Value converters | `Format`/`Parse` events or `IFormatProvider` | `IValueConverter` / `IMultiValueConverter` |
| Collection notification | `BindingList<T>` | `ObservableCollection<T>` |
| Template support | None | DataTemplate, implicit templates |
| Validation | `Validating` event, `ErrorProvider` | `IDataErrorInfo`, `INotifyDataErrorInfo`, validation rules |

WinForms binding works fine for simple CRUD forms but becomes unwieldy for complex UIs with nested data, converters, or conditional visibility.

---

### Q14: Explain GDI+ and custom painting in WinForms.

**Answer:** GDI+ (`System.Drawing`) is the rendering engine for WinForms. Custom painting is done by handling the `Paint` event or overriding `OnPaint`, using a `Graphics` object to draw shapes, text, and images.

```csharp
public class GaugeControl : Control
{
    private float _value; // 0 to 100

    public float Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, 100); Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background arc
        using var bgPen = new Pen(Color.LightGray, 10f);
        g.DrawArc(bgPen, 10, 10, Width - 20, Height - 20, 180, 180);

        // Value arc
        using var fgPen = new Pen(Color.DodgerBlue, 10f);
        float sweep = _value / 100f * 180f;
        g.DrawArc(fgPen, 10, 10, Width - 20, Height - 20, 180, sweep);

        // Value text
        using var font = new Font("Segoe UI", 16f, FontStyle.Bold);
        var text = $"{_value:F0}%";
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, Brushes.Black,
            (Width - size.Width) / 2,
            Height / 2 - size.Height / 2);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate(); // Repaint on resize
    }
}
```

**Key GDI+ concepts:**

- **Double buffering:** Prevents flickering. Set `DoubleBuffered = true` in the constructor or use `SetStyle(ControlStyles.OptimizedDoubleBuffer, true)`.
- **Invalidate vs Refresh:** `Invalidate()` marks the region as needing repaint (coalesced, efficient). `Refresh()` forces an immediate repaint. Always prefer `Invalidate()`.
- **Dispose pattern:** `Pen`, `Brush`, `Font`, `Bitmap` are unmanaged resources. Always use `using` statements or cache and dispose them in the control's `Dispose`.
- **Clipping:** `e.ClipRectangle` tells you which region needs repainting — you can optimize by only drawing within that rectangle.

GDI+ is CPU-rendered and lacks GPU acceleration, which is one of the main reasons WPF (DirectX-based) outperforms it for complex graphics and animations.

---

### Q15: Compare WPF and WinForms across key technical dimensions.

**Answer:**

| Dimension | WinForms | WPF |
|---|---|---|
| **Rendering** | GDI+ (CPU, immediate mode) | DirectX (GPU, retained mode) |
| **Resolution** | Pixel-based; DPI scaling is bolt-on | Vector-based; resolution-independent natively |
| **UI Definition** | Designer-generated C# code | XAML (declarative, toolable) |
| **Control Model** | Each control = native HWND | Lookless controls; visual tree rendered on one HWND |
| **Styling** | Per-control properties; no cascading | Styles, templates, triggers, resources |
| **Data Binding** | Basic (`DataBindings.Add`) | Rich (XAML, converters, multi-binding, validation) |
| **Architecture** | Event-driven (code-behind heavy) | MVVM-friendly by design |
| **Layout** | Absolute positioning / anchors / dock | Flexible panels (Grid, StackPanel, DockPanel) |
| **Graphics/Animation** | Manual GDI+ / Timer-based | Storyboards, visual states, 3D, media |
| **Learning Curve** | Lower; familiar Win32-style model | Steeper; requires understanding XAML, binding, DP |
| **Testability** | Harder (logic coupled to UI) | Easier (MVVM separates logic) |
| **Legacy/Interop** | Excellent Win32 interop, ActiveX hosting | WindowsFormsHost / HwndHost for interop |
| **.NET Support** | .NET Framework & .NET 5+ (Windows) | .NET Framework & .NET 5+ (Windows) |
| **Maturity** | Stable, maintenance mode | Actively improved (especially in .NET 5+) |

**When WinForms wins:** Simple internal tools, rapid prototyping, brownfield apps with heavy Win32 interop, teams without XAML experience.

**When WPF wins:** Modern-looking UIs, complex data visualization, apps needing rich styling/theming, resolution-independent UIs, projects that benefit from MVVM and testability.

---

## General

### Q16: When would you choose WPF over WinForms (and vice versa) for a new project?

**Answer:** This is a decision matrix I would walk through:

**Choose WPF when:**

- The application needs a **modern, visually rich UI** with custom styles, animations, and theming (e.g., customer-facing dashboards, media apps).
- **High-DPI / multi-monitor** support is a requirement — WPF handles DPI scaling natively with its vector rendering.
- The team values **testability** and wants to use MVVM with unit-tested ViewModels.
- The application involves **complex data visualization** (charts, diagrams, real-time displays) that benefits from GPU-accelerated rendering.
- You need **flexible layouts** that adapt to different window sizes and resolutions without manual repositioning.
- The project is **greenfield** and the team has (or can acquire) XAML skills.

**Choose WinForms when:**

- Building **simple internal tools, forms-over-data** apps, or quick prototypes where development speed matters more than visual polish.
- The team is experienced in WinForms and there is **no time to ramp up on XAML/WPF**.
- Heavy use of **third-party WinForms control libraries** already licensed (DevExpress, Telerik, Infragistics).
- Deep **Win32 interop** or **ActiveX control** hosting is needed — WinForms handles this more naturally.
- Maintaining or extending an **existing WinForms codebase** where a full rewrite is not justified.

**Neutral factors:** Both are supported on .NET 8+ (Windows only). Both can access the same backend libraries, databases, and services. Neither is cross-platform (for that, look at MAUI or Avalonia).

In a senior interview, I would add: "The choice also depends on the team. A great WinForms app with clean separation of concerns will outperform a poorly structured WPF app. Architecture discipline matters more than framework choice."

---

### Q17: What strategies would you use to migrate a WinForms application to WPF?

**Answer:** A full rewrite is rarely practical. I recommend an **incremental migration** strategy:

**Phase 1: Preparation (in WinForms)**

Refactor the existing WinForms code to separate concerns before touching WPF:

- Extract business logic from code-behind into service classes and ViewModels.
- Introduce interfaces for dependencies (`IOrderService`, `INavigationService`).
- Apply the **MVP (Model-View-Presenter)** pattern to WinForms code — this is structurally similar to MVVM, so the transition is smoother.

```csharp
// Before: logic tangled in code-behind
private void btnSave_Click(object sender, EventArgs e)
{
    var conn = new SqlConnection("...");
    // 50 lines of data access + validation
}

// After: extracted into a testable presenter/service
private void btnSave_Click(object sender, EventArgs e)
{
    _presenter.SaveOrder();
}
```

**Phase 2: Hybrid hosting**

Use `ElementHost` (in WinForms) to embed WPF controls inside WinForms forms, or `WindowsFormsHost` (in WPF) to embed WinForms controls inside WPF windows.

```csharp
// Hosting a WPF UserControl inside a WinForms form
var elementHost = new ElementHost
{
    Dock = DockStyle.Fill,
    Child = new WpfOrderDetailsView() // WPF UserControl
};
panelContainer.Controls.Add(elementHost);
```

This lets you build new screens in WPF while keeping existing WinForms screens running.

**Phase 3: Screen-by-screen migration**

- Migrate one form at a time to a WPF Window/UserControl.
- Reuse the ViewModels/services extracted in Phase 1.
- Start with less critical screens to build team confidence.

**Phase 4: Full WPF shell**

Once most screens are WPF, flip the architecture: make the shell a WPF `Application` and host remaining WinForms forms via `WindowsFormsHost`.

**Key pitfalls to avoid:**

- **Airspace issues:** WinForms controls hosted in WPF render in a separate HWND and always appear on top of WPF content. This has improved in .NET Core but still has edge cases.
- **Threading differences:** Both are STA, but their Dispatcher/message loop interactions can cause subtle bugs.
- **Data binding mismatch:** WinForms `BindingList<T>` does not fire the same notifications as `ObservableCollection<T>`. You may need adapter collections during the transition.
- **Do not try to reuse WinForms visual styles in WPF** — embrace WPF's styling system from the start.

---

### Q18: What are the modern deployment strategies for .NET desktop applications?

**Answer:** There are several deployment strategies, each with trade-offs:

**1. ClickOnce**

The classic .NET deployment mechanism. The app is published to a web server, file share, or Azure Blob Storage. Users click a link to install, and updates are checked automatically.

```xml
<!-- In .csproj -->
<PropertyGroup>
    <PublishUrl>\\server\share\MyApp\</PublishUrl>
    <InstallUrl>https://mycompany.com/apps/MyApp/</InstallUrl>
    <UpdateEnabled>true</UpdateEnabled>
    <UpdateMode>Foreground</UpdateMode>
</PropertyGroup>
```

```bash
dotnet publish -p:PublishProfile=ClickOnceProfile
```

Pros: Simple, auto-update, no admin rights needed, rollback support.
Cons: Windows-only, limited install customization, no service installation, code signing is important for trust.

**2. MSIX**

Microsoft's modern packaging format. It runs the app in a lightweight container with clean install/uninstall, automatic updates, and Microsoft Store distribution.

```bash
# Using Windows Application Packaging Project in Visual Studio
# Or via MSIX Packaging Tool for existing apps
```

Pros: Clean install/uninstall (no registry pollution), sideloading or Store distribution, works with both WinForms and WPF, supports auto-update via App Installer.
Cons: Some Win32 API restrictions in the container, requires code signing certificate, initial setup complexity.

**3. Self-contained single-file deployment**

Bundles the .NET runtime with your app into one executable:

```bash
dotnet publish -c Release -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true
```

Pros: No .NET runtime prerequisite, xcopy-deployable, simple.
Cons: Large file size (60-100+ MB), no built-in auto-update (roll your own with libraries like Squirrel.Windows or Velopack).

**4. Framework-dependent deployment**

Requires the target .NET runtime to be installed. Smaller payload:

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

Typically combined with a prerequisite check in the installer.

**5. Third-party installers (WiX, Inno Setup, NSIS)**

For maximum control: custom install paths, registry entries, Windows Services, firewall rules, prerequisites. WiX is the de facto standard for enterprise .NET deployments.

**Auto-update strategy comparison:**

| Strategy | Auto-Update Mechanism |
|---|---|
| ClickOnce | Built-in |
| MSIX | App Installer / Store |
| Squirrel.Windows / Velopack | Library-based (GitHub Releases, S3, etc.) |
| WiX / custom installer | Manual (check version endpoint + download) |

For a senior role, I would recommend: **MSIX for new enterprise apps** (clean, modern, IT-friendly), **ClickOnce for internal tools** (simple), and **self-contained + Velopack for apps distributed outside the enterprise** (maximum compatibility, no runtime dependency).
