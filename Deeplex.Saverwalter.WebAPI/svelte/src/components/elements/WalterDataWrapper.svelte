<script lang="ts">
	import { AccordionItem, Loading } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataTable } from '$components';

	export let title: string | undefined = undefined;
	export let rows: Promise<any[]>;
	export let headers: {
		key: string;
		value: string;
	}[];
	export let search: boolean = false;
	export let navigate: (
		e: CustomEvent<DataTableRow>
	) => Promise<void> | void = () => {};
</script>

{#if title !== undefined}
	{#await rows}
		<AccordionItem>
			<svelte:fragment slot="title">
				<div style="display: flex; flex-direction: row; margin-left: -1em">
					<p class="bx--accordion__title" style="width: auto;">{title}</p>
					<Loading withOverlay={false} small style="margin-left: 1em" />
				</div>
			</svelte:fragment>
		</AccordionItem>
	{:then x}
		<AccordionItem title={`${title} (${x.length})`}>
			<WalterDataTable {search} {navigate} {rows} {headers} />
		</AccordionItem>
	{/await}
{:else}
	<WalterDataTable {search} {navigate} {rows} {headers} />
{/if}
