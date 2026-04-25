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
    import {
        Button,
        Content,
        DataTable,
        DataTableSkeleton,
        SkeletonPlaceholder,
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
    import { walter_goto } from '$walter/services/utils';
    import { dates, euro, formatToTableDate, time } from './WalterDataTable';
    import { Add } from 'carbon-icons-svelte';
    import { onMount } from 'svelte';
    import { page } from '$app/stores';

    type WalterDataTableRow = DataTableRow & {
        id: string | number;
        permissions?: {
            read?: boolean;
        };
    };

    export let fullHeight = false;
    export let size: 'compact' | 'short' | 'medium' | 'tall' | undefined =
        undefined;
    export let headers: {
        key: string;
        value: string;
    }[];
    export let readonly = false;
    export let rows: WalterDataTableRow[];
    export let add: (() => void) | undefined = undefined;
    export let rowHref:
        | ((row: WalterDataTableRow) => string | undefined)
        | undefined = undefined;

    const searchParams: URLSearchParams | null = $page
        ? new URL($page.url).searchParams
        : null;
    let searchQuery = searchParams?.get('search') || '';

    function toolbarSearchInput() {
        if (searchParams) {
            if (searchQuery) {
                searchParams.set('search', searchQuery);
                window.history.replaceState(
                    {},
                    '',
                    `?${searchParams.toString()}`
                );
            } else {
                searchParams.delete('search');
                window.history.replaceState(
                    {},
                    '',
                    `?${searchParams.toString()}`
                );
            }
        }
    }

    export let on_click_row: (
        e: CustomEvent<DataTableRow>
    ) => Promise<void> | void = () => {};

    function resolveRowHref(row: DataTableRow): string | undefined {
        const typedRow = row as WalterDataTableRow;

        if (typedRow.permissions && !typedRow.permissions.read) {
            return undefined;
        }

        return rowHref?.(typedRow);
    }

    function onRowLinkClick(event: MouseEvent, href: string) {
        // Keep browser-native link behavior for new-tab/window gestures.
        if (
            event.defaultPrevented ||
            event.button !== 0 ||
            event.metaKey ||
            event.ctrlKey ||
            event.shiftKey ||
            event.altKey
        ) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        walter_goto(href);
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
            return convertTime(cell.value as Date | string | undefined) || '---';
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

    const disabledRows = rows
        .filter((row) => row.permissions && !row.permissions?.read)
        .map((row) => row.id);
    const enabledRows = rows
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
    <DataTable
        {size}
        on:click:row={on_click_row}
        sortable
        zebra
        stickyHeader
        {headers}
        {rows}
        class={fullHeight ? 'proper-list' : ''}
        style="cursor-events: none !important; max-height: none !important;"
    >
        <Toolbar>
            <ToolbarContent>
                <ToolbarSearch
                    placeholder="Suche mit ; separierter Liste..."
                    persistent
                    on:input={toolbarSearchInput}
                    bind:value={searchQuery}
                    {shouldFilterRows}
                />
                {#if !!add && !readonly}
                    <Button
                        style="right: -1em; position: sticky;"
                        on:click={add}
                        iconDescription="Eintrag hinzufügen"
                        icon={Add}
                    >
                        Eintrag hinzufügen
                    </Button>
                {/if}
            </ToolbarContent>
        </Toolbar>
        <span
            style="text-overflow: ellipsis; white-space: nowrap; overflow:hidden;"
            slot="cell"
            let:cell
            let:row
        >
            {#if cell.key === 'button'}
                <Button
                    disabled={cell.value === 'disabled'}
                    on:click={cell.value}
                    tooltipPosition="left"
                    style="position: absolute; margin-top: -15px; margin-left: 1em; scale: 0.65"
                    kind="tertiary"
                    icon={Add}
                    iconDescription={'Hinzufügen'}
                />
            {:else}
                {@const displayValue = getCellDisplayValue(cell)}
                {@const tooltip = displayValue === '---' ? undefined : displayValue}
                {@const href = resolveRowHref(row)}
                {#if href}
                    <a
                        class="walter-table-link"
                        {href}
                        title={tooltip}
                        on:click={(event) => onRowLinkClick(event, href)}
                    >
                        {displayValue}
                    </a>
                {:else}
                    <span title={tooltip}>{displayValue}</span>
                {/if}
            {/if}
        </span>
    </DataTable>

    <style>
        .proper-list
            > .bx--data-table_inner-container
            > .bx--data-table--sticky-header {
            max-height: none !important;
        }

        .walter-table-link {
            color: inherit;
            text-decoration: none;
            display: block;
            width: 100%;
            height: 100%;
        }
    </style>
</Content>
