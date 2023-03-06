<script lang="ts">
	import { WalterDataTable, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];
</script>

<WalterHeaderList url={data.url} title="Anhänge" />

<WalterDataTable search {headers} rows={data.rows} />
