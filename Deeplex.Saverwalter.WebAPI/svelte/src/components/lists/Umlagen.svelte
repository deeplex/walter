<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataWrapper } from '..';
	import type { UmlageListEntry } from '../../types/umlagelist.type';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungen', value: 'Wohnungen' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/umlagen/${e.detail.id}`);

	export let rows: Promise<UmlageListEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
