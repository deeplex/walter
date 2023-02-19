<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { Umlagen, WalterDataTable, WalterHeader } from '../../components';
	import { walter_get } from '../../services/utils';
	import type { UmlageListEntry } from '../../types/umlagelist.type';

	const rows: Promise<UmlageListEntry[]> = walter_get('/api/umlagen');
</script>

<WalterHeader title="Umlagen" />
<Umlagen search {rows} />
