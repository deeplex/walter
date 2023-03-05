<script lang="ts">
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { goto } from '$app/navigation';
	import {
		WalterAdressen,
		WalterDataTable,
		WalterHeaderList
	} from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList url={data.url} title="Adressen" />
<WalterAdressen search rows={data.rows} />
