<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataWrapper } from '..';
	import type { ZaehlerEntry } from '../../types/zaehler.type';

	const headers = [
		{ key: 'kennnummer', value: 'Kennnummer' },
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'typ', value: 'Typ' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/zaehler/${e.detail.id}`);

	export let rows: Promise<ZaehlerEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
