<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterMietminderungEntry } from '$WalterTypes';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' },
		{ key: 'minderung', value: 'Minderung' }
	];

	const addUrl = `/api/mietminderungen/`;

	export let rows: Promise<WalterMietminderungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry: Partial<WalterMietminderungEntry> | undefined = undefined;
</script>

<WalterDataWrapper {addUrl} addEntry={entry} {title} {search} {rows} {headers}>
	{#if entry}
		<p>TODO WALTERMIETMINDERUNG</p>
		<!-- <WalterMietminderung {entry} /> -->
	{/if}
</WalterDataWrapper>
