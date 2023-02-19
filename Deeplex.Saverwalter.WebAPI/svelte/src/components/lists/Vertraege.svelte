<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataWrapper } from '..';
	import type { VertragListEntry } from '../../types/vertraglist.type';

	const headers = [
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'mieter', value: 'Mieter' },
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertraege/${e.detail.id}`);

	export let rows: Promise<VertragListEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
