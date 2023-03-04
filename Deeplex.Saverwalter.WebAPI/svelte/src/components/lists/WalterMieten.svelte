<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterTypes';

	const headers = [
		{ key: 'betreffenderMonat', value: 'Betreffender Monat' },
		{ key: 'zahlungsdatum', value: 'Zahlungsdatum' },
		{ key: 'betrag', value: 'Betrag' }
	];

	const addUrl = `/api/mieten/`;

	export let rows: Promise<WalterMieteEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry: Partial<WalterMieteEntry> | undefined = undefined;
</script>

<WalterDataWrapper {addUrl} addEntry={entry} {title} {search} {rows} {headers}>
	{#if entry}
		<p>TODO WALTERMIETE</p>
		<!-- <WalterMiete {entry} /> -->
	{/if}
</WalterDataWrapper>
