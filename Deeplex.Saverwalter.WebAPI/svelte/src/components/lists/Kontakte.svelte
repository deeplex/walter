<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$components';
	import type { PersonEntry } from '$types';

	const headers = [
		{ key: 'name', value: 'Name', default: '' },
		{ key: 'adresse.anschrift', value: 'Anschrift' },
		{ key: 'telefon', value: 'Telefon' },
		{ key: 'mobil', value: 'Mobil' },
		{ key: 'email', value: 'E-Mail' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(
			`/kontakte/${e.detail.id > 0 ? 'nat' : 'jur'}/${Math.abs(e.detail.id)}`
		);

	export let rows: Promise<PersonEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
