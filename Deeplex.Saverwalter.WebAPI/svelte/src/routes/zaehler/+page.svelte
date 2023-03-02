<script lang="ts">
	import { WalterHeaderList, WalterZaehlerList } from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterZaehlerEntry } from '$WalterTypes';

	const url = 'zaehler';

	const rows: Promise<WalterZaehlerEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Zähler" />
<WalterZaehlerList search {rows} />
