<script lang="ts">
	import {
		AccordionItem,
		Button,
		Loading,
		Modal,
		Tile
	} from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataTable } from '$WalterComponents';
	import { Add } from 'carbon-icons-svelte';
	import { walter_post } from '$WalterServices/requests';

	export let addUrl: string | undefined = undefined;
	export let addEntry: any | undefined = undefined;
	export let title: string | undefined = undefined;
	export let rows: any[];
	export let headers: {
		key: string;
		value: string;
	}[];
	export let search: boolean = false;
	export let navigate: (
		e: CustomEvent<DataTableRow>
	) => Promise<void> | void = () => {};

	let addModalOpen: boolean = false;
	let open: boolean = false;

	async function click_post(url: string | undefined, body: any) {
		if (!url) {
			return;
		}
		const j = await walter_post(url, body);
		rows = [...rows, j];
		open = true;
	}
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
		<AccordionItem title={`${title} (${x.length})`} bind:open>
			{#if x.length}
				<WalterDataTable {search} {navigate} bind:rows {headers} />
			{/if}
			{#if addUrl && addEntry}
				<div style="float: right">
					<Button
						on:click={() => (addModalOpen = true)}
						iconDescription="Eintrag hinzufügen"
						icon={Add}
					/>
				</div>
				<Modal
					secondaryButtonText="Abbrechen"
					primaryButtonText="Bestätigen"
					on:submit={() => click_post(addUrl, addEntry)}
					on:click:button--secondary={() => (addModalOpen = false)}
					on:click:button--primary={() => (addModalOpen = false)}
					modalHeading="Eintrag zu {title} hinzufügen"
					bind:open={addModalOpen}
				>
					<slot />
					<!-- Spacer for DatePickers. Otherwise the modal is too narrow -->
					<Tile style="height: 13em" />
				</Modal>
			{/if}
		</AccordionItem>
	{/await}
{:else}
	<WalterDataTable {search} {navigate} {rows} {headers} />
{/if}
