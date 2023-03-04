<script lang="ts">
	import {
		AccordionItem,
		Button,
		Loading,
		Modal
	} from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataTable } from '$WalterComponents';
	import { Add } from 'carbon-icons-svelte';
	import { walter_post } from '$WalterServices/requests';

	export let addUrl: string | undefined = undefined;
	export let addEntry: any | undefined = undefined;
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

	let addModalOpen: boolean = false;
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
			{#if x.length}
				<WalterDataTable {search} {navigate} {rows} {headers} />
			{/if}
			<div style="float: right">
				<Button
					on:click={() => (addModalOpen = true)}
					iconDescription="Eintrag hinzufügen"
					icon={Add}
				/>
			</div>
			{#if addUrl && addEntry}
				<Modal
					secondaryButtonText="Abbrechen"
					primaryButtonText="Bestätigen"
					on:submit={() => walter_post(addUrl || '', addEntry)}
					on:click:button--secondary={() => (addModalOpen = false)}
					on:click:button--primary={() => (addModalOpen = false)}
					modalHeading="Eintrag zu {title} hinzufügen"
					bind:open={addModalOpen}
				>
					<slot />
				</Modal>
			{/if}
		</AccordionItem>
	{/await}
{:else}
	<WalterDataTable {search} {navigate} {rows} {headers} />
{/if}
