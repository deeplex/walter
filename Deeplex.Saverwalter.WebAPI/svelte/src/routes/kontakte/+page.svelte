<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { request_options, walter_get } from '../../services/utils';
	import type { ZaehlerListEntry } from '../../types/zaehlerlist.type';
	import { WalterDataTable, WalterHeader } from '../../components';
	import type { KontaktListEntry } from '../../types/kontaktlist.type';
	import Kontakte from '../../components/lists/Kontakte.svelte';

	const rows: Promise<KontaktListEntry[]> = walter_get('/api/kontakte');
</script>

<WalterHeader title="Kontakte" />
<Kontakte search {rows} />
