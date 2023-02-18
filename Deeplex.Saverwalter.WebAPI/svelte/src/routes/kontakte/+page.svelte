<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { request_options, walter_get } from '../../services/utils';
	import type { ZaehlerListEntry } from '../../types/zaehlerlist.type';
	import { WalterDataTable, WalterHeader } from '../../components';

	const headers = [
		{ key: 'name', value: 'Name' },
		{ key: 'anschrift', value: 'Anschrift' },
		{ key: 'telefon', value: 'Telefon' },
		{ key: 'mobil', value: 'Mobil' },
		{ key: 'email', value: 'E-Mail' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(
			`/kontakte/${e.detail.id > 0 ? 'nat' : 'jur'}/${Math.abs(e.detail.id)}`
		);

	const rows: Promise<ZaehlerListEntry[]> = walter_get('/api/kontakte');
</script>

<WalterHeader title="Kontakte" />
<WalterDataTable {navigate} {rows} {headers} />
