using Spectre.Tui.Doodads.Doodads.Help;
using Spectre.Tui.Doodads.Doodads.Paginator;
using Spectre.Tui.Doodads.Doodads.Spinner;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// A feature-rich browsable, filterable list with pagination, help, and status messages.
/// </summary>
/// <typeparam name="TItem">The type of items in the list.</typeparam>
public record ListModel<TItem> : IDoodad<ListModel<TItem>>, ISizedRenderable
    where TItem : IListItem
{
    // ReSharper disable once StaticMemberInGenericType
    private static int _nextStatusId;

    /// <summary>
    /// Gets the currently visible items (filtered or all).
    /// </summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];

    /// <summary>
    /// Gets the index of the selected item within the visible items.
    /// </summary>
    public int SelectedIndex { get; init; }

    /// <summary>
    /// Gets the title displayed above the list.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum display width of the list.
    /// </summary>
    public int MinWidth { get; init; } = 80;

    /// <summary>
    /// Gets the minimum display height of the list.
    /// </summary>
    public int MinHeight { get; init; } = 24;

    /// <summary>
    /// Gets a value indicating whether the title bar is shown.
    /// </summary>
    public bool ShowTitle { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the filter input is available.
    /// </summary>
    public bool ShowFilter { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the status bar is shown.
    /// </summary>
    public bool ShowStatusBar { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether pagination dots are shown.
    /// </summary>
    public bool ShowPagination { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the help bar is shown.
    /// </summary>
    public bool ShowHelp { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether filtering is enabled.
    /// </summary>
    public bool FilteringEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether infinite scrolling (wrap-around) is enabled.
    /// </summary>
    public bool InfiniteScrolling { get; init; } = true;

    /// <summary>
    /// Gets the auto-dismiss duration for status messages.
    /// </summary>
    public TimeSpan StatusMessageLifetime { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets the unified styles for list chrome elements.
    /// </summary>
    public ListStyles Styles { get; init; } = new();

    /// <summary>
    /// Gets the current filter state.
    /// </summary>
    public ListFilterState FilterState { get; init; } = ListFilterState.Unfiltered;

    /// <summary>
    /// Gets the delegate responsible for rendering list items.
    /// </summary>
    public IListItemDelegate<TItem> Delegate { get; init; } = null!;

    /// <summary>
    /// Gets the key map for list navigation.
    /// </summary>
    public ListKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets the custom filter function. If null, a default case-insensitive substring match is used.
    /// </summary>
    public Func<string, IReadOnlyList<TItem>, IReadOnlyList<TItem>>? Filter { get; init; }

    /// <summary>
    /// Gets an optional custom update function invoked on key messages.
    /// </summary>
    public Func<KeyMessage, ListModel<TItem>, (ListModel<TItem>, Command?)>? UpdateFunc { get; init; }

    /// <summary>
    /// Gets an optional function to provide short help bindings.
    /// </summary>
    public Func<IEnumerable<KeyBinding>>? ShortHelpFunc { get; init; }

    /// <summary>
    /// Gets an optional function to provide full help binding groups.
    /// </summary>
    public Func<IEnumerable<IEnumerable<KeyBinding>>>? FullHelpFunc { get; init; }

    /// <summary>
    /// Gets the filter text input sub-doodad.
    /// </summary>
    internal TextInputModel FilterInput { get; init; } = new() { Prompt = "/ ", Placeholder = "Filter...", MinWidth = 40 };

    /// <summary>
    /// Gets the paginator sub-doodad.
    /// </summary>
    internal PaginatorModel Paginator { get; init; } = new();

    /// <summary>
    /// Gets the help sub-doodad.
    /// </summary>
    internal HelpModel Help { get; init; } = new();

    /// <summary>
    /// Gets the spinner sub-doodad for loading state.
    /// </summary>
    internal SpinnerModel Spinner { get; init; } = new();

    /// <summary>
    /// Gets all items before filtering.
    /// </summary>
    internal IReadOnlyList<TItem> AllItems { get; init; } = [];

    /// <summary>
    /// Gets the current status bar message.
    /// </summary>
    internal string StatusMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the loading spinner is shown.
    /// </summary>
    internal bool ShowSpinner { get; init; }

    /// <summary>
    /// Gets the unique identifier for the current status message (for auto-dismiss).
    /// </summary>
    internal int StatusId { get; init; }

    /// <inheritdoc />
    public Command Init()
    {
        var spinnerCmd = Spinner.Init();
        return spinnerCmd;
    }

    /// <inheritdoc />
    public (ListModel<TItem> Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case ListStatusMessage statusMsg when statusMsg.Id == StatusId:
                return (this with { StatusMessage = string.Empty }, null);

            case ListStatusMessage:
                return (this, null);

            case SpinnerTickMessage when ShowSpinner:
                var (updatedSpinner, spinnerCmd) = Spinner.Update(message);
                return (this with { Spinner = updatedSpinner }, spinnerCmd);

            case KeyMessage km when FilterState == ListFilterState.Filtering:
                return HandleFilteringKey(km);

            case KeyMessage km:
                return HandleKey(km);

            case not null when FilterState == ListFilterState.Filtering:
                var (filterInput, filterCmd) = FilterInput.Update(message);
                return (this with { FilterInput = filterInput }, filterCmd);

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var y = 0;

        // Title
        if (ShowTitle && Title.Length > 0)
        {
            surface.SetString(0, y, Title, Styles.Title);
            y++;
            y++; // blank line after title
        }

        // Filter input
        if (FilterState == ListFilterState.Filtering)
        {
            surface.Render(FilterInput, new Rectangle(0, y, width, 1));
            y++;
            y++; // blank line after filter
        }
        else if (FilterState == ListFilterState.FilterApplied)
        {
            var filterText = "Filter: " + FilterInput.GetValue();
            surface.SetString(0, y, filterText, Styles.FilterPrompt);
            y++;
            y++; // blank line after filter display
        }

        // Loading spinner
        if (ShowSpinner)
        {
            surface.Render(Spinner, new Rectangle(0, y, width, 1));
            return;
        }

        // List items
        if (Items.Count > 0)
        {
            var (start, end) = Paginator.GetSliceBounds(Items.Count);
            var itemHeight = Delegate.Height;
            var spacing = Delegate.Spacing;

            for (var i = start; i < end; i++)
            {
                var isSelected = i == SelectedIndex;
                var itemY = y + ((i - start) * (itemHeight + spacing));
                if (itemY + itemHeight > height)
                {
                    break;
                }

                surface.Render(
                    new ListItemRenderable<TItem>(Delegate, Items[i], i, isSelected),
                    new Rectangle(0, itemY, width, itemHeight));
            }

            y += (end - start) * (itemHeight + spacing);
        }
        else
        {
            surface.SetString(0, y, "No items.", Styles.NoItems);
            y++;
        }

        y++; // blank line before footer

        // Status bar
        if (ShowStatusBar)
        {
            var statusText = StatusMessage.Length > 0
                ? StatusMessage
                : GetDefaultStatusText();
            surface.SetString(0, y, statusText, Styles.StatusBar);
            y++;
        }

        // Pagination
        if (ShowPagination && Paginator.TotalPages > 1)
        {
            surface.Render(Paginator, new Rectangle(0, y, width, 1));
            y++;
        }

        // Help
        if (ShowHelp)
        {
            surface.Render(Help.SetKeyMap(KeyMap), new Rectangle(0, y, width, 1));
        }
    }

    /// <summary>
    /// Sets the items in the list and resets pagination.
    /// </summary>
    /// <param name="items">The items to display.</param>
    /// <returns>The updated list model.</returns>
    public ListModel<TItem> SetItems(IReadOnlyList<TItem> items)
    {
        var model = this with
        {
            AllItems = items,
            Items = items,
            SelectedIndex = 0,
            FilterState = ListFilterState.Unfiltered,
        };
        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(items.Count) with { Page = 0 },
        };
        return model;
    }

    /// <summary>
    /// Gets the currently selected item, or default if the list is empty.
    /// </summary>
    public TItem? SelectedItem =>
        Items.Count > 0 && SelectedIndex >= 0 && SelectedIndex < Items.Count
            ? Items[SelectedIndex]
            : default;

    /// <summary>
    /// Sets the display size of the list.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <returns>The updated list model.</returns>
    public ListModel<TItem> SetSize(int width, int height)
    {
        return this with
        {
            MinWidth = width,
            MinHeight = height,
            Help = Help with { MinWidth = width },
        };
    }

    /// <summary>
    /// Toggles the loading spinner visibility.
    /// </summary>
    /// <param name="show">Whether to show the spinner.</param>
    /// <returns>A tuple of the updated model and an optional command.</returns>
    public (ListModel<TItem> Model, Command? Command) SetShowSpinner(bool show)
    {
        var model = this with { ShowSpinner = show };
        if (show)
        {
            var cmd = model.Spinner.Init();
            return (model, cmd);
        }

        return (model, null);
    }

    /// <summary>
    /// Sets a status bar message that auto-dismisses after the configured lifetime.
    /// </summary>
    /// <param name="message">The status message to display.</param>
    /// <returns>A tuple of the updated model and the auto-dismiss command.</returns>
    public (ListModel<TItem> Model, Command? Command) NewStatusMessage(string message)
    {
        var id = Interlocked.Increment(ref _nextStatusId);
        var model = this with { StatusMessage = message, StatusId = id };
        var cmd = Commands.Tick(
            StatusMessageLifetime,
            _ => new ListStatusMessage { Id = id });
        return (model, cmd);
    }

    /// <summary>
    /// Programmatically selects an item at the given index.
    /// </summary>
    /// <param name="index">The index to select.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> Select(int index)
    {
        if (Items.Count == 0)
        {
            return this;
        }

        var clamped = Math.Clamp(index, 0, Items.Count - 1);
        var model = this with { SelectedIndex = clamped };
        return model.EnsureSelectedVisible();
    }

    /// <summary>
    /// Resets the selected index to 0.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> ResetSelected()
    {
        return Select(0);
    }

    /// <summary>
    /// Resets the filter, showing all items.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> ResetFilter()
    {
        var model = this with
        {
            Items = AllItems,
            FilterState = ListFilterState.Unfiltered,
            SelectedIndex = 0,
        };
        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(model.Items.Count) with { Page = 0 },
        };
        return model;
    }

    /// <summary>
    /// Gets the currently visible items.
    /// </summary>
    /// <returns>The items currently displayed (filtered or all).</returns>
    public IReadOnlyList<TItem> VisibleItems() => Items;

    /// <summary>
    /// Gets the filter match positions for a given item index (returns the matching substring indices).
    /// </summary>
    /// <param name="index">The item index in the visible items list.</param>
    /// <returns>The list of match position indices, or empty if no filter is active.</returns>
    public IReadOnlyList<int> MatchesForItem(int index)
    {
        if (FilterState == ListFilterState.Unfiltered || index < 0 || index >= Items.Count)
        {
            return [];
        }

        var filterText = FilterInput.GetValue();
        if (string.IsNullOrEmpty(filterText))
        {
            return [];
        }

        var item = Items[index];
        var matches = new List<int>();
        var searchIn = item.FilterValue;
        var searchFor = filterText;
        var pos = 0;

        while (pos < searchIn.Length)
        {
            var idx = searchIn.IndexOf(searchFor, pos, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                break;
            }

            for (var i = idx; i < idx + searchFor.Length; i++)
            {
                matches.Add(i);
            }

            pos = idx + 1;
        }

        return matches;
    }

    /// <summary>
    /// Gets the index of the currently selected item within the visible items.
    /// </summary>
    public int Index() => SelectedIndex;

    /// <summary>
    /// Gets the global index of the currently selected item (within AllItems).
    /// </summary>
    public int GlobalIndex()
    {
        if (FilterState == ListFilterState.Unfiltered || Items.Count == 0)
        {
            return SelectedIndex;
        }

        var selectedItem = Items[SelectedIndex];
        for (var i = 0; i < AllItems.Count; i++)
        {
            if (EqualityComparer<TItem>.Default.Equals(AllItems[i], selectedItem))
            {
                return i;
            }
        }

        return SelectedIndex;
    }

    /// <summary>
    /// Gets the cursor position (same as Index).
    /// </summary>
    public int Cursor() => SelectedIndex;

    /// <summary>
    /// Gets the current filter value.
    /// </summary>
    public string FilterValue() => FilterInput.GetValue();

    /// <summary>
    /// Gets a value indicating whether the user is currently typing a filter.
    /// </summary>
    public bool SettingFilter() => FilterState == ListFilterState.Filtering;

    /// <summary>
    /// Gets a value indicating whether a filter is currently applied.
    /// </summary>
    public bool IsFiltered() => FilterState == ListFilterState.FilterApplied;

    /// <summary>
    /// Sets the filter text programmatically and applies live filtering.
    /// </summary>
    /// <param name="text">The filter text.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> SetFilterText(string text)
    {
        var input = FilterInput.SetValue(text);
        var model = this with { FilterInput = input };
        return model.ApplyLiveFilter();
    }

    /// <summary>
    /// Sets the filter state programmatically.
    /// </summary>
    /// <param name="state">The new filter state.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> SetFilterState(ListFilterState state)
    {
        return this with { FilterState = state };
    }

    /// <summary>
    /// Replaces the item at the given index.
    /// </summary>
    /// <param name="index">The index of the item to replace.</param>
    /// <param name="item">The new item.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> SetItem(int index, TItem item)
    {
        if (index < 0 || index >= AllItems.Count)
        {
            return this;
        }

        var newAll = AllItems.ToList();
        newAll[index] = item;
        return this with { AllItems = newAll, Items = newAll };
    }

    /// <summary>
    /// Inserts an item at the given index.
    /// </summary>
    /// <param name="index">The index to insert at.</param>
    /// <param name="item">The item to insert.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> InsertItem(int index, TItem item)
    {
        var newAll = AllItems.ToList();
        var clampedIndex = Math.Clamp(index, 0, newAll.Count);
        newAll.Insert(clampedIndex, item);
        var model = this with { AllItems = newAll, Items = newAll };
        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(newAll.Count),
        };
        return model;
    }

    /// <summary>
    /// Removes the item at the given index.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> RemoveItem(int index)
    {
        if (index < 0 || index >= AllItems.Count)
        {
            return this;
        }

        var newAll = AllItems.ToList();
        newAll.RemoveAt(index);
        var model = this with { AllItems = newAll, Items = newAll };
        if (model.SelectedIndex >= newAll.Count && newAll.Count > 0)
        {
            model = model with { SelectedIndex = newAll.Count - 1 };
        }

        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(newAll.Count),
        };
        return model;
    }

    /// <summary>
    /// Moves the cursor up programmatically.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> CursorUp()
    {
        return CursorUpInternal();
    }

    /// <summary>
    /// Moves the cursor down programmatically.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> CursorDown()
    {
        return CursorDownInternal();
    }

    /// <summary>
    /// Moves the cursor to the first item.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> GoToStart()
    {
        return GoToStartInternal();
    }

    /// <summary>
    /// Moves the cursor to the last item.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> GoToEnd()
    {
        return GoToEndInternal();
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> PrevPage()
    {
        var (pag, _) = Paginator.PrevPage();
        return this with { Paginator = pag };
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> NextPage()
    {
        var (pag, _) = Paginator.NextPage();
        return this with { Paginator = pag };
    }

    /// <summary>
    /// Toggles the loading spinner.
    /// </summary>
    /// <returns>A tuple of the updated model and an optional command.</returns>
    public (ListModel<TItem> Model, Command? Command) ToggleSpinner()
    {
        return SetShowSpinner(!ShowSpinner);
    }

    /// <summary>
    /// Starts the loading spinner.
    /// </summary>
    /// <returns>A tuple of the updated model and an optional command.</returns>
    public (ListModel<TItem> Model, Command? Command) StartSpinner()
    {
        return SetShowSpinner(true);
    }

    /// <summary>
    /// Stops the loading spinner.
    /// </summary>
    /// <returns>A tuple of the updated model and an optional command.</returns>
    public (ListModel<TItem> Model, Command? Command) StopSpinner()
    {
        return SetShowSpinner(false);
    }

    /// <summary>
    /// Sets the spinner type.
    /// </summary>
    /// <param name="spinnerType">The spinner type to use.</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> SetSpinner(SpinnerType spinnerType)
    {
        return this with { Spinner = Spinner with { Spinner = spinnerType } };
    }

    /// <summary>
    /// Disables quit keybindings (q and Ctrl+C).
    /// </summary>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> DisableQuitKeybindings()
    {
        return this with
        {
            KeyMap = KeyMap with
            {
                Quit = KeyMap.Quit.Disabled(),
                ForceQuit = KeyMap.ForceQuit.Disabled(),
            },
        };
    }

    /// <summary>
    /// Gets or sets the singular item name for the status bar.
    /// </summary>
    public string StatusBarItemNameSingular { get; init; } = "item";

    /// <summary>
    /// Gets or sets the plural item name for the status bar.
    /// </summary>
    public string StatusBarItemNamePlural { get; init; } = "items";

    /// <summary>
    /// Sets the item name for the status bar.
    /// </summary>
    /// <param name="singular">Singular form (e.g., "file").</param>
    /// <param name="plural">Plural form (e.g., "files").</param>
    /// <returns>The updated model.</returns>
    public ListModel<TItem> SetStatusBarItemName(string singular, string plural)
    {
        return this with { StatusBarItemNameSingular = singular, StatusBarItemNamePlural = plural };
    }

    private (ListModel<TItem> Model, Command? Command) HandleKey(KeyMessage km)
    {
        // Cursor navigation with vim-style keys
        // Delegate update function
        if (UpdateFunc is { } updateFunc)
        {
            var (delegateModel, delegateCmd) = updateFunc(km, this);
            if (!ReferenceEquals(delegateModel, this) || delegateCmd is not null)
            {
                return (delegateModel, delegateCmd);
            }
        }

        if (KeyMap.CursorUp.Matches(km))
        {
            return (CursorUpInternal(), null);
        }

        if (KeyMap.CursorDown.Matches(km))
        {
            return (CursorDownInternal(), null);
        }

        if (KeyMap.GoToStart.Matches(km))
        {
            return (GoToStartInternal(), null);
        }

        if (KeyMap.GoToEnd.Matches(km))
        {
            return (GoToEndInternal(), null);
        }

        // Page navigation
        if (KeyMap.NextPage.Matches(km))
        {
            var (pag, pagCmd) = Paginator.NextPage();
            return (this with { Paginator = pag }, pagCmd);
        }

        if (KeyMap.PrevPage.Matches(km))
        {
            var (pag, pagCmd) = Paginator.PrevPage();
            return (this with { Paginator = pag }, pagCmd);
        }

        // Filter: "/" starts filter mode
        if (FilteringEnabled && KeyMap.StartFilter.Matches(km))
        {
            return EnterFilterMode();
        }

        // Toggle help: "?"
        if (KeyMap.ToggleHelp.Matches(km))
        {
            var (help, helpCmd) = Help.Update(km);
            return (this with { Help = help }, helpCmd);
        }

        // Clear filter when FilterApplied
        if (FilterState == ListFilterState.FilterApplied && KeyMap.ClearFilter.Matches(km) && km.Key == Key.Escape)
        {
            return (ResetFilter(), null);
        }

        // Quit: q or Ctrl+C
        if (KeyMap.Quit.Matches(km))
        {
            return (this, Commands.Quit());
        }

        // Force quit: Ctrl+C always
        if (KeyMap.ForceQuit.Matches(km))
        {
            return (this, Commands.Quit());
        }

        return (this, null);
    }

    private (ListModel<TItem> Model, Command? Command) HandleFilteringKey(KeyMessage km)
    {
        // Accept filter
        if (KeyMap.AcceptFilter.Matches(km))
        {
            return ApplyFilter();
        }

        // Cancel filter
        if (KeyMap.CancelFilter.Matches(km))
        {
            return CancelFilter();
        }

        // Forward to text input for typing
        var (input, inputCmd) = FilterInput.Update(km);
        var model = this with { FilterInput = input };

        // Apply live filter as user types
        model = model.ApplyLiveFilter();

        return (model, inputCmd);
    }

    private (ListModel<TItem> Model, Command? Command) EnterFilterMode()
    {
        var (input, inputCmd) = FilterInput.Focus();
        input = input.SetValue(string.Empty);
        var model = this with
        {
            FilterState = ListFilterState.Filtering,
            FilterInput = input,
        };
        return (model, inputCmd);
    }

    private (ListModel<TItem> Model, Command? Command) ApplyFilter()
    {
        var filterText = FilterInput.GetValue();
        var (input, inputCmd) = FilterInput.Blur();
        var model = this with { FilterInput = input };

        if (string.IsNullOrWhiteSpace(filterText))
        {
            // Empty filter: show all items
            model = model with
            {
                Items = model.AllItems,
                FilterState = ListFilterState.Unfiltered,
                SelectedIndex = 0,
            };
            model = model with
            {
                Paginator = model.Paginator.SetTotalPages(model.Items.Count) with { Page = 0 },
            };
            return (model, inputCmd);
        }

        model = model with { FilterState = ListFilterState.FilterApplied };
        return (model, inputCmd);
    }

    private (ListModel<TItem> Model, Command? Command) CancelFilter()
    {
        var (input, inputCmd) = FilterInput.Blur();
        var model = this with
        {
            FilterInput = input,
            Items = AllItems,
            FilterState = ListFilterState.Unfiltered,
            SelectedIndex = 0,
        };
        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(model.Items.Count) with { Page = 0 },
        };
        return (model, inputCmd);
    }

    private ListModel<TItem> ApplyLiveFilter()
    {
        var filterText = FilterInput.GetValue();
        if (string.IsNullOrWhiteSpace(filterText))
        {
            var reset = this with
            {
                Items = AllItems,
                SelectedIndex = 0,
            };
            reset = reset with
            {
                Paginator = reset.Paginator.SetTotalPages(reset.Items.Count) with { Page = 0 },
            };
            return reset;
        }

        var filtered = Filter is not null
            ? Filter(filterText, AllItems)
            : DefaultFilter(filterText, AllItems);

        var model = this with
        {
            Items = filtered,
            SelectedIndex = 0,
        };
        model = model with
        {
            Paginator = model.Paginator.SetTotalPages(filtered.Count) with { Page = 0 },
        };
        return model;
    }

    private static IReadOnlyList<TItem> DefaultFilter(string filter, IReadOnlyList<TItem> items)
    {
        return items
            .Where(item => item.FilterValue.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private ListModel<TItem> CursorUpInternal()
    {
        if (Items.Count == 0)
        {
            return this;
        }

        var newIndex = SelectedIndex - 1;
        if (newIndex < 0)
        {
            newIndex = InfiniteScrolling ? Items.Count - 1 : 0;
        }

        var model = this with { SelectedIndex = newIndex };
        return model.EnsureSelectedVisible();
    }

    private ListModel<TItem> CursorDownInternal()
    {
        if (Items.Count == 0)
        {
            return this;
        }

        var newIndex = SelectedIndex + 1;
        if (newIndex >= Items.Count)
        {
            newIndex = InfiniteScrolling ? 0 : Items.Count - 1;
        }

        var model = this with { SelectedIndex = newIndex };
        return model.EnsureSelectedVisible();
    }

    private ListModel<TItem> GoToStartInternal()
    {
        if (Items.Count == 0)
        {
            return this;
        }

        var model = this with { SelectedIndex = 0 };
        model = model with
        {
            Paginator = model.Paginator with { Page = 0 },
        };
        return model;
    }

    private ListModel<TItem> GoToEndInternal()
    {
        if (Items.Count == 0)
        {
            return this;
        }

        var model = this with { SelectedIndex = Items.Count - 1 };
        model = model with
        {
            Paginator = model.Paginator with { Page = model.Paginator.TotalPages - 1 },
        };
        return model;
    }

    private ListModel<TItem> EnsureSelectedVisible()
    {
        if (Paginator.PerPage <= 0)
        {
            return this;
        }

        var page = SelectedIndex / Paginator.PerPage;
        if (page != Paginator.Page)
        {
            return this with
            {
                Paginator = Paginator with { Page = page },
            };
        }

        return this;
    }

    private string GetDefaultStatusText()
    {
        var name = Items.Count == 1 ? StatusBarItemNameSingular : StatusBarItemNamePlural;
        if (FilterState == ListFilterState.FilterApplied)
        {
            return $"{Items.Count} {name} matched";
        }

        return $"{Items.Count} {name}";
    }

}

/// <summary>
/// Internal renderable adapter for rendering a single list item via its delegate.
/// </summary>
internal sealed class ListItemRenderable<TItem> : IRenderable
    where TItem : IListItem
{
    private readonly IListItemDelegate<TItem> _delegate;
    private readonly TItem _item;
    private readonly int _index;
    private readonly bool _selected;

    public ListItemRenderable(IListItemDelegate<TItem> itemDelegate, TItem item, int index, bool selected)
    {
        _delegate = itemDelegate;
        _item = item;
        _index = index;
        _selected = selected;
    }

    public void Render(IRenderSurface surface)
    {
        _delegate.Render(surface, _item, _index, _selected);
    }
}
