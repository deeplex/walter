<script lang="ts">
	import { WalterHeaderList, Zaehler } from '$components';
	import { walter_get } from '$services/utils';
	import type { ZaehlerEntry } from '$types';

	const url = 'zaehler';

	const rows: Promise<ZaehlerEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Zähler" />
<Zaehler search {rows} />
