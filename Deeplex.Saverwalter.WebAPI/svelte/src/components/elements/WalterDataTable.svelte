<script lang="ts">
  import {
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
    convertDate,
    convertEuro,
    convertPercent,
    convertTime
  } from '$WalterServices/utils';

  export let headers: {
    key: string;
    value: string;
  }[];
  export let rows: any[];

  export let navigate: (
    e: CustomEvent<DataTableRow>
  ) => Promise<void> | void = () => {};
  export let search: boolean = false;

  function dates(key: string) {
    switch (key) {
      case 'beginn':
      case 'ende':
      case 'datum':
      case 'betreffenderMonat':
      case 'zahlungsdatum':
        return true;
      default:
        return false;
    }
  }
</script>

<Content>
  {#await rows}
    {#if search}
      <SkeletonPlaceholder style="margin:0; width: 100%; height:3rem" />
    {/if}
    <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
  {:then x}
    <DataTable
      on:click:row={navigate}
      sortable
      zebra
      stickyHeader
      {headers}
      rows={x}
      style="cursor: pointer"
    >
      {#if search}
        <Toolbar>
          <ToolbarContent>
            <ToolbarSearch placeholder="Suche..." persistent shouldFilterRows />
          </ToolbarContent>
        </Toolbar>
      {/if}
      <span
        style="text-overflow: ellipsis; white-space: nowrap; overflow:hidden;"
        slot="cell"
        let:cell
      >
        {#if cell.value === null || cell.value === undefined || cell.value === ''}
          ---
        {:else if dates(cell.key)}
          {new Date(cell.value).toLocaleDateString('de-DE', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
          })}
        {:else if cell.key === 'creationTime'}
          {convertTime(cell.value)}
        {:else if cell.key === 'betrag' || cell.key === 'grundmiete' || cell.key === 'kosten'}
          {convertEuro(cell.value)}
        {:else if cell.key === 'anteil'}
          {convertPercent(cell.value)}
        {:else}
          {cell.value}
        {/if}
      </span>
    </DataTable>
  {/await}
</Content>
