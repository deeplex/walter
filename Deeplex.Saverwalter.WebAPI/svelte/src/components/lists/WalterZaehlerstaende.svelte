<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';

	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' },
		{ key: 'einheit', value: 'Einheit' }
	];

	const addUrl = `/api/zaehlerstaende/`;

	export let rows: Promise<WalterZaehlerstandEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let a: Promise<Partial<WalterZaehlerstandEntry>> | undefined =
		undefined;
	let entry: Partial<WalterZaehlerstandEntry> | undefined = undefined;
	a?.then((e) => (entry = e));
</script>

<WalterDataWrapper {addUrl} addEntry={entry} {title} {search} {rows} {headers}>
	{#if entry}
		<p>TODO WALTERZAEHLERSTAND</p>
		<!-- <WalterZaehlerstaende {a} {entry} /> -->
	{/if}
</WalterDataWrapper>
