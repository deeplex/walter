<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataTable, WalterHeader } from '../../../components';
	import Erhaltungsaufwendungen from '../../../components/lists/Erhaltungsaufwendungen.svelte';
	import { walter_get } from '../../../services/utils';
	import type { ErhaltungsaufwendungListEntry } from '../../../types/erhaltungsaufwendunglist.type';
	const rows: Promise<ErhaltungsaufwendungListEntry[]> = walter_get(
		'/api/erhaltungsaufwendungen'
	);
</script>

<WalterHeader title="Erhaltungsaufwendungen" />
<Erhaltungsaufwendungen search {rows} />
