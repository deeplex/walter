<script lang="ts">
	import { walter_get } from '../../services/utils';
	import { WalterHeader, WalterDataTable } from '../../components';
	import type { AnhangListEntry } from '../../types/anhanglist.type';

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const rows: Promise<AnhangListEntry[]> = walter_get('/api/anhaenge');
</script>

<WalterHeader title="Anhänge" />

<WalterDataTable {headers} {rows} />
