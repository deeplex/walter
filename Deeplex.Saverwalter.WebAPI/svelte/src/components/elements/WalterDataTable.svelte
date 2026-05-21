<!-- Copyright (C) 2023-2025  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import type { ComponentType } from 'svelte';
    import {
        Button,
        Content,
        DataTable,
        DataTableSkeleton,
        Pagination,
        Toolbar,
        ToolbarContent,
        ToolbarSearch
    } from 'carbon-components-svelte';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        convertEuro,
        convertPercent,
        convertTime
    } from '$walter/services/utils';
    import { dates, euro, formatToTableDate, time } from './WalterDataTable';
    import { Add } from 'carbon-icons-svelte';
    import { onMount } from 'svelte';
    import { page } from '$app/stores';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';
    import WalterDataTableContainer from './WalterDataTableContainer.svelte';

    type WalterDataTableRow = DataTableRow & {
        id: string | number;
        permissions?: {
            read?: boolean;
            update?: boolean;
        };
    };

    type FetchParams = {
        search?: string;
        sortBy?: string;
        sortDir: 'asc' | 'desc';
        skip: number;
        take: number;
    };

    export let fullHeight = false;
    export let size: 'compact' | 'short' | 'medium' | 'tall' | undefined =
        undefined;
    export let headers: {
        key: string;
        value: string;
    }[];
    export let readonly = false;
    export let rows: WalterDataTableRow[] | undefined = undefined;
    export let rowHref:
        | ((row: WalterDataTableRow) => string | undefined)
        | undefined = undefined;
    export let layout: 'inline' | 'accordion' = 'inline';
    export let accordionTitle: string | undefined = undefined;
    export let initialOpen = false;
    export let quickAddTitle: string | undefined = undefined;
    export let addUrl: string | undefined = undefined;
    export let addEntry: unknown = undefined;
    export let onAdd: (() => void) | undefined = undefined;
    export let beforeSubmit:
        | undefined
        | ((entry: unknown) => boolean | Promise<boolean>) = undefined;
    export let on_click_row: (
        e: CustomEvent<DataTableRow>
    ) => Promise<void> | void = () => {};

    // When provided, WalterDataTable manages server-side pagination internally.
    export let fetchData:
        | ((
              params: FetchParams
          ) => Promise<{ items: WalterDataTableRow[]; totalCount: number }>)
        | undefined = undefined;
    export let initialSortBy = '';
    export let initialSortDir: 'asc' | 'desc' = 'desc';
    export let initialPageSize = 50;
    // Optional row transform applied to server-fetched rows before rendering.
    export let transformRow:
        | ((row: WalterDataTableRow) => WalterDataTableRow)
        | undefined = undefined;

    let addModalOpen = false;

    const searchParams: URLSearchParams | null = $page
        ? new URL($page.url).searchParams
        : null;
    let searchQuery = searchParams?.get('search') || '';

    $: resolvedAccordionTitle = accordionTitle || '';
    $: resolvedQuickAddTitle = quickAddTitle ?? accordionTitle;

    // ── Internal server-pagination state ──────────────────────────────────────
    let _pagedRows: WalterDataTableRow[] = [];
    let _totalCount = 0;
    let _page = 1;
    let _pageSize = initialPageSize;
    let _sortBy = initialSortBy;
    let _sortDir: 'asc' | 'desc' = initialSortDir;
    // Initialise from URL so that a page reload preserves the search term.
    let _appliedSearch = searchParams?.get('search') || '';
    let _loading = false;

    async function _fetchData() {
        if (!fetchData) return;
        _loading = true;
        try {
            const result = await fetchData({
                search: _appliedSearch || undefined,
                sortBy: _sortBy || undefined,
                sortDir: _sortDir,
                skip: (_page - 1) * _pageSize,
                take: _pageSize
            });
            _pagedRows = result.items;
            _totalCount = result.totalCount;
        } finally {
            _loading = false;
        }
    }

    $: if (fetchData)
        void (_page,
        _pageSize,
        _sortBy,
        _sortDir,
        _appliedSearch,
        _fetchData());

    $: showSkeleton =
        fetchData !== undefined && _loading && _pagedRows.length === 0;

    $: displayRows = fetchData
        ? transformRow
            ? _pagedRows.map(transformRow)
            : _pagedRows
        : (rows ?? []);

    function quick_add() {
        addModalOpen = true;
    }

    function handleAddClick() {
        quick_add();
    }

    function onSubmit(new_value: unknown) {
        if (fetchData) {
            void _fetchData();
        } else {
            rows = [...(rows ?? []), new_value as WalterDataTableRow];
        }
    }

    $: useServerSearch = !!fetchData;

    function handleHeaderClick(e: CustomEvent) {
        if (!fetchData) return;
        const { header, sortDirection } = e.detail as {
            header: { key: string };
            sortDirection?: 'ascending' | 'descending' | 'none';
        };
        if (!sortDirection || sortDirection === 'none') return;
        _sortBy = header.key;
        _sortDir = sortDirection === 'ascending' ? 'asc' : 'desc';
        _page = 1;
    }

    let searchMounted = false;
    onMount(() => {
        searchMounted = true;
    });

    let _searchTimer: ReturnType<typeof setTimeout> | undefined;

    function updateSearchUrl(query: string) {
        if (!searchParams) return;
        if (query) {
            searchParams.set('search', query);
        } else {
            searchParams.delete('search');
        }
        window.history.replaceState({}, '', `?${searchParams.toString()}`);
    }

    function dispatchServerSearch(query: string) {
        if (!fetchData) return;
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => {
            _appliedSearch = query;
            _page = 1;
            updateSearchUrl(query);
        }, 300);
    }

    $: if (searchMounted) dispatchServerSearch(searchQuery);

    function toolbarSearchInput() {
        if (fetchData) return;
        updateSearchUrl(searchQuery);
    }

    $: hasButtonColumn = headers.some((header) => header.key === 'button');
    $: hasCompactButtonColumn = headers.some(
        (header) =>
            header.key === 'button' && `${header.value || ''}`.trim() === ''
    );

    function resolveActionButton(cellValue: unknown): {
        disabled: boolean;
        onClick: (e: MouseEvent) => void;
        kind:
            | 'danger'
            | 'primary'
            | 'secondary'
            | 'tertiary'
            | 'ghost'
            | 'danger-tertiary'
            | 'danger-ghost';
        icon: ComponentType;
        iconDescription: string;
    } {
        if (cellValue === 'disabled') {
            return {
                disabled: true,
                onClick: () => {},
                kind: 'tertiary',
                icon: Add,
                iconDescription: 'Hinzufügen'
            };
        }

        if (typeof cellValue === 'function') {
            return {
                disabled: false,
                onClick: cellValue as (e: MouseEvent) => void,
                kind: 'tertiary',
                icon: Add,
                iconDescription: 'Hinzufügen'
            };
        }

        if (typeof cellValue === 'object' && cellValue !== null) {
            const buttonValue = cellValue as {
                disabled?: boolean;
                onClick?: (e: MouseEvent) => void;
                kind?:
                    | 'danger'
                    | 'primary'
                    | 'secondary'
                    | 'tertiary'
                    | 'ghost'
                    | 'danger-tertiary'
                    | 'danger-ghost';
                icon?: ComponentType;
                iconDescription?: string;
            };

            return {
                disabled: !!buttonValue.disabled,
                onClick: buttonValue.onClick || (() => {}),
                kind: buttonValue.kind || 'tertiary',
                icon: buttonValue.icon || Add,
                iconDescription: buttonValue.iconDescription || 'Hinzufügen'
            };
        }

        return {
            disabled: true,
            onClick: () => {},
            kind: 'tertiary',
            icon: Add,
            iconDescription: 'Hinzufügen'
        };
    }

    function resolveRowHref(row: DataTableRow): string | undefined {
        const typedRow = row as WalterDataTableRow;

        if (typedRow.permissions && !typedRow.permissions.read) {
            return undefined;
        }

        return rowHref?.(typedRow);
    }

    function onRowLinkClick(event: MouseEvent) {
        // Prevent DataTable row click handlers from firing when the user
        // explicitly clicked the anchor, while preserving native link behavior.
        event.stopPropagation();
    }

    function getCellDisplayValue(cell: {
        key: string;
        value: unknown;
    }): string {
        if (
            cell.value === null ||
            cell.value === undefined ||
            cell.value === ''
        ) {
            return '---';
        }
        if (dates(cell.key)) {
            return formatToTableDate(`${cell.value}`);
        }
        if (time(cell.key)) {
            return (
                convertTime(cell.value as Date | string | undefined) || '---'
            );
        }
        if (euro(cell.key)) {
            return convertEuro(cell.value as number | undefined) || '---';
        }
        if (cell.key === 'anteil') {
            return convertPercent(cell.value as number | undefined) || '---';
        }

        return `${cell.value}`;
    }

    function shouldFilterRows(row: { [key: string]: unknown }, value: unknown) {
        const filteredValues = headers.map((headerObj) => {
            const keys = headerObj.key.split('.');
            let value: unknown | null = row;
            keys.forEach((key) => {
                if (
                    typeof value === 'object' &&
                    value !== null &&
                    key in value
                ) {
                    value = (value as { [key: string]: unknown })[key];
                } else {
                    value = null;
                }
            });
            return value;
        });

        const values = `${value}`
            .toLowerCase()
            .split(';')
            .map((e) => e.trim());
        return values.every((val) =>
            filteredValues.some((e) => `${e}`.toLowerCase().includes(val))
        );
    }

    const disabledRows = (rows ?? [])
        .filter((row) => row.permissions && !row.permissions?.read)
        .map((row) => row.id);
    const enabledRows = (rows ?? [])
        .filter((row) => row.permissions?.read)
        .map((row) => row.id);

    const disabledSelector = disabledRows
        .map((id) => `[data-row="${id}"]`)
        .join(',');
    const enabledSelector = enabledRows
        .map((id) => `[data-row="${id}"]`)
        .join(',');
    const styleRules = [
        disabledSelector
            ? `${disabledSelector} {
        opacity: 0.5;
        cursor: not-allowed !important;
        pointer-events: none !important;
    }`
            : '',
        enabledSelector
            ? `${enabledSelector} {
        color: #ff0;
        cursor: pointer !important;
      }`
            : ''
    ].filter(Boolean);

    onMount(() => {
        const styleElements = styleRules.map((rule) => {
            const styleElement = document.createElement('style');
            styleElement.textContent = rule;
            document.head.appendChild(styleElement);
            return styleElement;
        });

        return () => {
            styleElements.forEach((styleElement) => styleElement.remove());
        };
    });
