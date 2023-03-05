<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterVertragVersionEntry } from '$WalterTypes';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'personenzahl', value: 'Personenzahl' },
		{ key: 'grundmiete', value: 'Grundmiete' }
	];

	const addUrl = `/api/vertragversionen/`;

	export let rows: Promise<WalterVertragVersionEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let a: Promise<Partial<WalterVertragVersionEntry>> | undefined =
		undefined;
	let entry: Partial<WalterVertragVersionEntry> | undefined = undefined;
	a?.then((e) => (entry = e));
</script>

<WalterDataWrapper {addUrl} addEntry={entry} {title} {search} {rows} {headers}>
	{#if entry}
		<p>TODO WALTERVERTRAGVERSION</p>
		<!-- <WalterVertragVersion {a} {entry} /> -->
	{/if}
</WalterDataWrapper>
