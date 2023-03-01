<script lang="ts">
	import { WalterHeader, Zaehler } from '$components';
	import { walter_get } from '$services/utils';
	import type { ZaehlerEntry } from '$types';

	const rows: Promise<ZaehlerEntry[]> = walter_get('/api/zaehler');
</script>

<WalterHeader title="Zähler" />
<Zaehler search {rows} />