</script>

<Content>
    {#if addUrl}
        <WalterDataWrapperQuickAdd
            bind:addEntry
            {addUrl}
            bind:addModalOpen
            {onSubmit}
            {beforeSubmit}
            title={resolvedQuickAddTitle}
        >
            <slot />
        </WalterDataWrapperQuickAdd>
    {/if}

    <WalterDataTableContainer
        {layout}
        accordionTitle={resolvedAccordionTitle}
        count={fetchData ? _totalCount : displayRows.length}
        {initialOpen}
    >
        {#if showSkeleton}
            <DataTableSkeleton showHeader={false} showToolbar={false} />
        {:else}
            <div
                class:has-compact-button-column={hasCompactButtonColumn}
                class:sticky-chrome={!!fetchData}
            >
                <DataTable
                    {size}
                    on:click:row={on_click_row}
                    on:click:header={handleHeaderClick}
                    sortable
                    zebra
                    stickyHeader={!fetchData}
                    {headers}
                    rows={displayRows}
                    class={`${fullHeight ? 'proper-list' : ''} ${hasButtonColumn ? 'has-button-column' : ''}`}
                    style="cursor-events: none !important; max-height: none !important;"
                >
                    <Toolbar>
                        <ToolbarContent>
                            <ToolbarSearch
                                placeholder={useServerSearch
                                    ? 'Suche...'
                                    : 'Suche mit ; separierter Liste...'}
                                persistent
                                on:input={toolbarSearchInput}
                                bind:value={searchQuery}
                                shouldFilterRows={useServerSearch
                                    ? () => true
                                    : shouldFilterRows}
                            />
                            {#if (addUrl || onAdd) && !readonly}
                                <Button
                                    style="right: -1em; position: sticky;"
                                    on:click={onAdd ?? handleAddClick}
                                    iconDescription="Eintrag hinzufügen"
                                    icon={Add}
                                >
                                    Eintrag hinzufügen
                                </Button>
                            {/if}
                        </ToolbarContent>
                    </Toolbar>
                    <span
                        class="walter-table-cell"
                        slot="cell"
                        let:cell
                        let:row
                    >
                        {#if cell.key === 'button'}
                            {@const buttonConfig = resolveActionButton(
                                cell.value
                            )}
                            <span class="button-cell">
                                <Button
                                    disabled={buttonConfig.disabled}
                                    on:click={buttonConfig.onClick}
                                    tooltipPosition="left"
                                    size="small"
                                    style="margin: 0;"
                                    kind={buttonConfig.kind}
                                    icon={buttonConfig.icon}
                                    iconDescription={buttonConfig.iconDescription}
                                />
                            </span>
                        {:else}
                            {@const displayValue = getCellDisplayValue(cell)}
                            {@const tooltip =
                                displayValue === '---'
                                    ? undefined
                                    : displayValue}
                            {@const href = resolveRowHref(row)}
                            {#if href}
                                <a
                                    class="walter-table-link"
                                    {href}
                                    title={tooltip}
                                    on:click={onRowLinkClick}
                                >
                                    <span class="walter-table-link__label"
                                        >{displayValue}</span
                                    >
                                </a>
                            {:else}
                                <span class="walter-table-text" title={tooltip}
                                    >{displayValue}</span
                                >
                            {/if}
                        {/if}
                    </span>
                </DataTable>
            </div>
            {#if fetchData && _totalCount > _pageSize}
                <div class="pagination-sticky">
                    <Pagination
                        totalItems={_totalCount}
                        pageSize={_pageSize}
                        page={_page}
                        pageSizes={[25, 50, 100, 250]}
                        on:update={(e) => {
                            _page = e.detail.page;
                            _pageSize = e.detail.pageSize;
                        }}
                    />
                </div>
            {/if}
        {/if}
    </WalterDataTableContainer>

    <style>
        .pagination-sticky {
            position: sticky;
            bottom: 0;
        }

        .proper-list
            > .bx--data-table_inner-container
            > .bx--data-table--sticky-header {
            max-height: none !important;
        }

        .walter-table-cell {
            display: block;
            width: 100%;
        }

        .walter-table-link {
            color: inherit;
            text-decoration: none;
            display: block;
            box-sizing: border-box;
            width: calc(100% + 2rem);
            margin: -0.5rem -1rem;
            padding: 0.5rem 1rem;
        }

        .walter-table-link__label,
        .walter-table-text {
            display: block;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .button-cell {
            align-items: center;
            display: flex;
            height: 100%;
            justify-content: center;
            min-height: 1.5rem;
        }

        :global(.has-compact-button-column .bx--data-table th:last-child),
        :global(.has-compact-button-column .bx--data-table td:last-child) {
            max-width: 2.75rem !important;
            min-width: 2.75rem !important;
            padding-left: 0.25rem !important;
            padding-right: 0.25rem !important;
            text-align: center;
            vertical-align: middle;
            white-space: nowrap;
            width: 2.75rem !important;
        }
    </style>
</Content>
