<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataWrapper } from '..';
	import type { VertragEntry } from '../../types/vertrag.type';

	const headers = [
		{ key: 'wohnung.anschrift', value: 'Wohnung' },
		{ key: 'mieterAuflistung', value: 'Mieter' },
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertraege/${e.detail.id}`);

	export let rows: Promise<VertragEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
